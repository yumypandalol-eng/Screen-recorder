# Super Light Recorder (for Windows 10 64-bit)

A high-performance, minimal-resource screen recorder designed for gaming (like Minecraft) on low-end hardware.

## Features:
- **Intel QuickSync Acceleration**: Uses your Intel HD 610 GPU for hardware-accelerated H.264 encoding to keep your CPU free for games.
- **GDI Capture (gdigrab)**: A highly compatible way to capture frames on Windows 10.
- **720p 60fps**: Optimized for stable high frame rates.
- **System Audio**: Captures game sounds directly.

## Installation Instructions:

### 1. Setup FFmpeg
FFmpeg is the "engine" that powers this recorder. I've provided a script to download it automatically.
- Right-click `setup.ps1` and select **Run with PowerShell**.
- This will download and place `ffmpeg.exe` in this folder.

### 2. Build the App
You don't need Visual Studio. I've provided a simple build script.
- Double-click `build.bat`.
- This will create a file named `Recorder.exe`.

### 3. Start Recording
- Run `Recorder.exe`.
- Click **Start Recording** to begin.
- Click **Stop Recording** when you are done.
- Your recordings will be saved as `.mp4` files in the same folder.

## Troubleshooting:
- If `setup.ps1` doesn't run, open PowerShell as Administrator and run: `Set-ExecutionPolicy RemoteSigned`
- Make sure you are using Windows 10 64-bit with an Intel GPU for the best performance.
