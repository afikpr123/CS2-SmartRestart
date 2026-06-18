# 🌍 Language Customization Guide

<div align="center">

**Personalize messages, colors, and styles**

</div>

---

## 🗣️ Available Languages

<table>
<tr>
<td align="center" width="50%">

### 🇺🇸 **English**
```json
{"Language": "en"}
```
**Status:** ✅ Default language  
**File:** `lang/en.json`

</td>
<td align="center" width="50%">

### 🇮🇱 **Hebrew**
```json
{"Language": "he"}
```
**Status:** ✅ RTL (Right-to-Left) support  
**File:** `lang/he.json`

</td>
</tr>
</table>

---

## 🔧 Change Language

### 1. Edit Configuration
Open `config.json` and change the language:
```json
{
  "Language": "he"
}
```

### 2. Reload Plugin
```
css_plugins reload SmartRestart
```

### 3. Verify
Check console for:
```
[SmartRestart] Loaded language: he
```

💡 **Auto-Generation:** Missing language files are created automatically with English defaults.

---

## ✏️ Customize Messages

### Edit Language Files

**English:** `lang/en.json`  
**Hebrew:** `lang/he.json`

```json
{
  "RestartWarning": "Server will restart in {time}",
  "RestartWarningCenter": "⏰ SERVER RESTART\n{time} remaining",
  "RestartingNow": "Server is restarting now!",
  "RestartCancelled": "Scheduled restart has been cancelled",
  "NoPermission": "You don't have permission to use this command",
  "DatabaseConnectionFailed": "Database connection failed"
}
```

### Available Variables

| Variable | Replaced With | Example |
|----------|---------------|---------|
| `{time}` | Time remaining | `5 minutes`, `30 seconds` |

---

## 🎨 Color System

### Available Colors

```
{red}        {green}      {blue}       {yellow}
{gold}       {orange}     {purple}     {white}
{grey}       {lime}       {lightblue}  {lightred}
{darkred}    {darkblue}   {pink}       {olive}
{default}    (resets color)
```

### How to Use Colors

```json
{
  "RestartWarning": "{red}WARNING:{default} Server restart in {yellow}{time}{default}"
}
```

**Result:** <span style="color:red">WARNING:</span> Server restart in <span style="color:yellow">5 minutes</span>

### Color Best Practices

<table>
<tr>
<td width="33%">

**⚠️ Warnings**
```json
"{red}WARNING{default}"
"{yellow}ALERT{default}"
"{gold}NOTICE{default}"
```

</td>
<td width="33%">

**ℹ️ Information**
```json
"{blue}INFO{default}"
"{lightblue}TIP{default}"
"{white}STATUS{default}"
```

</td>
<td width="33%">

**✅ Success**
```json
"{green}SUCCESS{default}"
"{lime}COMPLETE{default}"
"{gold}DONE{default}"
```

</td>
</tr>
</table>

---

## 📺 Center Alert Customization

### What is Center Alert?

The **center alert** is an overlay countdown that appears in the middle of the screen during the last **30 seconds** before restart.

### Customize Center Alert

```json
{
  "RestartWarningCenter": "⏰ {red}RESTART{default}\n{yellow}{time}{default}"
}
```

### Formatting Tips

| Feature | Syntax | Example |
|---------|--------|---------|
| **New Line** | `\n` | `"Line 1\nLine 2"` |
| **Colors** | `{color}text{default}` | `"{red}WARNING{default}"` |
| **Emojis** | Unicode | `"⚠ ⏰ 🔄"` |
| **Variables** | `{time}` | `"Restart: {time}"` |

### Center Alert Examples

**Minimal:**
```json
{
  "RestartWarningCenter": "RESTART IN\n{time}"
}
```

**Styled:**
```json
{
  "RestartWarningCenter": "{red}⚠ SERVER RESTART ⚠{default}\n{yellow}{time} remaining{default}"
}
```

**Professional:**
```json
{
  "RestartWarningCenter": "🔄 {gold}MAINTENANCE{default}\n{white}Server restarting in{default}\n{red}{time}{default}"
}
```

---

## 📋 Message Examples

### Simple Style
Clean and minimal:
```json
{
  "RestartWarning": "Restart in {time}",
  "RestartWarningCenter": "RESTART\n{time}",
  "RestartingNow": "Restarting now"
}
```

### Professional Style
Corporate and clear:
```json
{
  "RestartWarning": "{blue}[System]{default} Scheduled maintenance in {yellow}{time}{default}",
  "RestartWarningCenter": "{blue}SCHEDULED MAINTENANCE{default}\n{yellow}{time}{default}",
  "RestartingNow": "{green}Maintenance started{default}"
}
```

### Vibrant Style
Eye-catching with emojis:
```json
{
  "RestartWarning": "{gold}⚠{default} {red}Server restart in {yellow}{time}{default} {gold}⚠{default}",
  "RestartWarningCenter": "{red}🔥 RESTART ALERT 🔥{default}\n{yellow}⏰ {time} ⏰{default}",
  "RestartingNow": "{green}✅ Server restarting!{default}"
}
```

### Hebrew Example
RTL support with Hebrew text:
```json
{
  "RestartWarning": "{red}אזהרה:{default} השרת יאותחל בעוד {yellow}{time}{default}",
  "RestartWarningCenter": "{red}⚠ אתחול שרת ⚠{default}\n{yellow}{time} נותרו{default}",
  "RestartingNow": "{green}השרת מאותחל כעת{default}"
}
```

---

## 🔄 Apply Changes

### After Editing Language Files:

1. **Save** your `lang/en.json` or `lang/he.json` file
2. **Reload** the plugin:
   ```
   css_plugins reload SmartRestart
   ```
3. **Test** with a scheduled restart or `!nextrestart` command

💡 **Tip:** Make small changes and test frequently to ensure formatting looks correct in-game.

---

## 💡 Tips & Tricks

<table>
<tr>
<td width="50%">

### ✅ **Do's**
- Always use `{default}` after colored text
- Test colors in-game (some may look different)
- Keep center alerts short (2-3 lines max)
- Use emojis sparingly for visual appeal
- Back up your custom messages

</td>
<td width="50%">

### ❌ **Don'ts**
- Don't forget to close color tags
- Don't make center alerts too long
- Don't use too many colors (hard to read)
- Don't remove `{time}` variable from warnings
- Don't edit files while server is running

</td>
</tr>
</table>

---

## 🔗 Related Guides

- **[Configuration Guide](02-Configuration.md)** - Chat prefix and warning system
- **[Scheduled Restarts](03-Scheduled-Restarts.md)** - When warnings appear
- **[Commands](07-Commands.md)** - Test your messages
