# Use the 'Full' build from Gyan.dev which is guaranteed to include ddagrab and wasapi.
# Since it's in .7z format, we will download a portable 7-Zip (7zr.exe) first to extract it.
$url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z"
$szUrl = "https://www.7-zip.org/a/7zr.exe"
$szExe = "7zr.exe"
$szArchive = "ffmpeg.7z"
$destFolder = "ffmpeg_temp"

Write-Host "Downloading portable 7-Zip for extraction..." -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri $szUrl -OutFile $szExe -ErrorAction Stop
} catch {
    Write-Host "Failed to download 7-Zip. Please check your internet connection." -ForegroundColor Red
    Pause
    exit
}

Write-Host "Downloading FFmpeg FULL build (this may take a minute)..." -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri $url -OutFile $szArchive -ErrorAction Stop
} catch {
    Write-Host "Failed to download FFmpeg FULL. Please check your internet connection." -ForegroundColor Red
    Pause
    exit
}

Write-Host "Extracting FFmpeg FULL build using 7-Zip..." -ForegroundColor Cyan
if (Test-Path $szArchive) {
    # Extract using 7zr.exe: x = extract, -o = output directory, -y = assume yes to all
    # Using 'x' to maintain folder structure
    .\7zr.exe x $szArchive "-offmpeg_temp" -y | Out-Null

    # Find ffmpeg.exe inside the extracted folder
    $ffmpegExe = Get-ChildItem -Path $destFolder -Filter "ffmpeg.exe" -Recurse | Select-Object -First 1

    if ($ffmpegExe) {
        Copy-Item -Path $ffmpegExe.FullName -Destination "." -Force
        Write-Host "FFmpeg FULL successfully installed!" -ForegroundColor Green
    } else {
        Write-Host "Failed to find ffmpeg.exe in the extracted folder." -ForegroundColor Red
    }

    # Cleanup
    Remove-Item -Path $szExe -Force
    Remove-Item -Path $szArchive -Force
    Remove-Item -Path $destFolder -Recurse -Force
} else {
    Write-Host "Download failed - ffmpeg.7z not found." -ForegroundColor Red
}

Write-Host "Setup complete. You can now run build.bat to create the recorder." -ForegroundColor Green
Pause
