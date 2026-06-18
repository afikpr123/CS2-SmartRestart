# Contributing to CS2-SmartRestart

Thank you for your interest in contributing to CS2-SmartRestart! 🎉

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Coding Guidelines](#coding-guidelines)
- [Pull Request Process](#pull-request-process)

---

## 📜 Code of Conduct

This project follows a simple code of conduct:
- Be respectful and constructive
- Focus on what is best for the community
- Show empathy towards other community members

---

## 🤝 How Can I Contribute?

### Reporting Bugs
- Use the [Bug Report template](.github/ISSUE_TEMPLATE/bug_report.md)
- Include detailed steps to reproduce
- Provide console logs and configuration
- Check if the issue already exists

### Suggesting Features
- Use the [Feature Request template](.github/ISSUE_TEMPLATE/feature_request.md)
- Explain the use case clearly
- Consider how it fits with existing features

### Translating
- Add new language files in `lang/` folder
- Follow the structure of existing language files
- Test your translations in-game
- Update documentation to mention the new language

### Improving Documentation
- Fix typos or unclear explanations
- Add examples or use cases
- Improve formatting and readability
- Keep guides up-to-date with code changes

### Contributing Code
- Fork the repository
- Create a feature branch
- Follow coding guidelines
- Test your changes thoroughly
- Submit a pull request

---

## 🛠️ Development Setup

### Prerequisites
- Visual Studio 2022/2026 or VS Code
- .NET 10 SDK
- CounterStrikeSharp development environment
- CS2 Dedicated Server (for testing)

### Setup Steps

1. **Fork and Clone**
   ```bash
   git clone https://github.com/afikpr123/CS2-SmartRestart.git
   cd CS2-SmartRestart
   ```

2. **Open Solution**
   - Open `SmartRestart.slnx` in Visual Studio
   - Restore NuGet packages

3. **Build**
   ```bash
   dotnet build
   ```

4. **Test**
   - Copy built DLL to your test server
   - Test all features thoroughly
   - Check console for errors

---

## 📝 Coding Guidelines

### Style
- Follow existing code style and conventions
- Use meaningful variable and method names
- Add comments for complex logic
- Keep methods focused and concise

### Structure
- **Main.cs** - Core plugin logic and orchestration
- **Events/** - Event handlers (player events, etc.)
- **Config.cs** - Configuration schema
- **LanguageConfig.cs** - Localization
- **MessageHelper.cs** - Message formatting and output
- **DatabaseManager.cs** - Database operations
- **DiscordWebhook.cs** - Discord integration

### Best Practices
- ✅ Test on empty and populated servers
- ✅ Verify all warning timings work correctly
- ✅ Check Discord notifications send properly
- ✅ Test database permissions if changed
- ✅ Ensure backward compatibility with configs
- ✅ Update documentation for new features

---

## 🔄 Pull Request Process

1. **Before Submitting**
   - [ ] Code builds without errors
   - [ ] All features tested on a live server
   - [ ] No breaking changes to existing configs
   - [ ] Documentation updated if needed
   - [ ] Console output is clear and helpful

2. **PR Description**
   - Explain what your PR does
   - Reference related issues
   - List breaking changes (if any)
   - Add screenshots/videos for UI changes

3. **Review Process**
   - Maintainers will review your PR
   - Address feedback and requested changes
   - Keep the PR focused on one feature/fix
   - Be patient and respectful

4. **After Merge**
   - Your changes will be included in the next release
   - You'll be credited in release notes
   - Thank you for contributing! 🎉

---

## 🌍 Translation Guidelines

### Adding a New Language

1. Create `lang/[code].json` (e.g., `lang/es.json` for Spanish)
2. Copy structure from `lang/en.json`
3. Translate all strings
4. Test in-game with `{"Language": "[code]"}`
5. Update documentation to list the new language

### Translation Keys

| Key | Usage | Variables |
|-----|-------|-----------|
| `RestartWarning` | Chat warning message | `{time}` |
| `RestartWarningCenter` | Center screen countdown | `{time}` |
| `RestartingNow` | Server restarting message | None |
| `RestartCancelled` | Restart cancelled message | None |
| `NoPermission` | Permission denied | None |

### Testing Translations
- Test with different time values (minutes, seconds)
- Verify color tags work correctly
- Check RTL languages display properly
- Ensure center alerts are readable

---

## 🐛 Debugging Tips

### Enable Verbose Logging
Add debug output in your changes:
```csharp
Console.WriteLine($"[SmartRestart Debug] Your debug message here");
```

### Test Scenarios
- Empty server restart after minimum uptime
- Scheduled restart with warnings
- Player joins during countdown
- Manual restart command
- Cancel restart command
- Discord notifications
- Database permission checks

---

## 📞 Questions?

- Check the [documentation](guide/README.md)
- Search [existing issues](https://github.com/afikpr123/CS2-SmartRestart/issues)
- Ask in [discussions](https://github.com/afikpr123/CS2-SmartRestart/discussions)

---

**Thank you for helping make CS2-SmartRestart better!** ❤️
