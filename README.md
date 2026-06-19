<div align="center">

# 🚀 SmartRestart

**Automated Server Restart Plugin for Counter-Strike 2**

Scheduled restarts • Discord notifications • Database logging • Multi-language support

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)
[![Version](https://img.shields.io/github/v/release/afikpr123/CS2-SmartRestart?color=green)](https://github.com/afikpr123/CS2-SmartRestart/releases)
[![Issues](https://img.shields.io/github/issues/afikpr123/CS2-SmartRestart?color=orange)](https://github.com/afikpr123/CS2-SmartRestart/issues)
[![Discord](https://img.shields.io/badge/Discord-Community-7289da)](https://discord.gg/example)

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

Create a `config.json` file in your plugin folder:

```json
{ 
  "Enabled": true, 
  "ScheduledRestarts": { "Enabled": true, "Times": ["12:00:00", "00:00:00"] },
  "PreRestartWarning": 300,
  "RestartMessage": "Server restarting in {0} seconds!",
  "BroadcastToChat": true,
  "Discord": { "Enabled": false }
}
```

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

### Example 1: Daily Restarts at Specific Times

```json
{ 
   "ScheduledRestarts": { "Enabled": true, 
   "Times": ["06:00:00", "14:00:00", "22:00:00"] }, 
   "PreRestartWarning": 300, 
   "RestartMessage": "Server restarting in {0} seconds!" 
}
```

### Example 2: Restart Every 2 Hours

```json
{ 
   "IntervalRestarts": { 
   "Enabled": true,
   "IntervalMinutes": 120,
   "FirstRestartDelay": 30 
   } 
}
```

### Example 3: With Discord Notifications

```json
{ 
   "Discord": { 
   "Enabled": true, 
   "WebhookUrl": "https://discord.com/api/webhooks/YOUR_ID/YOUR_TOKEN", 
   "NotifyOnRestart": true 
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

---

## 🤝 Contributing

We welcome contributions! Here's how you can help:

- **Found a bug?** [Report it on Issues](https://github.com/afikpr123/CS2-SmartRestart/issues)
- **Have an idea?** [Share it on Discussions](https://github.com/afikpr123/CS2-SmartRestart/discussions)
- **Want to contribute?** See [Contributing Guidelines](.github/CONTRIBUTING.md)

---

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