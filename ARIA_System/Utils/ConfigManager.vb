Imports System
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json
Imports ARIA_Premium_System.Utils

Namespace Utils

    ''' <summary>
    ''' Gerenciador de configurações do sistema ARIA
    ''' </summary>
    Public Class ConfigManager

        Private Shared ReadOnly configPath As String = Path.Combine(Application.StartupPath, "config", "aria_config.json")
        Private Shared ReadOnly encryptionKey As Byte() = Encoding.UTF8.GetBytes("ARIA_PREMIUM_SYSTEM_2024_KEY_32B")
        Private Shared configuration As AriaConfiguration

        ''' <summary>
        ''' Estrutura de configuração principal
        ''' </summary>
        Public Class AriaConfiguration
            Public Property AIProviders As AIProvidersConfig
            Public Property Voice As VoiceConfig
            Public Property Budget As BudgetConfig
            Public Property Security As SecurityConfig
            Public Property System As SystemConfig

            Public Sub New()
                AIProviders = New AIProvidersConfig()
                Voice = New VoiceConfig()
                Budget = New BudgetConfig()
                Security = New SecurityConfig()
                System = New SystemConfig()
            End Sub
        End Class

        Public Class AIProvidersConfig
            Public Property Grok As ProviderConfig
            Public Property Claude As ProviderConfig
            Public Property OpenAI As ProviderConfig
            Public Property Gemini As ProviderConfig
            Public Property Ollama As ProviderConfig

            Public Sub New()
                Grok = New ProviderConfig With {.Endpoint = "https://api.x.ai/v1/", .Model = "grok-beta", .Priority = 1}
                Claude = New ProviderConfig With {.Endpoint = "https://api.anthropic.com/v1/", .Model = "claude-3-sonnet-20240229", .Priority = 2}
                OpenAI = New ProviderConfig With {.Endpoint = "https://api.openai.com/v1/", .Model = "gpt-4o-mini", .Priority = 3}
                Gemini = New ProviderConfig With {.Endpoint = "https://generativelanguage.googleapis.com/v1/", .Model = "gemini-pro", .Priority = 4}
                Ollama = New ProviderConfig With {.Endpoint = "http://localhost:11434/", .Model = "llama3.1:8b", .Priority = 5}
            End Sub
        End Class

        Public Class ProviderConfig
            Public Property APIKey As String = ""
            Public Property Endpoint As String = ""
            Public Property Model As String = ""
            Public Property Priority As Integer = 1
            Public Property Enabled As Boolean = True
            Public Property MaxTokens As Integer = 1000
            Public Property Temperature As Double = 0.7
        End Class

        Public Class VoiceConfig
            Public Property AssemblyAI As New VoiceProviderConfig With {.APIKey = "", .Language = "pt"}
            Public Property ElevenLabs As New VoiceProviderConfig With {.APIKey = "", .VoiceId = "pNInz6obpgDQGcFmaJgB"}
            Public Property Picovoice As New VoiceProviderConfig With {.APIKey = "", .WakeWord = "Aria"}
            Public Property InputDevice As Integer = -1 ' -1 = default
            Public Property OutputDevice As Integer = -1 ' -1 = default
            Public Property NoiseReduction As Boolean = True
            Public Property AutoGainControl As Boolean = True
        End Class

        Public Class VoiceProviderConfig
            Public Property APIKey As String = ""
            Public Property Language As String = "pt"
            Public Property VoiceId As String = ""
            Public Property WakeWord As String = ""
        End Class

        Public Class BudgetConfig
            Public Property MonthlyLimit As Decimal = 29.0D
            Public Property AlertThreshold As Double = 0.8
            Public Property AutoSwitchToFree As Boolean = True
            Public Property TrackingEnabled As Boolean = True
        End Class

        Public Class SecurityConfig
            Public Property EncryptAPIKeys As Boolean = True
            Public Property RequireConfirmation As String() = {"delete", "send_email", "file_operation"}
            Public Property AuditLogEnabled As Boolean = True
            Public Property MaxCacheSize As Integer = 1000
            Public Property CacheExpiryHours As Integer = 1
        End Class

        Public Class SystemConfig
            Public Property LogLevel As String = "Info" ' Debug, Info, Warning, Error
            Public Property AutoStartOllama As Boolean = True
            Public Property CheckUpdates As Boolean = True
            Public Property TelemetryEnabled As Boolean = False
            Public Property Theme As String = "Dark" ' Light, Dark, Auto
        End Class

        ''' <summary>
        ''' Carrega configurações do arquivo
        ''' </summary>
        Public Shared Sub LoadConfiguration()
            Try
                Logger.LogInfo("Carregando configurações...")

                ' Criar diretório de configuração se não existir
                Dim configDir = Path.GetDirectoryName(configPath)
                If Not Directory.Exists(configDir) Then
                    Directory.CreateDirectory(configDir)
                End If

                If File.Exists(configPath) Then
                    ' Carregar configuração existente
                    Dim jsonContent = File.ReadAllText(configPath)
                    configuration = JsonConvert.DeserializeObject(Of AriaConfiguration)(jsonContent)

                    ' Descriptografar API keys se necessário
                    If configuration.Security.EncryptAPIKeys Then
                        DecryptAPIKeys()
                    End If

                    Logger.LogInfo("Configurações carregadas com sucesso")
                Else
                    ' Criar configuração padrão
                    configuration = New AriaConfiguration()
                    SaveConfiguration()
                    Logger.LogInfo("Configuração padrão criada")
                End If

            Catch ex As Exception
                Logger.LogError($"Erro ao carregar configurações: {ex.Message}", ex)
                ' Usar configuração padrão em caso de erro
                configuration = New AriaConfiguration()
            End Try
        End Sub

        ''' <summary>
        ''' Salva configurações no arquivo
        ''' </summary>
        Public Shared Sub SaveConfiguration()
            Try
                Logger.LogInfo("Salvando configurações...")

                ' Criptografar API keys se necessário
                If configuration.Security.EncryptAPIKeys Then
                    EncryptAPIKeys()
                End If

                ' Serializar para JSON
                Dim jsonContent = JsonConvert.SerializeObject(configuration, Formatting.Indented)

                ' Salvar arquivo
                File.WriteAllText(configPath, jsonContent)

                Logger.LogInfo("Configurações salvas com sucesso")

            Catch ex As Exception
                Logger.LogError($"Erro ao salvar configurações: {ex.Message}", ex)
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' Criptografa as API keys
        ''' </summary>
        Private Shared Sub EncryptAPIKeys()
            Try
                If Not String.IsNullOrEmpty(configuration.AIProviders.Grok.APIKey) Then
                    configuration.AIProviders.Grok.APIKey = EncryptString(configuration.AIProviders.Grok.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.AIProviders.Claude.APIKey) Then
                    configuration.AIProviders.Claude.APIKey = EncryptString(configuration.AIProviders.Claude.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.AIProviders.OpenAI.APIKey) Then
                    configuration.AIProviders.OpenAI.APIKey = EncryptString(configuration.AIProviders.OpenAI.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.AIProviders.Gemini.APIKey) Then
                    configuration.AIProviders.Gemini.APIKey = EncryptString(configuration.AIProviders.Gemini.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.Voice.AssemblyAI.APIKey) Then
                    configuration.Voice.AssemblyAI.APIKey = EncryptString(configuration.Voice.AssemblyAI.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.Voice.ElevenLabs.APIKey) Then
                    configuration.Voice.ElevenLabs.APIKey = EncryptString(configuration.Voice.ElevenLabs.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.Voice.Picovoice.APIKey) Then
                    configuration.Voice.Picovoice.APIKey = EncryptString(configuration.Voice.Picovoice.APIKey)
                End If

            Catch ex As Exception
                Logger.LogWarning($"Erro ao criptografar API keys: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Descriptografa as API keys
        ''' </summary>
        Private Shared Sub DecryptAPIKeys()
            Try
                If Not String.IsNullOrEmpty(configuration.AIProviders.Grok.APIKey) Then
                    configuration.AIProviders.Grok.APIKey = DecryptString(configuration.AIProviders.Grok.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.AIProviders.Claude.APIKey) Then
                    configuration.AIProviders.Claude.APIKey = DecryptString(configuration.AIProviders.Claude.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.AIProviders.OpenAI.APIKey) Then
                    configuration.AIProviders.OpenAI.APIKey = DecryptString(configuration.AIProviders.OpenAI.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.AIProviders.Gemini.APIKey) Then
                    configuration.AIProviders.Gemini.APIKey = DecryptString(configuration.AIProviders.Gemini.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.Voice.AssemblyAI.APIKey) Then
                    configuration.Voice.AssemblyAI.APIKey = DecryptString(configuration.Voice.AssemblyAI.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.Voice.ElevenLabs.APIKey) Then
                    configuration.Voice.ElevenLabs.APIKey = DecryptString(configuration.Voice.ElevenLabs.APIKey)
                End If

                If Not String.IsNullOrEmpty(configuration.Voice.Picovoice.APIKey) Then
                    configuration.Voice.Picovoice.APIKey = DecryptString(configuration.Voice.Picovoice.APIKey)
                End If

            Catch ex As Exception
                Logger.LogWarning($"Erro ao descriptografar API keys: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Criptografa uma string usando AES
        ''' </summary>
        ''' <param name="plainText">Texto a ser criptografado</param>
        ''' <returns>Texto criptografado em Base64</returns>
        Private Shared Function EncryptString(plainText As String) As String
            Try
                Using aes = Aes.Create()
                    aes.Key = encryptionKey
                    aes.GenerateIV()

                    Using encryptor = aes.CreateEncryptor()
                        Dim plainBytes = Encoding.UTF8.GetBytes(plainText)
                        Dim encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length)

                        ' Combinar IV + dados criptografados
                        Dim result(aes.IV.Length + encryptedBytes.Length - 1) As Byte
                        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length)
                        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length)

                        Return Convert.ToBase64String(result)
                    End Using
                End Using

            Catch ex As Exception
                Logger.LogError($"Erro ao criptografar string: {ex.Message}", ex)
                Return plainText ' Retornar texto original em caso de erro
            End Try
        End Function

        ''' <summary>
        ''' Descriptografa uma string usando AES
        ''' </summary>
        ''' <param name="cipherText">Texto criptografado em Base64</param>
        ''' <returns>Texto descriptografado</returns>
        Private Shared Function DecryptString(cipherText As String) As String
            Try
                Dim cipherBytes = Convert.FromBase64String(cipherText)

                Using aes = Aes.Create()
                    aes.Key = encryptionKey

                    ' Extrair IV dos primeiros 16 bytes
                    Dim iv(15) As Byte
                    Array.Copy(cipherBytes, 0, iv, 0, 16)
                    aes.IV = iv

                    ' Extrair dados criptografados
                    Dim encryptedData(cipherBytes.Length - 17) As Byte
                    Array.Copy(cipherBytes, 16, encryptedData, 0, encryptedData.Length)

                    Using decryptor = aes.CreateDecryptor()
                        Dim decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length)
                        Return Encoding.UTF8.GetString(decryptedBytes)
                    End Using
                End Using

            Catch ex As Exception
                Logger.LogError($"Erro ao descriptografar string: {ex.Message}", ex)
                Return cipherText ' Retornar texto original em caso de erro
            End Try
        End Function

        ' Métodos de acesso às configurações
        Public Shared Function GetConfiguration() As AriaConfiguration
            If configuration Is Nothing Then
                LoadConfiguration()
            End If
            Return configuration
        End Function

        Public Shared Function GetGrokAPIKey() As String
            Return GetConfiguration().AIProviders.Grok.APIKey
        End Function

        Public Shared Function GetClaudeAPIKey() As String
            Return GetConfiguration().AIProviders.Claude.APIKey
        End Function

        Public Shared Function GetOpenAIAPIKey() As String
            Return GetConfiguration().AIProviders.OpenAI.APIKey
        End Function

        Public Shared Function GetGeminiAPIKey() As String
            Return GetConfiguration().AIProviders.Gemini.APIKey
        End Function

        Public Shared Function GetAssemblyAIAPIKey() As String
            Return GetConfiguration().Voice.AssemblyAI.APIKey
        End Function

        Public Shared Function GetElevenLabsAPIKey() As String
            Return GetConfiguration().Voice.ElevenLabs.APIKey
        End Function

        Public Shared Function GetPicovoiceAPIKey() As String
            Return GetConfiguration().Voice.Picovoice.APIKey
        End Function

        Public Shared Sub SetAPIKey(provider As String, apiKey As String)
            Dim config = GetConfiguration()

            Select Case provider.ToLower()
                Case "grok"
                    config.AIProviders.Grok.APIKey = apiKey
                Case "claude"
                    config.AIProviders.Claude.APIKey = apiKey
                Case "openai"
                    config.AIProviders.OpenAI.APIKey = apiKey
                Case "gemini"
                    config.AIProviders.Gemini.APIKey = apiKey
                Case "assemblyai"
                    config.Voice.AssemblyAI.APIKey = apiKey
                Case "elevenlabs"
                    config.Voice.ElevenLabs.APIKey = apiKey
                Case "picovoice"
                    config.Voice.Picovoice.APIKey = apiKey
            End Select

            SaveConfiguration()
        End Sub

        Public Shared Function ValidateConfiguration() As List(Of String)
            Dim errors As New List(Of String)
            Dim config = GetConfiguration()

            ' Verificar se pelo menos uma IA está configurada
            If String.IsNullOrEmpty(config.AIProviders.Grok.APIKey) AndAlso
               String.IsNullOrEmpty(config.AIProviders.Claude.APIKey) AndAlso
               String.IsNullOrEmpty(config.AIProviders.OpenAI.APIKey) AndAlso
               String.IsNullOrEmpty(config.AIProviders.Gemini.APIKey) Then
                errors.Add("Pelo menos uma API de IA deve estar configurada")
            End If

            ' Verificar configurações de voz
            If String.IsNullOrEmpty(config.Voice.AssemblyAI.APIKey) Then
                errors.Add("API Key do AssemblyAI é obrigatória para STT")
            End If

            If String.IsNullOrEmpty(config.Voice.ElevenLabs.APIKey) Then
                errors.Add("API Key do ElevenLabs é obrigatória para TTS")
            End If

            Return errors
        End Function

    End Class

End Namespace