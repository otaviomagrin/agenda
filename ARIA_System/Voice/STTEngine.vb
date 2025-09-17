' ===================================================================
' ARIA PREMIUM VOICE SYSTEM - SPEECH-TO-TEXT ENGINE
' Engine de reconhecimento de fala usando AssemblyAI
'
' Características:
' - Precisão >93% com modelo Universal
' - Suporte a 99 idiomas
' - Modelo Slam-1 para compreensão contextual
' - Processamento rápido e eficiente
' ===================================================================

Imports System.Net.Http
Imports System.Text
Imports System.IO
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Voice
    Public Class STTEngine
        Implements IDisposable

        ' Configurações da API AssemblyAI
        Private ReadOnly voiceConfig As VoiceConfiguration
        Private ReadOnly httpClient As HttpClient
        Private Const ASSEMBLYAI_BASE_URL As String = "https://api.assemblyai.com/v2"

        ' Estados do engine
        Private isInitialized As Boolean = False

        ' Eventos
        Public Event TranscriptionComplete(sender As Object, e As TranscriptionEventArgs)
        Public Event TranscriptionError(sender As Object, e As TranscriptionErrorEventArgs)
        Public Event TranscriptionProgress(sender As Object, e As TranscriptionProgressEventArgs)

        Public Sub New(configuration As VoiceConfiguration)
            voiceConfig = configuration
            httpClient = New HttpClient()

            ' Configurar timeout para requests longos
            httpClient.Timeout = TimeSpan.FromMinutes(5)
        End Sub

        ' ===================================================================
        ' INICIALIZAÇÃO
        ' ===================================================================
        Public Async Function InitializeAsync() As Task(Of Boolean)
            Try
                If String.IsNullOrEmpty(voiceConfig.AssemblyAIApiKey) Then
                    Throw New InvalidOperationException("Chave da API AssemblyAI não configurada")
                End If

                ' Configurar headers do HTTP client
                httpClient.DefaultRequestHeaders.Clear()
                httpClient.DefaultRequestHeaders.Add("Authorization", voiceConfig.AssemblyAIApiKey)

                ' Testar conectividade com a API
                Dim testResult As Boolean = Await TestAPIConnection()

                If testResult Then
                    isInitialized = True
                    Return True
                Else
                    Throw New Exception("Falha no teste de conectividade com AssemblyAI")
                End If

            Catch ex As Exception
                RaiseEvent TranscriptionError(Me, New TranscriptionErrorEventArgs($"Erro na inicialização do STT: {ex.Message}", ex))
                Return False
            End Try
        End Function

        ' ===================================================================
        ' TRANSCRIÇÃO DE ÁUDIO
        ' ===================================================================
        Public Async Function TranscribeAsync(audioFilePath As String) As Task(Of String)
            Try
                If Not isInitialized Then
                    Throw New InvalidOperationException("STT Engine não inicializado")
                End If

                If Not File.Exists(audioFilePath) Then
                    Throw New FileNotFoundException($"Arquivo de áudio não encontrado: {audioFilePath}")
                End If

                ' 1. Upload do arquivo de áudio
                Dim uploadUrl As String = Await UploadAudioFileAsync(audioFilePath)

                ' 2. Iniciar transcrição
                Dim transcriptId As String = Await StartTranscriptionAsync(uploadUrl)

                ' 3. Aguardar conclusão e obter resultado
                Dim transcription As String = Await WaitForTranscriptionAsync(transcriptId)

                RaiseEvent TranscriptionComplete(Me, New TranscriptionEventArgs(transcription, audioFilePath))

                Return transcription

            Catch ex As Exception
                Dim errorMsg As String = $"Erro na transcrição: {ex.Message}"
                RaiseEvent TranscriptionError(Me, New TranscriptionErrorEventArgs(errorMsg, ex))
                Return String.Empty
            End Try
        End Function

        ' ===================================================================
        ' TRANSCRIÇÃO EM TEMPO REAL (STREAMING)
        ' ===================================================================
        Public Async Function TranscribeStreamAsync(audioStream As Stream) As Task(Of String)
            Try
                If Not isInitialized Then
                    Throw New InvalidOperationException("STT Engine não inicializado")
                End If

                ' Para streaming em tempo real, implementar WebSocket connection
                ' Por ora, implementamos salvando stream temporariamente
                Dim tempFilePath As String = Path.Combine(Path.GetTempPath(), $"temp_audio_{DateTime.Now:yyyyMMdd_HHmmss}.wav")

                Using fileStream As New FileStream(tempFilePath, FileMode.Create)
                    Await audioStream.CopyToAsync(fileStream)
                End Using

                Dim result As String = Await TranscribeAsync(tempFilePath)

                ' Limpar arquivo temporário
                Try
                    File.Delete(tempFilePath)
                Catch
                    ' Ignorar erro de limpeza
                End Try

                Return result

            Catch ex As Exception
                Dim errorMsg As String = $"Erro na transcrição de stream: {ex.Message}"
                RaiseEvent TranscriptionError(Me, New TranscriptionErrorEventArgs(errorMsg, ex))
                Return String.Empty
            End Try
        End Function

        ' ===================================================================
        ' UPLOAD DE ARQUIVO DE ÁUDIO
        ' ===================================================================
        Private Async Function UploadAudioFileAsync(audioFilePath As String) As Task(Of String)
            Try
                Dim audioBytes As Byte() = File.ReadAllBytes(audioFilePath)

                Using content As New ByteArrayContent(audioBytes)
                    content.Headers.ContentType = New Headers.MediaTypeHeaderValue("application/octet-stream")

                    Dim response As HttpResponseMessage = Await httpClient.PostAsync($"{ASSEMBLYAI_BASE_URL}/upload", content)
                    Dim responseContent As String = Await response.Content.ReadAsStringAsync()

                    If response.IsSuccessStatusCode Then
                        Dim jsonResponse As JObject = JObject.Parse(responseContent)
                        Return jsonResponse("upload_url").ToString()
                    Else
                        Throw New Exception($"Erro no upload: {response.StatusCode} - {responseContent}")
                    End If
                End Using

            Catch ex As Exception
                Throw New Exception($"Falha no upload do arquivo: {ex.Message}", ex)
            End Try
        End Function

        ' ===================================================================
        ' INICIAR TRANSCRIÇÃO
        ' ===================================================================
        Private Async Function StartTranscriptionAsync(audioUrl As String) As Task(Of String)
            Try
                ' Configurar parâmetros da transcrição
                Dim transcriptionRequest As New With {
                    .audio_url = audioUrl,
                    .model = If(voiceConfig.UseAdvancedSTTModel, "slam-1", "universal"),
                    .language_code = voiceConfig.STTLanguage,
                    .speech_model = "best",
                    .punctuate = True,
                    .format_text = True,
                    .disfluencies = False,
                    .dual_channel = False,
                    .speaker_labels = False,
                    .auto_highlights = False,
                    .sentiment_analysis = False,
                    .entity_detection = False,
                    .iab_categories = False,
                    .content_safety = False
                }

                Dim jsonContent As String = JsonConvert.SerializeObject(transcriptionRequest)

                Using content As New StringContent(jsonContent, Encoding.UTF8, "application/json")
                    Dim response As HttpResponseMessage = Await httpClient.PostAsync($"{ASSEMBLYAI_BASE_URL}/transcript", content)
                    Dim responseContent As String = Await response.Content.ReadAsStringAsync()

                    If response.IsSuccessStatusCode Then
                        Dim jsonResponse As JObject = JObject.Parse(responseContent)
                        Return jsonResponse("id").ToString()
                    Else
                        Throw New Exception($"Erro ao iniciar transcrição: {response.StatusCode} - {responseContent}")
                    End If
                End Using

            Catch ex As Exception
                Throw New Exception($"Falha ao iniciar transcrição: {ex.Message}", ex)
            End Try
        End Function

        ' ===================================================================
        ' AGUARDAR CONCLUSÃO DA TRANSCRIÇÃO
        ' ===================================================================
        Private Async Function WaitForTranscriptionAsync(transcriptId As String) As Task(Of String)
            Try
                Dim maxAttempts As Integer = 60 ' 5 minutos máximo (5s * 60)
                Dim attempt As Integer = 0

                While attempt < maxAttempts
                    Dim response As HttpResponseMessage = Await httpClient.GetAsync($"{ASSEMBLYAI_BASE_URL}/transcript/{transcriptId}")
                    Dim responseContent As String = Await response.Content.ReadAsStringAsync()

                    If response.IsSuccessStatusCode Then
                        Dim jsonResponse As JObject = JObject.Parse(responseContent)
                        Dim status As String = jsonResponse("status").ToString()

                        Select Case status.ToLower()
                            Case "completed"
                                Dim text As String = jsonResponse("text")?.ToString()
                                If String.IsNullOrEmpty(text) Then
                                    Return String.Empty
                                End If
                                Return text

                            Case "error"
                                Dim errorMsg As String = jsonResponse("error")?.ToString() Or "Erro desconhecido na transcrição"
                                Throw New Exception($"Erro na transcrição: {errorMsg}")

                            Case "processing", "queued"
                                ' Continuar aguardando
                                Dim progress As Double = If(jsonResponse("audio_duration")?.Value(Of Double)() > 0,
                                    (attempt / maxAttempts) * 100, 0)

                                RaiseEvent TranscriptionProgress(Me, New TranscriptionProgressEventArgs(progress, status))

                                Await Task.Delay(5000) ' Aguardar 5 segundos
                                attempt += 1

                            Case Else
                                Throw New Exception($"Status desconhecido: {status}")
                        End Select
                    Else
                        Throw New Exception($"Erro ao verificar status: {response.StatusCode} - {responseContent}")
                    End If
                End While

                Throw New TimeoutException("Timeout aguardando conclusão da transcrição")

            Catch ex As Exception
                Throw New Exception($"Falha ao aguardar transcrição: {ex.Message}", ex)
            End Try
        End Function

        ' ===================================================================
        ' TESTE DE CONECTIVIDADE
        ' ===================================================================
        Private Async Function TestAPIConnection() As Task(Of Boolean)
            Try
                ' Fazer uma requisição simples para testar a conectividade
                Dim response As HttpResponseMessage = Await httpClient.GetAsync($"{ASSEMBLYAI_BASE_URL}/transcript")

                ' Se não for erro de autenticação, a API está acessível
                Return response.StatusCode <> Net.HttpStatusCode.Unauthorized

            Catch ex As Exception
                Return False
            End Try
        End Function

        ' ===================================================================
        ' MÉTODOS UTILITÁRIOS
        ' ===================================================================
        Public Function GetSupportedLanguages() As List(Of String)
            ' Lista dos principais idiomas suportados pelo AssemblyAI
            Return New List(Of String) From {
                "pt-br", "en-us", "es-es", "fr-fr", "de-de", "it-it",
                "ja-jp", "ko-kr", "zh-cn", "ar-sa", "hi-in", "ru-ru"
            }
        End Function

        Public Function IsLanguageSupported(languageCode As String) As Boolean
            Return GetSupportedLanguages().Contains(languageCode.ToLower())
        End Function

        ' ===================================================================
        ' PROPRIEDADES
        ' ===================================================================
        Public ReadOnly Property IsInitialized As Boolean
            Get
                Return isInitialized
            End Get
        End Property

        Public ReadOnly Property SupportedFormats As List(Of String)
            Get
                Return New List(Of String) From {
                    ".wav", ".mp3", ".mp4", ".m4a", ".aac", ".ogg", ".flac", ".wma"
                }
            End Get
        End Property

        ' ===================================================================
        ' LIMPEZA E DISPOSIÇÃO
        ' ===================================================================
        Public Sub Dispose() Implements IDisposable.Dispose
            Try
                httpClient?.Dispose()
                isInitialized = False
            Catch
                ' Falha silenciosa na limpeza
            End Try
        End Sub

    End Class

    ' ===================================================================
    ' CLASSES DE EVENTOS
    ' ===================================================================
    Public Class TranscriptionEventArgs
        Inherits EventArgs

        Public Property Transcription As String
        Public Property AudioFilePath As String
        Public Property Timestamp As DateTime

        Public Sub New(transcription As String, audioFilePath As String)
            Me.Transcription = transcription
            Me.AudioFilePath = audioFilePath
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

    Public Class TranscriptionErrorEventArgs
        Inherits EventArgs

        Public Property ErrorMessage As String
        Public Property Exception As Exception
        Public Property Timestamp As DateTime

        Public Sub New(errorMessage As String, Optional exception As Exception = Nothing)
            Me.ErrorMessage = errorMessage
            Me.Exception = exception
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

    Public Class TranscriptionProgressEventArgs
        Inherits EventArgs

        Public Property Progress As Double
        Public Property Status As String
        Public Property Timestamp As DateTime

        Public Sub New(progress As Double, status As String)
            Me.Progress = progress
            Me.Status = status
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

End Namespace