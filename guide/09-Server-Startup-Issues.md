# ⚠️ Server Startup Issues

This guide helps you troubleshoot common issues when starting your server with SmartRestart plugin.

---

## Common Startup Errors

### Plugin Not Loading

**Solutions:**
1. Ensure CounterStrikeSharp is installed and updated
2. Check that `CounterStrikeSharp.API` NuGet package is referenced
3. Verify plugin folder structure matches requirements
4. Check file permissions on plugin DLL

### Missing Dependencies

**Solutions:**
1. Verify all dependencies are copied to plugin folder
2. Check `bin/Release/` contains all required DLLs
3. Ensure `CopyLocalLockFileAssemblies` is `true` in `.csproj`
4. Rebuild: `dotnet build -c Release`

---

## Troubleshooting Steps

1. Enable debug logging in config
2. Check server console for initialization messages
3. Review log files in `cs2/logs/`
4. Verify permissions on plugin folder
5. Test build manually

---

## Configuration Issues

- Validate JSON syntax at https://jsonlint.com/
- Check for trailing commas
- Use correct data types (boolean, number, array)
- Include all required fields

---

## Getting Help

- Check Documentation
- Search Issues: https://github.com/afikpr123/CS2-SmartRestart/issues
- Join Discussions: https://github.com/afikpr123/CS2-SmartRestart/discussions