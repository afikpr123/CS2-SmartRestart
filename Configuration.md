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

## 🔄 Empty Server Restart

Control automatic restart behavior when the server is empty:

```json
{
  "EnableAutoRestart": true,
  "DelayAfterLastPlayerLeaves": 60,
  "MinimumUptimeMinutes": 5
}
```

| Setting | Description | Recommended |
|---------|-------------|-------------|
| **EnableAutoRestart** | Enable empty-server restarts | `true` |
| **DelayAfterLastPlayerLeaves** | Seconds to wait after last player | `60` |
| **MinimumUptimeMinutes** | Minimum uptime before allowing restart | `5` |

⚠️ **Safety:** Server won't restart immediately - it waits for the delay period.

---

## 🧠 Smart Features (Optional)

Advanced intelligence for production servers:

```json
{
  "SmartEmptyRestart": {
	"Enabled": true,
	"MinimumUptimeForEmptyRestartHours": 4,
	"EmptyRestartCooldownMinutes": 30,
	"RequireMinimumSessions": true,
	"MinimumCompletedSessions": 2,
	"PeakHours": {
	  "Enabled": true,
	  "StartHour": 16,
	  "EndHour": 23,
	  "DelayMinutes": 10
	}
  }
}
```

<table>
<tr>
<td width="40%">

### 🎯 **Session Tracking**
- Requires real player activity
- Prevents restart spam
- Tracks completed sessions

</td>
<td width="30%">

### ⏰ **Peak Hours**
- Longer delays during peak times
- Smart timing adjustments
- Configurable hours

</td>
<td width="30%">

### 🛡️ **Cooldown System**
- Prevents multiple restarts
- Respects minimum uptime
- Anti-spam protection

</td>
</tr>
</table>

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
	"User": "your_user",
	"Password": "your_password",
	"RequiredFlags": "@css/smartrestart"
  }
}
```

| Setting | Description | Default |
|---------|-------------|---------|
| **Enabled** | Enable database permissions | `false` |
| **Host** | Database server address | `localhost` |
| **Port** | MySQL port | `3306` |
| **Database** | SimpleAdmin database name | - |
| **RequiredFlags** | Permission flag for commands | `@css/smartrestart` |

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
  "DelayAfterLastPlayerLeaves": 60,
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

  "SmartEmptyRestart": {
	"Enabled": true,
	"MinimumUptimeForEmptyRestartHours": 4,
	"EmptyRestartCooldownMinutes": 30,
	"RequireMinimumSessions": true,
	"MinimumCompletedSessions": 2,
	"RequireRecentActivity": true,
	"RecentActivityWindowMinutes": 60,
	"PeakHours": {
	  "Enabled": true,
	  "StartHour": 16,
	  "EndHour": 23,
	  "DelayMinutes": 10,
	  "OffPeakDelayMinutes": 3
	}
  },

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
	"User": "cs2_user",
	"Password": "your_secure_password",
	"RequiredFlags": "@css/smartrestart"
  }
}
```

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