# 🎮 Commands Reference

<div align="center">

**Control your server restarts with simple commands**

</div>

---

## 📋 Command Overview

<table>
<tr>
<th width="25%">Command</th>
<th width="25%">Permission</th>
<th width="50%">Description</th>
</tr>
<tr>
<td><code>!serverrestart</code></td>
<td>🔒 Admin Only</td>
<td>Manually restart server with 5-minute warnings</td>
</tr>
<tr>
<td><code>!cancelrestart</code></td>
<td>🔒 Admin Only</td>
<td>Cancel a pending restart</td>
</tr>
<tr>
<td><code>!nextrestart</code></td>
<td>✅ Everyone</td>
<td>Check when next scheduled restart is</td>
</tr>
</table>

---

## 🔧 Command Details

### 🔄 !serverrestart

Initiates a **manual server restart** with full warning system.

#### Usage
```
!serverrestart
```

#### What Happens
1. **5-minute warning** starts immediately
2. **Chat warnings** sent at configured intervals
3. **Center countdown** displays last 30 seconds
4. **Discord notification** sent (if enabled)
5. **Server restarts** after countdown

#### Example Response
```
[SmartRestart] Manual server restart initiated.
[SmartRestart] Server will restart in 5 minutes.
```

#### Requirements
- 🔒 **Permission:** `@css/smartrestart` (default)
- 🗄️ **Database:** Must be enabled
- 👤 **Admin:** Player in SimpleAdmin `sa_admins` table

---

### 🚫 !cancelrestart

Cancels any **pending restart** (scheduled or manual).

#### Usage
```
!cancelrestart
```

#### What Happens
1. **Pending restart** is cancelled
2. **All warnings** stop
3. **Center countdown** clears
4. **Discord notification** sent (if enabled)
5. **Players notified** in chat

#### Example Response
```
[SmartRestart] Pending restart has been cancelled.
```

#### Requirements
- 🔒 **Permission:** Same as `!serverrestart`
- ⚠️ **Only works** if restart is pending

---

### ⏰ !nextrestart

Shows when the **next scheduled restart** will occur.

#### Usage
```
!nextrestart
```

#### Example Responses

**With scheduled restart:**
```
[SmartRestart] Next scheduled restart: 06:00 (Morning maintenance)
[SmartRestart] Time until restart: 2 hours 34 minutes
```

**No scheduled restart:**
```
[SmartRestart] No scheduled restarts configured.
```

**Restart pending:**
```
[SmartRestart] Server restart in progress: 3 minutes 42 seconds remaining
```

#### Requirements
- ✅ **No permission required** - any player can use
- 📅 Works with scheduled restarts only

---

## 💻 Console Commands

All commands work in **server console** without the `!` prefix:

<table>
<tr>
<td width="50%">

### Chat Command
```
!serverrestart
!cancelrestart
!nextrestart
```

</td>
<td width="50%">

### Console Command
```
css_serverrestart
css_cancelrestart
css_nextrestart
```

</td>
</tr>
</table>

💡 **Note:** Console commands **bypass permission checks** - server console always has full access.

---

## 🔒 Permission Setup

### Enable Database Permissions

Edit `config.json`:

```json
{
  "Database": {
    "Enabled": true,
    "Host": "localhost",
    "Port": 3306,
    "Database": "cs2_server",
    "User": "your_user",
    "Password": "your_password",
    "RequiredFlags": "@css/smartrestart"
  }
}
```

### Requirements for Players

To use `!serverrestart` and `!cancelrestart`, players must:

1. ✅ Be listed in SimpleAdmin `sa_admins` table
2. ✅ Have the required flags (e.g., `@css/smartrestart`)
3. ✅ Be connected with matching Steam64 ID

### Check Player Permissions

Query SimpleAdmin database:
```sql
SELECT player_steamid, player_name, flags 
FROM sa_admins 
WHERE player_steamid = 'STEAM_1:0:123456';
```

---

## ⚠️ Common Errors & Solutions

<table>
<tr>
<td width="40%">

### **"You don't have permission"**

</td>
<td width="60%">

**Causes:**
- Player not in SimpleAdmin database
- Missing or incorrect flags
- Database integration disabled

**Solutions:**
1. Add player to `sa_admins` table
2. Grant `@css/smartrestart` flag
3. Verify database connection working
4. Check `RequiredFlags` matches player's flags

</td>
</tr>
<tr>
<td width="40%">

### **"No restart is currently pending"**

</td>
<td width="60%">

**Causes:**
- No restart scheduled or in progress
- Restart already completed
- Using `!cancelrestart` when nothing to cancel

**Solutions:**
- Check with `!nextrestart`
- Only use `!cancelrestart` when restart is active
- Restart already cancelled or completed

</td>
</tr>
<tr>
<td width="40%">

### **Command not responding**

</td>
<td width="60%">

**Check:**
1. Correct spelling: `!serverrestart` (with `!`)
2. Plugin loaded: `css_plugins list`
3. Database connected (if permissions enabled)
4. No typos in command

**Console Check:**
```
css_plugins list
```
Should show SmartRestart as loaded.

</td>
</tr>
<tr>
<td width="40%">

### **"Database connection failed"**

</td>
<td width="60%">

**Causes:**
- Database credentials incorrect
- MySQL server not running
- Network/firewall issue

**Solutions:**
1. Verify config: Host, Port, Database, User, Password
2. Test connection with MySQL client
3. Check MySQL server status
4. See [Database Integration Guide](06-Database-Integration.md)

</td>
</tr>
</table>

---

## 🧪 Testing Commands

### Test Sequence

1. **Check plugin loaded:**
   ```
   css_plugins list
   ```

2. **Test public command:**
   ```
   !nextrestart
   ```
   Should work for everyone.

3. **Test admin command (if authorized):**
   ```
   !serverrestart
   ```
   Should initiate 5-minute countdown.

4. **Test cancel:**
   ```
   !cancelrestart
   ```
   Should stop the restart.

---

## 💡 Usage Examples

### Scenario: Manual Maintenance

1. Admin announces maintenance in Discord/chat
2. Admin runs: `!serverrestart`
3. Players see 5-minute warning countdown
4. Server restarts automatically after countdown

### Scenario: Cancel Accidental Restart

1. Admin accidentally triggers: `!serverrestart`
2. Admin immediately runs: `!cancelrestart`
3. Restart cancelled, players notified
4. Normal gameplay continues

### Scenario: Player Asks About Restart

1. Player runs: `!nextrestart`
2. Response: "Next scheduled restart: 06:00 (Morning maintenance)"
3. Player knows when to expect downtime

---

## 🎯 Best Practices

<table>
<tr>
<td width="33%">

### 🗣️ **Communication**
- Announce manual restarts
- Use `!serverrestart` for planned maintenance
- Cancel if not needed
- Keep players informed

</td>
<td width="33%">

### 🔒 **Security**
- Limit admin permissions
- Use specific flags
- Audit admin list regularly
- Monitor command usage

</td>
<td width="33%">

### 📊 **Monitoring**
- Check console logs
- Watch Discord notifications
- Track restart frequency
- Document procedures

</td>
</tr>
</table>

---

## 🔗 Related Guides

- **[Configuration Guide](02-Configuration.md)** - Warning system settings
- **[Database Integration](06-Database-Integration.md)** - Permission setup
- **[Scheduled Restarts](03-Scheduled-Restarts.md)** - Automatic restart times
- **[Discord Integration](05-Discord-Integration.md)** - Command notifications

---

<div align="center">

**Commands not working?** Check the [Troubleshooting Guide](08-Troubleshooting.md)

</div>
