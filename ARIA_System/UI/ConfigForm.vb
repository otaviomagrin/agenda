Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports ARIA_Premium_System.Core
Imports ARIA_Premium_System.Security
Imports ARIA_Premium_System.Utils

Public Class ConfigForm
    Private configManager As ConfigurationManager
    Private encryptionService As EncryptionService
    Private isDarkTheme As Boolean = True
    Private hasUnsavedChanges As Boolean = False

    Public Sub New()
        InitializeComponent()
        InitializeConfiguration()
        ApplyTheme()
        LoadSettings()
    End Sub

    Private Sub InitializeConfiguration()
        Try
            configManager = New ConfigurationManager()
            encryptionService = New EncryptionService()
        Catch ex As Exception
            MessageBox.Show($"Error initializing configuration: {ex.Message}", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
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

        ' Update tab control
        tabConfiguration.BackColor = Color.FromArgb(40, 40, 40)

        ' Update all panels
        For Each tabPage As TabPage In tabConfiguration.TabPages
            tabPage.BackColor = Color.FromArgb(40, 40, 40)
            tabPage.ForeColor = Color.White

            For Each ctrl As Control In tabPage.Controls
                If TypeOf ctrl Is Panel Then
                    ctrl.BackColor = Color.FromArgb(45, 45, 45)
                ElseIf TypeOf ctrl Is TextBox Then
                    ctrl.BackColor = Color.FromArgb(60, 60, 60)
                    ctrl.ForeColor = Color.White
                ElseIf TypeOf ctrl Is ComboBox Then
                    ctrl.BackColor = Color.FromArgb(60, 60, 60)
                    ctrl.ForeColor = Color.White
                ElseIf TypeOf ctrl Is CheckBox Then
                    ctrl.BackColor = Color.FromArgb(45, 45, 45)
                    ctrl.ForeColor = Color.White
                ElseIf TypeOf ctrl Is Button Then
                    ctrl.BackColor = Color.FromArgb(60, 60, 60)
                    ctrl.ForeColor = Color.White
                End If
            Next
        Next
    End Sub

    Private Sub ApplyLightTheme()
        Me.BackColor = Color.White
        Me.ForeColor = Color.Black

        tabConfiguration.BackColor = Color.White

        For Each tabPage As TabPage In tabConfiguration.TabPages
            tabPage.BackColor = Color.White
            tabPage.ForeColor = Color.Black

            For Each ctrl As Control In tabPage.Controls
                If TypeOf ctrl Is Panel Then
                    ctrl.BackColor = Color.FromArgb(250, 250, 250)
                ElseIf TypeOf ctrl Is TextBox Then
                    ctrl.BackColor = Color.White
                    ctrl.ForeColor = Color.Black
                ElseIf TypeOf ctrl Is ComboBox Then
                    ctrl.BackColor = Color.White
                    ctrl.ForeColor = Color.Black
                ElseIf TypeOf ctrl Is CheckBox Then
                    ctrl.BackColor = Color.White
                    ctrl.ForeColor = Color.Black
                ElseIf TypeOf ctrl Is Button Then
                    ctrl.BackColor = Color.FromArgb(225, 225, 225)
                    ctrl.ForeColor = Color.Black
                End If
            Next
        Next
    End Sub

    Private Sub LoadSettings()
        Try
            If configManager Is Nothing Then Return

            ' Load API Keys
            txtOpenAIKey.Text = configManager.GetDecryptedApiKey("OpenAI")
            txtAzureKey.Text = configManager.GetDecryptedApiKey("Azure")
            txtGoogleKey.Text = configManager.GetDecryptedApiKey("Google")
            txtAzureEndpoint.Text = configManager.GetSetting("AzureEndpoint", "")
            txtAzureRegion.Text = configManager.GetSetting("AzureRegion", "")

            ' Load Voice Settings
            cmbVoiceProvider.SelectedItem = configManager.GetSetting("VoiceProvider", "Azure")
            cmbVoiceLanguage.SelectedItem = configManager.GetSetting("VoiceLanguage", "en-US")
            cmbVoiceGender.SelectedItem = configManager.GetSetting("VoiceGender", "Female")
            trackVoiceSpeed.Value = Integer.Parse(configManager.GetSetting("VoiceSpeed", "50"))
            trackVoicePitch.Value = Integer.Parse(configManager.GetSetting("VoicePitch", "50"))
            chkContinuousListening.Checked = Boolean.Parse(configManager.GetSetting("ContinuousListening", "False"))
            chkVoiceActivation.Checked = Boolean.Parse(configManager.GetSetting("VoiceActivation", "True"))

            ' Load Recording Settings
            cmbAudioDevice.SelectedItem = configManager.GetSetting("AudioDevice", "Default")
            cmbRecordingFormat.SelectedItem = configManager.GetSetting("RecordingFormat", "WAV")
            cmbSampleRate.SelectedItem = configManager.GetSetting("SampleRate", "44100")
            chkAutoSave.Checked = Boolean.Parse(configManager.GetSetting("AutoSave", "True"))
            chkBackgroundRecording.Checked = Boolean.Parse(configManager.GetSetting("BackgroundRecording", "False"))
            txtSaveLocation.Text = configManager.GetSetting("SaveLocation", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))

            ' Load System Settings
            chkStartWithWindows.Checked = Boolean.Parse(configManager.GetSetting("StartWithWindows", "False"))
            chkMinimizeToTray.Checked = Boolean.Parse(configManager.GetSetting("MinimizeToTray", "True"))
            chkEnableLogging.Checked = Boolean.Parse(configManager.GetSetting("EnableLogging", "True"))
            cmbLogLevel.SelectedItem = configManager.GetSetting("LogLevel", "Info")
            chkAutoUpdate.Checked = Boolean.Parse(configManager.GetSetting("AutoUpdate", "True"))
            chkUseDarkTheme.Checked = Boolean.Parse(configManager.GetSetting("UseDarkTheme", "True"))

            ' Load Cost Settings
            chkEnableCostTracking.Checked = Boolean.Parse(configManager.GetSetting("EnableCostTracking", "True"))
            numCostLimit.Value = Decimal.Parse(configManager.GetSetting("CostLimit", "10.00"))
            chkCostAlerts.Checked = Boolean.Parse(configManager.GetSetting("CostAlerts", "True"))
            numCostAlertThreshold.Value = Decimal.Parse(configManager.GetSetting("CostAlertThreshold", "5.00"))

            UpdateVoiceSpeedLabel()
            UpdateVoicePitchLabel()
            hasUnsavedChanges = False

        Catch ex As Exception
            MessageBox.Show($"Error loading settings: {ex.Message}", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub SaveSettings()
        Try
            If configManager Is Nothing Then Return

            ' Save API Keys (encrypted)
            configManager.SetEncryptedApiKey("OpenAI", txtOpenAIKey.Text)
            configManager.SetEncryptedApiKey("Azure", txtAzureKey.Text)
            configManager.SetEncryptedApiKey("Google", txtGoogleKey.Text)
            configManager.SetSetting("AzureEndpoint", txtAzureEndpoint.Text)
            configManager.SetSetting("AzureRegion", txtAzureRegion.Text)

            ' Save Voice Settings
            configManager.SetSetting("VoiceProvider", cmbVoiceProvider.SelectedItem?.ToString() ?? "Azure")
            configManager.SetSetting("VoiceLanguage", cmbVoiceLanguage.SelectedItem?.ToString() ?? "en-US")
            configManager.SetSetting("VoiceGender", cmbVoiceGender.SelectedItem?.ToString() ?? "Female")
            configManager.SetSetting("VoiceSpeed", trackVoiceSpeed.Value.ToString())
            configManager.SetSetting("VoicePitch", trackVoicePitch.Value.ToString())
            configManager.SetSetting("ContinuousListening", chkContinuousListening.Checked.ToString())
            configManager.SetSetting("VoiceActivation", chkVoiceActivation.Checked.ToString())

            ' Save Recording Settings
            configManager.SetSetting("AudioDevice", cmbAudioDevice.SelectedItem?.ToString() ?? "Default")
            configManager.SetSetting("RecordingFormat", cmbRecordingFormat.SelectedItem?.ToString() ?? "WAV")
            configManager.SetSetting("SampleRate", cmbSampleRate.SelectedItem?.ToString() ?? "44100")
            configManager.SetSetting("AutoSave", chkAutoSave.Checked.ToString())
            configManager.SetSetting("BackgroundRecording", chkBackgroundRecording.Checked.ToString())
            configManager.SetSetting("SaveLocation", txtSaveLocation.Text)

            ' Save System Settings
            configManager.SetSetting("StartWithWindows", chkStartWithWindows.Checked.ToString())
            configManager.SetSetting("MinimizeToTray", chkMinimizeToTray.Checked.ToString())
            configManager.SetSetting("EnableLogging", chkEnableLogging.Checked.ToString())
            configManager.SetSetting("LogLevel", cmbLogLevel.SelectedItem?.ToString() ?? "Info")
            configManager.SetSetting("AutoUpdate", chkAutoUpdate.Checked.ToString())
            configManager.SetSetting("UseDarkTheme", chkUseDarkTheme.Checked.ToString())

            ' Save Cost Settings
            configManager.SetSetting("EnableCostTracking", chkEnableCostTracking.Checked.ToString())
            configManager.SetSetting("CostLimit", numCostLimit.Value.ToString())
            configManager.SetSetting("CostAlerts", chkCostAlerts.Checked.ToString())
            configManager.SetSetting("CostAlertThreshold", numCostAlertThreshold.Value.ToString())

            configManager.SaveConfiguration()
            hasUnsavedChanges = False

            MessageBox.Show("Settings saved successfully!", "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Error saving settings: {ex.Message}", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Event handlers for control changes
    Private Sub OnSettingChanged(sender As Object, e As EventArgs) Handles txtOpenAIKey.TextChanged, txtAzureKey.TextChanged, txtGoogleKey.TextChanged, txtAzureEndpoint.TextChanged, txtAzureRegion.TextChanged, cmbVoiceProvider.SelectedIndexChanged, cmbVoiceLanguage.SelectedIndexChanged, cmbVoiceGender.SelectedIndexChanged, chkContinuousListening.CheckedChanged, chkVoiceActivation.CheckedChanged, cmbAudioDevice.SelectedIndexChanged, cmbRecordingFormat.SelectedIndexChanged, cmbSampleRate.SelectedIndexChanged, chkAutoSave.CheckedChanged, chkBackgroundRecording.CheckedChanged, txtSaveLocation.TextChanged, chkStartWithWindows.CheckedChanged, chkMinimizeToTray.CheckedChanged, chkEnableLogging.CheckedChanged, cmbLogLevel.SelectedIndexChanged, chkAutoUpdate.CheckedChanged, chkUseDarkTheme.CheckedChanged, chkEnableCostTracking.CheckedChanged, chkCostAlerts.CheckedChanged
        hasUnsavedChanges = True
    End Sub

    Private Sub trackVoiceSpeed_Scroll(sender As Object, e As EventArgs) Handles trackVoiceSpeed.Scroll
        UpdateVoiceSpeedLabel()
        hasUnsavedChanges = True
    End Sub

    Private Sub trackVoicePitch_Scroll(sender As Object, e As EventArgs) Handles trackVoicePitch.Scroll
        UpdateVoicePitchLabel()
        hasUnsavedChanges = True
    End Sub

    Private Sub numCostLimit_ValueChanged(sender As Object, e As EventArgs) Handles numCostLimit.ValueChanged
        hasUnsavedChanges = True
    End Sub

    Private Sub numCostAlertThreshold_ValueChanged(sender As Object, e As EventArgs) Handles numCostAlertThreshold.ValueChanged
        hasUnsavedChanges = True
    End Sub

    Private Sub UpdateVoiceSpeedLabel()
        lblVoiceSpeedValue.Text = $"Speed: {trackVoiceSpeed.Value}%"
    End Sub

    Private Sub UpdateVoicePitchLabel()
        lblVoicePitchValue.Text = $"Pitch: {trackVoicePitch.Value}%"
    End Sub

    ' Button event handlers
    Private Sub btnTestOpenAI_Click(sender As Object, e As EventArgs) Handles btnTestOpenAI.Click
        TestAPIConnection("OpenAI", txtOpenAIKey.Text)
    End Sub

    Private Sub btnTestAzure_Click(sender As Object, e As EventArgs) Handles btnTestAzure.Click
        TestAPIConnection("Azure", txtAzureKey.Text, txtAzureEndpoint.Text)
    End Sub

    Private Sub btnTestGoogle_Click(sender As Object, e As EventArgs) Handles btnTestGoogle.Click
        TestAPIConnection("Google", txtGoogleKey.Text)
    End Sub

    Private Sub TestAPIConnection(provider As String, apiKey As String, Optional endpoint As String = "")
        If String.IsNullOrWhiteSpace(apiKey) Then
            MessageBox.Show($"Please enter the {provider} API key first.", "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim testButton As Button = Nothing
            Select Case provider
                Case "OpenAI"
                    testButton = btnTestOpenAI
                Case "Azure"
                    testButton = btnTestAzure
                Case "Google"
                    testButton = btnTestGoogle
            End Select

            If testButton IsNot Nothing Then
                testButton.Text = "Testing..."
                testButton.Enabled = False
            End If

            ' Simulate API test (replace with actual API test)
            Task.Run(Sub()
                         Thread.Sleep(2000) ' Simulate network delay

                         Me.Invoke(Sub()
                                       If testButton IsNot Nothing Then
                                           testButton.Text = $"Test {provider}"
                                           testButton.Enabled = True
                                       End If
                                       MessageBox.Show($"{provider} API connection successful!", "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                   End Sub)
                     End Sub)

        Catch ex As Exception
            MessageBox.Show($"Error testing {provider} connection: {ex.Message}", "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBrowseSaveLocation_Click(sender As Object, e As EventArgs) Handles btnBrowseSaveLocation.Click
        Using folderDialog As New FolderBrowserDialog()
            folderDialog.Description = "Select folder for saving recordings"
            folderDialog.SelectedPath = txtSaveLocation.Text

            If folderDialog.ShowDialog() = DialogResult.OK Then
                txtSaveLocation.Text = folderDialog.SelectedPath
                hasUnsavedChanges = True
            End If
        End Using
    End Sub

    Private Sub btnThemeToggle_Click(sender As Object, e As EventArgs) Handles btnThemeToggle.Click
        isDarkTheme = Not isDarkTheme
        ApplyTheme()
        btnThemeToggle.Text = If(isDarkTheme, "Light Theme", "Dark Theme")
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        SaveSettings()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        If hasUnsavedChanges Then
            Dim result As DialogResult = MessageBox.Show("You have unsaved changes. Are you sure you want to cancel?", "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.No Then
                Return
            End If
        End If

        Me.Close()
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Dim result As DialogResult = MessageBox.Show("This will reset all settings to their default values. Are you sure?", "Reset Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
        If result = DialogResult.Yes Then
            ResetToDefaults()
        End If
    End Sub

    Private Sub btnExportSettings_Click(sender As Object, e As EventArgs) Handles btnExportSettings.Click
        Using saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Configuration Files (*.config)|*.config|All Files (*.*)|*.*"
            saveDialog.Title = "Export Settings"
            saveDialog.FileName = "ARIA_Settings.config"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                Try
                    configManager.ExportSettings(saveDialog.FileName)
                    MessageBox.Show("Settings exported successfully!", "Export Settings", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show($"Error exporting settings: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub btnImportSettings_Click(sender As Object, e As EventArgs) Handles btnImportSettings.Click
        Using openDialog As New OpenFileDialog()
            openDialog.Filter = "Configuration Files (*.config)|*.config|All Files (*.*)|*.*"
            openDialog.Title = "Import Settings"

            If openDialog.ShowDialog() = DialogResult.OK Then
                Try
                    configManager.ImportSettings(openDialog.FileName)
                    LoadSettings()
                    MessageBox.Show("Settings imported successfully!", "Import Settings", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    MessageBox.Show($"Error importing settings: {ex.Message}", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Private Sub ResetToDefaults()
        Try
            ' Reset API Keys
            txtOpenAIKey.Clear()
            txtAzureKey.Clear()
            txtGoogleKey.Clear()
            txtAzureEndpoint.Clear()
            txtAzureRegion.Clear()

            ' Reset Voice Settings
            cmbVoiceProvider.SelectedItem = "Azure"
            cmbVoiceLanguage.SelectedItem = "en-US"
            cmbVoiceGender.SelectedItem = "Female"
            trackVoiceSpeed.Value = 50
            trackVoicePitch.Value = 50
            chkContinuousListening.Checked = False
            chkVoiceActivation.Checked = True

            ' Reset Recording Settings
            cmbAudioDevice.SelectedItem = "Default"
            cmbRecordingFormat.SelectedItem = "WAV"
            cmbSampleRate.SelectedItem = "44100"
            chkAutoSave.Checked = True
            chkBackgroundRecording.Checked = False
            txtSaveLocation.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)

            ' Reset System Settings
            chkStartWithWindows.Checked = False
            chkMinimizeToTray.Checked = True
            chkEnableLogging.Checked = True
            cmbLogLevel.SelectedItem = "Info"
            chkAutoUpdate.Checked = True
            chkUseDarkTheme.Checked = True

            ' Reset Cost Settings
            chkEnableCostTracking.Checked = True
            numCostLimit.Value = 10.0D
            chkCostAlerts.Checked = True
            numCostAlertThreshold.Value = 5.0D

            UpdateVoiceSpeedLabel()
            UpdateVoicePitchLabel()
            hasUnsavedChanges = True

        Catch ex As Exception
            MessageBox.Show($"Error resetting settings: {ex.Message}", "Reset Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ConfigForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If hasUnsavedChanges Then
            Dim result As DialogResult = MessageBox.Show("You have unsaved changes. Do you want to save them before closing?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)

            Select Case result
                Case DialogResult.Yes
                    SaveSettings()
                Case DialogResult.Cancel
                    e.Cancel = True
            End Select
        End If
    End Sub

    Private Sub ConfigForm_Load(sender As Object, e As EventArgs) Handles Me.Load
        Me.Text = "ARIA Configuration"
        LoadAvailableDevices()
        PopulateComboBoxes()
    End Sub

    Private Sub LoadAvailableDevices()
        Try
            ' Load available audio devices (this would normally come from audio system)
            cmbAudioDevice.Items.Clear()
            cmbAudioDevice.Items.AddRange({"Default", "Microphone (Built-in)", "USB Microphone", "Bluetooth Headset"})
            cmbAudioDevice.SelectedIndex = 0
        Catch ex As Exception
            ' Handle device enumeration errors gracefully
        End Try
    End Sub

    Private Sub PopulateComboBoxes()
        ' Populate voice providers
        cmbVoiceProvider.Items.Clear()
        cmbVoiceProvider.Items.AddRange({"Azure", "OpenAI", "Google"})

        ' Populate languages
        cmbVoiceLanguage.Items.Clear()
        cmbVoiceLanguage.Items.AddRange({"en-US", "en-GB", "es-ES", "fr-FR", "de-DE", "it-IT", "pt-BR", "zh-CN", "ja-JP", "ko-KR"})

        ' Populate voice genders
        cmbVoiceGender.Items.Clear()
        cmbVoiceGender.Items.AddRange({"Female", "Male", "Neutral"})

        ' Populate recording formats
        cmbRecordingFormat.Items.Clear()
        cmbRecordingFormat.Items.AddRange({"WAV", "MP3", "FLAC", "AAC"})

        ' Populate sample rates
        cmbSampleRate.Items.Clear()
        cmbSampleRate.Items.AddRange({"8000", "16000", "22050", "44100", "48000", "96000"})

        ' Populate log levels
        cmbLogLevel.Items.Clear()
        cmbLogLevel.Items.AddRange({"Debug", "Info", "Warning", "Error", "Critical"})
    End Sub
End Class