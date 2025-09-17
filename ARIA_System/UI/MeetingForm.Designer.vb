<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MeetingForm
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MeetingForm))
        Me.pnlHeader = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.btnThemeToggle = New System.Windows.Forms.Button()
        Me.btnNewMeeting = New System.Windows.Forms.Button()
        Me.pnlMain = New System.Windows.Forms.Panel()
        Me.pnlLeft = New System.Windows.Forms.Panel()
        Me.pnlRecordingControls = New System.Windows.Forms.Panel()
        Me.lblRecordingControlsTitle = New System.Windows.Forms.Label()
        Me.btnStartRecording = New System.Windows.Forms.Button()
        Me.btnPauseRecording = New System.Windows.Forms.Button()
        Me.btnResumeRecording = New System.Windows.Forms.Button()
        Me.btnStopRecording = New System.Windows.Forms.Button()
        Me.lblRecordingStatusLabel = New System.Windows.Forms.Label()
        Me.lblRecordingStatus = New System.Windows.Forms.Label()
        Me.picRecordingIndicator = New System.Windows.Forms.PictureBox()
        Me.lblRecordingTimeLabel = New System.Windows.Forms.Label()
        Me.lblRecordingTime = New System.Windows.Forms.Label()
        Me.lblAudioLevelLabel = New System.Windows.Forms.Label()
        Me.prgAudioLevel = New System.Windows.Forms.ProgressBar()
        Me.pnlMeetingInfo = New System.Windows.Forms.Panel()
        Me.lblMeetingInfoTitle = New System.Windows.Forms.Label()
        Me.lblMeetingTitleLabel = New System.Windows.Forms.Label()
        Me.txtMeetingTitle = New System.Windows.Forms.TextBox()
        Me.lblParticipantsLabel = New System.Windows.Forms.Label()
        Me.txtParticipants = New System.Windows.Forms.TextBox()
        Me.lblDateTimeLabel = New System.Windows.Forms.Label()
        Me.lblDateTime = New System.Windows.Forms.Label()
        Me.lblCurrentCost = New System.Windows.Forms.Label()
        Me.lblTotalCost = New System.Windows.Forms.Label()
        Me.pnlRight = New System.Windows.Forms.Panel()
        Me.pnlTranscription = New System.Windows.Forms.Panel()
        Me.lblTranscriptionTitle = New System.Windows.Forms.Label()
        Me.txtTranscription = New System.Windows.Forms.TextBox()
        Me.lblNotesLabel = New System.Windows.Forms.Label()
        Me.txtNotes = New System.Windows.Forms.TextBox()
        Me.pnlMeetingHistory = New System.Windows.Forms.Panel()
        Me.lblMeetingHistoryTitle = New System.Windows.Forms.Label()
        Me.dgvMeetingHistory = New System.Windows.Forms.DataGridView()
        Me.pnlMeetingActions = New System.Windows.Forms.Panel()
        Me.btnSaveMeeting = New System.Windows.Forms.Button()
        Me.btnExportMeeting = New System.Windows.Forms.Button()
        Me.btnDeleteMeeting = New System.Windows.Forms.Button()
        Me.btnViewDetails = New System.Windows.Forms.Button()
        Me.tmrAudioLevel = New System.Windows.Forms.Timer(Me.components)
        Me.pnlHeader.SuspendLayout()
        Me.pnlMain.SuspendLayout()
        Me.pnlLeft.SuspendLayout()
        Me.pnlRecordingControls.SuspendLayout()
        CType(Me.picRecordingIndicator, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlMeetingInfo.SuspendLayout()
        Me.pnlRight.SuspendLayout()
        Me.pnlTranscription.SuspendLayout()
        Me.pnlMeetingHistory.SuspendLayout()
        CType(Me.dgvMeetingHistory, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlMeetingActions.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlHeader
        '
        Me.pnlHeader.Controls.Add(Me.lblTitle)
        Me.pnlHeader.Controls.Add(Me.btnThemeToggle)
        Me.pnlHeader.Controls.Add(Me.btnNewMeeting)
        Me.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Size = New System.Drawing.Size(1400, 60)
        Me.pnlHeader.TabIndex = 0
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.Location = New System.Drawing.Point(12, 15)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(256, 32)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "ARIA Meeting Manager"
        '
        'btnThemeToggle
        '
        Me.btnThemeToggle.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnThemeToggle.Location = New System.Drawing.Point(1180, 15)
        Me.btnThemeToggle.Name = "btnThemeToggle"
        Me.btnThemeToggle.Size = New System.Drawing.Size(100, 30)
        Me.btnThemeToggle.TabIndex = 1
        Me.btnThemeToggle.Text = "Light Theme"
        Me.btnThemeToggle.UseVisualStyleBackColor = True
        '
        'btnNewMeeting
        '
        Me.btnNewMeeting.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnNewMeeting.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnNewMeeting.Location = New System.Drawing.Point(1290, 15)
        Me.btnNewMeeting.Name = "btnNewMeeting"
        Me.btnNewMeeting.Size = New System.Drawing.Size(100, 30)
        Me.btnNewMeeting.TabIndex = 2
        Me.btnNewMeeting.Text = "New Meeting"
        Me.btnNewMeeting.UseVisualStyleBackColor = True
        '
        'pnlMain
        '
        Me.pnlMain.Controls.Add(Me.pnlLeft)
        Me.pnlMain.Controls.Add(Me.pnlRight)
        Me.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMain.Location = New System.Drawing.Point(0, 60)
        Me.pnlMain.Name = "pnlMain"
        Me.pnlMain.Padding = New System.Windows.Forms.Padding(10)
        Me.pnlMain.Size = New System.Drawing.Size(1400, 740)
        Me.pnlMain.TabIndex = 1
        '
        'pnlLeft
        '
        Me.pnlLeft.Controls.Add(Me.pnlRecordingControls)
        Me.pnlLeft.Controls.Add(Me.pnlMeetingInfo)
        Me.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left
        Me.pnlLeft.Location = New System.Drawing.Point(10, 10)
        Me.pnlLeft.Name = "pnlLeft"
        Me.pnlLeft.Size = New System.Drawing.Size(400, 720)
        Me.pnlLeft.TabIndex = 0
        '
        'pnlRecordingControls
        '
        Me.pnlRecordingControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlRecordingControls.Controls.Add(Me.lblRecordingControlsTitle)
        Me.pnlRecordingControls.Controls.Add(Me.btnStartRecording)
        Me.pnlRecordingControls.Controls.Add(Me.btnPauseRecording)
        Me.pnlRecordingControls.Controls.Add(Me.btnResumeRecording)
        Me.pnlRecordingControls.Controls.Add(Me.btnStopRecording)
        Me.pnlRecordingControls.Controls.Add(Me.lblRecordingStatusLabel)
        Me.pnlRecordingControls.Controls.Add(Me.lblRecordingStatus)
        Me.pnlRecordingControls.Controls.Add(Me.picRecordingIndicator)
        Me.pnlRecordingControls.Controls.Add(Me.lblRecordingTimeLabel)
        Me.pnlRecordingControls.Controls.Add(Me.lblRecordingTime)
        Me.pnlRecordingControls.Controls.Add(Me.lblAudioLevelLabel)
        Me.pnlRecordingControls.Controls.Add(Me.prgAudioLevel)
        Me.pnlRecordingControls.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlRecordingControls.Location = New System.Drawing.Point(0, 0)
        Me.pnlRecordingControls.Name = "pnlRecordingControls"
        Me.pnlRecordingControls.Size = New System.Drawing.Size(400, 350)
        Me.pnlRecordingControls.TabIndex = 0
        '
        'lblRecordingControlsTitle
        '
        Me.lblRecordingControlsTitle.AutoSize = True
        Me.lblRecordingControlsTitle.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRecordingControlsTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblRecordingControlsTitle.Name = "lblRecordingControlsTitle"
        Me.lblRecordingControlsTitle.Size = New System.Drawing.Size(157, 21)
        Me.lblRecordingControlsTitle.TabIndex = 0
        Me.lblRecordingControlsTitle.Text = "Recording Controls"
        '
        'btnStartRecording
        '
        Me.btnStartRecording.Font = New System.Drawing.Font("Segoe UI", 11.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnStartRecording.Location = New System.Drawing.Point(15, 45)
        Me.btnStartRecording.Name = "btnStartRecording"
        Me.btnStartRecording.Size = New System.Drawing.Size(120, 40)
        Me.btnStartRecording.TabIndex = 1
        Me.btnStartRecording.Text = "🔴 Start"
        Me.btnStartRecording.UseVisualStyleBackColor = True
        '
        'btnPauseRecording
        '
        Me.btnPauseRecording.Location = New System.Drawing.Point(145, 45)
        Me.btnPauseRecording.Name = "btnPauseRecording"
        Me.btnPauseRecording.Size = New System.Drawing.Size(75, 40)
        Me.btnPauseRecording.TabIndex = 2
        Me.btnPauseRecording.Text = "⏸️ Pause"
        Me.btnPauseRecording.UseVisualStyleBackColor = True
        '
        'btnResumeRecording
        '
        Me.btnResumeRecording.Location = New System.Drawing.Point(230, 45)
        Me.btnResumeRecording.Name = "btnResumeRecording"
        Me.btnResumeRecording.Size = New System.Drawing.Size(75, 40)
        Me.btnResumeRecording.TabIndex = 3
        Me.btnResumeRecording.Text = "▶️ Resume"
        Me.btnResumeRecording.UseVisualStyleBackColor = True
        '
        'btnStopRecording
        '
        Me.btnStopRecording.Location = New System.Drawing.Point(315, 45)
        Me.btnStopRecording.Name = "btnStopRecording"
        Me.btnStopRecording.Size = New System.Drawing.Size(70, 40)
        Me.btnStopRecording.TabIndex = 4
        Me.btnStopRecording.Text = "⏹️ Stop"
        Me.btnStopRecording.UseVisualStyleBackColor = True
        '
        'lblRecordingStatusLabel
        '
        Me.lblRecordingStatusLabel.AutoSize = True
        Me.lblRecordingStatusLabel.Location = New System.Drawing.Point(15, 100)
        Me.lblRecordingStatusLabel.Name = "lblRecordingStatusLabel"
        Me.lblRecordingStatusLabel.Size = New System.Drawing.Size(40, 13)
        Me.lblRecordingStatusLabel.TabIndex = 5
        Me.lblRecordingStatusLabel.Text = "Status:"
        '
        'lblRecordingStatus
        '
        Me.lblRecordingStatus.AutoSize = True
        Me.lblRecordingStatus.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRecordingStatus.ForeColor = System.Drawing.Color.Green
        Me.lblRecordingStatus.Location = New System.Drawing.Point(90, 100)
        Me.lblRecordingStatus.Name = "lblRecordingStatus"
        Me.lblRecordingStatus.Size = New System.Drawing.Size(90, 15)
        Me.lblRecordingStatus.TabIndex = 6
        Me.lblRecordingStatus.Text = "Ready to Record"
        '
        'picRecordingIndicator
        '
        Me.picRecordingIndicator.BackColor = System.Drawing.Color.Green
        Me.picRecordingIndicator.Location = New System.Drawing.Point(65, 100)
        Me.picRecordingIndicator.Name = "picRecordingIndicator"
        Me.picRecordingIndicator.Size = New System.Drawing.Size(15, 15)
        Me.picRecordingIndicator.TabIndex = 7
        Me.picRecordingIndicator.TabStop = False
        '
        'lblRecordingTimeLabel
        '
        Me.lblRecordingTimeLabel.AutoSize = True
        Me.lblRecordingTimeLabel.Location = New System.Drawing.Point(15, 130)
        Me.lblRecordingTimeLabel.Name = "lblRecordingTimeLabel"
        Me.lblRecordingTimeLabel.Size = New System.Drawing.Size(78, 13)
        Me.lblRecordingTimeLabel.TabIndex = 8
        Me.lblRecordingTimeLabel.Text = "Recording Time:"
        '
        'lblRecordingTime
        '
        Me.lblRecordingTime.AutoSize = True
        Me.lblRecordingTime.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRecordingTime.Location = New System.Drawing.Point(105, 125)
        Me.lblRecordingTime.Name = "lblRecordingTime"
        Me.lblRecordingTime.Size = New System.Drawing.Size(74, 21)
        Me.lblRecordingTime.TabIndex = 9
        Me.lblRecordingTime.Text = "00:00:00"
        '
        'lblAudioLevelLabel
        '
        Me.lblAudioLevelLabel.AutoSize = True
        Me.lblAudioLevelLabel.Location = New System.Drawing.Point(15, 160)
        Me.lblAudioLevelLabel.Name = "lblAudioLevelLabel"
        Me.lblAudioLevelLabel.Size = New System.Drawing.Size(64, 13)
        Me.lblAudioLevelLabel.TabIndex = 10
        Me.lblAudioLevelLabel.Text = "Audio Level:"
        '
        'prgAudioLevel
        '
        Me.prgAudioLevel.Location = New System.Drawing.Point(15, 180)
        Me.prgAudioLevel.Name = "prgAudioLevel"
        Me.prgAudioLevel.Size = New System.Drawing.Size(365, 20)
        Me.prgAudioLevel.TabIndex = 11
        '
        'pnlMeetingInfo
        '
        Me.pnlMeetingInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlMeetingInfo.Controls.Add(Me.lblMeetingInfoTitle)
        Me.pnlMeetingInfo.Controls.Add(Me.lblMeetingTitleLabel)
        Me.pnlMeetingInfo.Controls.Add(Me.txtMeetingTitle)
        Me.pnlMeetingInfo.Controls.Add(Me.lblParticipantsLabel)
        Me.pnlMeetingInfo.Controls.Add(Me.txtParticipants)
        Me.pnlMeetingInfo.Controls.Add(Me.lblDateTimeLabel)
        Me.pnlMeetingInfo.Controls.Add(Me.lblDateTime)
        Me.pnlMeetingInfo.Controls.Add(Me.lblCurrentCost)
        Me.pnlMeetingInfo.Controls.Add(Me.lblTotalCost)
        Me.pnlMeetingInfo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMeetingInfo.Location = New System.Drawing.Point(0, 350)
        Me.pnlMeetingInfo.Name = "pnlMeetingInfo"
        Me.pnlMeetingInfo.Size = New System.Drawing.Size(400, 370)
        Me.pnlMeetingInfo.TabIndex = 1
        '
        'lblMeetingInfoTitle
        '
        Me.lblMeetingInfoTitle.AutoSize = True
        Me.lblMeetingInfoTitle.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMeetingInfoTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblMeetingInfoTitle.Name = "lblMeetingInfoTitle"
        Me.lblMeetingInfoTitle.Size = New System.Drawing.Size(132, 21)
        Me.lblMeetingInfoTitle.TabIndex = 0
        Me.lblMeetingInfoTitle.Text = "Meeting Details"
        '
        'lblMeetingTitleLabel
        '
        Me.lblMeetingTitleLabel.AutoSize = True
        Me.lblMeetingTitleLabel.Location = New System.Drawing.Point(15, 45)
        Me.lblMeetingTitleLabel.Name = "lblMeetingTitleLabel"
        Me.lblMeetingTitleLabel.Size = New System.Drawing.Size(73, 13)
        Me.lblMeetingTitleLabel.TabIndex = 1
        Me.lblMeetingTitleLabel.Text = "Meeting Title:"
        '
        'txtMeetingTitle
        '
        Me.txtMeetingTitle.Location = New System.Drawing.Point(15, 65)
        Me.txtMeetingTitle.Name = "txtMeetingTitle"
        Me.txtMeetingTitle.Size = New System.Drawing.Size(365, 20)
        Me.txtMeetingTitle.TabIndex = 2
        '
        'lblParticipantsLabel
        '
        Me.lblParticipantsLabel.AutoSize = True
        Me.lblParticipantsLabel.Location = New System.Drawing.Point(15, 95)
        Me.lblParticipantsLabel.Name = "lblParticipantsLabel"
        Me.lblParticipantsLabel.Size = New System.Drawing.Size(67, 13)
        Me.lblParticipantsLabel.TabIndex = 3
        Me.lblParticipantsLabel.Text = "Participants:"
        '
        'txtParticipants
        '
        Me.txtParticipants.Location = New System.Drawing.Point(15, 115)
        Me.txtParticipants.Multiline = True
        Me.txtParticipants.Name = "txtParticipants"
        Me.txtParticipants.Size = New System.Drawing.Size(365, 60)
        Me.txtParticipants.TabIndex = 4
        '
        'lblDateTimeLabel
        '
        Me.lblDateTimeLabel.AutoSize = True
        Me.lblDateTimeLabel.Location = New System.Drawing.Point(15, 185)
        Me.lblDateTimeLabel.Name = "lblDateTimeLabel"
        Me.lblDateTimeLabel.Size = New System.Drawing.Size(63, 13)
        Me.lblDateTimeLabel.TabIndex = 5
        Me.lblDateTimeLabel.Text = "Date/Time:"
        '
        'lblDateTime
        '
        Me.lblDateTime.AutoSize = True
        Me.lblDateTime.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDateTime.Location = New System.Drawing.Point(85, 185)
        Me.lblDateTime.Name = "lblDateTime"
        Me.lblDateTime.Size = New System.Drawing.Size(122, 15)
        Me.lblDateTime.TabIndex = 6
        Me.lblDateTime.Text = "Not Started"
        '
        'lblCurrentCost
        '
        Me.lblCurrentCost.AutoSize = True
        Me.lblCurrentCost.Location = New System.Drawing.Point(15, 220)
        Me.lblCurrentCost.Name = "lblCurrentCost"
        Me.lblCurrentCost.Size = New System.Drawing.Size(130, 13)
        Me.lblCurrentCost.TabIndex = 7
        Me.lblCurrentCost.Text = "Current Session: $0.0000"
        '
        'lblTotalCost
        '
        Me.lblTotalCost.AutoSize = True
        Me.lblTotalCost.Location = New System.Drawing.Point(15, 240)
        Me.lblTotalCost.Name = "lblTotalCost"
        Me.lblTotalCost.Size = New System.Drawing.Size(85, 13)
        Me.lblTotalCost.TabIndex = 8
        Me.lblTotalCost.Text = "Total Cost: $0.0000"
        '
        'pnlRight
        '
        Me.pnlRight.Controls.Add(Me.pnlTranscription)
        Me.pnlRight.Controls.Add(Me.pnlMeetingHistory)
        Me.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlRight.Location = New System.Drawing.Point(410, 10)
        Me.pnlRight.Name = "pnlRight"
        Me.pnlRight.Size = New System.Drawing.Size(980, 720)
        Me.pnlRight.TabIndex = 1
        '
        'pnlTranscription
        '
        Me.pnlTranscription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlTranscription.Controls.Add(Me.lblTranscriptionTitle)
        Me.pnlTranscription.Controls.Add(Me.txtTranscription)
        Me.pnlTranscription.Controls.Add(Me.lblNotesLabel)
        Me.pnlTranscription.Controls.Add(Me.txtNotes)
        Me.pnlTranscription.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlTranscription.Location = New System.Drawing.Point(0, 0)
        Me.pnlTranscription.Name = "pnlTranscription"
        Me.pnlTranscription.Size = New System.Drawing.Size(980, 350)
        Me.pnlTranscription.TabIndex = 0
        '
        'lblTranscriptionTitle
        '
        Me.lblTranscriptionTitle.AutoSize = True
        Me.lblTranscriptionTitle.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTranscriptionTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblTranscriptionTitle.Name = "lblTranscriptionTitle"
        Me.lblTranscriptionTitle.Size = New System.Drawing.Size(182, 21)
        Me.lblTranscriptionTitle.TabIndex = 0
        Me.lblTranscriptionTitle.Text = "Real-time Transcription"
        '
        'txtTranscription
        '
        Me.txtTranscription.Location = New System.Drawing.Point(15, 40)
        Me.txtTranscription.Multiline = True
        Me.txtTranscription.Name = "txtTranscription"
        Me.txtTranscription.ReadOnly = True
        Me.txtTranscription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtTranscription.Size = New System.Drawing.Size(950, 200)
        Me.txtTranscription.TabIndex = 1
        '
        'lblNotesLabel
        '
        Me.lblNotesLabel.AutoSize = True
        Me.lblNotesLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNotesLabel.Location = New System.Drawing.Point(15, 250)
        Me.lblNotesLabel.Name = "lblNotesLabel"
        Me.lblNotesLabel.Size = New System.Drawing.Size(98, 19)
        Me.lblNotesLabel.TabIndex = 2
        Me.lblNotesLabel.Text = "Meeting Notes:"
        '
        'txtNotes
        '
        Me.txtNotes.Location = New System.Drawing.Point(15, 275)
        Me.txtNotes.Multiline = True
        Me.txtNotes.Name = "txtNotes"
        Me.txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtNotes.Size = New System.Drawing.Size(950, 60)
        Me.txtNotes.TabIndex = 3
        '
        'pnlMeetingHistory
        '
        Me.pnlMeetingHistory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlMeetingHistory.Controls.Add(Me.lblMeetingHistoryTitle)
        Me.pnlMeetingHistory.Controls.Add(Me.dgvMeetingHistory)
        Me.pnlMeetingHistory.Controls.Add(Me.pnlMeetingActions)
        Me.pnlMeetingHistory.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMeetingHistory.Location = New System.Drawing.Point(0, 350)
        Me.pnlMeetingHistory.Name = "pnlMeetingHistory"
        Me.pnlMeetingHistory.Size = New System.Drawing.Size(980, 370)
        Me.pnlMeetingHistory.TabIndex = 1
        '
        'lblMeetingHistoryTitle
        '
        Me.lblMeetingHistoryTitle.AutoSize = True
        Me.lblMeetingHistoryTitle.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMeetingHistoryTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblMeetingHistoryTitle.Name = "lblMeetingHistoryTitle"
        Me.lblMeetingHistoryTitle.Size = New System.Drawing.Size(131, 21)
        Me.lblMeetingHistoryTitle.TabIndex = 0
        Me.lblMeetingHistoryTitle.Text = "Meeting History"
        '
        'dgvMeetingHistory
        '
        Me.dgvMeetingHistory.AllowUserToAddRows = False
        Me.dgvMeetingHistory.AllowUserToDeleteRows = False
        Me.dgvMeetingHistory.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgvMeetingHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvMeetingHistory.Location = New System.Drawing.Point(15, 40)
        Me.dgvMeetingHistory.Name = "dgvMeetingHistory"
        Me.dgvMeetingHistory.ReadOnly = True
        Me.dgvMeetingHistory.Size = New System.Drawing.Size(950, 270)
        Me.dgvMeetingHistory.TabIndex = 1
        '
        'pnlMeetingActions
        '
        Me.pnlMeetingActions.Controls.Add(Me.btnSaveMeeting)
        Me.pnlMeetingActions.Controls.Add(Me.btnExportMeeting)
        Me.pnlMeetingActions.Controls.Add(Me.btnDeleteMeeting)
        Me.pnlMeetingActions.Controls.Add(Me.btnViewDetails)
        Me.pnlMeetingActions.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlMeetingActions.Location = New System.Drawing.Point(0, 320)
        Me.pnlMeetingActions.Name = "pnlMeetingActions"
        Me.pnlMeetingActions.Size = New System.Drawing.Size(978, 48)
        Me.pnlMeetingActions.TabIndex = 2
        '
        'btnSaveMeeting
        '
        Me.btnSaveMeeting.Location = New System.Drawing.Point(15, 10)
        Me.btnSaveMeeting.Name = "btnSaveMeeting"
        Me.btnSaveMeeting.Size = New System.Drawing.Size(100, 30)
        Me.btnSaveMeeting.TabIndex = 0
        Me.btnSaveMeeting.Text = "💾 Save"
        Me.btnSaveMeeting.UseVisualStyleBackColor = True
        '
        'btnExportMeeting
        '
        Me.btnExportMeeting.Location = New System.Drawing.Point(125, 10)
        Me.btnExportMeeting.Name = "btnExportMeeting"
        Me.btnExportMeeting.Size = New System.Drawing.Size(100, 30)
        Me.btnExportMeeting.TabIndex = 1
        Me.btnExportMeeting.Text = "📄 Export"
        Me.btnExportMeeting.UseVisualStyleBackColor = True
        '
        'btnDeleteMeeting
        '
        Me.btnDeleteMeeting.Location = New System.Drawing.Point(235, 10)
        Me.btnDeleteMeeting.Name = "btnDeleteMeeting"
        Me.btnDeleteMeeting.Size = New System.Drawing.Size(100, 30)
        Me.btnDeleteMeeting.TabIndex = 2
        Me.btnDeleteMeeting.Text = "🗑️ Delete"
        Me.btnDeleteMeeting.UseVisualStyleBackColor = True
        '
        'btnViewDetails
        '
        Me.btnViewDetails.Location = New System.Drawing.Point(345, 10)
        Me.btnViewDetails.Name = "btnViewDetails"
        Me.btnViewDetails.Size = New System.Drawing.Size(100, 30)
        Me.btnViewDetails.TabIndex = 3
        Me.btnViewDetails.Text = "👁️ View Details"
        Me.btnViewDetails.UseVisualStyleBackColor = True
        '
        'tmrAudioLevel
        '
        Me.tmrAudioLevel.Enabled = True
        Me.tmrAudioLevel.Interval = 100
        '
        'MeetingForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1400, 800)
        Me.Controls.Add(Me.pnlMain)
        Me.Controls.Add(Me.pnlHeader)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimumSize = New System.Drawing.Size(1200, 700)
        Me.Name = "MeetingForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "ARIA Meeting Manager"
        Me.pnlHeader.ResumeLayout(False)
        Me.pnlHeader.PerformLayout()
        Me.pnlMain.ResumeLayout(False)
        Me.pnlLeft.ResumeLayout(False)
        Me.pnlRecordingControls.ResumeLayout(False)
        Me.pnlRecordingControls.PerformLayout()
        CType(Me.picRecordingIndicator, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlMeetingInfo.ResumeLayout(False)
        Me.pnlMeetingInfo.PerformLayout()
        Me.pnlRight.ResumeLayout(False)
        Me.pnlTranscription.ResumeLayout(False)
        Me.pnlTranscription.PerformLayout()
        Me.pnlMeetingHistory.ResumeLayout(False)
        Me.pnlMeetingHistory.PerformLayout()
        CType(Me.dgvMeetingHistory, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlMeetingActions.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents btnThemeToggle As Button
    Friend WithEvents btnNewMeeting As Button
    Friend WithEvents pnlMain As Panel
    Friend WithEvents pnlLeft As Panel
    Friend WithEvents pnlRecordingControls As Panel
    Friend WithEvents lblRecordingControlsTitle As Label
    Friend WithEvents btnStartRecording As Button
    Friend WithEvents btnPauseRecording As Button
    Friend WithEvents btnResumeRecording As Button
    Friend WithEvents btnStopRecording As Button
    Friend WithEvents lblRecordingStatusLabel As Label
    Friend WithEvents lblRecordingStatus As Label
    Friend WithEvents picRecordingIndicator As PictureBox
    Friend WithEvents lblRecordingTimeLabel As Label
    Friend WithEvents lblRecordingTime As Label
    Friend WithEvents lblAudioLevelLabel As Label
    Friend WithEvents prgAudioLevel As ProgressBar
    Friend WithEvents pnlMeetingInfo As Panel
    Friend WithEvents lblMeetingInfoTitle As Label
    Friend WithEvents lblMeetingTitleLabel As Label
    Friend WithEvents txtMeetingTitle As TextBox
    Friend WithEvents lblParticipantsLabel As Label
    Friend WithEvents txtParticipants As TextBox
    Friend WithEvents lblDateTimeLabel As Label
    Friend WithEvents lblDateTime As Label
    Friend WithEvents lblCurrentCost As Label
    Friend WithEvents lblTotalCost As Label
    Friend WithEvents pnlRight As Panel
    Friend WithEvents pnlTranscription As Panel
    Friend WithEvents lblTranscriptionTitle As Label
    Friend WithEvents txtTranscription As TextBox
    Friend WithEvents lblNotesLabel As Label
    Friend WithEvents txtNotes As TextBox
    Friend WithEvents pnlMeetingHistory As Panel
    Friend WithEvents lblMeetingHistoryTitle As Label
    Friend WithEvents dgvMeetingHistory As DataGridView
    Friend WithEvents pnlMeetingActions As Panel
    Friend WithEvents btnSaveMeeting As Button
    Friend WithEvents btnExportMeeting As Button
    Friend WithEvents btnDeleteMeeting As Button
    Friend WithEvents btnViewDetails As Button
    Friend WithEvents tmrAudioLevel As Timer
End Class