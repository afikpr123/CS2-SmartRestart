# SmartRestart

Automated server restart plugin for Counter-Strike 2 with scheduling, Discord integration, and database logging.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![GitHub Release](https://img.shields.io/github/v/release/afikpr123/CS2-SmartRestart)](https://github.com/afikpr123/CS2-SmartRestart/releases)

---

## Features

✅ **Scheduled Restarts** - Set specific times for automatic restarts
✅ **Interval Restarts** - Restart every X minutes
✅ **Discord Notifications** - Get alerts when server restarts
✅ **Database Logging** - Track all restarts (optional)
✅ **Multi-Language** - English, Spanish, French, Arabic, Hebrew support
✅ **Manual Commands** - Restart on demand with admin commands
✅ **Player Warnings** - Customizable countdown messages

---

## Quick Start

### 1. Install
- Download from [Releases](https://github.com/afikpr123/CS2-SmartRestart/releases)
- Extract to: `game/csgo/addons/counterstrikesharp/plugins/SmartRestart/`
- Restart server

### 2. Configure
Create `config.json`:

{ 
  "Enabled": true, 
  "ScheduledRestarts": { "Enabled": true, "Times": ["12:00:00", "00:00:00"] }, 
  "PreRestartWarning": 300,
  "RestartMessage": "Server restarting in {0} seconds!" 
}


### 3. Done!
Server restarts automatically at set times.

---

## Documentation

📖 [View Full Wiki](https://github.com/afikpr123/CS2-SmartRestart/wiki)

- [Installation](https://github.com/afikpr123/CS2-SmartRestart/wiki/Installation)
- [Configuration](https://github.com/afikpr123/CS2-SmartRestart/wiki/Configuration)
- [Troubleshooting](https://github.com/afikpr123/CS2-SmartRestart/wiki/Troubleshooting)
- [Discord Integration](https://github.com/afikpr123/CS2-SmartRestart/wiki/Discord-Integration)
- [Database Integration](https://github.com/afikpr123/CS2-SmartRestart/wiki/Database-Integration)

---

## Support

- 📖 [Documentation](https://github.com/afikpr123/CS2-SmartRestart/wiki)
- 🐛 [Issues](https://github.com/afikpr123/CS2-SmartRestart/issues)
- 💬 [Discussions](https://github.com/afikpr123/CS2-SmartRestart/discussions)

---

## License

MIT License - See [LICENSE.txt](LICENSE.txt)