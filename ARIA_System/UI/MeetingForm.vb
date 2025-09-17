Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.IO
Imports ARIA_Premium_System.Core
Imports ARIA_Premium_System.Meeting
Imports ARIA_Premium_System.Voice
Imports ARIA_Premium_System.Utils

Public Class MeetingForm
    Private WithEvents meetingManager As MeetingManager
    Private WithEvents voiceEngine As VoiceEngine
    Private WithEvents costTracker As CostTracker
    Private isDarkTheme As Boolean = True
    Private isRecording As Boolean = False
    Private isPaused As Boolean = False
    Private recordingStartTime As DateTime
    Private currentMeeting As MeetingSession
    Private recordingTimer As Timer
    Private transcriptionBuilder As System.Text.StringBuilder

    Public Sub New()
        InitializeComponent()
        InitializeMeetingSystem()
        ApplyTheme()
        SetupDataGrid()
        LoadMeetingHistory()
    End Sub

    Private Sub InitializeMeetingSystem()
        Try
            meetingManager = New MeetingManager()
            voiceEngine = New VoiceEngine()
            costTracker = New CostTracker()
            transcriptionBuilder = New System.Text.StringBuilder()

            ' Initialize recording timer
            recordingTimer = New Timer()
            recordingTimer.Interval = 1000 ' Update every second
            AddHandler recordingTimer.Tick, AddressOf UpdateRecordingTime

            UpdateUI()
        Catch ex As Exception
            MessageBox.Show($"Error initializing meeting system: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
        pnlRecordingControls.BackColor = Color.FromArgb(40, 40, 40)
        pnlMeetingInfo.BackColor = Color.FromArgb(40, 40, 40)
        pnlTranscription.BackColor = Color.FromArgb(40, 40, 40)
        pnlMeetingHistory.BackColor = Color.FromArgb(40, 40, 40)

        ' Update text controls
        txtMeetingTitle.BackColor = Color.FromArgb(60, 60, 60)
        txtMeetingTitle.ForeColor = Color.White
        txtParticipants.BackColor = Color.FromArgb(60, 60, 60)
        txtParticipants.ForeColor = Color.White
        txtTranscription.BackColor = Color.FromArgb(60, 60, 60)
        txtTranscription.ForeColor = Color.White
        txtNotes.BackColor = Color.FromArgb(60, 60, 60)
        txtNotes.ForeColor = Color.White

        ' Update data grid
        dgvMeetingHistory.BackgroundColor = Color.FromArgb(45, 45, 45)
        dgvMeetingHistory.GridColor = Color.FromArgb(60, 60, 60)
        dgvMeetingHistory.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45)
        dgvMeetingHistory.DefaultCellStyle.ForeColor = Color.White
        dgvMeetingHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35)
        dgvMeetingHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White

        ' Update buttons
        For Each ctrl As Control In GetAllControls(Me)
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
        pnlRecordingControls.BackColor = Color.FromArgb(250, 250, 250)
        pnlMeetingInfo.BackColor = Color.FromArgb(250, 250, 250)
        pnlTranscription.BackColor = Color.FromArgb(250, 250, 250)
        pnlMeetingHistory.BackColor = Color.FromArgb(250, 250, 250)

        ' Update text controls
        txtMeetingTitle.BackColor = Color.White
        txtMeetingTitle.ForeColor = Color.Black
        txtParticipants.BackColor = Color.White
        txtParticipants.ForeColor = Color.Black
        txtTranscription.BackColor = Color.White
        txtTranscription.ForeColor = Color.Black
        txtNotes.BackColor = Color.White
        txtNotes.ForeColor = Color.Black

        ' Update data grid
        dgvMeetingHistory.BackgroundColor = Color.White
        dgvMeetingHistory.GridColor = Color.LightGray
        dgvMeetingHistory.DefaultCellStyle.BackColor = Color.White
        dgvMeetingHistory.DefaultCellStyle.ForeColor = Color.Black
        dgvMeetingHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240)
        dgvMeetingHistory.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black

        ' Update buttons
        For Each ctrl As Control In GetAllControls(Me)
            If TypeOf ctrl Is Button Then
                Dim btn As Button = DirectCast(ctrl, Button)
                btn.BackColor = Color.FromArgb(225, 225, 225)
                btn.ForeColor = Color.Black
                btn.FlatStyle = FlatStyle.Standard
            End If
        Next
    End Sub

    Private Function GetAllControls(container As Control) As IEnumerable(Of Control)
        Dim controls As New List(Of Control)
        For Each ctrl As Control In container.Controls
            controls.Add(ctrl)
            controls.AddRange(GetAllControls(ctrl))
        Next
        Return controls
    End Function

    Private Sub SetupDataGrid()
        dgvMeetingHistory.Columns.Clear()
        dgvMeetingHistory.Columns.Add("Title", "Meeting Title")
        dgvMeetingHistory.Columns.Add("Date", "Date")
        dgvMeetingHistory.Columns.Add("Duration", "Duration")
        dgvMeetingHistory.Columns.Add("Participants", "Participants")
        dgvMeetingHistory.Columns.Add("Status", "Status")

        ' Set column widths
        dgvMeetingHistory.Columns("Title").Width = 200
        dgvMeetingHistory.Columns("Date").Width = 120
        dgvMeetingHistory.Columns("Duration").Width = 80
        dgvMeetingHistory.Columns("Participants").Width = 150
        dgvMeetingHistory.Columns("Status").Width = 80

        dgvMeetingHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvMeetingHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvMeetingHistory.MultiSelect = False
    End Sub

    Private Sub LoadMeetingHistory()
        Try
            dgvMeetingHistory.Rows.Clear()

            If meetingManager IsNot Nothing Then
                Dim meetings = meetingManager.GetMeetingHistory()
                For Each meeting In meetings
                    dgvMeetingHistory.Rows.Add(
                        meeting.Title,
                        meeting.StartTime.ToString("MM/dd/yyyy HH:mm"),
                        FormatDuration(meeting.Duration),
                        meeting.Participants,
                        meeting.Status
                    )
                Next
            End If
        Catch ex As Exception
            ' Handle loading errors gracefully
        End Try
    End Sub

    Private Function FormatDuration(duration As TimeSpan) As String
        If duration.TotalHours >= 1 Then
            Return $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}"
        Else
            Return $"{duration.Minutes:D2}:{duration.Seconds:D2}"
        End If
    End Function

    Private Sub UpdateUI()
        btnStartRecording.Enabled = Not isRecording
        btnPauseRecording.Enabled = isRecording And Not isPaused
        btnResumeRecording.Enabled = isRecording And isPaused
        btnStopRecording.Enabled = isRecording

        txtMeetingTitle.Enabled = Not isRecording
        txtParticipants.Enabled = Not isRecording

        If isRecording Then
            lblRecordingStatus.Text = If(isPaused, "Recording Paused", "Recording Active")
            lblRecordingStatus.ForeColor = If(isPaused, Color.Orange, Color.Red)
            picRecordingIndicator.BackColor = If(isPaused, Color.Orange, Color.Red)
        Else
            lblRecordingStatus.Text = "Ready to Record"
            lblRecordingStatus.ForeColor = Color.Green
            picRecordingIndicator.BackColor = Color.Green
        End If

        UpdateCostDisplay()
    End Sub

    Private Sub UpdateRecordingTime()
        If isRecording And Not isPaused Then
            Dim elapsed = DateTime.Now - recordingStartTime
            lblRecordingTime.Text = FormatDuration(elapsed)
        End If
    End Sub

    Private Sub UpdateCostDisplay()
        If costTracker IsNot Nothing Then
            lblCurrentCost.Text = $"Current Session: ${costTracker.CurrentSessionCost:F4}"
            lblTotalCost.Text = $"Total Cost: ${costTracker.TotalCost:F4}"
        End If
    End Sub

    ' Recording control event handlers
    Private Sub btnStartRecording_Click(sender As Object, e As EventArgs) Handles btnStartRecording.Click
        If String.IsNullOrWhiteSpace(txtMeetingTitle.Text) Then
            MessageBox.Show("Please enter a meeting title before starting recording.", "Meeting Title Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtMeetingTitle.Focus()
            Return
        End If

        Try
            ' Create new meeting session
            currentMeeting = New MeetingSession() With {
                .Title = txtMeetingTitle.Text.Trim(),
                .Participants = txtParticipants.Text.Trim(),
                .StartTime = DateTime.Now,
                .Status = "Recording"
            }

            ' Start recording
            meetingManager.StartMeeting(currentMeeting)
            voiceEngine.StartListening()

            isRecording = True
            isPaused = False
            recordingStartTime = DateTime.Now
            recordingTimer.Start()

            transcriptionBuilder.Clear()
            txtTranscription.Clear()
            txtNotes.Clear()

            UpdateUI()

        Catch ex As Exception
            MessageBox.Show($"Error starting recording: {ex.Message}", "Recording Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnPauseRecording_Click(sender As Object, e As EventArgs) Handles btnPauseRecording.Click
        Try
            meetingManager.PauseMeeting()
            voiceEngine.StopListening()
            isPaused = True
            UpdateUI()
        Catch ex As Exception
            MessageBox.Show($"Error pausing recording: {ex.Message}", "Recording Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnResumeRecording_Click(sender As Object, e As EventArgs) Handles btnResumeRecording.Click
        Try
            meetingManager.ResumeMeeting()
            voiceEngine.StartListening()
            isPaused = False
            UpdateUI()
        Catch ex As Exception
            MessageBox.Show($"Error resuming recording: {ex.Message}", "Recording Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnStopRecording_Click(sender As Object, e As EventArgs) Handles btnStopRecording.Click
        Try
            ' Stop recording
            meetingManager.StopMeeting()
            voiceEngine.StopListening()
            recordingTimer.Stop()

            If currentMeeting IsNot Nothing Then
                currentMeeting.EndTime = DateTime.Now
                currentMeeting.Duration = currentMeeting.EndTime - currentMeeting.StartTime
                currentMeeting.Status = "Completed"
                currentMeeting.Transcription = transcriptionBuilder.ToString()
                currentMeeting.Notes = txtNotes.Text

                ' Save meeting
                meetingManager.SaveMeeting(currentMeeting)
            End If

            isRecording = False
            isPaused = False
            currentMeeting = Nothing

            UpdateUI()
            LoadMeetingHistory()

            MessageBox.Show("Meeting recording stopped and saved successfully!", "Recording Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Error stopping recording: {ex.Message}", "Recording Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Meeting management event handlers
    Private Sub btnSaveMeeting_Click(sender As Object, e As EventArgs) Handles btnSaveMeeting.Click
        If currentMeeting Is Nothing Then
            MessageBox.Show("No active meeting to save.", "Save Meeting", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            currentMeeting.Notes = txtNotes.Text
            meetingManager.SaveMeeting(currentMeeting)
            MessageBox.Show("Meeting saved successfully!", "Save Meeting", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show($"Error saving meeting: {ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnExportMeeting_Click(sender As Object, e As EventArgs) Handles btnExportMeeting.Click
        If dgvMeetingHistory.SelectedRows.Count = 0 Then
            MessageBox.Show("Please select a meeting to export.", "Export Meeting", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Using saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Text Files (*.txt)|*.txt|Word Documents (*.docx)|*.docx|PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*"
            saveDialog.Title = "Export Meeting"
            saveDialog.FileName = $"Meeting_{dgvMeetingHistory.SelectedRows(0).Cells("Title").Value}_{DateTime.Now:yyyyMMdd}.txt"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                Try
                    ExportMeetingToFile(saveDialog.FileName, dgvMeetingHistory.SelectedRows(0))
                    MessageBox.Show("Meeting exported successfully!", "Export Meeting", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show($"Error exporting meeting: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub ExportMeetingToFile(filePath As String, selectedRow As DataGridViewRow)
        Dim content As New System.Text.StringBuilder()

        content.AppendLine("ARIA MEETING REPORT")
        content.AppendLine("=" & New String("="c, 50))
        content.AppendLine()
        content.AppendLine($"Meeting Title: {selectedRow.Cells("Title").Value}")
        content.AppendLine($"Date: {selectedRow.Cells("Date").Value}")
        content.AppendLine($"Duration: {selectedRow.Cells("Duration").Value}")
        content.AppendLine($"Participants: {selectedRow.Cells("Participants").Value}")
        content.AppendLine($"Status: {selectedRow.Cells("Status").Value}")
        content.AppendLine()
        content.AppendLine("TRANSCRIPTION:")
        content.AppendLine("-" & New String("-"c, 30))
        content.AppendLine(txtTranscription.Text)
        content.AppendLine()
        content.AppendLine("NOTES:")
        content.AppendLine("-" & New String("-"c, 30))
        content.AppendLine(txtNotes.Text)
        content.AppendLine()
        content.AppendLine($"Report generated on: {DateTime.Now}")

        File.WriteAllText(filePath, content.ToString())
    End Sub

    Private Sub btnDeleteMeeting_Click(sender As Object, e As EventArgs) Handles btnDeleteMeeting.Click
        If dgvMeetingHistory.SelectedRows.Count = 0 Then
            MessageBox.Show("Please select a meeting to delete.", "Delete Meeting", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim result As DialogResult = MessageBox.Show("Are you sure you want to delete this meeting? This action cannot be undone.", "Delete Meeting", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            Try
                Dim meetingTitle = dgvMeetingHistory.SelectedRows(0).Cells("Title").Value.ToString()
                meetingManager.DeleteMeeting(meetingTitle)
                LoadMeetingHistory()
                MessageBox.Show("Meeting deleted successfully!", "Delete Meeting", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show($"Error deleting meeting: {ex.Message}", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnThemeToggle_Click(sender As Object, e As EventArgs) Handles btnThemeToggle.Click
        isDarkTheme = Not isDarkTheme
        ApplyTheme()
        btnThemeToggle.Text = If(isDarkTheme, "Light Theme", "Dark Theme")
    End Sub

    ' Voice engine event handlers
    Private Sub voiceEngine_TranscriptionReceived(sender As Object, e As TranscriptionEventArgs) Handles voiceEngine.TranscriptionReceived
        If InvokeRequired Then
            Invoke(Sub() AddTranscription(e.Text))
        Else
            AddTranscription(e.Text)
        End If
    End Sub

    Private Sub AddTranscription(text As String)
        Dim timestampedText = $"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}"
        transcriptionBuilder.Append(timestampedText)
        txtTranscription.AppendText(timestampedText)

        ' Auto-scroll to bottom
        txtTranscription.SelectionStart = txtTranscription.Text.Length
        txtTranscription.ScrollToCaret()
    End Sub

    Private Sub voiceEngine_ErrorOccurred(sender As Object, e As ErrorEventArgs) Handles voiceEngine.ErrorOccurred
        If InvokeRequired Then
            Invoke(Sub() ShowVoiceError(e.GetException()))
        Else
            ShowVoiceError(e.GetException())
        End If
    End Sub

    Private Sub ShowVoiceError(ex As Exception)
        lblRecordingStatus.Text = $"Error: {ex.Message}"
        lblRecordingStatus.ForeColor = Color.Red
        picRecordingIndicator.BackColor = Color.Red
    End Sub

    ' Cost tracker event handlers
    Private Sub costTracker_CostUpdated(sender As Object, e As CostUpdateEventArgs) Handles costTracker.CostUpdated
        If InvokeRequired Then
            Invoke(Sub() UpdateCostDisplay())
        Else
            UpdateCostDisplay()
        End If
    End Sub

    ' Data grid event handlers
    Private Sub dgvMeetingHistory_SelectionChanged(sender As Object, e As EventArgs) Handles dgvMeetingHistory.SelectionChanged
        If dgvMeetingHistory.SelectedRows.Count > 0 Then
            Try
                Dim selectedTitle = dgvMeetingHistory.SelectedRows(0).Cells("Title").Value.ToString()
                Dim meeting = meetingManager.GetMeeting(selectedTitle)

                If meeting IsNot Nothing Then
                    txtTranscription.Text = meeting.Transcription
                    txtNotes.Text = meeting.Notes
                End If
            Catch ex As Exception
                ' Handle selection errors gracefully
            End Try
        End If
    End Sub

    Private Sub dgvMeetingHistory_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvMeetingHistory.CellDoubleClick
        If e.RowIndex >= 0 Then
            btnExportMeeting_Click(sender, e)
        End If
    End Sub

    ' Form event handlers
    Private Sub MeetingForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Text = "ARIA Meeting Manager"
        lblRecordingTime.Text = "00:00:00"
        UpdateUI()
    End Sub

    Private Sub MeetingForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If isRecording Then
            Dim result As DialogResult = MessageBox.Show("A recording is currently in progress. Do you want to stop the recording and save the meeting?", "Recording in Progress", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)

            Select Case result
                Case DialogResult.Yes
                    btnStopRecording_Click(sender, e)
                Case DialogResult.Cancel
                    e.Cancel = True
                    Return
            End Select
        End If

        Try
            recordingTimer?.Stop()
            voiceEngine?.Dispose()
            meetingManager?.Dispose()
            costTracker?.Dispose()
        Catch ex As Exception
            ' Handle cleanup errors gracefully
        End Try
    End Sub

    ' Audio level monitoring
    Private Sub tmrAudioLevel_Tick(sender As Object, e As EventArgs) Handles tmrAudioLevel.Tick
        If isRecording And Not isPaused And voiceEngine IsNot Nothing Then
            Try
                Dim audioLevel = voiceEngine.GetAudioLevel()
                UpdateAudioLevelIndicator(audioLevel)
            Catch ex As Exception
                ' Handle audio level errors gracefully
            End Try
        End If
    End Sub

    Private Sub UpdateAudioLevelIndicator(level As Single)
        ' Update progress bar or custom audio level visualization
        If prgAudioLevel IsNot Nothing Then
            prgAudioLevel.Value = Math.Min(100, Math.Max(0, CInt(level * 100)))
        End If
    End Sub

    ' Additional helper methods
    Private Sub ClearCurrentSession()
        txtMeetingTitle.Clear()
        txtParticipants.Clear()
        txtTranscription.Clear()
        txtNotes.Clear()
        transcriptionBuilder.Clear()
        lblRecordingTime.Text = "00:00:00"
        currentMeeting = Nothing
    End Sub

    Private Sub btnNewMeeting_Click(sender As Object, e As EventArgs) Handles btnNewMeeting.Click
        If isRecording Then
            MessageBox.Show("Please stop the current recording before starting a new meeting.", "Recording in Progress", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ClearCurrentSession()
        txtMeetingTitle.Focus()
    End Sub
End Class