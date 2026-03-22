using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class RecorderApp : Form
{
    private Button btnStart;
    private Button btnStop;
    private CheckBox chkAudio;
    private Label lblStatus;
    private Process ffmpegProcess;
    private string outputFilePath;
    private string logFilePath;

    public RecorderApp()
    {
        this.Text = "Super Light Recorder";
        this.Size = new Size(300, 220);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MaximizeBox = false;

        btnStart = new Button { Text = "Start Recording", Left = 50, Top = 20, Width = 200, Height = 30 };
        btnStop = new Button { Text = "Stop Recording", Left = 50, Top = 60, Width = 200, Height = 30, Enabled = false };
        chkAudio = new CheckBox { Text = "Record System Audio", Left = 50, Top = 100, Width = 200, Checked = true };
        lblStatus = new Label { Text = "Ready", Left = 10, Top = 140, Width = 280, TextAlign = ContentAlignment.MiddleCenter };

        btnStart.Click += BtnStart_Click;
        btnStop.Click += BtnStop_Click;

        this.Controls.Add(btnStart);
        this.Controls.Add(btnStop);
        this.Controls.Add(chkAudio);
        this.Controls.Add(lblStatus);

        this.FormClosing += (s, e) => {
            if (ffmpegProcess != null && !ffmpegProcess.HasExited)
            {
                StopFfmpeg();
            }
        };
    }

    private void BtnStart_Click(object sender, EventArgs e)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string ffmpegPath = Path.Combine(baseDir, "ffmpeg.exe");

        if (!File.Exists(ffmpegPath))
        {
            MessageBox.Show("ffmpeg.exe not found!\n\nPlease right-click 'setup.ps1' and select 'Run with PowerShell' first.", "Error");
            return;
        }

        string fileName = "recording_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4";
        outputFilePath = Path.Combine(baseDir, fileName);
        logFilePath = Path.Combine(baseDir, "last_recording_log.txt");

        // Clean up old log
        if (File.Exists(logFilePath)) try { File.Delete(logFilePath); } catch {}

        // FFmpeg command for 720p 60fps using Intel QuickSync (h264_qsv)
        // ddagrab is the fastest way to capture screen on Windows 10
        // -f wasapi -i default captures the default recording device (often Mic).
        // Note: Capturing system audio loopback automatically is hard without knowing the device name.
        string audioArgs = chkAudio.Checked ? "-f wasapi -i default " : "";

        // We use -vf "scale=1280:720,format=nv12" because QSV requires nv12 input
        // Added -loglevel info to help debugging
        string args = string.Format("-loglevel info -f ddagrab -framerate 60 -i desktop {0}-c:v h264_qsv -global_quality 25 -vf \"scale=1280:720,format=nv12\" -c:a aac -b:a 128k -y \"{1}\"",
            audioArgs, outputFilePath);

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardError = true // FFmpeg logs to stderr
        };

        try
        {
            ffmpegProcess = new Process { StartInfo = psi };

            // Log FFmpeg output to a file in the background
            ffmpegProcess.ErrorDataReceived += (s, ev) => {
                if (!string.IsNullOrEmpty(ev.Data))
                {
                    File.AppendAllText(logFilePath, ev.Data + Environment.NewLine);
                }
            };

            ffmpegProcess.Start();
            ffmpegProcess.BeginErrorReadLine();

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            chkAudio.Enabled = false;
            lblStatus.Text = "Recording... (Saving to same folder)";
            lblStatus.ForeColor = Color.Red;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to start FFmpeg: " + ex.Message);
        }
    }

    private void BtnStop_Click(object sender, EventArgs e)
    {
        StopFfmpeg();

        btnStart.Enabled = true;
        btnStop.Enabled = false;
        chkAudio.Enabled = true;

        if (File.Exists(outputFilePath))
        {
            lblStatus.Text = "Saved: " + Path.GetFileName(outputFilePath);
            lblStatus.ForeColor = Color.Green;
        }
        else
        {
            lblStatus.Text = "Error: File not created. Check log.";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show("Recording file was not created. Check 'last_recording_log.txt' for errors.", "Recording Failed");
        }
    }

    private void StopFfmpeg()
    {
        if (ffmpegProcess != null && !ffmpegProcess.HasExited)
        {
            try {
                // Send 'q' to FFmpeg to stop it gracefully
                ffmpegProcess.StandardInput.WriteLine("q");
                if (!ffmpegProcess.WaitForExit(5000))
                {
                    ffmpegProcess.Kill();
                }
            } catch {
                try { ffmpegProcess.Kill(); } catch {}
            }
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new RecorderApp());
    }
}
