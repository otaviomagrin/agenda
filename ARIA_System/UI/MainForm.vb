Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports ARIA_Premium_System.Core
Imports ARIA_Premium_System.Voice
Imports ARIA_Premium_System.Security
Imports ARIA_Premium_System.Utils

Public Class MainForm
    Private WithEvents voiceEngine As VoiceEngine
    Private WithEvents costTracker As CostTracker
    Private WithEvents systemMonitor As SystemMonitor
    Private isDarkTheme As Boolean = True
    Private isRecording As Boolean = False
    Private currentWaveformData As Single() = {}
    Private waveformTimer As Timer

    Private configForm As ConfigForm
    Private meetingForm As MeetingForm

    Public Sub New()
        InitializeComponent()
        InitializeARIASystem()
        ApplyTheme()
    End Sub

    Private Sub InitializeARIASystem()
        Try
            ' Initialize core components
            voiceEngine = New VoiceEngine()
            costTracker = New CostTracker()
            systemMonitor = New SystemMonitor()

            ' Initialize waveform timer
            waveformTimer = New Timer()
            waveformTimer.Interval = 50 ' Update every 50ms for smooth animation
            AddHandler waveformTimer.Tick, AddressOf UpdateWaveform
            waveformTimer.Start()

            ' Update initial status
            UpdateVoiceStatus("System Ready", Color.Green)
            UpdateCostDisplay(0.0)
            UpdateSystemStatus()

        Catch ex As Exception
            MessageBox.Show($"Error initializing ARIA system: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ApplyTheme()
        If isDarkTheme Then
            ApplyDarkTheme()
        Else
            ApplyLightTheme()
        End If
    End Sub

    Private Sub ApplyDarkTheme()
        Me.BackColor = Color.FromArgb(32, 32, 32)
        Me.ForeColor = Color.White

        ' Update panels
        pnlHeader.BackColor = Color.FromArgb(24, 24, 24)
        pnlVoiceControls.BackColor = Color.FromArgb(40, 40, 40)
        pnlStatus.BackColor = Color.FromArgb(40, 40, 40)
        pnlCostTracking.BackColor = Color.FromArgb(40, 40, 40)
        pnlSystemMonitor.BackColor = Color.FromArgb(40, 40, 40)

        ' Update buttons
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Button Then
                Dim btn As Button = DirectCast(ctrl, Button)
                btn.BackColor = Color.FromArgb(60, 60, 60)
                btn.ForeColor = Color.White
                btn.FlatStyle = FlatStyle.Flat
                btn.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80)
            End If
        Next
    End Sub

    Private Sub ApplyLightTheme()
        Me.BackColor = Color.White
        Me.ForeColor = Color.Black

        ' Update panels
        pnlHeader.BackColor = Color.FromArgb(240, 240, 240)
        pnlVoiceControls.BackColor = Color.FromArgb(250, 250, 250)
        pnlStatus.BackColor = Color.FromArgb(250, 250, 250)
        pnlCostTracking.BackColor = Color.FromArgb(250, 250, 250)
        pnlSystemMonitor.BackColor = Color.FromArgb(250, 250, 250)

        ' Update buttons
        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Button Then
                Dim btn As Button = DirectCast(ctrl, Button)
                btn.BackColor = Color.FromArgb(225, 225, 225)
                btn.ForeColor = Color.Black
                btn.FlatStyle = FlatStyle.Standard
            End If
        Next
    End Sub

    Private Sub UpdateVoiceStatus(status As String, statusColor As Color)
        If InvokeRequired Then
            Invoke(Sub() UpdateVoiceStatus(status, statusColor))
            Return
        End If

        lblVoiceStatus.Text = status
        lblVoiceStatus.ForeColor = statusColor
        picVoiceIndicator.BackColor = statusColor
    End Sub

    Private Sub UpdateCostDisplay(totalCost As Double)
        If InvokeRequired Then
            Invoke(Sub() UpdateCostDisplay(totalCost))
            Return
        End If

        lblTotalCost.Text = $"${totalCost:F4}"

        ' Update cost breakdown if available
        If costTracker IsNot Nothing Then
            lblOpenAICost.Text = $"OpenAI: ${costTracker.OpenAICost:F4}"
            lblAzureCost.Text = $"Azure: ${costTracker.AzureCost:F4}"
            lblGoogleCost.Text = $"Google: ${costTracker.GoogleCost:F4}"
        End If
    End Sub

    Private Sub UpdateSystemStatus()
        If InvokeRequired Then
            Invoke(Sub() UpdateSystemStatus())
            Return
        End If

        Try
            ' Update AI provider status
            lblOpenAIStatus.Text = If(systemMonitor.IsOpenAIAvailable, "✓ Connected", "✗ Disconnected")
            lblOpenAIStatus.ForeColor = If(systemMonitor.IsOpenAIAvailable, Color.Green, Color.Red)

            lblAzureStatus.Text = If(systemMonitor.IsAzureAvailable, "✓ Connected", "✗ Disconnected")
            lblAzureStatus.ForeColor = If(systemMonitor.IsAzureAvailable, Color.Green, Color.Red)

            lblGoogleStatus.Text = If(systemMonitor.IsGoogleAvailable, "✓ Connected", "✗ Disconnected")
            lblGoogleStatus.ForeColor = If(systemMonitor.IsGoogleAvailable, Color.Green, Color.Red)

            ' Update system resources
            lblCPUUsage.Text = $"CPU: {systemMonitor.CPUUsage:F1}%"
            lblMemoryUsage.Text = $"Memory: {systemMonitor.MemoryUsage:F1}%"
            lblNetworkStatus.Text = If(systemMonitor.IsNetworkAvailable, "Network: Connected", "Network: Disconnected")

        Catch ex As Exception
            ' Handle monitoring errors gracefully
        End Try
    End Sub

    Private Sub UpdateWaveform()
        If voiceEngine IsNot Nothing AndAlso isRecording Then
            currentWaveformData = voiceEngine.GetCurrentWaveformData()
            pnlWaveform.Invalidate() ' Trigger repaint
        End If
    End Sub

    ' Event handlers for voice controls
    Private Sub btnStartListening_Click(sender As Object, e As EventArgs) Handles btnStartListening.Click
        Try
            If Not isRecording Then
                voiceEngine.StartListening()
                isRecording = True
                btnStartListening.Text = "Stop Listening"
                btnStartListening.BackColor = Color.Red
                UpdateVoiceStatus("Listening...", Color.Orange)
            Else
                voiceEngine.StopListening()
                isRecording = False
                btnStartListening.Text = "Start Listening"
                btnStartListening.BackColor = If(isDarkTheme, Color.FromArgb(60, 60, 60), Color.FromArgb(225, 225, 225))
                UpdateVoiceStatus("Ready", Color.Green)
            End If
        Catch ex As Exception
            MessageBox.Show($"Error controlling voice engine: {ex.Message}", "Voice Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnMuteToggle_Click(sender As Object, e As EventArgs) Handles btnMuteToggle.Click
        Try
            voiceEngine.ToggleMute()
            btnMuteToggle.Text = If(voiceEngine.IsMuted, "Unmute", "Mute")
            UpdateVoiceStatus(If(voiceEngine.IsMuted, "Muted", "Ready"), If(voiceEngine.IsMuted, Color.Gray, Color.Green))
        Catch ex As Exception
            MessageBox.Show($"Error toggling mute: {ex.Message}", "Voice Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub trackVolume_Scroll(sender As Object, e As EventArgs) Handles trackVolume.Scroll
        Try
            If voiceEngine IsNot Nothing Then
                voiceEngine.SetVolume(trackVolume.Value / 100.0)
                lblVolumeLevel.Text = $"Volume: {trackVolume.Value}%"
            End If
        Catch ex As Exception
            ' Handle volume control errors gracefully
        End Try
    End Sub

    ' Menu event handlers
    Private Sub btnConfiguration_Click(sender As Object, e As EventArgs) Handles btnConfiguration.Click
        If configForm Is Nothing OrElse configForm.IsDisposed Then
            configForm = New ConfigForm()
        End If
        configForm.ShowDialog(Me)
    End Sub

    Private Sub btnMeetings_Click(sender As Object, e As EventArgs) Handles btnMeetings.Click
        If meetingForm Is Nothing OrElse meetingForm.IsDisposed Then
            meetingForm = New MeetingForm()
        End If
        meetingForm.ShowDialog(Me)
    End Sub

    Private Sub btnThemeToggle_Click(sender As Object, e As EventArgs) Handles btnThemeToggle.Click
        isDarkTheme = Not isDarkTheme
        ApplyTheme()
        btnThemeToggle.Text = If(isDarkTheme, "Light Theme", "Dark Theme")
    End Sub

    Private Sub btnMinimize_Click(sender As Object, e As EventArgs) Handles btnMinimize.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
        Application.Exit()
    End Sub

    ' Waveform painting
    Private Sub pnlWaveform_Paint(sender As Object, e As PaintEventArgs) Handles pnlWaveform.Paint
        If currentWaveformData Is Nothing OrElse currentWaveformData.Length = 0 Then Return

        Dim g As Graphics = e.Graphics
        Dim width As Integer = pnlWaveform.Width
        Dim height As Integer = pnlWaveform.Height
        Dim centerY As Integer = height \ 2

        g.Clear(pnlWaveform.BackColor)

        Using pen As New Pen(If(isDarkTheme, Color.LimeGreen, Color.Blue), 2)
            For i As Integer = 0 To Math.Min(currentWaveformData.Length - 2, width - 1)
                Dim x1 As Integer = i
                Dim y1 As Integer = centerY + CInt(currentWaveformData(i) * centerY * 0.8)
                Dim x2 As Integer = i + 1
                Dim y2 As Integer = centerY + CInt(currentWaveformData(i + 1) * centerY * 0.8)

                g.DrawLine(pen, x1, y1, x2, y2)
            Next
        End Using
    End Sub

    ' Voice engine event handlers
    Private Sub voiceEngine_VoiceDetected(sender As Object, e As VoiceDetectedEventArgs) Handles voiceEngine.VoiceDetected
        UpdateVoiceStatus("Voice Detected", Color.Yellow)
    End Sub

    Private Sub voiceEngine_TranscriptionReceived(sender As Object, e As TranscriptionEventArgs) Handles voiceEngine.TranscriptionReceived
        If InvokeRequired Then
            Invoke(Sub() txtTranscription.AppendText($"[{DateTime.Now:HH:mm:ss}] {e.Text}{Environment.NewLine}"))
        Else
            txtTranscription.AppendText($"[{DateTime.Now:HH:mm:ss}] {e.Text}{Environment.NewLine}")
        End If
    End Sub

    Private Sub voiceEngine_ErrorOccurred(sender As Object, e As ErrorEventArgs) Handles voiceEngine.ErrorOccurred
        UpdateVoiceStatus($"Error: {e.GetException().Message}", Color.Red)
    End Sub

    ' Cost tracker event handlers
    Private Sub costTracker_CostUpdated(sender As Object, e As CostUpdateEventArgs) Handles costTracker.CostUpdated
        UpdateCostDisplay(e.TotalCost)
    End Sub

    ' System monitor event handlers
    Private Sub systemMonitor_StatusChanged(sender As Object, e As SystemStatusEventArgs) Handles systemMonitor.StatusChanged
        UpdateSystemStatus()
    End Sub

    ' Form events
    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            waveformTimer?.Stop()
            voiceEngine?.Dispose()
            costTracker?.Dispose()
            systemMonitor?.Dispose()
        Catch ex As Exception
            ' Handle cleanup errors gracefully
        End Try
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Text = "ARIA Premium System v1.0"
        Me.Icon = My.Resources.AppIcon

        ' Start system monitoring
        systemMonitor?.StartMonitoring()

        ' Initialize status update timer
        Dim statusTimer As New Timer()
        statusTimer.Interval = 5000 ' Update every 5 seconds
        AddHandler statusTimer.Tick, Sub() UpdateSystemStatus()
        statusTimer.Start()
    End Sub
End Class