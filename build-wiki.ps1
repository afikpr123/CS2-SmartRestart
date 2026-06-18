# Build Wiki from Guide Folder
# This script syncs the guide folder to the GitHub wiki

param(
	[string]$GuideFolder = ".\guide",
	[string]$WikiPath = "wiki-build"
)

Write-Host "[*] Building wiki from guide folder..." -ForegroundColor Cyan

# Clone wiki
Write-Host "[1] Cloning wiki repository..." -ForegroundColor Yellow
if (Test-Path $WikiPath) {
	Remove-Item $WikiPath -Recurse -Force
}
git clone "https://github.com/afikpr123/CS2-SmartRestart.wiki.git" $WikiPath 2>&1

# Clear old guide files from wiki (keep Home.md and _Sidebar.md)
Push-Location $WikiPath
Write-Host "[2] Cleaning old guide files..." -ForegroundColor Yellow
Get-ChildItem -File -Filter "*.md" | Where-Object { $_.Name -ne "Home.md" -and $_.Name -ne "_Sidebar.md" } | Remove-Item -Force

# Copy guides from guide folder
Write-Host "[3] Copying guides to wiki..." -ForegroundColor Yellow
$guides = Get-ChildItem "$GuideFolder\*.md" -File | Where-Object { $_.Name -ne "README.md" }
foreach ($guide in $guides) {
	$newName = $guide.Name -replace '^\d+-', ''  # Remove numbers (01-, 02-, etc.)
	Copy-Item $guide.FullName -Destination $newName -Force
	Write-Host "  - Added: $newName"
}

# Create Home.md (wiki homepage)
Write-Host "[4] Creating Home.md..." -ForegroundColor Yellow
$homeContent = @"
# SmartRestart Wiki

Welcome to SmartRestart documentation!

## Quick Links
- [[Installation]]
- [[Configuration]]
- [[Scheduled-Restarts]]
- [[Language-Customization]]
- [[Discord-Integration]]
- [[Database-Integration]]
- [[Commands]]
- [[Troubleshooting]]
- [[Server-Startup-Issues]]

## Resources
- [GitHub Repository](https://github.com/afikpr123/CS2-SmartRestart)
- [Issues](https://github.com/afikpr123/CS2-SmartRestart/issues)
- [Discussions](https://github.com/afikpr123/CS2-SmartRestart/discussions)
"@
$homeContent | Out-File -FilePath "Home.md" -Encoding UTF8

# Create _Sidebar.md (wiki navigation)
Write-Host "[5] Creating _Sidebar.md..." -ForegroundColor Yellow
$sidebarContent = @"
## Documentation
- [[Home]]
- [[Installation]]
- [[Configuration]]
- [[Scheduled-Restarts]]
- [[Language-Customization]]
- [[Discord-Integration]]
- [[Database-Integration]]
- [[Commands]]
- [[Troubleshooting]]
- [[Server-Startup-Issues]]

## Resources
- [GitHub Repo](https://github.com/afikpr123/CS2-SmartRestart)
- [Issues](https://github.com/afikpr123/CS2-SmartRestart/issues)
- [Discussions](https://github.com/afikpr123/CS2-SmartRestart/discussions)
"@
$sidebarContent | Out-File -FilePath "_Sidebar.md" -Encoding UTF8

# Commit and push
Write-Host "[6] Pushing to wiki..." -ForegroundColor Yellow
git config user.email "github-actions[bot]@users.noreply.github.com"
git config user.name "Build Wiki"
git add .
git commit -m "Update wiki from guide folder"
git push origin master

Pop-Location

Write-Host "[OK] Wiki updated successfully!" -ForegroundColor Green
Write-Host "Visit: https://github.com/afikpr123/CS2-SmartRestart/wiki" -ForegroundColor Cyan

# Clean up
Remove-Item $WikiPath -Recurse -Force
Write-Host "[*] Cleaned up temporary files" -ForegroundColor Gray
