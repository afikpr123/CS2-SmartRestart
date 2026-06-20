# Server Startup Issues

Problems with SmartRestart loading or showing the wrong startup information? Start here.

---

## Expected Startup Output

On load, SmartRestart prints one startup table:

```text
====================================================
 SMARTRESTART STARTUP
====================================================
 Time: 2026-06-20 18:07:13
----------------------------------------------------
| Checklist Item               | Status | Details
----------------------------------------------------
| Logger ready                 | ✔      | ...
| Config loaded                | ✔      | ...
| Language loaded              | ✔      | en (...)
| Event handlers               | ✔      | Connect/Disconnect
| Scheduler timer              | ✔      | Every 60s
| Auto-empty map refresh       | ✔      | Delay: ...
| Scheduled restarts           | ✔      | Count: ...
| Discord webhook              | ✔      | Enabled/Disabled
| Database integration         | ✔      | Enabled/Disabled
| Debug logging                | ✖      | Disabled
----------------------------------------------------
Next scheduled restart: 2026-06-21 01:00 (392 min)
SmartRestart ready.
```

The main table is cyan. `✔` is green, `✖` is red, error text is yellow, and the final ready lines are green.

---

## Plugin Does Not Load

Check:
- CounterStrikeSharp is installed and updated.
- .NET 10 runtime is installed.
- Files are in `game/csgo/addons/counterstrikesharp/plugins/SmartRestart/`.
- `SmartRestart.dll` and dependencies are present.
- Server has permission to read the plugin folder and write `config.json`.

Verify with:

```text
css_plugins list
```

---

## Config Not Created

SmartRestart creates `config.json` in the plugin folder:

```text
addons/counterstrikesharp/plugins/SmartRestart/config.json
```

If it does not appear:
- Check folder write permissions.
- Restart the server fully.
- Check console for config or language file errors.
- Create the file manually from the example in `README.md` or `guide/02-Configuration.md`.

---

## Scheduled Restarts Not Showing

Use the current config format:

```json
{
  "ScheduledRestarts": [
    {
      "Enabled": true,
      "Hour": 1,
      "Minute": 0,
      "Description": "Night restart"
    }
  ]
}
```

Check:
- `Enabled` is `true`.
- `Hour` uses 24-hour format, `0` through `23`.
- `Minute` is `0` through `59`.
- Server timezone is the timezone you expect.
- Startup table shows `Scheduled restarts` with the correct count.

---

## Wrong Language

Set language in `config.json`:

```json
{
  "Language": "en"
}
```

Supported default language files:
- `lang/en.json`
- `lang/he.json`

The startup table shows the loaded language and language file path.

---

## Database Startup Issues

Use the current database field names:

```json
{
  "Database": {
    "Enabled": true,
    "Host": "localhost",
    "Port": 3306,
    "Database": "cs2_simpleadmin",
    "Username": "root",
    "Password": "",
    "RequiredPermission": "@css/smartrestart",
    "PermissionCacheSeconds": 60
  }
}
```

If the database fails:
- Verify host, port, database, username, and password.
- Confirm the database user has `SELECT` permission for `sa_admins`, `sa_admins_groups`, and `sa_servers`.
- Temporarily set `"Enabled": false` if you do not need SimpleAdmin permissions.

---

## Debug Logging

Debug logging is disabled by default for performance:

```json
{
  "Logging": {
    "DebugEnabled": false
  }
}
```

Enable it only while troubleshooting:

```json
{
  "Logging": {
    "DebugEnabled": true
  }
}
```

Reload or restart the plugin after changing this setting.

---

## Common Mistakes

- Using old fields like `User`, `RequiredFlags`, `SmartEmptyRestart`, or `DelayAfterLastPlayerLeaves`.
- Using 12-hour time instead of 24-hour time.
- Editing `config.json` but not reloading or restarting the plugin.
- Enabling debug logging permanently on a busy server.

Help: https://github.com/afikpr123/CS2-SmartRestart/issues
