using MySqlConnector;
using System.Data;

namespace SmartRestart;

public class DatabaseManager
{
    private readonly DatabaseConfig _config;
    private readonly string _connectionString;
    private int? _serverId = null;
    private readonly string _hostname;
    private readonly bool _startupTableMode;
    private readonly Dictionary<string, (bool HasPermission, DateTime ExpiresAt)> _permissionCache = new();
    private readonly object _permissionCacheLock = new();

    public DatabaseManager(DatabaseConfig config, string hostname, bool startupTableMode = false)
    {
        _config = config;
        _hostname = hostname;
        _startupTableMode = startupTableMode;
        // Add connection string options to avoid threading issues
        _connectionString = $"Server={config.Host};Port={config.Port};Database={config.Database};Uid={config.Username};Pwd={config.Password};AllowUserVariables=True;UseCompression=False;";
    }

    private static void SafeLog(string message)
    {
        // Use standard output to avoid CS2 main thread requirement
        System.Console.WriteLine(message);
    }

    private static void SafeLogVerbose(string message)
    {
        // Intentionally silent in normal operation. Keep this method as a single
        // switch point if verbose database tracing is needed during troubleshooting.
    }

    private void SafeLogDuringStartup(string message)
    {
        if (!_startupTableMode)
        {
            SafeLog(message);
        }
    }

    public async Task<bool> InitializeAsync()
    {
        try
        {
            SafeLogDuringStartup($"[SmartRestart] Connecting to database: {_config.Host}:{_config.Port} / Database: {_config.Database} / User: {_config.Username}");

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            SafeLogDuringStartup($"[SmartRestart] ✅ Database connection successful!");

            if (string.IsNullOrWhiteSpace(_hostname))
            {
                SafeLogDuringStartup($"[SmartRestart] WARNING: Server hostname is empty!");
                return false;
            }

            SafeLogDuringStartup($"[SmartRestart] Looking for hostname: {_hostname}");

            // Find server ID by hostname in sa_servers table
            var query = "SELECT id FROM sa_servers WHERE hostname = @hostname LIMIT 1";
            await using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@hostname", _hostname);

            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);
            if (result != null)
            {
                _serverId = Convert.ToInt32(result);
                SafeLogDuringStartup($"[SmartRestart] ✅ Found Server ID: {_serverId}");
                return true;
            }
            else
            {
                SafeLogDuringStartup($"[SmartRestart] WARNING: Hostname '{_hostname}' not found in sa_servers table!");
                SafeLogDuringStartup($"[SmartRestart] Please ensure your server's hostname matches what's in SimpleAdmin's sa_servers table.");
                return false;
            }
        }
        catch (MySqlException ex)
        {
            SafeLog($"[SmartRestart] ❌ MySQL Error: {ex.Message}");
            SafeLog($"[SmartRestart] Error Code: {ex.Number}");

            // Provide specific error guidance
            switch (ex.Number)
            {
                case 1042: // Unable to connect
                case 1045: // Access denied
                    SafeLog($"[SmartRestart] → Check your database credentials in config.json");
                    SafeLog($"[SmartRestart] → Verify: Host={_config.Host}, Port={_config.Port}, User={_config.Username}, Database={_config.Database}");
                    break;
                case 1049: // Unknown database
                    SafeLog($"[SmartRestart] → Database '{_config.Database}' does not exist");
                    SafeLog($"[SmartRestart] → Verify the database name is correct");
                    break;
                case 1044: // Access denied to database
                    SafeLog($"[SmartRestart] → User '{_config.Username}' does not have access to database '{_config.Database}'");
                    break;
                default:
                    SafeLog($"[SmartRestart] → Check database server is running and accessible");
                    break;
            }

            return false;
        }
        catch (Exception ex)
        {
            SafeLog($"[SmartRestart] ❌ Database initialization error: {ex.Message}");
            SafeLog($"[SmartRestart] → Check your config.json database settings");
            return false;
        }
    }

    public async Task<bool> CheckPlayerPermissionAsync(string steamId)
    {
        if (!_config.Enabled)
        {
            SafeLog("[SmartRestart] Database is disabled in config");
            return false;
        }

        if (TryGetCachedPermission(steamId, out var cachedPermission))
        {
            return cachedPermission;
        }

        try
        {
            SafeLogVerbose($"[SmartRestart] Checking permissions for SteamID: {steamId}");

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);
            SafeLogVerbose("[SmartRestart] Database connection opened");

            // Step 1: Find all groups in sa_admins_groups that have the required permission
            SafeLogVerbose($"[SmartRestart] Step 1: Checking sa_admins_groups for groups with permission '{_config.RequiredPermission}'");

            var groupsQuery = @"
                SELECT id, name, flags, server
                FROM sa_admins_groups
                WHERE flags LIKE @permission 
                   OR flags LIKE '%@css/root%' 
                   OR flags LIKE '%#css/root%'";

            await using var groupsCommand = new MySqlCommand(groupsQuery, connection);
            groupsCommand.Parameters.AddWithValue("@permission", $"%{_config.RequiredPermission}%");

            var authorizedGroups = new List<(string id, string name, int? serverId)>();

            await using var groupsReader = await groupsCommand.ExecuteReaderAsync().ConfigureAwait(false);

            while (await groupsReader.ReadAsync())
            {
                try
                {
                    // Read all fields by name
                    var idValue = groupsReader["id"];
                    var nameValue = groupsReader["name"];
                    var flagsValue = groupsReader["flags"];
                    var serverValue = groupsReader["server"];

                    var groupId = idValue?.ToString() ?? "";
                    var groupName = nameValue?.ToString() ?? "";
                    var groupFlags = flagsValue?.ToString() ?? "";
                    string? groupServerStr = serverValue != null && serverValue != DBNull.Value ? serverValue.ToString() : null;

                    // Skip if id or flags are empty
                    if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(groupFlags))
                    {
                        SafeLogVerbose($"[SmartRestart] Skipping group with empty id or flags: id={groupId}");
                        continue;
                    }

                    // Try to parse server string to int
                    int? groupServerId = null;
                    if (!string.IsNullOrEmpty(groupServerStr))
                    {
                        // Handle "False"/"0" or comma-separated IDs
                        if (groupServerStr.Equals("False", StringComparison.OrdinalIgnoreCase))
                        {
                            groupServerId = 0; // False means all servers
                        }
                        else if (int.TryParse(groupServerStr.Split(',')[0].Trim(), out int parsedServerId))
                        {
                            groupServerId = parsedServerId;
                        }
                    }

                    SafeLogVerbose($"[SmartRestart] Found authorized group: id={groupId}, name={groupName}, server={groupServerId}");
                    authorizedGroups.Add((groupId, groupName, groupServerId));
                }
                catch (Exception ex)
                {
                    SafeLog($"[SmartRestart] Error reading group row: {ex.Message}");
                }
            }

            await groupsReader.CloseAsync();

            if (authorizedGroups.Count == 0)
            {
                SafeLogVerbose($"[SmartRestart] No groups found with permission '{_config.RequiredPermission}'");
                CachePermission(steamId, false);
                return false;
            }

            SafeLogVerbose($"[SmartRestart] Found {authorizedGroups.Count} authorized group(s)");

            // Step 2: Check if player is in sa_admins and belongs to any of these groups
            SafeLogVerbose($"[SmartRestart] Step 2: Checking sa_admins for player {steamId}");

            var adminQuery = @"
                SELECT flags, server_id, servers_groups, ends
                FROM sa_admins
                WHERE player_steamid = @steamId
                AND (ends IS NULL OR ends > NOW())
                LIMIT 1";

            await using var adminCommand = new MySqlCommand(adminQuery, connection);
            adminCommand.Parameters.AddWithValue("@steamId", steamId);

            await using var adminReader = await adminCommand.ExecuteReaderAsync().ConfigureAwait(false);

            if (!await adminReader.ReadAsync())
            {
                SafeLogVerbose($"[SmartRestart] Player {steamId} not found in sa_admins table");
                CachePermission(steamId, false);
                return false;
            }

            var flags = adminReader.GetString("flags");
            var serverId = adminReader.IsDBNull(adminReader.GetOrdinal("server_id")) ? (int?)null : adminReader.GetInt32("server_id");
            var serversGroups = adminReader.IsDBNull(adminReader.GetOrdinal("servers_groups")) ? null : adminReader.GetString("servers_groups");

            await adminReader.CloseAsync();

            SafeLogVerbose($"[SmartRestart] Found admin: flags={flags}, server_id={serverId}, servers_groups={serversGroups}");

            // If servers_groups is empty/null, it means admin has access to all servers
            bool hasAccessToAllServers = string.IsNullOrEmpty(serversGroups);
            if (hasAccessToAllServers)
            {
                SafeLogVerbose("[SmartRestart] Admin has empty servers_groups (access to ALL servers)");
            }

            // Step 3: Check if admin's flags match any authorized group ID
            foreach (var (groupId, groupName, groupServerId) in authorizedGroups)
            {
                // Match admin's flags against the group's id (e.g., flags="#owner" matches id="#owner")
                if (flags.Equals(groupId, StringComparison.OrdinalIgnoreCase))
                {
                    SafeLogVerbose($"[SmartRestart] Admin's flags '{flags}' matches authorized group '{groupId}' (name: {groupName})");

                    // If admin has access to all servers, grant immediately
                    if (hasAccessToAllServers)
                    {
                        SafeLogVerbose($"[SmartRestart] Permission GRANTED via group '{groupId}' (all servers access)");
                        CachePermission(steamId, true);
                        return true;
                    }

                    // Otherwise check server access
                    if (await CheckServerAccessAsync(groupServerId ?? serverId, connection))
                    {
                        SafeLogVerbose($"[SmartRestart] Permission GRANTED via group '{groupId}'");
                        CachePermission(steamId, true);
                        return true;
                    }
                    else
                    {
                        SafeLogVerbose($"[SmartRestart] Group '{groupId}' matches but server access DENIED");
                    }
                }
            }

            // Step 4: Check if admin's servers_groups (ID) matches any authorized group ID
            if (!string.IsNullOrEmpty(serversGroups))
            {
                SafeLogVerbose($"[SmartRestart] Checking admin's servers_groups: {serversGroups}");

                // servers_groups can contain group IDs (could be numeric or string like "#owner")
                var groupIdList = serversGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                               .Select(x => x.Trim())
                                               .ToList();

                foreach (var adminGroupId in groupIdList)
                {
                    var matchingGroup = authorizedGroups.FirstOrDefault(g => g.id.Equals(adminGroupId, StringComparison.OrdinalIgnoreCase));
                    if (matchingGroup != default)
                    {
                        SafeLogVerbose($"[SmartRestart] Admin's servers_groups contains authorized group ID {adminGroupId} ('{matchingGroup.name}')");

                        // If admin has access to all servers, grant immediately
                        if (hasAccessToAllServers)
                        {
                            SafeLogVerbose($"[SmartRestart] Permission GRANTED via servers_groups ID {adminGroupId} (all servers access)");
                            CachePermission(steamId, true);
                            return true;
                        }

                        // Check server access
                        if (await CheckServerAccessAsync(matchingGroup.serverId ?? serverId, connection))
                        {
                            SafeLogVerbose($"[SmartRestart] Permission GRANTED via servers_groups ID {adminGroupId}");
                            CachePermission(steamId, true);
                            return true;
                        }
                        else
                        {
                            SafeLogVerbose($"[SmartRestart] Group ID {adminGroupId} matches but server access DENIED");
                        }
                    }
                }
            }

            // Step 5: Check if admin has direct permission flags
            SafeLogVerbose("[SmartRestart] Checking if admin has direct permission flags");
            bool hasDirectPermission = flags.Contains(_config.RequiredPermission) || 
                                       flags.Contains("@css/root") || 
                                       flags.Contains("#css/root");

            if (hasDirectPermission)
            {
                SafeLogVerbose("[SmartRestart] Admin has direct permission flag");

                // If admin has access to all servers, grant immediately
                if (hasAccessToAllServers)
                {
                    SafeLogVerbose("[SmartRestart] Direct permission GRANTED (all servers access)");
                    CachePermission(steamId, true);
                    return true;
                }

                // Otherwise check server access
                if (await CheckServerAccessAsync(serverId, connection))
                {
                    SafeLogVerbose("[SmartRestart] Direct permission GRANTED");
                    CachePermission(steamId, true);
                    return true;
                }
                else
                {
                    SafeLogVerbose("[SmartRestart] Direct permission found but server access DENIED");
                }
            }

            SafeLogVerbose("[SmartRestart] Permission check FAILED - no valid permission found");
            CachePermission(steamId, false);
            return false;
        }
        catch (Exception ex)
        {
            SafeLog($"[SmartRestart] Database error: {ex.GetType().Name}: {ex.Message}");
            SafeLog($"[SmartRestart] Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    private async Task<bool> CheckServerAccessAsync(int? serverId, MySqlConnection connection)
    {
        // If we couldn't detect our server ID, deny access for safety
        if (!_serverId.HasValue)
        {
            SafeLogVerbose("[SmartRestart] Cannot verify server access: Server ID not detected");
            return false;
        }

        SafeLogVerbose($"[SmartRestart] Checking server access: serverId={serverId}, currentServerId={_serverId}");

        // If serverId is null or 0, it means "all servers"
        if (!serverId.HasValue || serverId.Value == 0)
        {
            SafeLogVerbose("[SmartRestart] Server ID is null/0 (all servers) - GRANTED");
            return true;
        }

        // Check if it matches our server
        if (serverId.Value == _serverId.Value)
        {
            SafeLogVerbose("[SmartRestart] Server ID matches - GRANTED");
            return true;
        }

        SafeLogVerbose("[SmartRestart] Server ID does not match - DENIED");
        return false;
    }

    private bool TryGetCachedPermission(string steamId, out bool hasPermission)
    {
        hasPermission = false;
        if (_config.PermissionCacheSeconds <= 0)
            return false;

        lock (_permissionCacheLock)
        {
            if (_permissionCache.TryGetValue(steamId, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
            {
                hasPermission = cached.HasPermission;
                return true;
            }

            _permissionCache.Remove(steamId);
            return false;
        }
    }

    private void CachePermission(string steamId, bool hasPermission)
    {
        if (_config.PermissionCacheSeconds <= 0)
            return;

        lock (_permissionCacheLock)
        {
            _permissionCache[steamId] = (hasPermission, DateTime.UtcNow.AddSeconds(_config.PermissionCacheSeconds));
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            SafeLog("[SmartRestart] Database connection successful!");
            return true;
        }
        catch (Exception ex)
        {
            SafeLog($"[SmartRestart] Database connection failed: {ex.Message}");
            return false;
        }
    }
}
