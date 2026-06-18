# Security Policy

## 🔒 Reporting Security Issues

If you discover a security vulnerability in CS2-SmartRestart, please report it responsibly.

### ⚠️ Please Do Not:
- Open a public GitHub issue for security vulnerabilities
- Share the vulnerability publicly before it's been addressed

### ✅ Please Do:
1. **Email the maintainer** with details of the vulnerability
2. Include steps to reproduce the issue
3. Provide your assessment of the severity
4. Allow time for a fix to be developed and deployed

## 🛡️ Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| Latest  | ✅ Supported       |
| Older   | ❌ Update required |

I always recommend using the latest version for security and feature updates.

## 🔐 Security Best Practices

When using CS2-SmartRestart:

### Database Security
- ✅ Use **read-only** database permissions for the plugin user
- ✅ Use **strong passwords** for database credentials
- ✅ **Restrict database access** to necessary hosts only
- ✅ Keep database credentials in config file, not in code
- ⚠️ Never commit `config.json` with real credentials to public repos

### Discord Webhooks
- ✅ Keep webhook URLs private
- ✅ Regenerate webhooks if exposed publicly
- ✅ Use channel permissions to control webhook visibility
- ⚠️ Never share webhook URLs in screenshots or logs

### Server Security
- ✅ Keep CS2 server and CounterStrikeSharp updated
- ✅ Use proper file permissions on plugin files
- ✅ Regularly review admin permissions in SimpleAdmin
- ✅ Monitor server logs for suspicious activity

### Configuration
- ✅ Review permission flags regularly
- ✅ Use specific permission flags (e.g., `@css/smartrestart` not `@css/root`)
- ✅ Audit who has restart permissions
- ⚠️ Disable database integration if not needed

---

**Thank you for helping keep CS2-SmartRestart secure!**
