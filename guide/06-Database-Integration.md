# 🗄️ Database Integration Guide

<div align="center">

**SimpleAdmin permission system integration**

🔒 **Secure command access • No plugin dependencies • Read-only**

</div>

---

## 🎯 What is Database Integration?

Database integration allows you to **control who can use restart commands** using your existing **SimpleAdmin** database. No additional plugins required!

### Benefits

<table>
<tr>
<td width="33%" align="center">

### 🔒 **Security**
Only authorized admins  
can restart the server

</td>
<td width="33%" align="center">

### 🎮 **Flexibility**
Works with existing  
SimpleAdmin setup

</td>
<td width="33%" align="center">

### 🛡️ **Safety**
Read-only access  
No database modifications

</td>
</tr>
</table>

---

## ⚙️ Configuration

### Basic Setup

Edit `config.json`:

```json
{
  "Database": {
    "Enabled": true,
    "Host": "localhost",
    "Port": 3306,
    "Database": "cs2_server",
    "Username": "your_user",
    "Password": "your_password",
    "RequiredPermission": "@css/smartrestart",
    "PermissionCacheSeconds": 60
  }
}
```

### Settings Explained

| Setting | Description | Example |
|---------|-------------|---------|
| **Enabled** | Enable database permissions | `true` |
| **Host** | Database server address | `localhost` or `192.168.1.100` |
| **Port** | MySQL port | `3306` (default) |
| **Database** | SimpleAdmin database name | `cs2_server` |
| **Username** | Database username | `cs2_user` |
| **Password** | Database password | `your_secure_password` |
| **RequiredPermission** | Permission flag required | `@css/smartrestart` |
| **PermissionCacheSeconds** | Cache command permission results to reduce repeated queries | `60` |

---

## 📋 Configuration Examples

### Local Database Setup
For servers running MySQL on the same machine:

```json
{
  "Database": {
    "Enabled": true,
    "Host": "localhost",
    "Port": 3306,
    "Database": "cs2_simpleadmin",
    "Username": "cs2_user",
    "Password": "MySecurePassword123",
    "RequiredPermission": "@css/smartrestart",
    "PermissionCacheSeconds": 60
  }
}
```

✅ **Use when:** Database is on the same server  
⚡ **Performance:** Fastest connection

---

### Remote Database Setup
For servers connecting to external database:

```json
{
  "Database": {
    "Enabled": true,
    "Host": "db.yourhost.com",
    "Port": 3306,
    "Database": "cs2_simpleadmin",
    "Username": "remote_user",
    "Password": "RemotePassword456",
    "RequiredPermission": "@css/smartrestart",
    "PermissionCacheSeconds": 60
  }
}
```

✅ **Use when:** Centralized database for multiple servers  
🌐 **Network:** Ensure firewall allows connection

---

### Multiple Permission Flags
Allow multiple admin groups to use commands:

```json
{
  "RequiredPermission": "@css/smartrestart"
}
```

**Result:** Admins with `@css/smartrestart` can use commands. Root admins are also allowed by the permission checker.

#### Common Permission Combinations

```json
"@css/smartrestart"                    // SmartRestart admins
"@css/admin"                           // Admin flag
"@css/root"                            // Root admins
```

---

## 🔧 How It Works

### Command Authorization Flow

<table>
<tr>
<td width="20%" align="center">

### 1️⃣
**Player Uses Command**  
`!serverrestart`

</td>
<td width="20%" align="center">

### 2️⃣
**Database Lookup**  
Check `sa_admins` table

</td>
<td width="20%" align="center">

### 3️⃣
**Permission Check**  
Verify required flags

</td>
<td width="20%" align="center">

### 4️⃣
**Allow or Deny**  
Grant or refuse access

</td>
<td width="20%" align="center">

### 5️⃣
**Execute**  
Restart server if allowed

</td>
</tr>
</table>

### Database Tables Used

SmartRestart reads from SimpleAdmin's standard tables:
- ✅ `sa_admins` - Admin list with Steam IDs and flags
- ✅ Read-only access - No modifications
- ✅ Works with existing SimpleAdmin installation

---

## 🔒 Security Notes

<table>
<tr>
<td width="50%">

### ✅ **Read-Only Access**
- Plugin only **reads** from database
- Never writes or modifies data
- Safe to use with production databases
- Minimal permissions required

</td>
<td width="50%">

### 🛡️ **Recommended Practices**
- Use strong database passwords
- Limit user permissions (SELECT only)
- Use dedicated database user
- Regular security audits

</td>
</tr>
</table>

---

## ⚠️ Troubleshooting

<table>
<tr>
<td width="40%">

### **Database connection failed**

</td>
<td width="60%">

**Check:**
1. Host, Port, Database name are correct
2. Username and password are valid
3. MySQL server is running
4. Firewall allows connection (if remote)
5. Database user has SELECT permissions

**Console Error:**
```
[SmartRestart] Database connection failed: ...
```

**Fix:**
- Verify credentials
- Test connection with MySQL client
- Check MySQL server logs

</td>
</tr>
<tr>
<td width="40%">

### **Permission denied for player**

</td>
<td width="60%">

**Requirements:**
1. Player must be in `sa_admins` table
2. Player's Steam64 ID must match
3. Player must have the flag configured in `RequiredPermission`

**Check SimpleAdmin Database:**
```sql
SELECT * FROM sa_admins WHERE player_steamid = 'STEAM_1:0:123456';
```

**Common Issues:**
- Wrong Steam ID format
- Missing or typo in flags
- `RequiredPermission` doesn't match player's flags

</td>
</tr>
<tr>
<td width="40%">

### **Want to disable database?**

</td>
<td width="60%">

Set `Enabled` to `false`:
```json
{
  "Database": {
    "Enabled": false
  }
}
```

⚠️ **Warning:** Commands will be **unrestricted** - anyone can use them!

</td>
</tr>
</table>

---

## 🧪 Testing Your Setup

### Verification Steps

1. **Configure** database settings in `config.json`
2. **Reload** plugin: `css_plugins reload SmartRestart`
3. **Check** console for connection success:
   ```
   Database integration         | ✔      | Enabled (...)
   ```
4. **Test** with authorized admin: `!serverrestart`
5. **Test** with unauthorized player: Should see permission error

### Expected Behaviors

| Scenario | Result |
|----------|--------|
| Authorized admin uses `!serverrestart` | ✅ Restart initiated |
| Unauthorized player uses `!serverrestart` | ❌ "No permission" message |
| Database connection fails | ⚠️ Error in console, commands disabled |
| Database disabled | ⚠️ Commands work for everyone |

---

## 💡 Best Practices

<table>
<tr>
<td width="33%">

### 🔐 **Security**
- Use strong passwords
- Dedicated database user
- Minimal permissions (SELECT only)
- Regular credential rotation

</td>
<td width="33%">

### 🎯 **Permission Management**
- Use specific flags (`@css/smartrestart`)
- Avoid over-permissioning
- Document admin list
- Regular permission audits

</td>
<td width="33%">

### 🔧 **Maintenance**
- Test after updates
- Monitor connection logs
- Backup database regularly
- Document configuration

</td>
</tr>
</table>

---

## 📊 Database Setup Guide

### For SimpleAdmin Users

If you already have SimpleAdmin installed, SmartRestart will use the same database automatically!

**Steps:**
1. Use same database settings as SimpleAdmin
2. Set `RequiredPermission` to your admin flag
3. Reload plugin
4. Done! ✅

### Database User Permissions

Minimum required MySQL permissions:

```sql
GRANT SELECT ON cs2_server.sa_admins TO 'cs2_user'@'localhost';
GRANT SELECT ON cs2_server.sa_servers TO 'cs2_user'@'localhost';
FLUSH PRIVILEGES;
```

💡 **Note:** Only `SELECT` permission is needed - no INSERT, UPDATE, or DELETE required.

---

## 🔗 Related Guides

- **[Configuration Guide](02-Configuration.md)** - All database settings
- **[Commands](07-Commands.md)** - Commands requiring permissions
- **[Troubleshooting](08-Troubleshooting.md)** - Common issues

