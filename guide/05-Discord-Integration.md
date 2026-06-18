# 🔔 Discord Integration Guide

<div align="center">

**Beautiful real-time restart notifications in Discord**

</div>

---

## 🚀 Quick Setup

### Step 1: Create Discord Webhook

<table>
<tr>
<td width="10%" align="center">

**1️⃣**

</td>
<td width="90%">

Open your **Discord Server** → Select a channel for notifications

</td>
</tr>
<tr>
<td width="10%" align="center">

**2️⃣**

</td>
<td width="90%">

**Channel Settings** → **Integrations** → **Webhooks**

</td>
</tr>
<tr>
<td width="10%" align="center">

**3️⃣**

</td>
<td width="90%">

Click **"New Webhook"** or **"Create Webhook"**

</td>
</tr>
<tr>
<td width="10%" align="center">

**4️⃣**

</td>
<td width="90%">

Name it (e.g., "SmartRestart Bot"), customize avatar if desired

</td>
</tr>
<tr>
<td width="10%" align="center">

**5️⃣**

</td>
<td width="90%">

Click **"Copy Webhook URL"** - Save this URL!

</td>
</tr>
</table>

---

### Step 2: Configure Plugin

Edit `config.json`:

```json
{
  "DiscordWebhook": {
    "Enabled": true,
    "WebhookUrl": "https://discord.com/api/webhooks/1234567890/AbCdEfGhIjKlMnOpQrStUvWxYz",
    "EmbedStyle": "Clean",
    "FooterImageUrl": "https://i.imgur.com/yourlogo.png"
  }
}
```

---

### Step 3: Reload & Test

```bash
css_plugins reload SmartRestart
```

Trigger a test restart:
```bash
!serverrestart
```

Check your Discord channel for the notification! 🎉

---

## 🎨 Embed Styles

Choose the notification style that fits your server:

<table>
<tr>
<td width="50%">

### 📋 **Clean** (Recommended)
```json
{"EmbedStyle": "Clean"}
```

**Perfect For:** Most servers  
**Includes:**
- ✅ Server name
- ✅ Restart reason
- ✅ Uptime
- ✅ Timestamp
- ✅ Clean design

**Example:**
> 🔄 **Server Restarting**  
> Scheduled restart  
> ⏱️ Uptime: 6h 23m  
> 🕒 Today at 6:00 AM

</td>
<td width="50%">

### 📊 **Detailed**
```json
{"EmbedStyle": "Detailed"}
```

**Perfect For:** Community servers  
**Includes:**
- ✅ All Clean features
- ✅ Player count
- ✅ Session stats
- ✅ Extended info
- ✅ More fields

**Example:**
> 🔄 **Server Restarting**  
> Scheduled restart  
> ⏱️ Uptime: 6h 23m  
> 👥 Players: 12/20  
> 📊 Sessions: 45  
> 🕒 Today at 6:00 AM

</td>
</tr>
</table>

---

## 📬 What Gets Sent

### ✅ Notifications Sent

<table>
<tr>
<td width="25%" align="center">

### 🔄 **Scheduled Restart**
5 minutes before  
restart time

</td>
<td width="25%" align="center">

### 🎮 **Manual Restart**
When admin uses  
`!serverrestart`

</td>
<td width="25%" align="center">

### ✅ **Server Online**
After restart  
completes

</td>
<td width="25%" align="center">

### 🚫 **Restart Cancelled**
When restart is  
cancelled

</td>
</tr>
</table>

### ❌ NOT Sent

- ⚠️ Chat warnings (in-game only)
- ⏱️ Countdown updates (in-game only)
- 🔁 Empty server restarts (optional, configurable)

💡 **Why?** To avoid spam and keep Discord clean while still providing important notifications.

---

## ⚙️ Advanced Configuration

### Complete Options

```json
{
  "DiscordWebhook": {
    "Enabled": true,
    "WebhookUrl": "https://discord.com/api/webhooks/YOUR_WEBHOOK_URL",
    "EmbedStyle": "Clean",
    "FooterImageUrl": "https://i.imgur.com/yourlogo.png",
    "SendEmptyServerRestarts": false,
    "SendManualRestarts": true,
    "SendScheduledRestarts": true,
    "SendServerOnline": true
  }
}
```

| Setting | Default | Description |
|---------|---------|-------------|
| **Enabled** | `false` | Enable Discord notifications |
| **WebhookUrl** | - | Your Discord webhook URL |
| **EmbedStyle** | `"Clean"` | Notification style (`Clean`, `Detailed`) |
| **FooterImageUrl** | - | Small icon in footer (optional) |
| **SendEmptyServerRestarts** | `false` | Notify on empty server restarts |
| **SendManualRestarts** | `true` | Notify on `!serverrestart` |
| **SendScheduledRestarts** | `true` | Notify on scheduled restarts |
| **SendServerOnline** | `true` | Notify when server is back online |

---

## 🖼️ Footer Image

Add a custom footer icon to your notifications:

```json
{
  "FooterImageUrl": "https://i.imgur.com/yourlogo.png"
}
```

### Image Requirements
- ✅ **Format:** PNG, JPG, GIF
- ✅ **Size:** Recommended 64x64 to 256x256 pixels
- ✅ **Hosting:** Must be publicly accessible URL (Imgur, Discord CDN, your website)

### Popular Hosting Options
- **Imgur:** Free, easy, reliable - [imgur.com](https://imgur.com)
- **Discord CDN:** Upload to Discord, copy image URL
- **Your Website:** Host on your own server

---

## 🎨 Embed Colors

Embeds automatically use color coding:

| Type | Color | Icon |
|------|-------|------|
| **Scheduled Restart** | 🟡 Yellow | 🔄 |
| **Manual Restart** | 🔴 Red | ⚠️ |
| **Server Online** | 🟢 Green | ✅ |
| **Cancelled** | ⚪ Gray | 🚫 |

---

## ⚠️ Troubleshooting

<table>
<tr>
<td width="40%">

### **No notifications appearing?**

</td>
<td width="60%">

1. Verify `"Enabled": true`
2. Check webhook URL is correct (starts with `https://discord.com/api/webhooks/`)
3. Ensure webhook still exists in Discord (not deleted)
4. Check server console for errors
5. Test webhook manually: [Discord Webhook Tester](https://discohook.org/)

</td>
</tr>
<tr>
<td width="40%">

### **Webhook URL invalid error?**

</td>
<td width="60%">

- Copy the **full URL** from Discord
- Don't modify or shorten the URL
- URL format: `https://discord.com/api/webhooks/ID/TOKEN`
- Check for extra spaces before/after URL

</td>
</tr>
<tr>
<td width="40%">

### **Footer image not showing?**

</td>
<td width="60%">

- Ensure URL is publicly accessible
- Check image format (PNG/JPG/GIF)
- Test URL in browser - should display image
- Leave empty (`""`) to disable footer image

</td>
</tr>
<tr>
<td width="40%">

### **Too many notifications?**

</td>
<td width="60%">

Disable specific notification types:
```json
{
  "SendEmptyServerRestarts": false,
  "SendManualRestarts": false
}
```

</td>
</tr>
</table>

---

## 🧪 Testing Your Setup

### Quick Test

1. **Configure** webhook in `config.json`
2. **Reload** plugin: `css_plugins reload SmartRestart`
3. **Trigger** manual restart: `!serverrestart` (requires permission)
4. **Check** Discord channel - should see notification within seconds

### Expected Result

You should see a Discord embed message with:
- 🔄 Server restarting notification
- ⏱️ Uptime information
- 🕒 Timestamp
- Your custom footer image (if configured)

---

## 💡 Best Practices

<table>
<tr>
<td width="33%">

### 📢 **Channel Setup**
- Create dedicated #server-status channel
- Set permissions (read-only for members)
- Pin important notifications
- Use webhook avatar/name

</td>
<td width="33%">

### 🎨 **Customization**
- Use your server logo for footer
- Choose embed style matching your server
- Keep FooterImageUrl small
- Test styles before production

</td>
<td width="33%">

### 🔧 **Maintenance**
- Backup webhook URL
- Monitor notification frequency
- Update URL if webhook changes
- Document your setup

</td>
</tr>
</table>

---

## 📋 Example Notifications

### Scheduled Restart (Clean Style)
```
🔄 Server Restarting
Scheduled restart - Daily maintenance
⏱️ Uptime: 8 hours 45 minutes
🕒 Today at 6:00 AM
```

### Manual Restart (Detailed Style)
```
⚠️ Manual Server Restart
Initiated by AdminName
⏱️ Uptime: 2 hours 15 minutes
👥 Active Players: 8
📊 Total Sessions: 23
🕒 Today at 2:30 PM
```

### Server Online
```
✅ Server Online
Successfully restarted
🕒 Today at 6:05 AM
```

---

## 🔗 Related Guides

- **[Configuration Guide](02-Configuration.md)** - All webhook settings
- **[Scheduled Restarts](03-Scheduled-Restarts.md)** - When notifications are sent
- **[Commands](07-Commands.md)** - Trigger manual restarts


