# Use the BtbN GitHub release as it's a comprehensive 'master' build in .zip format.
# These builds include all required modules like wasapi, ddagrab, and hardware encoders.
$url = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"
$zipFile = "ffmpeg.zip"
$destFolder = "ffmpeg_temp"

Write-Host "Downloading high-performance FFmpeg from GitHub (BtbN Builds)..." -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri $url -OutFile $zipFile -ErrorAction Stop
} catch {
    Write-Host "Failed to download FFmpeg. Please check your internet connection." -ForegroundColor Red
    Pause
    exit
}

Write-Host "Extracting FFmpeg (this may take a minute)..." -ForegroundColor Cyan
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
