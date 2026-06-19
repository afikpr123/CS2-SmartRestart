<div align="center">

# 🚀 SmartRestart

**Automated Server Restart Plugin for Counter-Strike 2**

Scheduled restarts • Discord notifications • Database logging • Multi-language support

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)
[![Version](https://img.shields.io/github/v/release/afikpr123/CS2-SmartRestart?color=green)](https://github.com/afikpr123/CS2-SmartRestart/releases)
[![Issues](https://img.shields.io/github/issues/afikpr123/CS2-SmartRestart?color=orange)](https://github.com/afikpr123/CS2-SmartRestart/issues)
[Documentation](https://github.com/afikpr123/CS2-SmartRestart/wiki) • [Issues](https://github.com/afikpr123/CS2-SmartRestart/issues) • [Discussions](https://github.com/afikpr123/CS2-SmartRestart/discussions)

</div>

---

## ✨ Key Features

| Feature | Details |
|---------|---------|
| 🕐 **Scheduled Restarts** | Set specific times for automatic restarts |
| ⏱️ **Interval Restarts** | Restart every X minutes |
| 📢 **Player Warnings** | Customizable countdown messages in chat |
| 🎮 **Manual Control** | Admin commands to restart on demand |
| 💬 **Discord Integration** | Real-time restart notifications |
| 📊 **Database Logging** | Track restart history (optional) |
| 🌍 **Multi-Language** | English, Spanish, French, Arabic, Hebrew |
| ⚡ **Zero Downtime** | Seamless server restarts |

---

## 🚀 Quick Start

### Installation

1. **Download** the latest release from [Releases](https://github.com/afikpr123/CS2-SmartRestart/releases)
2. **Extract** to: `game/csgo/addons/counterstrikesharp/plugins/SmartRestart/`
3. **Restart** your Counter-Strike 2 server

### Configuration

A `config.json` file will auto generate into your plugin folder:

**That's it!** Your server will now restart automatically.

---

## 📖 Documentation

### Core Guides
- 📋 [Installation Guide](https://github.com/afikpr123/CS2-SmartRestart/wiki/Installation) - Setup instructions
- ⚙️ [Configuration Guide](https://github.com/afikpr123/CS2-SmartRestart/wiki/Configuration) - All settings explained
- 🎯 [Scheduled Restarts](https://github.com/afikpr123/CS2-SmartRestart/wiki/Scheduled-Restarts) - Time-based restarts
- ⏰ [Interval Restarts](https://github.com/afikpr123/CS2-SmartRestart/wiki/Interval-Restarts) - Restart every X minutes

### Advanced Features
- 💬 [Discord Integration](https://github.com/afikpr123/CS2-SmartRestart/wiki/Discord-Integration) - Send notifications to Discord
- 📚 [Database Integration](https://github.com/afikpr123/CS2-SmartRestart/wiki/Database-Integration) - Log restarts to MySQL
- 🌐 [Language Customization](https://github.com/afikpr123/CS2-SmartRestart/wiki/Language-Customization) - Add custom messages
- 🔧 [Commands Reference](https://github.com/afikpr123/CS2-SmartRestart/wiki/Commands) - Admin commands

### Troubleshooting
- 🆘 [Troubleshooting Guide](https://github.com/afikpr123/CS2-SmartRestart/wiki/Troubleshooting) - Common issues and fixes
- 🐛 [Server Startup Issues](https://github.com/afikpr123/CS2-SmartRestart/wiki/Server-Startup-Issues) - Plugin won't start

[📚 View Full Wiki →](https://github.com/afikpr123/CS2-SmartRestart/wiki)

---

## 🎮 Usage Examples

### Example of a `config.json` file with advanced smart restart settings:

```json
{
  // Auto-restart settings
  "EnableAutoRestart": true, // Restart server automatically when empty
  "DelayAfterLastPlayerLeaves": 60, // Seconds to wait after last player leaves (used if Smart restart disabled)
  "MinimumUptimeMinutes": 5, // Minimum uptime (minutes) for MANUAL restarts - prevents accidental rapid restarts
  "MinimumUptimeForEmptyRestartHours": 4, // Minimum uptime (hours) for AUTO empty-server restarts - prevents spam

  // Smart empty restart settings
  "EmptyRestartCooldownMinutes": 30, // Cooldown between empty restarts - prevents restart spam
  "MinimumPlayersBeforeEmptyRestart": 1, // Server must have had this many players before restarting when empty
  "RequirePlayerActivityForEmptyRestart": true, // Only restart if server had player activity since last restart

  // Advanced smart restart (time-aware + session-based)
  "SmartEmptyRestart": {
    "Enabled": true, // Enable advanced smart restart logic
    "Strategy": "Smart", // Options: Immediate, Smart
    "PeakHours": {
      "Enabled": true,
      "StartHour": 18, // 24-hour format (6 PM)
      "EndHour": 23, // 11 PM
      "DelayMinutes": 15, // Wait 15 minutes during peak hours (players might return)
      "OffPeakDelayMinutes": 3 // Fast restart during off-peak (3 minutes)
    },
    "SessionBased": {
      "Enabled": true,
      "MinimumSessionLengthMinutes": 5, // Only count sessions longer than 5 minutes
      "MinimumTotalPlaytimeMinutes": 30, // Server must have 30 min of playtime before restart
      "RecentActivityWindowMinutes": 10 // Delay restart if player left within 10 minutes
    },
    "MaximumEmptyWaitMinutes": 30 // Never wait longer than 30 minutes
  },

  // Scheduled restarts
  "ScheduledRestarts": [
    {
      "Enabled": true,
      "Hour": 6, // 24-hour format (0-23)
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

  // Warning messages before restart
  "WarningMessages": {
    "Enabled": true,
    "WarningTimes": [300, 180, 120, 60, 30, 10], // Seconds before restart to warn players
    "ShowCenterAlert": true, // Show large center-screen alerts
    "CenterAlertDuration": 5 // How long center alerts stay visible (seconds)
  },

  // Restart command (quit for Pelican/Pterodactyl, exit, or _restart)
  "RestartCommand": "quit",

  // Chat message prefix (supports color tags like {gold}, {blue}, {red}, etc.)
  "ChatPrefix": "[{gold}SmartRestart{default}]",

  // Discord webhook notifications
  "DiscordWebhook": {
    "Enabled": false,
    "WebhookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL_HERE",
    "EmbedStyle": "detailed", // Options: simple, detailed, professional
    "FooterImageUrl": "https://png.pngtree.com/png-vector/20250826/ourmid/pngtree-rack-illustration-isometric-server-png-image_17213766.webp",
    "RestartEmbed": {
      "Enabled": true,
      "Color": "231, 76, 60", // RGB format: R, G, B (Orange/Red)
      "Title": "🔄 Server Restarting",
      "ShowUptime": true,
      "ShowPlayers": true,
      "ShowReason": true,
      "ShowEstimatedDowntime": true,
      "EstimatedDowntimeSeconds": 120
    },
    "OnlineEmbed": {
      "Enabled": true,
      "Color": "46, 204, 113", // RGB format: R, G, B (Green)
      "Title": "✅ Server Back Online!",
      "ShowDowntime": true,
      "ShowMap": true,
      "ShowConnectInfo": true
    },
    "ManualEmbed": {
      "Enabled": true,
      "Color": "28, 109, 240", // RGB format: R, G, B (Blue)
      "Title": "🔧 Manual Server Restart",
      "ShowUptime": true,
      "ShowPlayers": true,
      "ShowAdmin": true,
      "ShowEstimatedDowntime": true,
      "EstimatedDowntimeSeconds": 120
    },
    "SendOnScheduledRestart": true,
    "SendOnManualRestart": true,
    "SendOnEmptyServerRestart": true,
    "SendWarnings": false
  },

  // SimpleAdmin database integration for permission checks
  "Database": {
    "Enabled": false, // Enable to use SimpleAdmin permissions for !serverrestart command
    "Host": "localhost",
    "Port": 3306,
    "Database": "cs2_server",
    "Username": "user",
    "Password": "password",
    "RequiredPermission": "@css/smartrestart" // Permission flag checked in sa_admins_groups and sa_admins tables
  }
}
```

---

## 💻 System Requirements

| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| **Framework** | .NET 10.0 | .NET 10.0+ |
| **Game** | Counter-Strike 2 | Latest version |
| **API** | CounterStrikeSharp | Latest |
| **Database** | None | MySQL 5.7+ (optional) |

---

## 🛠️ Admin Commands

| Command | Description |
|---------|-------------|
| `css_restart_now` | Restart server immediately |
| `css_restart_schedule` | View next scheduled restart |
| `css_restart_cancel` | Cancel pending restart |
| `css_restart_status` | Check plugin status |

---

## 📂 File Structure

```
SmartRestart/
├── SmartRestart.dll      # Main plugin 
├── config.json           # Configuration file 
├── lang/                 # Language files   
│   ├── en.json           # English  
│   └── he.json           # Hebrew
└── MySqlConnector.dll    # Database support
```


## 📞 Support

Having issues? We're here to help!

| Channel | Purpose |
|---------|---------|
| 📖 [Wiki](https://github.com/afikpr123/CS2-SmartRestart/wiki) | Full documentation |
| 🐛 [Issues](https://github.com/afikpr123/CS2-SmartRestart/issues) | Report bugs |
| 💬 [Discussions](https://github.com/afikpr123/CS2-SmartRestart/discussions) | Ask questions |

## 📄 License

This project is licensed under the **MIT License** - see [LICENSE.txt](LICENSE.txt) for details.

---

## 🙏 Credits

Made with ❤️ for the **Counter-Strike 2** community.

---

<div align="center">

**[⬆ back to top](#)**

**[View on GitHub](https://github.com/afikpr123/CS2-SmartRestart)** • **[Report Issue](https://github.com/afikpr123/CS2-SmartRestart/issues/new)**

</div>