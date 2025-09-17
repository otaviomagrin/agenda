<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ConfigForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ConfigForm))
        Me.tabConfiguration = New System.Windows.Forms.TabControl()
        Me.tabAPIKeys = New System.Windows.Forms.TabPage()
        Me.pnlAPIKeys = New System.Windows.Forms.Panel()
        Me.lblOpenAILabel = New System.Windows.Forms.Label()
        Me.txtOpenAIKey = New System.Windows.Forms.TextBox()
        Me.btnTestOpenAI = New System.Windows.Forms.Button()
        Me.lblAzureLabel = New System.Windows.Forms.Label()
        Me.txtAzureKey = New System.Windows.Forms.TextBox()
        Me.btnTestAzure = New System.Windows.Forms.Button()
        Me.lblAzureEndpointLabel = New System.Windows.Forms.Label()
        Me.txtAzureEndpoint = New System.Windows.Forms.TextBox()
        Me.lblAzureRegionLabel = New System.Windows.Forms.Label()
        Me.txtAzureRegion = New System.Windows.Forms.TextBox()
        Me.lblGoogleLabel = New System.Windows.Forms.Label()
        Me.txtGoogleKey = New System.Windows.Forms.TextBox()
        Me.btnTestGoogle = New System.Windows.Forms.Button()
        Me.tabVoiceSettings = New System.Windows.Forms.TabPage()
        Me.pnlVoiceSettings = New System.Windows.Forms.Panel()
        Me.lblVoiceProviderLabel = New System.Windows.Forms.Label()
        Me.cmbVoiceProvider = New System.Windows.Forms.ComboBox()
        Me.lblVoiceLanguageLabel = New System.Windows.Forms.Label()
        Me.cmbVoiceLanguage = New System.Windows.Forms.ComboBox()
        Me.lblVoiceGenderLabel = New System.Windows.Forms.Label()
        Me.cmbVoiceGender = New System.Windows.Forms.ComboBox()
        Me.lblVoiceSpeedLabel = New System.Windows.Forms.Label()
        Me.trackVoiceSpeed = New System.Windows.Forms.TrackBar()
        Me.lblVoiceSpeedValue = New System.Windows.Forms.Label()
        Me.lblVoicePitchLabel = New System.Windows.Forms.Label()
        Me.trackVoicePitch = New System.Windows.Forms.TrackBar()
        Me.lblVoicePitchValue = New System.Windows.Forms.Label()
        Me.chkContinuousListening = New System.Windows.Forms.CheckBox()
        Me.chkVoiceActivation = New System.Windows.Forms.CheckBox()
        Me.tabRecordingSettings = New System.Windows.Forms.TabPage()
        Me.pnlRecordingSettings = New System.Windows.Forms.Panel()
        Me.lblAudioDeviceLabel = New System.Windows.Forms.Label()
        Me.cmbAudioDevice = New System.Windows.Forms.ComboBox()
        Me.lblRecordingFormatLabel = New System.Windows.Forms.Label()
        Me.cmbRecordingFormat = New System.Windows.Forms.ComboBox()
        Me.lblSampleRateLabel = New System.Windows.Forms.Label()
        Me.cmbSampleRate = New System.Windows.Forms.ComboBox()
        Me.lblSaveLocationLabel = New System.Windows.Forms.Label()
        Me.txtSaveLocation = New System.Windows.Forms.TextBox()
        Me.btnBrowseSaveLocation = New System.Windows.Forms.Button()
        Me.chkAutoSave = New System.Windows.Forms.CheckBox()
        Me.chkBackgroundRecording = New System.Windows.Forms.CheckBox()
        Me.tabSystemSettings = New System.Windows.Forms.TabPage()
        Me.pnlSystemSettings = New System.Windows.Forms.Panel()
        Me.lblStartupLabel = New System.Windows.Forms.Label()
        Me.chkStartWithWindows = New System.Windows.Forms.CheckBox()
        Me.chkMinimizeToTray = New System.Windows.Forms.CheckBox()
        Me.lblLoggingLabel = New System.Windows.Forms.Label()
        Me.chkEnableLogging = New System.Windows.Forms.CheckBox()
        Me.lblLogLevelLabel = New System.Windows.Forms.Label()
        Me.cmbLogLevel = New System.Windows.Forms.ComboBox()
        Me.lblUpdatesLabel = New System.Windows.Forms.Label()
        Me.chkAutoUpdate = New System.Windows.Forms.CheckBox()
        Me.lblThemeLabel = New System.Windows.Forms.Label()
        Me.chkUseDarkTheme = New System.Windows.Forms.CheckBox()
        Me.tabCostSettings = New System.Windows.Forms.TabPage()
        Me.pnlCostSettings = New System.Windows.Forms.Panel()
        Me.chkEnableCostTracking = New System.Windows.Forms.CheckBox()
        Me.lblCostLimitLabel = New System.Windows.Forms.Label()
        Me.numCostLimit = New System.Windows.Forms.NumericUpDown()
        Me.lblCostLimitCurrency = New System.Windows.Forms.Label()
        Me.chkCostAlerts = New System.Windows.Forms.CheckBox()
        Me.lblCostAlertThresholdLabel = New System.Windows.Forms.Label()
        Me.numCostAlertThreshold = New System.Windows.Forms.NumericUpDown()
        Me.lblCostAlertCurrency = New System.Windows.Forms.Label()
        Me.pnlButtons = New System.Windows.Forms.Panel()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnReset = New System.Windows.Forms.Button()
        Me.btnThemeToggle = New System.Windows.Forms.Button()
        Me.btnExportSettings = New System.Windows.Forms.Button()
        Me.btnImportSettings = New System.Windows.Forms.Button()
        Me.tabConfiguration.SuspendLayout()
        Me.tabAPIKeys.SuspendLayout()
        Me.pnlAPIKeys.SuspendLayout()
        Me.tabVoiceSettings.SuspendLayout()
        Me.pnlVoiceSettings.SuspendLayout()
        CType(Me.trackVoiceSpeed, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.trackVoicePitch, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabRecordingSettings.SuspendLayout()
        Me.pnlRecordingSettings.SuspendLayout()
        Me.tabSystemSettings.SuspendLayout()
        Me.pnlSystemSettings.SuspendLayout()
        Me.tabCostSettings.SuspendLayout()
        Me.pnlCostSettings.SuspendLayout()
        CType(Me.numCostLimit, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numCostAlertThreshold, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlButtons.SuspendLayout()
        Me.SuspendLayout()
        '
        'tabConfiguration
        '
        Me.tabConfiguration.Controls.Add(Me.tabAPIKeys)
        Me.tabConfiguration.Controls.Add(Me.tabVoiceSettings)
        Me.tabConfiguration.Controls.Add(Me.tabRecordingSettings)
        Me.tabConfiguration.Controls.Add(Me.tabSystemSettings)
        Me.tabConfiguration.Controls.Add(Me.tabCostSettings)
        Me.tabConfiguration.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tabConfiguration.Location = New System.Drawing.Point(0, 0)
        Me.tabConfiguration.Name = "tabConfiguration"
        Me.tabConfiguration.SelectedIndex = 0
        Me.tabConfiguration.Size = New System.Drawing.Size(800, 520)
        Me.tabConfiguration.TabIndex = 0
        '
        'tabAPIKeys
        '
        Me.tabAPIKeys.Controls.Add(Me.pnlAPIKeys)
        Me.tabAPIKeys.Location = New System.Drawing.Point(4, 22)
        Me.tabAPIKeys.Name = "tabAPIKeys"
        Me.tabAPIKeys.Padding = New System.Windows.Forms.Padding(3)
        Me.tabAPIKeys.Size = New System.Drawing.Size(792, 494)
        Me.tabAPIKeys.TabIndex = 0
        Me.tabAPIKeys.Text = "API Keys"
        Me.tabAPIKeys.UseVisualStyleBackColor = True
        '
        'pnlAPIKeys
        '
        Me.pnlAPIKeys.Controls.Add(Me.lblOpenAILabel)
        Me.pnlAPIKeys.Controls.Add(Me.txtOpenAIKey)
        Me.pnlAPIKeys.Controls.Add(Me.btnTestOpenAI)
        Me.pnlAPIKeys.Controls.Add(Me.lblAzureLabel)
        Me.pnlAPIKeys.Controls.Add(Me.txtAzureKey)
        Me.pnlAPIKeys.Controls.Add(Me.btnTestAzure)
        Me.pnlAPIKeys.Controls.Add(Me.lblAzureEndpointLabel)
        Me.pnlAPIKeys.Controls.Add(Me.txtAzureEndpoint)
        Me.pnlAPIKeys.Controls.Add(Me.lblAzureRegionLabel)
        Me.pnlAPIKeys.Controls.Add(Me.txtAzureRegion)
        Me.pnlAPIKeys.Controls.Add(Me.lblGoogleLabel)
        Me.pnlAPIKeys.Controls.Add(Me.txtGoogleKey)
        Me.pnlAPIKeys.Controls.Add(Me.btnTestGoogle)
        Me.pnlAPIKeys.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlAPIKeys.Location = New System.Drawing.Point(3, 3)
        Me.pnlAPIKeys.Name = "pnlAPIKeys"
        Me.pnlAPIKeys.Size = New System.Drawing.Size(786, 488)
        Me.pnlAPIKeys.TabIndex = 0
        '
        'lblOpenAILabel
        '
        Me.lblOpenAILabel.AutoSize = True
        Me.lblOpenAILabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblOpenAILabel.Location = New System.Drawing.Point(20, 20)
        Me.lblOpenAILabel.Name = "lblOpenAILabel"
        Me.lblOpenAILabel.Size = New System.Drawing.Size(101, 19)
        Me.lblOpenAILabel.TabIndex = 0
        Me.lblOpenAILabel.Text = "OpenAI API Key:"
        '
        'txtOpenAIKey
        '
        Me.txtOpenAIKey.Location = New System.Drawing.Point(25, 45)
        Me.txtOpenAIKey.Name = "txtOpenAIKey"
        Me.txtOpenAIKey.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtOpenAIKey.Size = New System.Drawing.Size(400, 20)
        Me.txtOpenAIKey.TabIndex = 1
        '
        'btnTestOpenAI
        '
        Me.btnTestOpenAI.Location = New System.Drawing.Point(440, 43)
        Me.btnTestOpenAI.Name = "btnTestOpenAI"
        Me.btnTestOpenAI.Size = New System.Drawing.Size(100, 25)
        Me.btnTestOpenAI.TabIndex = 2
        Me.btnTestOpenAI.Text = "Test OpenAI"
        Me.btnTestOpenAI.UseVisualStyleBackColor = True
        '
        'lblAzureLabel
        '
        Me.lblAzureLabel.AutoSize = True
        Me.lblAzureLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAzureLabel.Location = New System.Drawing.Point(20, 90)
        Me.lblAzureLabel.Name = "lblAzureLabel"
        Me.lblAzureLabel.Size = New System.Drawing.Size(89, 19)
        Me.lblAzureLabel.TabIndex = 3
        Me.lblAzureLabel.Text = "Azure API Key:"
        '
        'txtAzureKey
        '
        Me.txtAzureKey.Location = New System.Drawing.Point(25, 115)
        Me.txtAzureKey.Name = "txtAzureKey"
        Me.txtAzureKey.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtAzureKey.Size = New System.Drawing.Size(400, 20)
        Me.txtAzureKey.TabIndex = 4
        '
        'btnTestAzure
        '
        Me.btnTestAzure.Location = New System.Drawing.Point(440, 113)
        Me.btnTestAzure.Name = "btnTestAzure"
        Me.btnTestAzure.Size = New System.Drawing.Size(100, 25)
        Me.btnTestAzure.TabIndex = 5
        Me.btnTestAzure.Text = "Test Azure"
        Me.btnTestAzure.UseVisualStyleBackColor = True
        '
        'lblAzureEndpointLabel
        '
        Me.lblAzureEndpointLabel.AutoSize = True
        Me.lblAzureEndpointLabel.Location = New System.Drawing.Point(25, 145)
        Me.lblAzureEndpointLabel.Name = "lblAzureEndpointLabel"
        Me.lblAzureEndpointLabel.Size = New System.Drawing.Size(81, 13)
        Me.lblAzureEndpointLabel.TabIndex = 6
        Me.lblAzureEndpointLabel.Text = "Azure Endpoint:"
        '
        'txtAzureEndpoint
        '
        Me.txtAzureEndpoint.Location = New System.Drawing.Point(25, 165)
        Me.txtAzureEndpoint.Name = "txtAzureEndpoint"
        Me.txtAzureEndpoint.Size = New System.Drawing.Size(400, 20)
        Me.txtAzureEndpoint.TabIndex = 7
        '
        'lblAzureRegionLabel
        '
        Me.lblAzureRegionLabel.AutoSize = True
        Me.lblAzureRegionLabel.Location = New System.Drawing.Point(25, 195)
        Me.lblAzureRegionLabel.Name = "lblAzureRegionLabel"
        Me.lblAzureRegionLabel.Size = New System.Drawing.Size(72, 13)
        Me.lblAzureRegionLabel.TabIndex = 8
        Me.lblAzureRegionLabel.Text = "Azure Region:"
        '
        'txtAzureRegion
        '
        Me.txtAzureRegion.Location = New System.Drawing.Point(25, 215)
        Me.txtAzureRegion.Name = "txtAzureRegion"
        Me.txtAzureRegion.Size = New System.Drawing.Size(200, 20)
        Me.txtAzureRegion.TabIndex = 9
        '
        'lblGoogleLabel
        '
        Me.lblGoogleLabel.AutoSize = True
        Me.lblGoogleLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblGoogleLabel.Location = New System.Drawing.Point(20, 260)
        Me.lblGoogleLabel.Name = "lblGoogleLabel"
        Me.lblGoogleLabel.Size = New System.Drawing.Size(107, 19)
        Me.lblGoogleLabel.TabIndex = 10
        Me.lblGoogleLabel.Text = "Google API Key:"
        '
        'txtGoogleKey
        '
        Me.txtGoogleKey.Location = New System.Drawing.Point(25, 285)
        Me.txtGoogleKey.Name = "txtGoogleKey"
        Me.txtGoogleKey.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtGoogleKey.Size = New System.Drawing.Size(400, 20)
        Me.txtGoogleKey.TabIndex = 11
        '
        'btnTestGoogle
        '
        Me.btnTestGoogle.Location = New System.Drawing.Point(440, 283)
        Me.btnTestGoogle.Name = "btnTestGoogle"
        Me.btnTestGoogle.Size = New System.Drawing.Size(100, 25)
        Me.btnTestGoogle.TabIndex = 12
        Me.btnTestGoogle.Text = "Test Google"
        Me.btnTestGoogle.UseVisualStyleBackColor = True
        '
        'tabVoiceSettings
        '
        Me.tabVoiceSettings.Controls.Add(Me.pnlVoiceSettings)
        Me.tabVoiceSettings.Location = New System.Drawing.Point(4, 22)
        Me.tabVoiceSettings.Name = "tabVoiceSettings"
        Me.tabVoiceSettings.Padding = New System.Windows.Forms.Padding(3)
        Me.tabVoiceSettings.Size = New System.Drawing.Size(792, 494)
        Me.tabVoiceSettings.TabIndex = 1
        Me.tabVoiceSettings.Text = "Voice Settings"
        Me.tabVoiceSettings.UseVisualStyleBackColor = True
        '
        'pnlVoiceSettings
        '
        Me.pnlVoiceSettings.Controls.Add(Me.lblVoiceProviderLabel)
        Me.pnlVoiceSettings.Controls.Add(Me.cmbVoiceProvider)
        Me.pnlVoiceSettings.Controls.Add(Me.lblVoiceLanguageLabel)
        Me.pnlVoiceSettings.Controls.Add(Me.cmbVoiceLanguage)
        Me.pnlVoiceSettings.Controls.Add(Me.lblVoiceGenderLabel)
        Me.pnlVoiceSettings.Controls.Add(Me.cmbVoiceGender)
        Me.pnlVoiceSettings.Controls.Add(Me.lblVoiceSpeedLabel)
        Me.pnlVoiceSettings.Controls.Add(Me.trackVoiceSpeed)
        Me.pnlVoiceSettings.Controls.Add(Me.lblVoiceSpeedValue)
        Me.pnlVoiceSettings.Controls.Add(Me.lblVoicePitchLabel)
        Me.pnlVoiceSettings.Controls.Add(Me.trackVoicePitch)
        Me.pnlVoiceSettings.Controls.Add(Me.lblVoicePitchValue)
        Me.pnlVoiceSettings.Controls.Add(Me.chkContinuousListening)
        Me.pnlVoiceSettings.Controls.Add(Me.chkVoiceActivation)
        Me.pnlVoiceSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlVoiceSettings.Location = New System.Drawing.Point(3, 3)
        Me.pnlVoiceSettings.Name = "pnlVoiceSettings"
        Me.pnlVoiceSettings.Size = New System.Drawing.Size(786, 488)
        Me.pnlVoiceSettings.TabIndex = 0
        '
        'lblVoiceProviderLabel
        '
        Me.lblVoiceProviderLabel.AutoSize = True
        Me.lblVoiceProviderLabel.Location = New System.Drawing.Point(20, 20)
        Me.lblVoiceProviderLabel.Name = "lblVoiceProviderLabel"
        Me.lblVoiceProviderLabel.Size = New System.Drawing.Size(76, 13)
        Me.lblVoiceProviderLabel.TabIndex = 0
        Me.lblVoiceProviderLabel.Text = "Voice Provider:"
        '
        'cmbVoiceProvider
        '
        Me.cmbVoiceProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbVoiceProvider.Location = New System.Drawing.Point(25, 40)
        Me.cmbVoiceProvider.Name = "cmbVoiceProvider"
        Me.cmbVoiceProvider.Size = New System.Drawing.Size(200, 21)
        Me.cmbVoiceProvider.TabIndex = 1
        '
        'lblVoiceLanguageLabel
        '
        Me.lblVoiceLanguageLabel.AutoSize = True
        Me.lblVoiceLanguageLabel.Location = New System.Drawing.Point(20, 80)
        Me.lblVoiceLanguageLabel.Name = "lblVoiceLanguageLabel"
        Me.lblVoiceLanguageLabel.Size = New System.Drawing.Size(58, 13)
        Me.lblVoiceLanguageLabel.TabIndex = 2
        Me.lblVoiceLanguageLabel.Text = "Language:"
        '
        'cmbVoiceLanguage
        '
        Me.cmbVoiceLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbVoiceLanguage.Location = New System.Drawing.Point(25, 100)
        Me.cmbVoiceLanguage.Name = "cmbVoiceLanguage"
        Me.cmbVoiceLanguage.Size = New System.Drawing.Size(200, 21)
        Me.cmbVoiceLanguage.TabIndex = 3
        '
        'lblVoiceGenderLabel
        '
        Me.lblVoiceGenderLabel.AutoSize = True
        Me.lblVoiceGenderLabel.Location = New System.Drawing.Point(20, 140)
        Me.lblVoiceGenderLabel.Name = "lblVoiceGenderLabel"
        Me.lblVoiceGenderLabel.Size = New System.Drawing.Size(45, 13)
        Me.lblVoiceGenderLabel.TabIndex = 4
        Me.lblVoiceGenderLabel.Text = "Gender:"
        '
        'cmbVoiceGender
        '
        Me.cmbVoiceGender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbVoiceGender.Location = New System.Drawing.Point(25, 160)
        Me.cmbVoiceGender.Name = "cmbVoiceGender"
        Me.cmbVoiceGender.Size = New System.Drawing.Size(200, 21)
        Me.cmbVoiceGender.TabIndex = 5
        '
        'lblVoiceSpeedLabel
        '
        Me.lblVoiceSpeedLabel.AutoSize = True
        Me.lblVoiceSpeedLabel.Location = New System.Drawing.Point(20, 200)
        Me.lblVoiceSpeedLabel.Name = "lblVoiceSpeedLabel"
        Me.lblVoiceSpeedLabel.Size = New System.Drawing.Size(71, 13)
        Me.lblVoiceSpeedLabel.TabIndex = 6
        Me.lblVoiceSpeedLabel.Text = "Voice Speed:"
        '
        'trackVoiceSpeed
        '
        Me.trackVoiceSpeed.Location = New System.Drawing.Point(25, 220)
        Me.trackVoiceSpeed.Maximum = 100
        Me.trackVoiceSpeed.Name = "trackVoiceSpeed"
        Me.trackVoiceSpeed.Size = New System.Drawing.Size(300, 45)
        Me.trackVoiceSpeed.TabIndex = 7
        Me.trackVoiceSpeed.TickFrequency = 10
        Me.trackVoiceSpeed.Value = 50
        '
        'lblVoiceSpeedValue
        '
        Me.lblVoiceSpeedValue.AutoSize = True
        Me.lblVoiceSpeedValue.Location = New System.Drawing.Point(340, 230)
        Me.lblVoiceSpeedValue.Name = "lblVoiceSpeedValue"
        Me.lblVoiceSpeedValue.Size = New System.Drawing.Size(64, 13)
        Me.lblVoiceSpeedValue.TabIndex = 8
        Me.lblVoiceSpeedValue.Text = "Speed: 50%"
        '
        'lblVoicePitchLabel
        '
        Me.lblVoicePitchLabel.AutoSize = True
        Me.lblVoicePitchLabel.Location = New System.Drawing.Point(20, 280)
        Me.lblVoicePitchLabel.Name = "lblVoicePitchLabel"
        Me.lblVoicePitchLabel.Size = New System.Drawing.Size(65, 13)
        Me.lblVoicePitchLabel.TabIndex = 9
        Me.lblVoicePitchLabel.Text = "Voice Pitch:"
        '
        'trackVoicePitch
        '
        Me.trackVoicePitch.Location = New System.Drawing.Point(25, 300)
        Me.trackVoicePitch.Maximum = 100
        Me.trackVoicePitch.Name = "trackVoicePitch"
        Me.trackVoicePitch.Size = New System.Drawing.Size(300, 45)
        Me.trackVoicePitch.TabIndex = 10
        Me.trackVoicePitch.TickFrequency = 10
        Me.trackVoicePitch.Value = 50
        '
        'lblVoicePitchValue
        '
        Me.lblVoicePitchValue.AutoSize = True
        Me.lblVoicePitchValue.Location = New System.Drawing.Point(340, 310)
        Me.lblVoicePitchValue.Name = "lblVoicePitchValue"
        Me.lblVoicePitchValue.Size = New System.Drawing.Size(58, 13)
        Me.lblVoicePitchValue.TabIndex = 11
        Me.lblVoicePitchValue.Text = "Pitch: 50%"
        '
        'chkContinuousListening
        '
        Me.chkContinuousListening.AutoSize = True
        Me.chkContinuousListening.Location = New System.Drawing.Point(25, 360)
        Me.chkContinuousListening.Name = "chkContinuousListening"
        Me.chkContinuousListening.Size = New System.Drawing.Size(123, 17)
        Me.chkContinuousListening.TabIndex = 12
        Me.chkContinuousListening.Text = "Continuous Listening"
        Me.chkContinuousListening.UseVisualStyleBackColor = True
        '
        'chkVoiceActivation
        '
        Me.chkVoiceActivation.AutoSize = True
        Me.chkVoiceActivation.Location = New System.Drawing.Point(25, 390)
        Me.chkVoiceActivation.Name = "chkVoiceActivation"
        Me.chkVoiceActivation.Size = New System.Drawing.Size(102, 17)
        Me.chkVoiceActivation.TabIndex = 13
        Me.chkVoiceActivation.Text = "Voice Activation"
        Me.chkVoiceActivation.UseVisualStyleBackColor = True
        '
        'tabRecordingSettings
        '
        Me.tabRecordingSettings.Controls.Add(Me.pnlRecordingSettings)
        Me.tabRecordingSettings.Location = New System.Drawing.Point(4, 22)
        Me.tabRecordingSettings.Name = "tabRecordingSettings"
        Me.tabRecordingSettings.Padding = New System.Windows.Forms.Padding(3)
        Me.tabRecordingSettings.Size = New System.Drawing.Size(792, 494)
        Me.tabRecordingSettings.TabIndex = 2
        Me.tabRecordingSettings.Text = "Recording"
        Me.tabRecordingSettings.UseVisualStyleBackColor = True
        '
        'pnlRecordingSettings
        '
        Me.pnlRecordingSettings.Controls.Add(Me.lblAudioDeviceLabel)
        Me.pnlRecordingSettings.Controls.Add(Me.cmbAudioDevice)
        Me.pnlRecordingSettings.Controls.Add(Me.lblRecordingFormatLabel)
        Me.pnlRecordingSettings.Controls.Add(Me.cmbRecordingFormat)
        Me.pnlRecordingSettings.Controls.Add(Me.lblSampleRateLabel)
        Me.pnlRecordingSettings.Controls.Add(Me.cmbSampleRate)
        Me.pnlRecordingSettings.Controls.Add(Me.lblSaveLocationLabel)
        Me.pnlRecordingSettings.Controls.Add(Me.txtSaveLocation)
        Me.pnlRecordingSettings.Controls.Add(Me.btnBrowseSaveLocation)
        Me.pnlRecordingSettings.Controls.Add(Me.chkAutoSave)
        Me.pnlRecordingSettings.Controls.Add(Me.chkBackgroundRecording)
        Me.pnlRecordingSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlRecordingSettings.Location = New System.Drawing.Point(3, 3)
        Me.pnlRecordingSettings.Name = "pnlRecordingSettings"
        Me.pnlRecordingSettings.Size = New System.Drawing.Size(786, 488)
        Me.pnlRecordingSettings.TabIndex = 0
        '
        'lblAudioDeviceLabel
        '
        Me.lblAudioDeviceLabel.AutoSize = True
        Me.lblAudioDeviceLabel.Location = New System.Drawing.Point(20, 20)
        Me.lblAudioDeviceLabel.Name = "lblAudioDeviceLabel"
        Me.lblAudioDeviceLabel.Size = New System.Drawing.Size(72, 13)
        Me.lblAudioDeviceLabel.TabIndex = 0
        Me.lblAudioDeviceLabel.Text = "Audio Device:"
        '
        'cmbAudioDevice
        '
        Me.cmbAudioDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbAudioDevice.Location = New System.Drawing.Point(25, 40)
        Me.cmbAudioDevice.Name = "cmbAudioDevice"
        Me.cmbAudioDevice.Size = New System.Drawing.Size(300, 21)
        Me.cmbAudioDevice.TabIndex = 1
        '
        'lblRecordingFormatLabel
        '
        Me.lblRecordingFormatLabel.AutoSize = True
        Me.lblRecordingFormatLabel.Location = New System.Drawing.Point(20, 80)
        Me.lblRecordingFormatLabel.Name = "lblRecordingFormatLabel"
        Me.lblRecordingFormatLabel.Size = New System.Drawing.Size(93, 13)
        Me.lblRecordingFormatLabel.TabIndex = 2
        Me.lblRecordingFormatLabel.Text = "Recording Format:"
        '
        'cmbRecordingFormat
        '
        Me.cmbRecordingFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRecordingFormat.Location = New System.Drawing.Point(25, 100)
        Me.cmbRecordingFormat.Name = "cmbRecordingFormat"
        Me.cmbRecordingFormat.Size = New System.Drawing.Size(200, 21)
        Me.cmbRecordingFormat.TabIndex = 3
        '
        'lblSampleRateLabel
        '
        Me.lblSampleRateLabel.AutoSize = True
        Me.lblSampleRateLabel.Location = New System.Drawing.Point(20, 140)
        Me.lblSampleRateLabel.Name = "lblSampleRateLabel"
        Me.lblSampleRateLabel.Size = New System.Drawing.Size(70, 13)
        Me.lblSampleRateLabel.TabIndex = 4
        Me.lblSampleRateLabel.Text = "Sample Rate:"
        '
        'cmbSampleRate
        '
        Me.cmbSampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbSampleRate.Location = New System.Drawing.Point(25, 160)
        Me.cmbSampleRate.Name = "cmbSampleRate"
        Me.cmbSampleRate.Size = New System.Drawing.Size(200, 21)
        Me.cmbSampleRate.TabIndex = 5
        '
        'lblSaveLocationLabel
        '
        Me.lblSaveLocationLabel.AutoSize = True
        Me.lblSaveLocationLabel.Location = New System.Drawing.Point(20, 200)
        Me.lblSaveLocationLabel.Name = "lblSaveLocationLabel"
        Me.lblSaveLocationLabel.Size = New System.Drawing.Size(79, 13)
        Me.lblSaveLocationLabel.TabIndex = 6
        Me.lblSaveLocationLabel.Text = "Save Location:"
        '
        'txtSaveLocation
        '
        Me.txtSaveLocation.Location = New System.Drawing.Point(25, 220)
        Me.txtSaveLocation.Name = "txtSaveLocation"
        Me.txtSaveLocation.Size = New System.Drawing.Size(400, 20)
        Me.txtSaveLocation.TabIndex = 7
        '
        'btnBrowseSaveLocation
        '
        Me.btnBrowseSaveLocation.Location = New System.Drawing.Point(440, 218)
        Me.btnBrowseSaveLocation.Name = "btnBrowseSaveLocation"
        Me.btnBrowseSaveLocation.Size = New System.Drawing.Size(75, 25)
        Me.btnBrowseSaveLocation.TabIndex = 8
        Me.btnBrowseSaveLocation.Text = "Browse..."
        Me.btnBrowseSaveLocation.UseVisualStyleBackColor = True
        '
        'chkAutoSave
        '
        Me.chkAutoSave.AutoSize = True
        Me.chkAutoSave.Location = New System.Drawing.Point(25, 260)
        Me.chkAutoSave.Name = "chkAutoSave"
        Me.chkAutoSave.Size = New System.Drawing.Size(73, 17)
        Me.chkAutoSave.TabIndex = 9
        Me.chkAutoSave.Text = "Auto Save"
        Me.chkAutoSave.UseVisualStyleBackColor = True
        '
        'chkBackgroundRecording
        '
        Me.chkBackgroundRecording.AutoSize = True
        Me.chkBackgroundRecording.Location = New System.Drawing.Point(25, 290)
        Me.chkBackgroundRecording.Name = "chkBackgroundRecording"
        Me.chkBackgroundRecording.Size = New System.Drawing.Size(133, 17)
        Me.chkBackgroundRecording.TabIndex = 10
        Me.chkBackgroundRecording.Text = "Background Recording"
        Me.chkBackgroundRecording.UseVisualStyleBackColor = True
        '
        'tabSystemSettings
        '
        Me.tabSystemSettings.Controls.Add(Me.pnlSystemSettings)
        Me.tabSystemSettings.Location = New System.Drawing.Point(4, 22)
        Me.tabSystemSettings.Name = "tabSystemSettings"
        Me.tabSystemSettings.Padding = New System.Windows.Forms.Padding(3)
        Me.tabSystemSettings.Size = New System.Drawing.Size(792, 494)
        Me.tabSystemSettings.TabIndex = 3
        Me.tabSystemSettings.Text = "System"
        Me.tabSystemSettings.UseVisualStyleBackColor = True
        '
        'pnlSystemSettings
        '
        Me.pnlSystemSettings.Controls.Add(Me.lblStartupLabel)
        Me.pnlSystemSettings.Controls.Add(Me.chkStartWithWindows)
        Me.pnlSystemSettings.Controls.Add(Me.chkMinimizeToTray)
        Me.pnlSystemSettings.Controls.Add(Me.lblLoggingLabel)
        Me.pnlSystemSettings.Controls.Add(Me.chkEnableLogging)
        Me.pnlSystemSettings.Controls.Add(Me.lblLogLevelLabel)
        Me.pnlSystemSettings.Controls.Add(Me.cmbLogLevel)
        Me.pnlSystemSettings.Controls.Add(Me.lblUpdatesLabel)
        Me.pnlSystemSettings.Controls.Add(Me.chkAutoUpdate)
        Me.pnlSystemSettings.Controls.Add(Me.lblThemeLabel)
        Me.pnlSystemSettings.Controls.Add(Me.chkUseDarkTheme)
        Me.pnlSystemSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlSystemSettings.Location = New System.Drawing.Point(3, 3)
        Me.pnlSystemSettings.Name = "pnlSystemSettings"
        Me.pnlSystemSettings.Size = New System.Drawing.Size(786, 488)
        Me.pnlSystemSettings.TabIndex = 0
        '
        'lblStartupLabel
        '
        Me.lblStartupLabel.AutoSize = True
        Me.lblStartupLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStartupLabel.Location = New System.Drawing.Point(20, 20)
        Me.lblStartupLabel.Name = "lblStartupLabel"
        Me.lblStartupLabel.Size = New System.Drawing.Size(57, 19)
        Me.lblStartupLabel.TabIndex = 0
        Me.lblStartupLabel.Text = "Startup:"
        '
        'chkStartWithWindows
        '
        Me.chkStartWithWindows.AutoSize = True
        Me.chkStartWithWindows.Location = New System.Drawing.Point(25, 50)
        Me.chkStartWithWindows.Name = "chkStartWithWindows"
        Me.chkStartWithWindows.Size = New System.Drawing.Size(115, 17)
        Me.chkStartWithWindows.TabIndex = 1
        Me.chkStartWithWindows.Text = "Start with Windows"
        Me.chkStartWithWindows.UseVisualStyleBackColor = True
        '
        'chkMinimizeToTray
        '
        Me.chkMinimizeToTray.AutoSize = True
        Me.chkMinimizeToTray.Location = New System.Drawing.Point(25, 80)
        Me.chkMinimizeToTray.Name = "chkMinimizeToTray"
        Me.chkMinimizeToTray.Size = New System.Drawing.Size(103, 17)
        Me.chkMinimizeToTray.TabIndex = 2
        Me.chkMinimizeToTray.Text = "Minimize to Tray"
        Me.chkMinimizeToTray.UseVisualStyleBackColor = True
        '
        'lblLoggingLabel
        '
        Me.lblLoggingLabel.AutoSize = True
        Me.lblLoggingLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLoggingLabel.Location = New System.Drawing.Point(20, 120)
        Me.lblLoggingLabel.Name = "lblLoggingLabel"
        Me.lblLoggingLabel.Size = New System.Drawing.Size(62, 19)
        Me.lblLoggingLabel.TabIndex = 3
        Me.lblLoggingLabel.Text = "Logging:"
        '
        'chkEnableLogging
        '
        Me.chkEnableLogging.AutoSize = True
        Me.chkEnableLogging.Location = New System.Drawing.Point(25, 150)
        Me.chkEnableLogging.Name = "chkEnableLogging"
        Me.chkEnableLogging.Size = New System.Drawing.Size(97, 17)
        Me.chkEnableLogging.TabIndex = 4
        Me.chkEnableLogging.Text = "Enable Logging"
        Me.chkEnableLogging.UseVisualStyleBackColor = True
        '
        'lblLogLevelLabel
        '
        Me.lblLogLevelLabel.AutoSize = True
        Me.lblLogLevelLabel.Location = New System.Drawing.Point(25, 180)
        Me.lblLogLevelLabel.Name = "lblLogLevelLabel"
        Me.lblLogLevelLabel.Size = New System.Drawing.Size(57, 13)
        Me.lblLogLevelLabel.TabIndex = 5
        Me.lblLogLevelLabel.Text = "Log Level:"
        '
        'cmbLogLevel
        '
        Me.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbLogLevel.Location = New System.Drawing.Point(25, 200)
        Me.cmbLogLevel.Name = "cmbLogLevel"
        Me.cmbLogLevel.Size = New System.Drawing.Size(150, 21)
        Me.cmbLogLevel.TabIndex = 6
        '
        'lblUpdatesLabel
        '
        Me.lblUpdatesLabel.AutoSize = True
        Me.lblUpdatesLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUpdatesLabel.Location = New System.Drawing.Point(20, 240)
        Me.lblUpdatesLabel.Name = "lblUpdatesLabel"
        Me.lblUpdatesLabel.Size = New System.Drawing.Size(64, 19)
        Me.lblUpdatesLabel.TabIndex = 7
        Me.lblUpdatesLabel.Text = "Updates:"
        '
        'chkAutoUpdate
        '
        Me.chkAutoUpdate.AutoSize = True
        Me.chkAutoUpdate.Location = New System.Drawing.Point(25, 270)
        Me.chkAutoUpdate.Name = "chkAutoUpdate"
        Me.chkAutoUpdate.Size = New System.Drawing.Size(85, 17)
        Me.chkAutoUpdate.TabIndex = 8
        Me.chkAutoUpdate.Text = "Auto Update"
        Me.chkAutoUpdate.UseVisualStyleBackColor = True
        '
        'lblThemeLabel
        '
        Me.lblThemeLabel.AutoSize = True
        Me.lblThemeLabel.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblThemeLabel.Location = New System.Drawing.Point(20, 310)
        Me.lblThemeLabel.Name = "lblThemeLabel"
        Me.lblThemeLabel.Size = New System.Drawing.Size(82, 19)
        Me.lblThemeLabel.TabIndex = 9
        Me.lblThemeLabel.Text = "Appearance:"
        '
        'chkUseDarkTheme
        '
        Me.chkUseDarkTheme.AutoSize = True
        Me.chkUseDarkTheme.Location = New System.Drawing.Point(25, 340)
        Me.chkUseDarkTheme.Name = "chkUseDarkTheme"
        Me.chkUseDarkTheme.Size = New System.Drawing.Size(102, 17)
        Me.chkUseDarkTheme.TabIndex = 10
        Me.chkUseDarkTheme.Text = "Use Dark Theme"
        Me.chkUseDarkTheme.UseVisualStyleBackColor = True
        '
        'tabCostSettings
        '
        Me.tabCostSettings.Controls.Add(Me.pnlCostSettings)
        Me.tabCostSettings.Location = New System.Drawing.Point(4, 22)
        Me.tabCostSettings.Name = "tabCostSettings"
        Me.tabCostSettings.Padding = New System.Windows.Forms.Padding(3)
        Me.tabCostSettings.Size = New System.Drawing.Size(792, 494)
        Me.tabCostSettings.TabIndex = 4
        Me.tabCostSettings.Text = "Cost Tracking"
        Me.tabCostSettings.UseVisualStyleBackColor = True
        '
        'pnlCostSettings
        '
        Me.pnlCostSettings.Controls.Add(Me.chkEnableCostTracking)
        Me.pnlCostSettings.Controls.Add(Me.lblCostLimitLabel)
        Me.pnlCostSettings.Controls.Add(Me.numCostLimit)
        Me.pnlCostSettings.Controls.Add(Me.lblCostLimitCurrency)
        Me.pnlCostSettings.Controls.Add(Me.chkCostAlerts)
        Me.pnlCostSettings.Controls.Add(Me.lblCostAlertThresholdLabel)
        Me.pnlCostSettings.Controls.Add(Me.numCostAlertThreshold)
        Me.pnlCostSettings.Controls.Add(Me.lblCostAlertCurrency)
        Me.pnlCostSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCostSettings.Location = New System.Drawing.Point(3, 3)
        Me.pnlCostSettings.Name = "pnlCostSettings"
        Me.pnlCostSettings.Size = New System.Drawing.Size(786, 488)
        Me.pnlCostSettings.TabIndex = 0
        '
        'chkEnableCostTracking
        '
        Me.chkEnableCostTracking.AutoSize = True
        Me.chkEnableCostTracking.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkEnableCostTracking.Location = New System.Drawing.Point(20, 20)
        Me.chkEnableCostTracking.Name = "chkEnableCostTracking"
        Me.chkEnableCostTracking.Size = New System.Drawing.Size(158, 23)
        Me.chkEnableCostTracking.TabIndex = 0
        Me.chkEnableCostTracking.Text = "Enable Cost Tracking"
        Me.chkEnableCostTracking.UseVisualStyleBackColor = True
        '
        'lblCostLimitLabel
        '
        Me.lblCostLimitLabel.AutoSize = True
        Me.lblCostLimitLabel.Location = New System.Drawing.Point(20, 60)
        Me.lblCostLimitLabel.Name = "lblCostLimitLabel"
        Me.lblCostLimitLabel.Size = New System.Drawing.Size(89, 13)
        Me.lblCostLimitLabel.TabIndex = 1
        Me.lblCostLimitLabel.Text = "Monthly Cost Limit:"
        '
        'numCostLimit
        '
        Me.numCostLimit.DecimalPlaces = 2
        Me.numCostLimit.Location = New System.Drawing.Point(25, 80)
        Me.numCostLimit.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.numCostLimit.Name = "numCostLimit"
        Me.numCostLimit.Size = New System.Drawing.Size(120, 20)
        Me.numCostLimit.TabIndex = 2
        Me.numCostLimit.Value = New Decimal(New Integer() {10, 0, 0, 0})
        '
        'lblCostLimitCurrency
        '
        Me.lblCostLimitCurrency.AutoSize = True
        Me.lblCostLimitCurrency.Location = New System.Drawing.Point(155, 82)
        Me.lblCostLimitCurrency.Name = "lblCostLimitCurrency"
        Me.lblCostLimitCurrency.Size = New System.Drawing.Size(26, 13)
        Me.lblCostLimitCurrency.TabIndex = 3
        Me.lblCostLimitCurrency.Text = "USD"
        '
        'chkCostAlerts
        '
        Me.chkCostAlerts.AutoSize = True
        Me.chkCostAlerts.Location = New System.Drawing.Point(25, 120)
        Me.chkCostAlerts.Name = "chkCostAlerts"
        Me.chkCostAlerts.Size = New System.Drawing.Size(99, 17)
        Me.chkCostAlerts.TabIndex = 4
        Me.chkCostAlerts.Text = "Enable Alerts"
        Me.chkCostAlerts.UseVisualStyleBackColor = True
        '
        'lblCostAlertThresholdLabel
        '
        Me.lblCostAlertThresholdLabel.AutoSize = True
        Me.lblCostAlertThresholdLabel.Location = New System.Drawing.Point(25, 150)
        Me.lblCostAlertThresholdLabel.Name = "lblCostAlertThresholdLabel"
        Me.lblCostAlertThresholdLabel.Size = New System.Drawing.Size(81, 13)
        Me.lblCostAlertThresholdLabel.TabIndex = 5
        Me.lblCostAlertThresholdLabel.Text = "Alert Threshold:"
        '
        'numCostAlertThreshold
        '
        Me.numCostAlertThreshold.DecimalPlaces = 2
        Me.numCostAlertThreshold.Location = New System.Drawing.Point(25, 170)
        Me.numCostAlertThreshold.Maximum = New Decimal(New Integer() {1000, 0, 0, 0})
        Me.numCostAlertThreshold.Name = "numCostAlertThreshold"
        Me.numCostAlertThreshold.Size = New System.Drawing.Size(120, 20)
        Me.numCostAlertThreshold.TabIndex = 6
        Me.numCostAlertThreshold.Value = New Decimal(New Integer() {5, 0, 0, 0})
        '
        'lblCostAlertCurrency
        '
        Me.lblCostAlertCurrency.AutoSize = True
        Me.lblCostAlertCurrency.Location = New System.Drawing.Point(155, 172)
        Me.lblCostAlertCurrency.Name = "lblCostAlertCurrency"
        Me.lblCostAlertCurrency.Size = New System.Drawing.Size(26, 13)
        Me.lblCostAlertCurrency.TabIndex = 7
        Me.lblCostAlertCurrency.Text = "USD"
        '
        'pnlButtons
        '
        Me.pnlButtons.Controls.Add(Me.btnSave)
        Me.pnlButtons.Controls.Add(Me.btnCancel)
        Me.pnlButtons.Controls.Add(Me.btnReset)
        Me.pnlButtons.Controls.Add(Me.btnThemeToggle)
        Me.pnlButtons.Controls.Add(Me.btnExportSettings)
        Me.pnlButtons.Controls.Add(Me.btnImportSettings)
        Me.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlButtons.Location = New System.Drawing.Point(0, 520)
        Me.pnlButtons.Name = "pnlButtons"
        Me.pnlButtons.Size = New System.Drawing.Size(800, 60)
        Me.pnlButtons.TabIndex = 1
        '
        'btnSave
        '
        Me.btnSave.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSave.Location = New System.Drawing.Point(500, 15)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(90, 35)
        Me.btnSave.TabIndex = 0
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        Me.btnCancel.Location = New System.Drawing.Point(600, 15)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(90, 35)
        Me.btnCancel.TabIndex = 1
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'btnReset
        '
        Me.btnReset.Location = New System.Drawing.Point(700, 15)
        Me.btnReset.Name = "btnReset"
        Me.btnReset.Size = New System.Drawing.Size(90, 35)
        Me.btnReset.TabIndex = 2
        Me.btnReset.Text = "Reset"
        Me.btnReset.UseVisualStyleBackColor = True
        '
        'btnThemeToggle
        '
        Me.btnThemeToggle.Location = New System.Drawing.Point(15, 15)
        Me.btnThemeToggle.Name = "btnThemeToggle"
        Me.btnThemeToggle.Size = New System.Drawing.Size(100, 35)
        Me.btnThemeToggle.TabIndex = 3
        Me.btnThemeToggle.Text = "Light Theme"
        Me.btnThemeToggle.UseVisualStyleBackColor = True
        '
        'btnExportSettings
        '
        Me.btnExportSettings.Location = New System.Drawing.Point(300, 15)
        Me.btnExportSettings.Name = "btnExportSettings"
        Me.btnExportSettings.Size = New System.Drawing.Size(90, 35)
        Me.btnExportSettings.TabIndex = 4
        Me.btnExportSettings.Text = "Export"
        Me.btnExportSettings.UseVisualStyleBackColor = True
        '
        'btnImportSettings
        '
        Me.btnImportSettings.Location = New System.Drawing.Point(400, 15)
        Me.btnImportSettings.Name = "btnImportSettings"
        Me.btnImportSettings.Size = New System.Drawing.Size(90, 35)
        Me.btnImportSettings.TabIndex = 5
        Me.btnImportSettings.Text = "Import"
        Me.btnImportSettings.UseVisualStyleBackColor = True
        '
        'ConfigForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 580)
        Me.Controls.Add(Me.tabConfiguration)
        Me.Controls.Add(Me.pnlButtons)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "ConfigForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "ARIA Configuration"
        Me.tabConfiguration.ResumeLayout(False)
        Me.tabAPIKeys.ResumeLayout(False)
        Me.pnlAPIKeys.ResumeLayout(False)
        Me.pnlAPIKeys.PerformLayout()
        Me.tabVoiceSettings.ResumeLayout(False)
        Me.pnlVoiceSettings.ResumeLayout(False)
        Me.pnlVoiceSettings.PerformLayout()
        CType(Me.trackVoiceSpeed, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.trackVoicePitch, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabRecordingSettings.ResumeLayout(False)
        Me.pnlRecordingSettings.ResumeLayout(False)
        Me.pnlRecordingSettings.PerformLayout()
        Me.tabSystemSettings.ResumeLayout(False)
        Me.pnlSystemSettings.ResumeLayout(False)
        Me.pnlSystemSettings.PerformLayout()
        Me.tabCostSettings.ResumeLayout(False)
        Me.pnlCostSettings.ResumeLayout(False)
        Me.pnlCostSettings.PerformLayout()
        CType(Me.numCostLimit, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numCostAlertThreshold, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlButtons.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents tabConfiguration As TabControl
    Friend WithEvents tabAPIKeys As TabPage
    Friend WithEvents pnlAPIKeys As Panel
    Friend WithEvents lblOpenAILabel As Label
    Friend WithEvents txtOpenAIKey As TextBox
    Friend WithEvents btnTestOpenAI As Button
    Friend WithEvents lblAzureLabel As Label
    Friend WithEvents txtAzureKey As TextBox
    Friend WithEvents btnTestAzure As Button
    Friend WithEvents lblAzureEndpointLabel As Label
    Friend WithEvents txtAzureEndpoint As TextBox
    Friend WithEvents lblAzureRegionLabel As Label
    Friend WithEvents txtAzureRegion As TextBox
    Friend WithEvents lblGoogleLabel As Label
    Friend WithEvents txtGoogleKey As TextBox
    Friend WithEvents btnTestGoogle As Button
    Friend WithEvents tabVoiceSettings As TabPage
    Friend WithEvents pnlVoiceSettings As Panel
    Friend WithEvents lblVoiceProviderLabel As Label
    Friend WithEvents cmbVoiceProvider As ComboBox
    Friend WithEvents lblVoiceLanguageLabel As Label
    Friend WithEvents cmbVoiceLanguage As ComboBox
    Friend WithEvents lblVoiceGenderLabel As Label
    Friend WithEvents cmbVoiceGender As ComboBox
    Friend WithEvents lblVoiceSpeedLabel As Label
    Friend WithEvents trackVoiceSpeed As TrackBar
    Friend WithEvents lblVoiceSpeedValue As Label
    Friend WithEvents lblVoicePitchLabel As Label
    Friend WithEvents trackVoicePitch As TrackBar
    Friend WithEvents lblVoicePitchValue As Label
    Friend WithEvents chkContinuousListening As CheckBox
    Friend WithEvents chkVoiceActivation As CheckBox
    Friend WithEvents tabRecordingSettings As TabPage
    Friend WithEvents pnlRecordingSettings As Panel
    Friend WithEvents lblAudioDeviceLabel As Label
    Friend WithEvents cmbAudioDevice As ComboBox
    Friend WithEvents lblRecordingFormatLabel As Label
    Friend WithEvents cmbRecordingFormat As ComboBox
    Friend WithEvents lblSampleRateLabel As Label
    Friend WithEvents cmbSampleRate As ComboBox
    Friend WithEvents lblSaveLocationLabel As Label
    Friend WithEvents txtSaveLocation As TextBox
    Friend WithEvents btnBrowseSaveLocation As Button
    Friend WithEvents chkAutoSave As CheckBox
    Friend WithEvents chkBackgroundRecording As CheckBox
    Friend WithEvents tabSystemSettings As TabPage
    Friend WithEvents pnlSystemSettings As Panel
    Friend WithEvents lblStartupLabel As Label
    Friend WithEvents chkStartWithWindows As CheckBox
    Friend WithEvents chkMinimizeToTray As CheckBox
    Friend WithEvents lblLoggingLabel As Label
    Friend WithEvents chkEnableLogging As CheckBox
    Friend WithEvents lblLogLevelLabel As Label
    Friend WithEvents cmbLogLevel As ComboBox
    Friend WithEvents lblUpdatesLabel As Label
    Friend WithEvents chkAutoUpdate As CheckBox
    Friend WithEvents lblThemeLabel As Label
    Friend WithEvents chkUseDarkTheme As CheckBox
    Friend WithEvents tabCostSettings As TabPage
    Friend WithEvents pnlCostSettings As Panel
    Friend WithEvents chkEnableCostTracking As CheckBox
    Friend WithEvents lblCostLimitLabel As Label
    Friend WithEvents numCostLimit As NumericUpDown
    Friend WithEvents lblCostLimitCurrency As Label
    Friend WithEvents chkCostAlerts As CheckBox
    Friend WithEvents lblCostAlertThresholdLabel As Label
    Friend WithEvents numCostAlertThreshold As NumericUpDown
    Friend WithEvents lblCostAlertCurrency As Label
    Friend WithEvents pnlButtons As Panel
    Friend WithEvents btnSave As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnReset As Button
    Friend WithEvents btnThemeToggle As Button
    Friend WithEvents btnExportSettings As Button
    Friend WithEvents btnImportSettings As Button
End Class