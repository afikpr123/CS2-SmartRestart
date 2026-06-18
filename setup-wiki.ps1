param(
	[string]$WikiPath = "wiki-temp"
)

Write-Host "[*] Setting up GitHub Wiki..." -ForegroundColor Cyan

# Step 1: Clone empty wiki repository
Write-Host "[1] Cloning wiki repository..." -ForegroundColor Yellow
if (Test-Path $WikiPath) {
	Remove-Item $WikiPath -Recurse -Force
}
git clone "https://github.com/afikpr123/CS2-SmartRestart.wiki.git" $WikiPath

# Step 2: Copy guide files
Write-Host "[2] Copying guide files..." -ForegroundColor Yellow
Push-Location $WikiPath

# Create Home.md (wiki homepage)
$homeContent = @"
# SmartRestart Wiki

Welcome to SmartRestart documentation!

## Quick Links
- [[Installation]]
- [[Configuration]]
- [[Language-Customization]]
- [[Discord-Integration]]
- [[Commands]]
- [[Troubleshooting]]

## Resources
- [GitHub Repository](https://github.com/afikpr123/CS2-SmartRestart)
- [Issues](https://github.com/afikpr123/CS2-SmartRestart/issues)
- [Discussions](https://github.com/afikpr123/CS2-SmartRestart/discussions)
"@
$homeContent | Out-File -FilePath "Home.md" -Encoding UTF8

# Copy and rename guide files
Write-Host "[3] Adding guide files to wiki..." -ForegroundColor Yellow
$guides = Get-ChildItem "..\guide\*.md" -File
foreach ($guide in $guides) {
	$newName = $guide.Name -replace '^\d+-', ''  # Remove numbers (01-, 02-, etc.)
	Copy-Item $guide.FullName -Destination $newName
	Write-Host "  - Added: $newName"
}

# Step 3: Commit and push
Write-Host "[4] Pushing to wiki..." -ForegroundColor Yellow
git config user.email "github-actions[bot]@users.noreply.github.com"
git config user.name "GitHub Actions"
git add .
git commit -m "Initialize wiki with guides"
git push origin master

Pop-Location

Write-Host "[OK] Wiki setup complete!" -ForegroundColor Green
Write-Host "Visit: https://github.com/afikpr123/CS2-SmartRestart/wiki" -ForegroundColor Cyan
