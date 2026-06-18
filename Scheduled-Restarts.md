# ⏰ Scheduled Restarts Guide

<div align="center">

**Automate your server maintenance with precision timing**

</div>

---

## 📅 Basic Configuration

Set up automatic restarts at specific times each day:

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

### Settings Explained

| Setting | Format | Description |
|---------|--------|-------------|
| **Enabled** | `true`/`false` | Enable/disable this restart |
| **Hour** | `0-23` | Hour in 24-hour format (server timezone) |
| **Minute** | `0-59` | Minute of the hour |
| **Description** | Text | Your reference label (not shown to players) |

---

## ⚡ How It Works

<table>
<tr>
<td width="25%" align="center">

### 📢 **5 Minutes Before**
Chat warnings begin  
Players notified

</td>
<td width="25%" align="center">

### ⏱️ **30 Seconds Before**
Center countdown starts  
Alert overlay shows

</td>
<td width="25%" align="center">

### 🎮 **Player Detection**
Late joiners get warnings  
Empty = instant restart

</td>
<td width="25%" align="center">

### 🔄 **Restart**
Server restarts gracefully  
Discord notified

</td>
</tr>
</table>

### Smart Behavior

✅ **Empty Server:** Restarts immediately at scheduled time (no warnings needed)  
✅ **Late Joiners:** Players who join mid-countdown receive instant warning  
✅ **Cancelled Restarts:** Automatically reschedules to next occurrence  
✅ **Discord Integration:** Sends notifications if webhook is configured

---

## 📋 Configuration Examples

### Single Daily Restart
Perfect for most servers:
```json
{
  "ScheduledRestarts": [
    {
      "Enabled": true,
      "Hour": 6,
      "Minute": 0,
      "Description": "Daily morning restart"
    }
  ]
}
```
✅ Simple and reliable  
✅ Low-traffic time (6 AM)  
✅ One restart per day

---

### Multiple Daily Restarts
For high-traffic 24/7 servers:
```json
{
  "ScheduledRestarts": [
    {
      "Enabled": true,
      "Hour": 3,
      "Minute": 0,
      "Description": "Night maintenance"
    },
    {
      "Enabled": true,
      "Hour": 9,
      "Minute": 30,
      "Description": "Morning refresh"
    },
    {
      "Enabled": true,
      "Hour": 15,
      "Minute": 0,
      "Description": "Afternoon restart"
    },
    {
      "Enabled": true,
      "Hour": 21,
      "Minute": 0,
      "Description": "Evening restart"
    }
  ]
}
```
✅ Keeps server fresh  
✅ Spreads load across day  
✅ Covers all timezones

---

### Regional Optimization
Target low-traffic hours for your region:

**North America:**
```json
{"Hour": 6, "Minute": 0, "Description": "Morning (EST)"}
{"Hour": 4, "Minute": 0, "Description": "Morning (PST)"}
```

**Europe:**
```json
{"Hour": 5, "Minute": 0, "Description": "Early morning (CET)"}
```

**Asia-Pacific:**
```json
{"Hour": 4, "Minute": 30, "Description": "Morning (JST)"}
```

---

## 🧪 Testing Your Schedule

### Quick Test Method

1. **Set a test restart** 7 minutes in the future:
   ```json
   {"Enabled": true, "Hour": 14, "Minute": 37, "Description": "Test"}
   ```

2. **Reload the plugin:**
   ```
   css_plugins reload SmartRestart
   ```

3. **Watch console output:**
   ```
   [SmartRestart] Next scheduled restart: 14:37 (Test)
   ```

4. **Wait for warnings:**
   - 5 minutes before: Chat warning appears
   - 30 seconds before: Center countdown shows
   - 0 seconds: Server restarts

### What to Check
- ✅ Console shows next restart time correctly
- ✅ Warnings appear at expected times
- ✅ Center countdown displays properly
- ✅ Discord notification sent (if enabled)

---

## ⚠️ Troubleshooting

<table>
<tr>
<td width="40%">

### **Restart not triggering?**

</td>
<td width="60%">

1. Verify `"Enabled": true` for the restart entry
2. Check Hour (0-23) and Minute (0-59) are valid
3. Confirm server timezone matches your expectations
4. Look for: `[SmartRestart] Next scheduled restart: ...` in console

</td>
</tr>
<tr>
<td width="40%">

### **Wrong timing?**

</td>
<td width="60%">

- Server uses **server timezone**, not your local time
- Check system time: Run `date` command on server
- Adjust hours to match server's timezone

</td>
</tr>
<tr>
<td width="40%">

### **Warnings not showing?**

</td>
<td width="60%">

- Check `WarningMessages.Enabled` is `true` in config
- Verify `WarningTimes` includes `[300, 60, 30]` or similar
- See [Configuration Guide](02-Configuration.md) for warning setup

</td>
</tr>
<tr>
<td width="40%">

### **Players not seeing countdown?**

</td>
<td width="60%">

- Ensure `ShowCenterAlert` is `true`
- Center countdown only shows last 30 seconds
- Check [Language Customization](04-Language-Customization.md)

</td>
</tr>
</table>

---

## 💡 Best Practices

<table>
<tr>
<td width="33%">

### 🎯 **Timing**
- Schedule during low-traffic hours
- Avoid peak gaming times
- Consider your region's player base

</td>
<td width="33%">

### ⚙️ **Configuration**
- Use descriptive labels
- Test before production
- Document your schedule
- Keep backups of config

</td>
<td width="33%">

### 📊 **Monitoring**
- Check console logs
- Monitor Discord notifications
- Track player feedback
- Adjust as needed

</td>
</tr>
</table>

---

## 🔗 Related Guides

- **[Configuration Guide](02-Configuration.md)** - Warning system setup
- **[Language Customization](04-Language-Customization.md)** - Edit warning messages
- **[Discord Integration](05-Discord-Integration.md)** - Get restart notifications
- **[Commands](07-Commands.md)** - Manual restart and status commands

---

<div align="center">

**Need more control?** Try the [Smart Empty Restart](02-Configuration.md#-smart-features-optional) features!

</div>
