# 🛠️ Troubleshooting Guide

<div align="center">

**Quick solutions to common issues**

</div>

---

## 🚨 Plugin Issues

### Plugin Not Loading

<table>
<tr>
<td width="30%">

**Expected Console Output:**
```
====================================================
 SMARTRESTART STARTUP
====================================================
...
Next scheduled restart: ...
SmartRestart ready.
```

</td>
<td width="70%">

**If missing, check:**

1. ✅ CounterStrikeSharp is installed correctly
2. ✅ .NET 10 runtime is installed on server
3. ✅ Files are in correct folder: `addons/counterstrikesharp/plugins/SmartRestart/`
4. ✅ All required DLLs present (`SmartRestart.dll`, `MySqlConnector.dll`)

**Verify plugin status:**
```
css_plugins list
```

Should show `SmartRestart` in the list.

</td>
</tr>
</table>

---

### Config File Not Generating

<table>
<tr>
<td width="30%">

**Problem:**
Config file not created on first run

</td>
<td width="70%">

**Solutions:**

1. Check folder permissions (write access required)
2. Config creates in **plugin folder**, not CSS config folder:
   ```
   plugins/SmartRestart/config.json
   ```
3. Wait for complete server startup
4. Check console for errors
5. Manually create config from example if needed

</td>
</tr>
</table>

---

## ⏰ Scheduled Restart Issues

### Restarts Not Triggering

<table>
<tr>
<td width="30%">

**Check Configuration:**

</td>
<td width="70%">

```json
{
  "ScheduledRestarts": [
    {
      "Enabled": true,
      "Hour": 6,
      "Minute": 0,
      "Description": "Morning restart"
    }
  ]
}
```

**Verify:**
- ✅ `"Enabled": true` (not `false`)
- ✅ Hour is 0-23 (24-hour format)
- ✅ Minute is 0-59
- ✅ Server timezone matches expected time
- ✅ Startup table shows: `Next scheduled restart: ...`

</td>
</tr>
</table>

---

### Wrong Timing / Timezone Issues

<table>
<tr>
<td width="30%">

**Problem:**
Restart happens at wrong time

</td>
<td width="70%">

**Cause:** Server uses **server timezone**, not your local timezone.

**Check server time:**
```bash
date
```

**Solution:**
1. Calculate time difference between server and your timezone
2. Adjust restart `Hour` accordingly
3. Example: Server is UTC, you want 6 AM EST (UTC-5)
   - Set `"Hour": 11` (6 AM + 5 hours)

</td>
</tr>
</table>

---

## ⚠️ Warning System Issues

### Chat Warnings Not Showing

<table>
<tr>
<td width="30%">

**Enable Warnings:**

</td>
<td width="70%">

```json
{
  "WarningMessages": {
    "Enabled": true,
    "WarningTimes": [300, 180, 60, 30]
  }
}
```

**Checklist:**
- ✅ `"Enabled": true`
- ✅ `WarningTimes` has values (in seconds)
- ✅ Language file exists (`lang/en.json` or `lang/he.json`)
- ✅ Plugin reloaded after config changes

**Note:** If server is empty, warnings are skipped (restarts immediately).

</td>
</tr>
</table>

---

### Center Countdown Not Visible

<table>
<tr>
<td width="30%">

**Enable Center Alert:**

</td>
<td width="70%">

```json
{
  "WarningMessages": {
    "ShowCenterAlert": true
  }
}
```

**Important:**
- ⏱️ Only shows **last 30 seconds** before restart
- 👁️ Must have at least 30 seconds in `WarningTimes`
- 🔄 Updates every second
- 📺 Overlays other center messages

**Test:** Set restart 1 minute in future, wait 30 seconds - should appear.

</td>
</tr>
</table>

---

## 🗄️ Database Issues

### Database Connection Failed

<table>
<tr>
<td width="30%">

**Error Message:**
```
[SmartRestart] Database connection failed
```

</td>
<td width="70%">

**Check Configuration:**
```json
{
  "Database": {
    "Enabled": true,
    "Host": "localhost",
    "Port": 3306,
    "Database": "cs2_server",
    "Username": "your_user",
    "Password": "your_password"
  }
}
```

**Verification Steps:**

1. **Test connection manually:**
   ```bash
   mysql -h localhost -u your_user -p -D cs2_server
   ```

2. **Check MySQL is running:**
   ```bash
   systemctl status mysql
   ```

3. **Verify credentials** match SimpleAdmin setup

4. **Check firewall** (if remote database)

5. **Ensure database exists:**
   ```sql
   SHOW DATABASES;
   ```

</td>
</tr>
</table>

---

### Permission Denied Errors

<table>
<tr>
<td width="30%">

**Error:**
"You don't have permission"

</td>
<td width="70%">

**Requirements for player:**
1. ✅ Listed in SimpleAdmin `sa_admins` table
2. ✅ Has required flags (e.g., `@css/smartrestart`)
3. ✅ Steam64 ID matches database entry

**Check database:**
```sql
SELECT player_steamid, player_name, flags 
FROM sa_admins 
WHERE player_steamid = 'STEAM_1:0:123456';
```

**Common issues:**
- Wrong Steam ID format
- Typo in flags column
- `RequiredPermission` config doesn't match player's flags
- Database integration disabled

**Temporary bypass (testing only):**
```json
{
  "Database": {
    "Enabled": false
  }
}
```
⚠️ **Warning:** Anyone can use commands when disabled!

</td>
</tr>
</table>

---

## 🎮 Command Issues

### Commands Not Responding

<table>
<tr>
<td width="30%">

**Correct Syntax:**

</td>
<td width="70%">

**In chat:**
```
!serverrestart
!cancelrestart
!nextrestart
```

**In console:**
```
css_serverrestart
css_cancelrestart
css_nextrestart
```

**Common mistakes:**
- ❌ Missing `!` in chat: `serverrestart`
- ❌ Using `!` in console: `!css_serverrestart`
- ❌ Typos: `!serverrstart`, `!restartserver`

**Verify plugin loaded:**
```
css_plugins list
```

</td>
</tr>
</table>

---

## 🔔 Discord Integration Issues

### Notifications Not Sending

<table>
<tr>
<td width="30%">

**Check Configuration:**

</td>
<td width="70%">

```json
{
  "DiscordWebhook": {
    "Enabled": true,
    "WebhookUrl": "https://discord.com/api/webhooks/1234/AbCdEf"
  }
}
```

**Verification:**
1. ✅ `"Enabled": true`
2. ✅ Webhook URL is correct and complete
3. ✅ URL starts with `https://discord.com/api/webhooks/`
4. ✅ Webhook still exists in Discord (not deleted)
5. ✅ No extra spaces before/after URL

**Test webhook:** Use [Discord Webhook Tester](https://discohook.org/) with your URL.

**Console output:**
Look for Discord-related messages or errors in console.

</td>
</tr>
</table>

---

### Webhook Invalid Error

<table>
<tr>
<td width="30%">

**Problem:**
"Invalid webhook URL" error

</td>
<td width="70%">

**Solutions:**

1. **Copy full URL** from Discord (don't modify it)
2. **Format:** `https://discord.com/api/webhooks/[ID]/[TOKEN]`
3. **Check for:**
   - Extra characters
   - Spaces before/after
   - Missing parts of URL
4. **Recreate webhook** if URL is corrupted
5. **Test in browser** - should show JSON response

</td>
</tr>
</table>

---

## 🔄 Restart Execution Issues

### Server Not Actually Restarting

<table>
<tr>
<td width="30%">

**Check Restart Command:**

</td>
<td width="70%">

```json
{
  "RestartCommand": "quit"
}
```

**Try alternatives:**
- `"quit"` - CS2 default (recommended)
- `"_restart"` - Alternative
- `"exit"` - Some environments
- Panel-specific command (check your hosting provider)

**For Panel Users:**
- ✅ Pterodactyl/Pelican: Use `"quit"` with auto-restart enabled
- ✅ Docker: Ensure restart policy set to `always` or `unless-stopped`
- ✅ Linux systemd: Set service to auto-restart

**Console output:**
```
[SmartRestart] Executing restart command: quit
```

If you see this but server doesn't restart, **the restart method is the issue**, not the plugin.

</td>
</tr>
</table>

---

## 🤖 Empty Server Map Refresh Issues

### Empty Server Not Changing Map

<table>
<tr>
<td width="30%">

**Enable Feature:**

</td>
<td width="70%">

```json
{
  "EnableAutoRestart": true,
  "EmptyServerBehavior": {
    "Enabled": true,
    "DelaySeconds": 180,
    "ExecuteOnceUntilPlayerJoins": true,
    "SkipIfScheduledRestartWithinMinutes": 15
  },
  "MinimumUptimeMinutes": 5
}
```

**Requirements:**
- ✅ Server must be empty (0 players)
- ✅ EmptyServerBehavior enabled
- ✅ DelaySeconds elapsed
- ✅ Not too close to scheduled restart

**If map change runs once and stops while empty:**
- This is expected when `ExecuteOnceUntilPlayerJoins` is `true`.
- It prevents spam during long empty periods.

**For testing (faster):**
```json
{
  "EmptyServerBehavior": {
    "Enabled": true,
    "DelaySeconds": 10,
    "ExecuteOnceUntilPlayerJoins": true,
    "SkipIfScheduledRestartWithinMinutes": 1
  }
}
```

</td>
</tr>
</table>

---

## 🔍 Debug & Logging

### Enable Detailed Debug Logging

Debug logs are disabled by default for performance. Enable them only while troubleshooting:

```json
{
  "Logging": {
    "DebugEnabled": true
  }
}
```

Then reload the plugin and watch console/log output:

```
[SmartRestart] Checking for scheduled restarts...
[SmartRestart] Next scheduled restart: 06:00 (Morning restart)
[SmartRestart] Sending warning: Server restart in 5 minutes
[SmartRestart] Sending warning: Server restart in 1 minute
[SmartRestart] Sending center countdown: 30 seconds
[SmartRestart] Executing restart command: quit
```

### Common Console Messages

| Message | Meaning |
|---------|---------|
| `SMARTRESTART STARTUP` | ✅ Plugin working |
| `Failed to connect to database` | ❌ Database issue |
| `Next scheduled restart: ...` | ✅ Schedule loaded |
| `Sending warning: ...` | ✅ Warnings working |
| `Executing restart command` | ✅ Restart triggered |

---

## 📋 Quick Diagnostic Checklist

<table>
<tr>
<td width="50%">

### ✅ **Basic Checks**
- [ ] Plugin shows in `css_plugins list`
- [ ] Config file exists in plugin folder
- [ ] Language files present (`lang/en.json`, `lang/he.json`)
- [ ] All DLL files present
- [ ] Console shows successful load message

</td>
<td width="50%">

### ⚙️ **Feature Checks**
- [ ] Scheduled restarts have `"Enabled": true`
- [ ] Warning system enabled
- [ ] Database credentials correct (if using)
- [ ] Discord webhook URL valid (if using)
- [ ] Restart command appropriate for environment

</td>
</tr>
</table>

---

## 🆘 Still Having Issues?

<table>
<tr>
<td align="center" width="33%">

### 📖 **Review Guides**
Check specific guides for:
- [Configuration](02-Configuration.md)
- [Scheduled Restarts](03-Scheduled-Restarts.md)
- [Commands](07-Commands.md)

</td>
<td align="center" width="33%">

### 🔍 **Console Logs**
Enable debug mode:
- Set `"Logging": { "DebugEnabled": true }`
- Watch console output and log file
- Look for error messages
- Check timestamps
- Note warning patterns

</td>
<td align="center" width="33%">

### 💬 **Get Help**
- Check [Main README](../README.md)
- Report issues on GitHub
- Include console logs
- Describe exact symptoms

</td>
</tr>
</table>

---

<div align="center">

**Most issues are configuration-related** - double-check your `config.json` settings!

</div>
