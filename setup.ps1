$url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
$zipFile = "ffmpeg.zip"
$destFolder = "ffmpeg_temp"

Write-Host "Downloading FFmpeg (this might take a minute)..." -ForegroundColor Cyan
Invoke-WebRequest -Uri $url -OutFile $zipFile

Write-Host "Extracting FFmpeg..." -ForegroundColor Cyan
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

Write-Host "Setup complete. You can now run build.bat to create the recorder." -ForegroundColor Green
Pause
