# ⚠️ Server Startup Issues
Problems with SmartRestart? Find solutions here.

---

## Restarts Not Happening

### Scheduled Restarts Not Triggering

Check your config:
- Is `"Enabled": true`?
- Is `"ScheduledRestarts"` enabled?
- Times in correct format? (HH:MM:SS in 24-hour)

Example:
{ "ScheduledRestarts": { "Enabled": true, "Times": ["12:00:00", "00:00:00"] } }


### Players Don't See Countdown

Check:
- Is `"BroadcastToChat": true`?
- Is `"RestartMessage"` set?
- Does message have `{0}` for countdown?

### Restart Cancels Too Easily

Disable player cancellation: { "CancelIfPlayersInGame": false, "AllowPlayersToCancel": false }

---

## Interval Restarts Not Working

### Restarts Every X Minutes

Check config: { "IntervalRestarts": { "Enabled": true, "IntervalMinutes": 120 } }


**Important:** Use MINUTES not seconds (120 = 2 hours)

---

## Messages & Discord

### Wrong Language

1. Check `/lang/` folder has correct files
2. Set correct language: `"Language": "en"`
3. Restart plugin

### Discord Webhook Issues

1. Create new webhook in Discord
2. Copy full URL to config
3. Restart plugin

---

## Database Issues

### Can't Connect

Check credentials and ask your hosting provider if database works.

Disable temporarily: { "Database": { "Enabled": false } }


---

## Common Mistakes

❌ Using 12-hour time (12 AM) → ✅ Use 24-hour (00:00)
❌ Interval in seconds → ✅ Use minutes (120 = 2 hours)
❌ Removing {0} from message → ✅ Keep it for countdown
❌ Editing config while running → ✅ Restart server after changes

---

## Verify Before Help

- Config valid at https://jsonlint.com
- All `"Enabled": true`
- No typos in settings
- Restart command works manually
- Fully restarted server after changes

Help: https://github.com/afikpr123/CS2-SmartRestart/issues
