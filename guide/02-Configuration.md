# ⚙️ Configuration Guide

<div align="center">

**Master all plugin settings**

📁 **Config File:** `plugins/SmartRestart/config.json`

</div>

---

## 🌍 Language Selection

Choose your preferred language for all player messages:

```json
{
  "Language": "en"
}
```

| Option | Language | Status |
|--------|----------|--------|
| `en` | 🇺🇸 English | ✅ Default |
| `he` | 🇮🇱 Hebrew | ✅ RTL Support |

💡 **Tip:** Missing language files are auto-generated with defaults.

---

## 💬 Chat Prefix Customization

Personalize the prefix for all plugin messages:

```json
{
  "ChatPrefix": "[{gold}SmartRestart{default}]"
}
```

### Available Colors
```
{red}, {green}, {blue}, {yellow}, {gold}, {orange}, 
{purple}, {white}, {grey}, {lightblue}, {darkred}
```

### Examples
```json
"ChatPrefix": "[{blue}Restart{default}]"
"ChatPrefix": "{gold}★{default} [SmartRestart]"
"ChatPrefix": "[{purple}SR{default}]"
```

---

## ⚠️ Warning System

Configure how and when players receive restart warnings:

```json
{
  "WarningMessages": {
	"Enabled": true,
	"WarningTimes": [300, 180, 60, 30],
	"ShowCenterAlert": true
  }
}
```

| Setting | Description | Default |
|---------|-------------|---------|
| **Enabled** | Enable chat warnings | `true` |
| **WarningTimes** | Times (in seconds) to warn | `[300, 180, 60, 30]` |
| **ShowCenterAlert** | Show countdown overlay (last 30s) | `true` |

### Warning Time Examples
```json
"WarningTimes": [600, 300, 60, 10]     // 10min, 5min, 1min, 10sec
"WarningTimes": [300, 60, 30]          // 5min, 1min, 30sec (minimal)
"WarningTimes": [900, 600, 300, 60]    // 15min, 10min, 5min, 1min (extended)
```

💡 **Center Alert:** Displays a countdown timer overlay for the last 30 seconds.

---

## 🔄 Empty Server Behavior (No Restart)

When server is empty, SmartRestart can refresh the current map. It does **not** restart the server.

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

| Setting | Description | Recommended |
|---------|-------------|-------------|
| **EnableAutoRestart** | Enable empty-server behavior | `true` |
| **EmptyServerBehavior.Enabled** | Enable map refresh when empty | `true` |
| **EmptyServerBehavior.DelaySeconds** | Delay before map refresh | `180` |
| **EmptyServerBehavior.ExecuteOnceUntilPlayerJoins** | Prevent spam while server stays empty | `true` |
| **EmptyServerBehavior.SkipIfScheduledRestartWithinMinutes** | Skip map refresh when scheduled restart is close | `15` |
| **MinimumUptimeMinutes** | Minimum uptime before allowing restart | `5` |

⚠️ **Safety:** Empty map refresh runs once and waits for a player join before running again.

---

## 🧠 Smart Empty Behavior (Simple)

The plugin now keeps empty-server logic simple:

- No empty-server restarts
- One map refresh while server is empty
- No repeated map-change spam
- Scheduled restarts continue exactly as configured

---

## 🔔 Discord Integration

Send beautiful restart notifications to Discord:

```json
{
  "DiscordWebhook": {
	"Enabled": true,
	"WebhookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL",
	"EmbedStyle": "Clean",
	"FooterImageUrl": "https://your-server.com/logo.png"
  }
}
```

| Setting | Options | Description |
|---------|---------|-------------|
| **Enabled** | `true`/`false` | Enable Discord notifications |
| **WebhookUrl** | Discord URL | Your webhook URL |
| **EmbedStyle** | `Clean`, `Detailed` | Notification style |
| **FooterImageUrl** | Image URL | Small footer icon (optional) |

### Embed Styles

<table>
<tr>
<td width="50%">

**Clean** (Recommended)
- Minimal information
- Clean design
- Server name + uptime

</td>
<td width="50%">

**Detailed**
- Full server info
- Extended uptime stats
- Restart reasons

</td>
</tr>
</table>

📚 **Full Guide:** [Discord Integration](05-Discord-Integration.md)

---

## 🗄️ Database Integration (SimpleAdmin)

Enable permission-based commands using SimpleAdmin database:

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

| Setting | Description | Default |
|---------|-------------|---------|
| **Enabled** | Enable database permissions | `false` |
| **Host** | Database server address | `localhost` |
| **Port** | MySQL port | `3306` |
| **Database** | SimpleAdmin database name | - |
| **Username** | Database username | - |
| **RequiredPermission** | Permission flag for commands | `@css/smartrestart` |
| **PermissionCacheSeconds** | Cache permission results to reduce MySQL load | `60` |

🔒 **Security:** Plugin uses **read-only** access - no writes to database.

📚 **Full Guide:** [Database Integration](06-Database-Integration.md)

---

## 📋 Complete Configuration Example

Production-ready configuration with all features enabled:

```json
{
  "Language": "en",
  "ChatPrefix": "[{gold}SmartRestart{default}]",

	"EnableAutoRestart": true,
  "EmptyServerBehavior": {
	"Enabled": true,
	"DelaySeconds": 180,
	"ExecuteOnceUntilPlayerJoins": true,
	"SkipIfScheduledRestartWithinMinutes": 15
	},
  "MinimumUptimeMinutes": 5,
  "RestartCommand": "quit",

  "WarningMessages": {
	"Enabled": true,
	"WarningTimes": [300, 180, 60, 30],
	"ShowCenterAlert": true
  },

  "ScheduledRestarts": [
	{
	  "Enabled": true,
	  "Hour": 6,
	  "Minute": 0,
	  "Description": "Morning restart"
	},
	{
	  "Enabled": true,
	  "Hour": 18,
	  "Minute": 0,
	  "Description": "Evening restart"
	}
  ],

  "DiscordWebhook": {
	"Enabled": true,
	"WebhookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL",
	"EmbedStyle": "Clean",
	"FooterImageUrl": "https://your-server.com/logo.png"
  },

  "Database": {
	"Enabled": true,
	"Host": "localhost",
	"Port": 3306,
	"Database": "cs2_server",
	"Username": "cs2_user",
	"Password": "your_secure_password",
	"RequiredPermission": "@css/smartrestart",
	"PermissionCacheSeconds": 60
  }
}
```

---

## Logging & Performance

Keep debug logging disabled during normal operation. Enable it only while troubleshooting.

```json
{
  "Logging": {
	"DebugEnabled": false
  }
}
```

| Setting | Description | Default |
|---------|-------------|---------|
| **DebugEnabled** | Writes verbose scheduler/countdown/permission debug logs | `false` |

Performance tip: `Database.PermissionCacheSeconds` defaults to `60`, which reduces repeated MySQL permission checks for restart commands.

---

## 🎯 Quick Configuration Tips

<table>
<tr>
<td width="33%">

### 🚀 **Getting Started**
Start with defaults, enable only:
- Language
- Chat prefix
- Basic warnings
- One scheduled restart

</td>
<td width="33%">

### 🔧 **Production Setup**
Add when stable:
- Smart empty restart
- Discord notifications
- Database permissions
- Multiple schedules

</td>
<td width="33%">

### 🎨 **Customization**
Personalize:
- Warning times
- Chat colors
- Embed styles
- Peak hours

</td>
</tr>
</table>

---

## 🔗 Related Guides

- **[Scheduled Restarts](03-Scheduled-Restarts.md)** - Set up automatic daily restarts
- **[Language Customization](04-Language-Customization.md)** - Edit messages and colors
- **[Discord Integration](05-Discord-Integration.md)** - Full webhook setup
- **[Database Integration](06-Database-Integration.md)** - SimpleAdmin permissions

---

<div align="center">

**Configuration done?** → Test with [Commands Guide](07-Commands.md)

</div>
