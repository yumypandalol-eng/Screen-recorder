# Use the 'essentials' zip link as it's the most reliable for direct download via PowerShell
# Full builds are usually .7z which PowerShell cannot extract by default.
$url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
$zipFile = "ffmpeg.zip"
$destFolder = "ffmpeg_temp"

Write-Host "Downloading FFmpeg Essentials (this is the most stable link)..." -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri $url -OutFile $zipFile -ErrorAction Stop
} catch {
    Write-Host "Failed to download FFmpeg from Gyan.dev. Attempting fallback link..." -ForegroundColor Yellow
    $fallbackUrl = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"
    try {
        Invoke-WebRequest -Uri $fallbackUrl -OutFile $zipFile -ErrorAction Stop
    } catch {
        Write-Host "Both download links failed. Please check your internet connection." -ForegroundColor Red
        Pause
        exit
    }
}

Write-Host "Extracting FFmpeg (this may take a few seconds)..." -ForegroundColor Cyan
if (Test-Path $zipFile) {
    Expand-Archive -Path $zipFile -DestinationPath $destFolder -Force

    # Find ffmpeg.exe inside the extracted folder
    $ffmpegExe = Get-ChildItem -Path $destFolder -Filter "ffmpeg.exe" -Recurse | Select-Object -First 1

    if ($ffmpegExe) {
        Copy-Item -Path $ffmpegExe.FullName -Destination "." -Force
        Write-Host "FFmpeg successfully installed!" -ForegroundColor Green
    } else {
        Write-Host "Failed to find ffmpeg.exe in the downloaded archive." -ForegroundColor Red
    }

    # Cleanup
    Remove-Item -Path $zipFile -Force
    Remove-Item -Path $destFolder -Recurse -Force
} else {
    Write-Host "Download failed - ffmpeg.zip not found." -ForegroundColor Red
}

Write-Host "Setup complete. You can now run build.bat to create the recorder." -ForegroundColor Green
Pause
