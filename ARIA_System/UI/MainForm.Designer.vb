<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.pnlHeader = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.btnMinimize = New System.Windows.Forms.Button()
        Me.btnExit = New System.Windows.Forms.Button()
        Me.btnThemeToggle = New System.Windows.Forms.Button()
        Me.pnlMain = New System.Windows.Forms.Panel()
        Me.pnlVoiceControls = New System.Windows.Forms.Panel()
        Me.lblVoiceControlsTitle = New System.Windows.Forms.Label()
        Me.btnStartListening = New System.Windows.Forms.Button()
        Me.btnMuteToggle = New System.Windows.Forms.Button()
        Me.lblVolumeLabel = New System.Windows.Forms.Label()
        Me.trackVolume = New System.Windows.Forms.TrackBar()
        Me.lblVolumeLevel = New System.Windows.Forms.Label()
        Me.pnlWaveform = New System.Windows.Forms.Panel()
        Me.pnlStatus = New System.Windows.Forms.Panel()
        Me.lblStatusTitle = New System.Windows.Forms.Label()
        Me.lblVoiceStatusLabel = New System.Windows.Forms.Label()
        Me.lblVoiceStatus = New System.Windows.Forms.Label()
        Me.picVoiceIndicator = New System.Windows.Forms.PictureBox()
        Me.txtTranscription = New System.Windows.Forms.TextBox()
        Me.lblTranscriptionLabel = New System.Windows.Forms.Label()
        Me.pnlCostTracking = New System.Windows.Forms.Panel()
        Me.lblCostTitle = New System.Windows.Forms.Label()
        Me.lblTotalCostLabel = New System.Windows.Forms.Label()
        Me.lblTotalCost = New System.Windows.Forms.Label()
        Me.lblOpenAICost = New System.Windows.Forms.Label()
        Me.lblAzureCost = New System.Windows.Forms.Label()
        Me.lblGoogleCost = New System.Windows.Forms.Label()
        Me.btnCostReset = New System.Windows.Forms.Button()
        Me.pnlSystemMonitor = New System.Windows.Forms.Panel()
        Me.lblSystemTitle = New System.Windows.Forms.Label()
        Me.lblAIProvidersLabel = New System.Windows.Forms.Label()
        Me.lblOpenAIStatus = New System.Windows.Forms.Label()
        Me.lblAzureStatus = New System.Windows.Forms.Label()
        Me.lblGoogleStatus = New System.Windows.Forms.Label()
        Me.lblSystemResourcesLabel = New System.Windows.Forms.Label()
        Me.lblCPUUsage = New System.Windows.Forms.Label()
        Me.lblMemoryUsage = New System.Windows.Forms.Label()
        Me.lblNetworkStatus = New System.Windows.Forms.Label()
        Me.pnlActions = New System.Windows.Forms.Panel()
        Me.btnConfiguration = New System.Windows.Forms.Button()
        Me.btnMeetings = New System.Windows.Forms.Button()
        Me.btnViewLogs = New System.Windows.Forms.Button()
        Me.btnAbout = New System.Windows.Forms.Button()
        Me.pnlHeader.SuspendLayout()
        Me.pnlMain.SuspendLayout()
        Me.pnlVoiceControls.SuspendLayout()
        CType(Me.trackVolume, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlStatus.SuspendLayout()
        CType(Me.picVoiceIndicator, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlCostTracking.SuspendLayout()
        Me.pnlSystemMonitor.SuspendLayout()
        Me.pnlActions.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlHeader
        '
        Me.pnlHeader.Controls.Add(Me.lblTitle)
        Me.pnlHeader.Controls.Add(Me.lblVersion)
        Me.pnlHeader.Controls.Add(Me.btnMinimize)
        Me.pnlHeader.Controls.Add(Me.btnExit)
        Me.pnlHeader.Controls.Add(Me.btnThemeToggle)
        Me.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Size = New System.Drawing.Size(1200, 60)
        Me.pnlHeader.TabIndex = 0
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.Location = New System.Drawing.Point(12, 15)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(278, 32)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "ARIA Premium System"
        '
        'lblVersion
        '
        Me.lblVersion.AutoSize = True
        Me.lblVersion.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVersion.Location = New System.Drawing.Point(296, 25)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(30, 15)
        Me.lblVersion.TabIndex = 1
        Me.lblVersion.Text = "v1.0"
        '
        'btnMinimize
        '
        Me.btnMinimize.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnMinimize.Location = New System.Drawing.Point(1088, 12)
        Me.btnMinimize.Name = "btnMinimize"
        Me.btnMinimize.Size = New System.Drawing.Size(30, 30)
        Me.btnMinimize.TabIndex = 2
        Me.btnMinimize.Text = "_"
        Me.btnMinimize.UseVisualStyleBackColor = True
        '
        'btnExit
        '
        Me.btnExit.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnExit.Location = New System.Drawing.Point(1124, 12)
        Me.btnExit.Name = "btnExit"
        Me.btnExit.Size = New System.Drawing.Size(30, 30)
        Me.btnExit.TabIndex = 3
        Me.btnExit.Text = "✕"
        Me.btnExit.UseVisualStyleBackColor = True
        '
        'btnThemeToggle
        '
        Me.btnThemeToggle.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnThemeToggle.Location = New System.Drawing.Point(950, 15)
        Me.btnThemeToggle.Name = "btnThemeToggle"
        Me.btnThemeToggle.Size = New System.Drawing.Size(100, 30)
        Me.btnThemeToggle.TabIndex = 4
        Me.btnThemeToggle.Text = "Light Theme"
        Me.btnThemeToggle.UseVisualStyleBackColor = True
        '
        'pnlMain
        '
        Me.pnlMain.Controls.Add(Me.pnlVoiceControls)
        Me.pnlMain.Controls.Add(Me.pnlStatus)
        Me.pnlMain.Controls.Add(Me.pnlCostTracking)
        Me.pnlMain.Controls.Add(Me.pnlSystemMonitor)
        Me.pnlMain.Controls.Add(Me.pnlActions)
        Me.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMain.Location = New System.Drawing.Point(0, 60)
        Me.pnlMain.Name = "pnlMain"
        Me.pnlMain.Padding = New System.Windows.Forms.Padding(10)
        Me.pnlMain.Size = New System.Drawing.Size(1200, 640)
        Me.pnlMain.TabIndex = 1
        '
        'pnlVoiceControls
        '
        Me.pnlVoiceControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlVoiceControls.Controls.Add(Me.lblVoiceControlsTitle)
        Me.pnlVoiceControls.Controls.Add(Me.btnStartListening)
        Me.pnlVoiceControls.Controls.Add(Me.btnMuteToggle)
        Me.pnlVoiceControls.Controls.Add(Me.lblVolumeLabel)
        Me.pnlVoiceControls.Controls.Add(Me.trackVolume)
        Me.pnlVoiceControls.Controls.Add(Me.lblVolumeLevel)
        Me.pnlVoiceControls.Controls.Add(Me.pnlWaveform)
        Me.pnlVoiceControls.Location = New System.Drawing.Point(15, 15)
        Me.pnlVoiceControls.Name = "pnlVoiceControls"
        Me.pnlVoiceControls.Size = New System.Drawing.Size(400, 300)
        Me.pnlVoiceControls.TabIndex = 0
        '
        'lblVoiceControlsTitle
        '
        Me.lblVoiceControlsTitle.AutoSize = True
        Me.lblVoiceControlsTitle.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVoiceControlsTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblVoiceControlsTitle.Name = "lblVoiceControlsTitle"
        Me.lblVoiceControlsTitle.Size = New System.Drawing.Size(124, 21)
        Me.lblVoiceControlsTitle.TabIndex = 0
        Me.lblVoiceControlsTitle.Text = "Voice Controls"
        '
        'btnStartListening
        '
        Me.btnStartListening.Font = New System.Drawing.Font("Segoe UI", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnStartListening.Location = New System.Drawing.Point(15, 45)
        Me.btnStartListening.Name = "btnStartListening"
        Me.btnStartListening.Size = New System.Drawing.Size(150, 40)
        Me.btnStartListening.TabIndex = 1
        Me.btnStartListening.Text = "Start Listening"
        Me.btnStartListening.UseVisualStyleBackColor = True
        '
        'btnMuteToggle
        '
        Me.btnMuteToggle.Location = New System.Drawing.Point(180, 45)
        Me.btnMuteToggle.Name = "btnMuteToggle"
        Me.btnMuteToggle.Size = New System.Drawing.Size(80, 40)
        Me.btnMuteToggle.TabIndex = 2
        Me.btnMuteToggle.Text = "Mute"
        Me.btnMuteToggle.UseVisualStyleBackColor = True
        '
        'lblVolumeLabel
        '
        Me.lblVolumeLabel.AutoSize = True
        Me.lblVolumeLabel.Location = New System.Drawing.Point(15, 100)
        Me.lblVolumeLabel.Name = "lblVolumeLabel"
        Me.lblVolumeLabel.Size = New System.Drawing.Size(47, 13)
        Me.lblVolumeLabel.TabIndex = 3
        Me.lblVolumeLabel.Text = "Volume:"
        '
        'trackVolume
        '
        Me.trackVolume.Location = New System.Drawing.Point(15, 115)
        Me.trackVolume.Maximum = 100
        Me.trackVolume.Name = "trackVolume"
        Me.trackVolume.Size = New System.Drawing.Size(250, 45)
        Me.trackVolume.TabIndex = 4
        Me.trackVolume.TickFrequency = 10
        Me.trackVolume.Value = 50
        '
        'lblVolumeLevel
        '
        Me.lblVolumeLevel.AutoSize = True
        Me.lblVolumeLevel.Location = New System.Drawing.Point(275, 125)
        Me.lblVolumeLevel.Name = "lblVolumeLevel"
        Me.lblVolumeLevel.Size = New System.Drawing.Size(67, 13)
        Me.lblVolumeLevel.TabIndex = 5
        Me.lblVolumeLevel.Text = "Volume: 50%"
        '
        'pnlWaveform
        '
        Me.pnlWaveform.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlWaveform.Location = New System.Drawing.Point(15, 170)
        Me.pnlWaveform.Name = "pnlWaveform"
        Me.pnlWaveform.Size = New System.Drawing.Size(365, 120)
        Me.pnlWaveform.TabIndex = 6
        '
        'pnlStatus
        '
        Me.pnlStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlStatus.Controls.Add(Me.lblStatusTitle)
        Me.pnlStatus.Controls.Add(Me.lblVoiceStatusLabel)
        Me.pnlStatus.Controls.Add(Me.lblVoiceStatus)
        Me.pnlStatus.Controls.Add(Me.picVoiceIndicator)
        Me.pnlStatus.Controls.Add(Me.txtTranscription)
        Me.pnlStatus.Controls.Add(Me.lblTranscriptionLabel)
        Me.pnlStatus.Location = New System.Drawing.Point(430, 15)
        Me.pnlStatus.Name = "pnlStatus"
        Me.pnlStatus.Size = New System.Drawing.Size(400, 300)
        Me.pnlStatus.TabIndex = 1
        '
        'lblStatusTitle
        '
        Me.lblStatusTitle.AutoSize = True
        Me.lblStatusTitle.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatusTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblStatusTitle.Name = "lblStatusTitle"
        Me.lblStatusTitle.Size = New System.Drawing.Size(108, 21)
        Me.lblStatusTitle.TabIndex = 0
        Me.lblStatusTitle.Text = "Voice Status"
        '
        'lblVoiceStatusLabel
        '
        Me.lblVoiceStatusLabel.AutoSize = True
        Me.lblVoiceStatusLabel.Location = New System.Drawing.Point(15, 45)
        Me.lblVoiceStatusLabel.Name = "lblVoiceStatusLabel"
        Me.lblVoiceStatusLabel.Size = New System.Drawing.Size(40, 13)
        Me.lblVoiceStatusLabel.TabIndex = 1
        Me.lblVoiceStatusLabel.Text = "Status:"
        '
        'lblVoiceStatus
        '
        Me.lblVoiceStatus.AutoSize = True
        Me.lblVoiceStatus.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVoiceStatus.ForeColor = System.Drawing.Color.Green
        Me.lblVoiceStatus.Location = New System.Drawing.Point(90, 45)
        Me.lblVoiceStatus.Name = "lblVoiceStatus"
        Me.lblVoiceStatus.Size = New System.Drawing.Size(39, 15)
        Me.lblVoiceStatus.TabIndex = 2
        Me.lblVoiceStatus.Text = "Ready"
        '
        'picVoiceIndicator
        '
        Me.picVoiceIndicator.BackColor = System.Drawing.Color.Green
        Me.picVoiceIndicator.Location = New System.Drawing.Point(65, 45)
        Me.picVoiceIndicator.Name = "picVoiceIndicator"
        Me.picVoiceIndicator.Size = New System.Drawing.Size(15, 15)
        Me.picVoiceIndicator.TabIndex = 3
        Me.picVoiceIndicator.TabStop = False
        '
        'txtTranscription
        '
        Me.txtTranscription.Location = New System.Drawing.Point(15, 90)
        Me.txtTranscription.Multiline = True
        Me.txtTranscription.Name = "txtTranscription"
        Me.txtTranscription.ReadOnly = True
        Me.txtTranscription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtTranscription.Size = New System.Drawing.Size(365, 200)
        Me.txtTranscription.TabIndex = 4
        '
        'lblTranscriptionLabel
        '
        Me.lblTranscriptionLabel.AutoSize = True
        Me.lblTranscriptionLabel.Location = New System.Drawing.Point(15, 70)
        Me.lblTranscriptionLabel.Name = "lblTranscriptionLabel"
        Me.lblTranscriptionLabel.Size = New System.Drawing.Size(75, 13)
        Me.lblTranscriptionLabel.TabIndex = 5
        Me.lblTranscriptionLabel.Text = "Transcription:"
        '
        'pnlCostTracking
        '
        Me.pnlCostTracking.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlCostTracking.Controls.Add(Me.lblCostTitle)
        Me.pnlCostTracking.Controls.Add(Me.lblTotalCostLabel)
        Me.pnlCostTracking.Controls.Add(Me.lblTotalCost)
        Me.pnlCostTracking.Controls.Add(Me.lblOpenAICost)
        Me.pnlCostTracking.Controls.Add(Me.lblAzureCost)
        Me.pnlCostTracking.Controls.Add(Me.lblGoogleCost)
        Me.pnlCostTracking.Controls.Add(Me.btnCostReset)
        Me.pnlCostTracking.Location = New System.Drawing.Point(845, 15)
        Me.pnlCostTracking.Name = "pnlCostTracking"
        Me.pnlCostTracking.Size = New System.Drawing.Size(330, 300)
        Me.pnlCostTracking.TabIndex = 2
        '
        'lblCostTitle
        '
        Me.lblCostTitle.AutoSize = True
        Me.lblCostTitle.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCostTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblCostTitle.Name = "lblCostTitle"
        Me.lblCostTitle.Size = New System.Drawing.Size(110, 21)
        Me.lblCostTitle.TabIndex = 0
        Me.lblCostTitle.Text = "Cost Tracking"
        '
        'lblTotalCostLabel
        '
        Me.lblTotalCostLabel.AutoSize = True
        Me.lblTotalCostLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTotalCostLabel.Location = New System.Drawing.Point(15, 45)
        Me.lblTotalCostLabel.Name = "lblTotalCostLabel"
        Me.lblTotalCostLabel.Size = New System.Drawing.Size(79, 19)
        Me.lblTotalCostLabel.TabIndex = 1
        Me.lblTotalCostLabel.Text = "Total Cost:"
        '
        'lblTotalCost
        '
        Me.lblTotalCost.AutoSize = True
        Me.lblTotalCost.Font = New System.Drawing.Font("Segoe UI", 14.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTotalCost.ForeColor = System.Drawing.Color.DarkGreen
        Me.lblTotalCost.Location = New System.Drawing.Point(110, 42)
        Me.lblTotalCost.Name = "lblTotalCost"
        Me.lblTotalCost.Size = New System.Drawing.Size(75, 25)
        Me.lblTotalCost.TabIndex = 2
        Me.lblTotalCost.Text = "$0.0000"
        '
        'lblOpenAICost
        '
        Me.lblOpenAICost.AutoSize = True
        Me.lblOpenAICost.Location = New System.Drawing.Point(15, 85)
        Me.lblOpenAICost.Name = "lblOpenAICost"
        Me.lblOpenAICost.Size = New System.Drawing.Size(90, 13)
        Me.lblOpenAICost.TabIndex = 3
        Me.lblOpenAICost.Text = "OpenAI: $0.0000"
        '
        'lblAzureCost
        '
        Me.lblAzureCost.AutoSize = True
        Me.lblAzureCost.Location = New System.Drawing.Point(15, 105)
        Me.lblAzureCost.Name = "lblAzureCost"
        Me.lblAzureCost.Size = New System.Drawing.Size(78, 13)
        Me.lblAzureCost.TabIndex = 4
        Me.lblAzureCost.Text = "Azure: $0.0000"
        '
        'lblGoogleCost
        '
        Me.lblGoogleCost.AutoSize = True
        Me.lblGoogleCost.Location = New System.Drawing.Point(15, 125)
        Me.lblGoogleCost.Name = "lblGoogleCost"
        Me.lblGoogleCost.Size = New System.Drawing.Size(86, 13)
        Me.lblGoogleCost.TabIndex = 5
        Me.lblGoogleCost.Text = "Google: $0.0000"
        '
        'btnCostReset
        '
        Me.btnCostReset.Location = New System.Drawing.Point(15, 155)
        Me.btnCostReset.Name = "btnCostReset"
        Me.btnCostReset.Size = New System.Drawing.Size(100, 30)
        Me.btnCostReset.TabIndex = 6
        Me.btnCostReset.Text = "Reset Costs"
        Me.btnCostReset.UseVisualStyleBackColor = True
        '
        'pnlSystemMonitor
        '
        Me.pnlSystemMonitor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlSystemMonitor.Controls.Add(Me.lblSystemTitle)
        Me.pnlSystemMonitor.Controls.Add(Me.lblAIProvidersLabel)
        Me.pnlSystemMonitor.Controls.Add(Me.lblOpenAIStatus)
        Me.pnlSystemMonitor.Controls.Add(Me.lblAzureStatus)
        Me.pnlSystemMonitor.Controls.Add(Me.lblGoogleStatus)
        Me.pnlSystemMonitor.Controls.Add(Me.lblSystemResourcesLabel)
        Me.pnlSystemMonitor.Controls.Add(Me.lblCPUUsage)
        Me.pnlSystemMonitor.Controls.Add(Me.lblMemoryUsage)
        Me.pnlSystemMonitor.Controls.Add(Me.lblNetworkStatus)
        Me.pnlSystemMonitor.Location = New System.Drawing.Point(15, 330)
        Me.pnlSystemMonitor.Name = "pnlSystemMonitor"
        Me.pnlSystemMonitor.Size = New System.Drawing.Size(815, 200)
        Me.pnlSystemMonitor.TabIndex = 3
        '
        'lblSystemTitle
        '
        Me.lblSystemTitle.AutoSize = True
        Me.lblSystemTitle.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSystemTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblSystemTitle.Name = "lblSystemTitle"
        Me.lblSystemTitle.Size = New System.Drawing.Size(127, 21)
        Me.lblSystemTitle.TabIndex = 0
        Me.lblSystemTitle.Text = "System Monitor"
        '
        'lblAIProvidersLabel
        '
        Me.lblAIProvidersLabel.AutoSize = True
        Me.lblAIProvidersLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAIProvidersLabel.Location = New System.Drawing.Point(15, 45)
        Me.lblAIProvidersLabel.Name = "lblAIProvidersLabel"
        Me.lblAIProvidersLabel.Size = New System.Drawing.Size(91, 19)
        Me.lblAIProvidersLabel.TabIndex = 1
        Me.lblAIProvidersLabel.Text = "AI Providers:"
        '
        'lblOpenAIStatus
        '
        Me.lblOpenAIStatus.AutoSize = True
        Me.lblOpenAIStatus.Location = New System.Drawing.Point(15, 70)
        Me.lblOpenAIStatus.Name = "lblOpenAIStatus"
        Me.lblOpenAIStatus.Size = New System.Drawing.Size(97, 13)
        Me.lblOpenAIStatus.TabIndex = 2
        Me.lblOpenAIStatus.Text = "✓ OpenAI: Ready"
        '
        'lblAzureStatus
        '
        Me.lblAzureStatus.AutoSize = True
        Me.lblAzureStatus.Location = New System.Drawing.Point(150, 70)
        Me.lblAzureStatus.Name = "lblAzureStatus"
        Me.lblAzureStatus.Size = New System.Drawing.Size(85, 13)
        Me.lblAzureStatus.TabIndex = 3
        Me.lblAzureStatus.Text = "✓ Azure: Ready"
        '
        'lblGoogleStatus
        '
        Me.lblGoogleStatus.AutoSize = True
        Me.lblGoogleStatus.Location = New System.Drawing.Point(275, 70)
        Me.lblGoogleStatus.Name = "lblGoogleStatus"
        Me.lblGoogleStatus.Size = New System.Drawing.Size(93, 13)
        Me.lblGoogleStatus.TabIndex = 4
        Me.lblGoogleStatus.Text = "✓ Google: Ready"
        '
        'lblSystemResourcesLabel
        '
        Me.lblSystemResourcesLabel.AutoSize = True
        Me.lblSystemResourcesLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSystemResourcesLabel.Location = New System.Drawing.Point(15, 105)
        Me.lblSystemResourcesLabel.Name = "lblSystemResourcesLabel"
        Me.lblSystemResourcesLabel.Size = New System.Drawing.Size(131, 19)
        Me.lblSystemResourcesLabel.TabIndex = 5
        Me.lblSystemResourcesLabel.Text = "System Resources:"
        '
        'lblCPUUsage
        '
        Me.lblCPUUsage.AutoSize = True
        Me.lblCPUUsage.Location = New System.Drawing.Point(15, 130)
        Me.lblCPUUsage.Name = "lblCPUUsage"
        Me.lblCPUUsage.Size = New System.Drawing.Size(59, 13)
        Me.lblCPUUsage.TabIndex = 6
        Me.lblCPUUsage.Text = "CPU: 0.0%"
        '
        'lblMemoryUsage
        '
        Me.lblMemoryUsage.AutoSize = True
        Me.lblMemoryUsage.Location = New System.Drawing.Point(150, 130)
        Me.lblMemoryUsage.Name = "lblMemoryUsage"
        Me.lblMemoryUsage.Size = New System.Drawing.Size(77, 13)
        Me.lblMemoryUsage.TabIndex = 7
        Me.lblMemoryUsage.Text = "Memory: 0.0%"
        '
        'lblNetworkStatus
        '
        Me.lblNetworkStatus.AutoSize = True
        Me.lblNetworkStatus.Location = New System.Drawing.Point(275, 130)
        Me.lblNetworkStatus.Name = "lblNetworkStatus"
        Me.lblNetworkStatus.Size = New System.Drawing.Size(107, 13)
        Me.lblNetworkStatus.TabIndex = 8
        Me.lblNetworkStatus.Text = "Network: Connected"
        '
        'pnlActions
        '
        Me.pnlActions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlActions.Controls.Add(Me.btnConfiguration)
        Me.pnlActions.Controls.Add(Me.btnMeetings)
        Me.pnlActions.Controls.Add(Me.btnViewLogs)
        Me.pnlActions.Controls.Add(Me.btnAbout)
        Me.pnlActions.Location = New System.Drawing.Point(845, 330)
        Me.pnlActions.Name = "pnlActions"
        Me.pnlActions.Size = New System.Drawing.Size(330, 200)
        Me.pnlActions.TabIndex = 4
        '
        'btnConfiguration
        '
        Me.btnConfiguration.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnConfiguration.Location = New System.Drawing.Point(15, 15)
        Me.btnConfiguration.Name = "btnConfiguration"
        Me.btnConfiguration.Size = New System.Drawing.Size(140, 40)
        Me.btnConfiguration.TabIndex = 0
        Me.btnConfiguration.Text = "⚙️ Configuration"
        Me.btnConfiguration.UseVisualStyleBackColor = True
        '
        'btnMeetings
        '
        Me.btnMeetings.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnMeetings.Location = New System.Drawing.Point(170, 15)
        Me.btnMeetings.Name = "btnMeetings"
        Me.btnMeetings.Size = New System.Drawing.Size(140, 40)
        Me.btnMeetings.TabIndex = 1
        Me.btnMeetings.Text = "🎤 Meetings"
        Me.btnMeetings.UseVisualStyleBackColor = True
        '
        'btnViewLogs
        '
        Me.btnViewLogs.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnViewLogs.Location = New System.Drawing.Point(15, 70)
        Me.btnViewLogs.Name = "btnViewLogs"
        Me.btnViewLogs.Size = New System.Drawing.Size(140, 40)
        Me.btnViewLogs.TabIndex = 2
        Me.btnViewLogs.Text = "📋 View Logs"
        Me.btnViewLogs.UseVisualStyleBackColor = True
        '
        'btnAbout
        '
        Me.btnAbout.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnAbout.Location = New System.Drawing.Point(170, 70)
        Me.btnAbout.Name = "btnAbout"
        Me.btnAbout.Size = New System.Drawing.Size(140, 40)
        Me.btnAbout.TabIndex = 3
        Me.btnAbout.Text = "ℹ️ About"
        Me.btnAbout.UseVisualStyleBackColor = True
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1200, 700)
        Me.Controls.Add(Me.pnlMain)
        Me.Controls.Add(Me.pnlHeader)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ARIA Premium System"
        Me.pnlHeader.ResumeLayout(False)
        Me.pnlHeader.PerformLayout()
        Me.pnlMain.ResumeLayout(False)
        Me.pnlVoiceControls.ResumeLayout(False)
        Me.pnlVoiceControls.PerformLayout()
        CType(Me.trackVolume, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlStatus.ResumeLayout(False)
        Me.pnlStatus.PerformLayout()
        CType(Me.picVoiceIndicator, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlCostTracking.ResumeLayout(False)
        Me.pnlCostTracking.PerformLayout()
        Me.pnlSystemMonitor.ResumeLayout(False)
        Me.pnlSystemMonitor.PerformLayout()
        Me.pnlActions.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblVersion As Label
    Friend WithEvents btnMinimize As Button
    Friend WithEvents btnExit As Button
    Friend WithEvents btnThemeToggle As Button
    Friend WithEvents pnlMain As Panel
    Friend WithEvents pnlVoiceControls As Panel
    Friend WithEvents lblVoiceControlsTitle As Label
    Friend WithEvents btnStartListening As Button
    Friend WithEvents btnMuteToggle As Button
    Friend WithEvents lblVolumeLabel As Label
    Friend WithEvents trackVolume As TrackBar
    Friend WithEvents lblVolumeLevel As Label
    Friend WithEvents pnlWaveform As Panel
    Friend WithEvents pnlStatus As Panel
    Friend WithEvents lblStatusTitle As Label
    Friend WithEvents lblVoiceStatusLabel As Label
    Friend WithEvents lblVoiceStatus As Label
    Friend WithEvents picVoiceIndicator As PictureBox
    Friend WithEvents txtTranscription As TextBox
    Friend WithEvents lblTranscriptionLabel As Label
    Friend WithEvents pnlCostTracking As Panel
    Friend WithEvents lblCostTitle As Label
    Friend WithEvents lblTotalCostLabel As Label
    Friend WithEvents lblTotalCost As Label
    Friend WithEvents lblOpenAICost As Label
    Friend WithEvents lblAzureCost As Label
    Friend WithEvents lblGoogleCost As Label
    Friend WithEvents btnCostReset As Button
    Friend WithEvents pnlSystemMonitor As Panel
    Friend WithEvents lblSystemTitle As Label
    Friend WithEvents lblAIProvidersLabel As Label
    Friend WithEvents lblOpenAIStatus As Label
    Friend WithEvents lblAzureStatus As Label
    Friend WithEvents lblGoogleStatus As Label
    Friend WithEvents lblSystemResourcesLabel As Label
    Friend WithEvents lblCPUUsage As Label
    Friend WithEvents lblMemoryUsage As Label
    Friend WithEvents lblNetworkStatus As Label
    Friend WithEvents pnlActions As Panel
    Friend WithEvents btnConfiguration As Button
    Friend WithEvents btnMeetings As Button
    Friend WithEvents btnViewLogs As Button
    Friend WithEvents btnAbout As Button
End Class