param(
	[string]$Version = "v1.0.0"
)

$ghExe = "C:\Program Files\GitHub CLI\gh.exe"
$releaseFolder = "bin/Release/SmartRestart"
$zipFile = "SmartRestart-$Version.zip"

# Create ZIP from compiled plugin folder
Write-Host "[*] Creating ZIP file from compiled plugin..." -ForegroundColor Cyan
if (Test-Path $zipFile) {
	Remove-Item $zipFile -Force
}

Compress-Archive -Path $releaseFolder -DestinationPath $zipFile -Force

# Delete old SmartRestart.dll from release
Write-Host "[!] Removing old SmartRestart.dll asset from release..." -ForegroundColor Yellow
&$ghExe release delete-asset $Version SmartRestart.dll --yes 2>&1 | Out-Null

# Upload new ZIP to release
Write-Host "[+] Uploading ZIP to release..." -ForegroundColor Cyan
&$ghExe release upload $Version $zipFile --clobber

Write-Host "[OK] Release updated successfully!" -ForegroundColor Green
Write-Host "[*] Uploaded: $zipFile" -ForegroundColor Green
