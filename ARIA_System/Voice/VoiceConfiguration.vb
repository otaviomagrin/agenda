' ===================================================================
' ARIA PREMIUM VOICE SYSTEM - CONFIGURATION MANAGER
' Gerenciador de configurações do sistema de voz
'
' Características:
' - Gerenciamento seguro de chaves de API
' - Configurações personalizáveis de qualidade
' - Persistência em arquivo criptografado
' - Validação de configurações
' ===================================================================

Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json

Namespace Voice
    Public Class VoiceConfiguration

        ' Caminhos de configuração
        Private Shared ReadOnly ConfigDirectory As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ARIA_Premium", "Voice")
        Private Shared ReadOnly ConfigFilePath As String = Path.Combine(ConfigDirectory, "voice_config.json")
        Private Shared ReadOnly EncryptedConfigPath As String = Path.Combine(ConfigDirectory, "voice_config.dat")

        ' Configurações de API
        Public Property AssemblyAIApiKey As String = ""
        Public Property ElevenLabsApiKey As String = ""
        Public Property PicovoiceAccessKey As String = ""
        Public Property OpenAIApiKey As String = ""

        ' Configurações de STT (Speech-to-Text)
        Public Property STTLanguage As String = "pt-br"
        Public Property UseAdvancedSTTModel As Boolean = False
        Public Property STTConfidenceThreshold As Double = 0.7

        ' Configurações de TTS (Text-to-Speech)
        Public Property SelectedVoiceId As String = ""
        Public Property UseHighQualityTTS As Boolean = True
        Public Property TTSStability As Double = 0.75
        Public Property TTSSimilarityBoost As Double = 0.85
        Public Property TTSStyle As Double = 0.5
        Public Property UseSpeakerBoost As Boolean = True

        ' Configurações de Wake Word
        Public Property WakeWordSensitivity As Double = 0.5
        Public Property CustomWakeWords As List(Of String)
        Public Property KeywordModelPath As String = ""
        Public Property ContextModelPath As String = ""

        ' Configurações de áudio
        Public Property AudioSampleRate As Integer = 16000
        Public Property AudioBitsPerSample As Integer = 16
        Public Property AudioChannels As Integer = 1

        ' Configurações avançadas
        Public Property EnableVoiceActivityDetection As Boolean = True
        Public Property NoiseReductionLevel As Integer = 2
        Public Property EchoReductionEnabled As Boolean = True
        Public Property AutoVolumeControl As Boolean = True

        ' Configurações de segurança
        Public Property EncryptConfiguration As Boolean = True
        Public Property UseSecureStorage As Boolean = True

        ' Configurações de log e debug
        Public Property EnableVoiceLogging As Boolean = False
        Public Property LogLevel As VoiceLogLevel = VoiceLogLevel.Info
        Public Property MaxLogFileSize As Long = 10 * 1024 * 1024 ' 10MB

        Public Sub New()
            ' Inicializar listas
            CustomWakeWords = New List(Of String) From {"aria", "hey aria", "ok aria"}

            ' Criar diretório se não existir
            EnsureConfigDirectoryExists()
        End Sub

        ' ===================================================================
        ' CARREGAMENTO E SALVAMENTO DE CONFIGURAÇÕES
        ' ===================================================================
        Public Sub LoadConfiguration()
            Try
                ' Tentar carregar configuração criptografada primeiro
                If EncryptConfiguration AndAlso File.Exists(EncryptedConfigPath) Then
                    LoadEncryptedConfiguration()
                ElseIf File.Exists(ConfigFilePath) Then
                    LoadPlainConfiguration()
                Else
                    ' Usar configurações padrão
                    SetDefaultConfiguration()
                    SaveConfiguration()
                End If

            Catch ex As Exception
                ' Em caso de erro, usar configurações padrão
                SetDefaultConfiguration()
                SaveConfiguration()
            End Try
        End Sub

        Public Sub SaveConfiguration()
            Try
                EnsureConfigDirectoryExists()

                If EncryptConfiguration Then
                    SaveEncryptedConfiguration()
                Else
                    SavePlainConfiguration()
                End If

            Catch ex As Exception
                Throw New Exception($"Erro ao salvar configurações: {ex.Message}", ex)
            End Try
        End Sub

        ' ===================================================================
        ' CONFIGURAÇÃO CRIPTOGRAFADA
        ' ===================================================================
        Private Sub LoadEncryptedConfiguration()
            Try
                Dim encryptedData As Byte() = File.ReadAllBytes(EncryptedConfigPath)
                Dim decryptedJson As String = DecryptData(encryptedData)

                Dim config As VoiceConfiguration = JsonConvert.DeserializeObject(Of VoiceConfiguration)(decryptedJson)
                CopyPropertiesFrom(config)

            Catch ex As Exception
                Throw New Exception($"Erro ao carregar configuração criptografada: {ex.Message}", ex)
            End Try
        End Sub

        Private Sub SaveEncryptedConfiguration()
            Try
                Dim jsonContent As String = JsonConvert.SerializeObject(Me, Formatting.Indented)
                Dim encryptedData As Byte() = EncryptData(jsonContent)

                File.WriteAllBytes(EncryptedConfigPath, encryptedData)

                ' Remover arquivo de configuração plain se existir
                If File.Exists(ConfigFilePath) Then
                    File.Delete(ConfigFilePath)
                End If

            Catch ex As Exception
                Throw New Exception($"Erro ao salvar configuração criptografada: {ex.Message}", ex)
            End Try
        End Sub

        ' ===================================================================
        ' CONFIGURAÇÃO PLAIN TEXT
        ' ===================================================================
        Private Sub LoadPlainConfiguration()
            Try
                Dim jsonContent As String = File.ReadAllText(ConfigFilePath, Encoding.UTF8)
                Dim config As VoiceConfiguration = JsonConvert.DeserializeObject(Of VoiceConfiguration)(jsonContent)
                CopyPropertiesFrom(config)

            Catch ex As Exception
                Throw New Exception($"Erro ao carregar configuração: {ex.Message}", ex)
            End Try
        End Sub

        Private Sub SavePlainConfiguration()
            Try
                Dim jsonContent As String = JsonConvert.SerializeObject(Me, Formatting.Indented)
                File.WriteAllText(ConfigFilePath, jsonContent, Encoding.UTF8)

            Catch ex As Exception
                Throw New Exception($"Erro ao salvar configuração: {ex.Message}", ex)
            End Try
        End Sub

        ' ===================================================================
        ' CRIPTOGRAFIA
        ' ===================================================================
        Private Function EncryptData(plainText As String) As Byte()
            Try
                Using aes As Aes = Aes.Create()
                    ' Usar chave derivada do usuário/máquina
                    Dim key As Byte() = GenerateKey()
                    aes.Key = key
                    aes.GenerateIV()

                    Using encryptor As ICryptoTransform = aes.CreateEncryptor()
                        Using ms As New MemoryStream()
                            ' Escrever IV primeiro
                            ms.Write(aes.IV, 0, aes.IV.Length)

                            Using cs As New CryptoStream(ms, encryptor, CryptoStreamMode.Write)
                                Using writer As New StreamWriter(cs)
                                    writer.Write(plainText)
                                End Using
                            End Using

                            Return ms.ToArray()
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Throw New Exception($"Erro na criptografia: {ex.Message}", ex)
            End Try
        End Function

        Private Function DecryptData(encryptedData As Byte()) As String
            Try
                Using aes As Aes = Aes.Create()
                    Dim key As Byte() = GenerateKey()
                    aes.Key = key

                    ' Ler IV dos primeiros bytes
                    Dim iv(aes.IV.Length - 1) As Byte
                    Array.Copy(encryptedData, 0, iv, 0, iv.Length)
                    aes.IV = iv

                    Using decryptor As ICryptoTransform = aes.CreateDecryptor()
                        Using ms As New MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length)
                            Using cs As New CryptoStream(ms, decryptor, CryptoStreamMode.Read)
                                Using reader As New StreamReader(cs)
                                    Return reader.ReadToEnd()
                                End Using
                            End Using
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Throw New Exception($"Erro na descriptografia: {ex.Message}", ex)
            End Try
        End Function

        Private Function GenerateKey() As Byte()
            ' Gerar chave baseada em características da máquina
            Dim machineKey As String = Environment.MachineName & Environment.UserName & "ARIA_VOICE_KEY"

            Using sha256 As SHA256 = SHA256.Create()
                Return sha256.ComputeHash(Encoding.UTF8.GetBytes(machineKey))
            End Using
        End Function

        ' ===================================================================
        ' CONFIGURAÇÕES PADRÃO
        ' ===================================================================
        Private Sub SetDefaultConfiguration()
            ' APIs - devem ser configuradas pelo usuário
            AssemblyAIApiKey = ""
            ElevenLabsApiKey = ""
            PicovoiceAccessKey = ""
            OpenAIApiKey = ""

            ' STT padrões
            STTLanguage = "pt-br"
            UseAdvancedSTTModel = False
            STTConfidenceThreshold = 0.7

            ' TTS padrões
            SelectedVoiceId = "21m00Tcm4TlvDq8ikWAM" ' Rachel (ElevenLabs)
            UseHighQualityTTS = True
            TTSStability = 0.75
            TTSSimilarityBoost = 0.85
            TTSStyle = 0.5
            UseSpeakerBoost = True

            ' Wake Word padrões
            WakeWordSensitivity = 0.5
            CustomWakeWords = New List(Of String) From {"aria", "hey aria", "ok aria"}

            ' Áudio padrões
            AudioSampleRate = 16000
            AudioBitsPerSample = 16
            AudioChannels = 1

            ' Configurações avançadas
            EnableVoiceActivityDetection = True
            NoiseReductionLevel = 2
            EchoReductionEnabled = True
            AutoVolumeControl = True

            ' Segurança
            EncryptConfiguration = True
            UseSecureStorage = True

            ' Log
            EnableVoiceLogging = False
            LogLevel = VoiceLogLevel.Info
            MaxLogFileSize = 10 * 1024 * 1024
        End Sub

        ' ===================================================================
        ' VALIDAÇÃO DE CONFIGURAÇÕES
        ' ===================================================================
        Public Function ValidateConfiguration() As List(Of String)
            Dim errors As New List(Of String)

            ' Validar chaves de API
            If String.IsNullOrEmpty(AssemblyAIApiKey) Then
                errors.Add("Chave da API AssemblyAI não configurada")
            End If

            If String.IsNullOrEmpty(ElevenLabsApiKey) Then
                errors.Add("Chave da API ElevenLabs não configurada")
            End If

            ' Validar configurações STT
            If STTConfidenceThreshold < 0.1 OrElse STTConfidenceThreshold > 1.0 Then
                errors.Add("Threshold de confiança STT deve estar entre 0.1 e 1.0")
            End If

            ' Validar configurações TTS
            If TTSStability < 0 OrElse TTSStability > 1 Then
                errors.Add("Estabilidade TTS deve estar entre 0 e 1")
            End If

            If TTSSimilarityBoost < 0 OrElse TTSSimilarityBoost > 1 Then
                errors.Add("Similarity Boost TTS deve estar entre 0 e 1")
            End If

            ' Validar Wake Word
            If WakeWordSensitivity < 0.1 OrElse WakeWordSensitivity > 1.0 Then
                errors.Add("Sensibilidade de Wake Word deve estar entre 0.1 e 1.0")
            End If

            If CustomWakeWords Is Nothing OrElse CustomWakeWords.Count = 0 Then
                errors.Add("Pelo menos uma palavra de ativação deve ser configurada")
            End If

            ' Validar configurações de áudio
            If AudioSampleRate < 8000 OrElse AudioSampleRate > 48000 Then
                errors.Add("Sample rate deve estar entre 8000 e 48000 Hz")
            End If

            Return errors
        End Function

        Public Function AreAllKeysConfigured() As Boolean
            Return Not String.IsNullOrEmpty(AssemblyAIApiKey) AndAlso
                   Not String.IsNullOrEmpty(ElevenLabsApiKey)
        End Function

        Public Function GetConfigurationStatus() As String
            Dim status As New StringBuilder()

            status.AppendLine("Status das APIs:")
            status.AppendLine($"AssemblyAI: {If(String.IsNullOrEmpty(AssemblyAIApiKey), "❌ Não configurado", "✅ Configurado")}")
            status.AppendLine($"ElevenLabs: {If(String.IsNullOrEmpty(ElevenLabsApiKey), "❌ Não configurado", "✅ Configurado")}")
            status.AppendLine($"Picovoice: {If(String.IsNullOrEmpty(PicovoiceAccessKey), "⚠️ Opcional", "✅ Configurado")}")
            status.AppendLine($"OpenAI: {If(String.IsNullOrEmpty(OpenAIApiKey), "⚠️ Opcional", "✅ Configurado")}")

            status.AppendLine()
            status.AppendLine("Configurações:")
            status.AppendLine($"Idioma STT: {STTLanguage}")
            status.AppendLine($"Qualidade TTS: {If(UseHighQualityTTS, "Alta", "Padrão")}")
            status.AppendLine($"Wake Words: {String.Join(", ", CustomWakeWords)}")
            status.AppendLine($"Criptografia: {If(EncryptConfiguration, "Habilitada", "Desabilitada")}")

            Return status.ToString()
        End Function

        ' ===================================================================
        ' MÉTODOS UTILITÁRIOS
        ' ===================================================================
        Private Sub EnsureConfigDirectoryExists()
            If Not Directory.Exists(ConfigDirectory) Then
                Directory.CreateDirectory(ConfigDirectory)
            End If
        End Sub

        Private Sub CopyPropertiesFrom(sourceConfig As VoiceConfiguration)
            If sourceConfig Is Nothing Then Return

            ' Copiar propriedades usando reflexão ou manualmente
            Me.AssemblyAIApiKey = sourceConfig.AssemblyAIApiKey
            Me.ElevenLabsApiKey = sourceConfig.ElevenLabsApiKey
            Me.PicovoiceAccessKey = sourceConfig.PicovoiceAccessKey
            Me.OpenAIApiKey = sourceConfig.OpenAIApiKey

            Me.STTLanguage = sourceConfig.STTLanguage
            Me.UseAdvancedSTTModel = sourceConfig.UseAdvancedSTTModel
            Me.STTConfidenceThreshold = sourceConfig.STTConfidenceThreshold

            Me.SelectedVoiceId = sourceConfig.SelectedVoiceId
            Me.UseHighQualityTTS = sourceConfig.UseHighQualityTTS
            Me.TTSStability = sourceConfig.TTSStability
            Me.TTSSimilarityBoost = sourceConfig.TTSSimilarityBoost
            Me.TTSStyle = sourceConfig.TTSStyle
            Me.UseSpeakerBoost = sourceConfig.UseSpeakerBoost

            Me.WakeWordSensitivity = sourceConfig.WakeWordSensitivity
            Me.CustomWakeWords = If(sourceConfig.CustomWakeWords, New List(Of String))
            Me.KeywordModelPath = sourceConfig.KeywordModelPath
            Me.ContextModelPath = sourceConfig.ContextModelPath

            Me.AudioSampleRate = sourceConfig.AudioSampleRate
            Me.AudioBitsPerSample = sourceConfig.AudioBitsPerSample
            Me.AudioChannels = sourceConfig.AudioChannels

            Me.EnableVoiceActivityDetection = sourceConfig.EnableVoiceActivityDetection
            Me.NoiseReductionLevel = sourceConfig.NoiseReductionLevel
            Me.EchoReductionEnabled = sourceConfig.EchoReductionEnabled
            Me.AutoVolumeControl = sourceConfig.AutoVolumeControl

            Me.EncryptConfiguration = sourceConfig.EncryptConfiguration
            Me.UseSecureStorage = sourceConfig.UseSecureStorage

            Me.EnableVoiceLogging = sourceConfig.EnableVoiceLogging
            Me.LogLevel = sourceConfig.LogLevel
            Me.MaxLogFileSize = sourceConfig.MaxLogFileSize
        End Sub

        Public Sub ResetToDefaults()
            SetDefaultConfiguration()
            SaveConfiguration()
        End Sub

        Public Sub ExportConfiguration(filePath As String)
            Try
                Dim exportConfig As Object = New With {
                    .HasAssemblyAI = Not String.IsNullOrEmpty(AssemblyAIApiKey),
                    .HasElevenLabs = Not String.IsNullOrEmpty(ElevenLabsApiKey),
                    .HasPicovoice = Not String.IsNullOrEmpty(PicovoiceAccessKey),
                    .HasOpenAI = Not String.IsNullOrEmpty(OpenAIApiKey),
                    .STTLanguage = STTLanguage,
                    .UseAdvancedSTTModel = UseAdvancedSTTModel,
                    .UseHighQualityTTS = UseHighQualityTTS,
                    .CustomWakeWords = CustomWakeWords,
                    .AudioSampleRate = AudioSampleRate
                }

                Dim jsonContent As String = JsonConvert.SerializeObject(exportConfig, Formatting.Indented)
                File.WriteAllText(filePath, jsonContent, Encoding.UTF8)

            Catch ex As Exception
                Throw New Exception($"Erro ao exportar configuração: {ex.Message}", ex)
            End Try
        End Sub

    End Class

    ' ===================================================================
    ' ENUMERAÇÕES
    ' ===================================================================
    Public Enum VoiceLogLevel
        Debug = 0
        Info = 1
        Warning = 2
        [Error] = 3
        Critical = 4
    End Enum

End Namespace