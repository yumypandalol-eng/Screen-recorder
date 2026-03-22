using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
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

    private string GetFfmpegProbeOutput(string ffmpegPath, string args)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        try {
            using (Process p = Process.Start(psi)) {
                StringBuilder sb = new StringBuilder();
                p.ErrorDataReceived += (s, e) => { if (e.Data != null) sb.AppendLine(e.Data); };
                p.OutputDataReceived += (s, e) => { if (e.Data != null) sb.AppendLine(e.Data); };
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit(5000);
                return sb.ToString();
            }
        } catch { return ""; }
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

        // PROBE FOR BEST CAPTURE METHOD
        string formats = GetFfmpegProbeOutput(ffmpegPath, "-formats");
        string devices = GetFfmpegProbeOutput(ffmpegPath, "-devices");

        File.AppendAllText(logFilePath, "--- PROBE START ---" + Environment.NewLine);

        string videoInput = "gdigrab"; // DEFAULT SAFEST
        string videoInputArgs = "-f gdigrab -framerate 60 -i desktop";

        if (devices.Contains("ddagrab")) {
            videoInput = "ddagrab";
            videoInputArgs = "-f ddagrab -framerate 60 -i desktop";
        }

        File.AppendAllText(logFilePath, "Selected Video Input: " + videoInput + Environment.NewLine);

        string audioInputArgs = "";
        if (chkAudio.Checked) {
            if (devices.Contains("wasapi")) {
                // To capture system audio loopback on Windows 10, WASAPI needs the 'out_default' target.
                audioInputArgs = "-f wasapi -i out_default ";
            } else if (devices.Contains("dshow")) {
                audioInputArgs = "-f dshow -i audio=\"virtual-audio-capturer\" "; // Very long shot
            }
        }

        File.AppendAllText(logFilePath, "Selected Audio Args: " + audioInputArgs + Environment.NewLine);

        // Encoder: Always try QSV first as requested for performance
        string encoder = "h264_qsv";
        string encoderArgs = "-c:v h264_qsv -global_quality 25";
        string encoders = GetFfmpegProbeOutput(ffmpegPath, "-encoders");
        if (!encoders.Contains("h264_qsv")) {
            encoder = "libx264";
            encoderArgs = "-c:v libx264 -preset ultrafast -crf 23";
        }

        File.AppendAllText(logFilePath, "Selected Encoder: " + encoder + Environment.NewLine);
        File.AppendAllText(logFilePath, "--- PROBE END ---" + Environment.NewLine + Environment.NewLine);

        // FINAL COMMAND
        string args = string.Format("-loglevel info {0} {1}{2} -vf \"scale=1280:720,format=nv12\" -c:a aac -b:a 128k -y \"{3}\"",
            videoInputArgs, audioInputArgs, encoderArgs, outputFilePath);

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardError = true
        };

        try
        {
            ffmpegProcess = new Process { StartInfo = psi };
            ffmpegProcess.ErrorDataReceived += (s, ev) => {
                if (!string.IsNullOrEmpty(ev.Data)) {
                    for (int i = 0; i < 3; i++) {
                        try { File.AppendAllText(logFilePath, ev.Data + Environment.NewLine); break; }
                        catch { System.Threading.Thread.Sleep(5); }
                    }
                }
            };

            ffmpegProcess.Start();
            ffmpegProcess.BeginErrorReadLine();

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            chkAudio.Enabled = false;
            lblStatus.Text = "Recording... (" + videoInput + " + " + encoder + ")";
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
        System.Threading.Thread.Sleep(1000);
        if (File.Exists(outputFilePath)) {
            lblStatus.Text = "Saved: " + Path.GetFileName(outputFilePath);
            lblStatus.ForeColor = Color.Green;
        } else {
            lblStatus.Text = "Error: File not created. Check log.";
            lblStatus.ForeColor = Color.Red;
            MessageBox.Show("Recording file was not created.\n\nPlease check 'last_recording_log.txt' for error details.", "Recording Failed");
        }
    }

    private void StopFfmpeg()
    {
        if (ffmpegProcess != null && !ffmpegProcess.HasExited) {
            try {
                ffmpegProcess.StandardInput.WriteLine("q");
                if (!ffmpegProcess.WaitForExit(5000)) ffmpegProcess.Kill();
            } catch { try { ffmpegProcess.Kill(); } catch {} }
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
