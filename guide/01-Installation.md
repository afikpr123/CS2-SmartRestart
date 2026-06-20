# 📦 Installation Guide

<div align="center">

**Get SmartRestart running in 3 minutes**

</div>

---

## ✅ Requirements

Before installing, ensure you have:

| Requirement | Version | Status |
|------------|---------|--------|
| **Counter-Strike 2 Server** | Latest | Required |
| **CounterStrikeSharp** | Latest | Required |
| **.NET Runtime** | 10.0 | Required |

---

## 🚀 Installation Steps

### 1️⃣ **Download the Plugin**
Get the latest release from GitHub or your preferred source.

### 2️⃣ **Extract Files**
Place all files into your CS2 server directory:
```
game/csgo/addons/counterstrikesharp/plugins/SmartRestart/
├── SmartRestart.dll
├── MySqlConnector.dll  (required for database features)
├── lang/
│   ├── en.json
│   └── he.json
└── (config.json will be auto-generated)
```

### 3️⃣ **Start Your Server**
The plugin will automatically:
- ✅ Load on server start
- ✅ Generate `config.json` in the plugin directory  
  ⚠️ **Note:** Config is created in the plugin folder, **NOT** in CSS config folder
- ✅ Create default language files if missing

### 4️⃣ **Verify Installation**
Check your console for:
```
====================================================
 SMARTRESTART STARTUP
====================================================
...
SmartRestart ready.
```

---

## 🎯 Quick Verification Checklist

- [ ] `SmartRestart.dll` is in the plugins folder
- [ ] `MySqlConnector.dll` is present (even if not using database)
- [ ] Server console shows successful load message
- [ ] `config.json` was auto-generated in plugin directory
- [ ] Language files exist in `lang/` folder

---

## ⚠️ Common Issues

<table>
<tr>
<td width="30%">

**Plugin not loading?**

</td>
<td width="70%">

- Verify CounterStrikeSharp is installed correctly
- Check .NET 10 runtime is available
- Ensure all DLL files are present

</td>
</tr>
<tr>
<td width="30%">

**Config not generating?**

</td>
<td width="70%">

- Check folder permissions
- Config creates in plugin folder, not CSS config folder
- Wait for full server start, then restart

</td>
</tr>
<tr>
<td width="30%">

**Database errors?**

</td>
<td width="70%">

- Make sure `MySqlConnector.dll` is present
- Database can be disabled in config if not needed

</td>
</tr>
</table>

---

## 🎉 Installation Complete!

### What's Next?

<table>
<tr>
<td align="center" width="33%">

### ⚙️ **Configure**
[Edit your settings](02-Configuration.md)  
*Set language, warnings, and delays*

</td>
<td align="center" width="33%">

### ⏰ **Schedule Restarts**
[Set up automatic restarts](03-Scheduled-Restarts.md)  
*Add daily restart times*

</td>
<td align="center" width="33%">

### 🚀 **Optional Features**
[Discord](05-Discord-Integration.md) • [Database](06-Database-Integration.md)  
*Advanced integrations*

</td>
</tr>
</table>

---

<div align="center">

**Need help?** Check the [Troubleshooting Guide](08-Troubleshooting.md)

</div>
