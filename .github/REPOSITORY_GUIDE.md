# CS2-SmartRestart Repository Structure

This document describes the structure of the CS2-SmartRestart GitHub repository.

## 📁 Repository Structure

```
CS2-SmartRestart/
├── .github/
│   ├── ISSUE_TEMPLATE/
│   │   ├── bug_report.md
│   │   └── feature_request.md
│   ├── workflows/              (Add CI/CD workflows here)
│   ├── CONTRIBUTING.md
│   ├── FUNDING.yml
│   ├── pull_request_template.md
│   └── SECURITY.md
│
├── SmartRestart/               (Source code)
│   ├── Events/
│   │   └── PlayerEventHandler.cs
│   ├── lang/
│   │   ├── en.json
│   │   ├── he.json
│   │   └── ... (other languages)
│   ├── Config.cs
│   ├── DatabaseManager.cs
│   ├── DiscordWebhook.cs
│   ├── LanguageConfig.cs
│   ├── Main.cs
│   ├── MessageHelper.cs
│   └── SmartRestart.csproj
│
├── guide/                      (Documentation)
│   ├── 01-Installation.md
│   ├── 02-Configuration.md
│   ├── 03-Scheduled-Restarts.md
│   ├── 04-Language-Customization.md
│   ├── 05-Discord-Integration.md
│   ├── 06-Database-Integration.md
│   ├── 07-Commands.md
│   ├── 08-Troubleshooting.md
│   └── README.md
│
├── .gitignore
├── LICENSE                     (Add your license)
├── README.md                   (Main project README)
└── SmartRestart.slnx          (Solution file)
```

## 🏷️ Release Process

### Version Numbering
Follow [Semantic Versioning](https://semver.org/):
- **MAJOR.MINOR.PATCH** (e.g., 1.0.0)
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

### Creating a Release

1. **Update Version**
   - Update version in `SmartRestart.csproj`
   - Update CHANGELOG (if you have one)

2. **Build Release**
   ```bash
   dotnet build -c Release
   ```

3. **Package Files**
   Create a ZIP with:
   ```
   SmartRestart/
   ├── SmartRestart.dll
   ├── MySqlConnector.dll
   └── lang/
	   ├── en.json
	   ├── he.json
	   └── ... (all language files)
   ```

4. **Create GitHub Release**
   - Go to Releases → Draft a new release
   - Tag: `v1.0.0` (or your version)
   - Title: `CS2-SmartRestart v1.0.0`
   - Description: List changes, new features, bug fixes
   - Attach the ZIP file
   - Publish release

### Release Notes Template

```markdown
## 🎉 CS2-SmartRestart v1.0.0

### ✨ New Features
- Feature 1 description
- Feature 2 description

### 🐛 Bug Fixes
- Fix 1 description
- Fix 2 description

### 📝 Changes
- Change 1 description
- Change 2 description

### ⚠️ Breaking Changes
- None (or list breaking changes)

### 📦 Installation
1. Download `CS2-SmartRestart-v1.0.0.zip`
2. Extract to `addons/counterstrikesharp/plugins/`
3. Restart server or reload plugin

### 📚 Documentation
- [Installation Guide](guide/01-Installation.md)
- [Configuration Guide](guide/02-Configuration.md)
- [Full Documentation](guide/README.md)

### 🙏 Contributors
Thanks to @username1, @username2 for their contributions!
```

## 🔄 Suggested GitHub Workflows

### Build and Test (`.github/workflows/build.yml`)
Automatically build on push/PR to ensure code compiles.

### Auto-Release (`.github/workflows/release.yml`)
Automatically create releases when you push a version tag.

## 📄 License Recommendations

Consider adding one of these licenses:
- **MIT License** - Very permissive, allows commercial use
- **GPL-3.0** - Copyleft, requires derivative works to be open source
- **Apache 2.0** - Similar to MIT but includes patent rights

Add your chosen `LICENSE` file to the repository root.

## 🏷️ Repository Topics (GitHub)

Add these topics to your repository for discoverability:
- `counterstrike`
- `cs2`
- `counter-strike-2`
- `counterstrikesharp`
- `plugin`
- `server-management`
- `restart-manager`
- `csharp`
- `dotnet`

## 📊 Repository Settings

### Suggested Settings:
- ✅ Enable Issues
- ✅ Enable Discussions (for community Q&A)
- ✅ Enable Wiki (optional, for extended docs)
- ✅ Require PR reviews before merge
- ✅ Auto-merge allowed (for trusted contributors)
- ✅ Allow squash merging
- ✅ Automatically delete head branches

### Branch Protection (main/master):
- ✅ Require pull request reviews
- ✅ Require status checks to pass
- ✅ Require branches to be up to date
- ✅ Include administrators

## 🎨 About Section

Fill out the repository "About" section:
- **Description**: "Advanced server restart management system for CS2 with smart scheduling, Discord integration, and SimpleAdmin support"
- **Website**: Link to documentation or your website
- **Topics**: Add relevant tags

---

**Ready to publish?** Make sure you've:
- [x] Updated all `YOUR_USERNAME` placeholders with your GitHub username (afikpr123)
- [ ] Added a LICENSE file
- [ ] Tested the build process
- [ ] Reviewed all documentation
- [ ] Created your first release
