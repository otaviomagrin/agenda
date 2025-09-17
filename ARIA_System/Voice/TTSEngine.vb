' ===================================================================
' ARIA PREMIUM VOICE SYSTEM - TEXT-TO-SPEECH ENGINE
' Engine de síntese de fala usando ElevenLabs
'
' Características:
' - Vozes ultra-realistas com IA
' - Baixa latência para respostas rápidas
' - Clonagem de voz customizada
' - Múltiplos idiomas e estilos
' ===================================================================

Imports System.Net.Http
Imports System.Text
Imports System.IO
Imports System.Threading.Tasks
Imports System.Media
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Voice
    Public Class TTSEngine
        Implements IDisposable

        ' Configurações da API ElevenLabs
        Private ReadOnly voiceConfig As VoiceConfiguration
        Private ReadOnly httpClient As HttpClient
        Private ReadOnly soundPlayer As SoundPlayer
        Private Const ELEVENLABS_BASE_URL As String = "https://api.elevenlabs.io/v1"

        ' Estados do engine
        Private isInitialized As Boolean = False
        Private isSpeaking As Boolean = False

        ' Cache de vozes disponíveis
        Private availableVoices As List(Of ElevenLabsVoice)

        ' Eventos
        Public Event SynthesisComplete(sender As Object, e As SynthesisEventArgs)
        Public Event SynthesisError(sender As Object, e As SynthesisErrorEventArgs)
        Public Event SynthesisProgress(sender As Object, e As SynthesisProgressEventArgs)
        Public Event VoicePlaybackStarted(sender As Object, e As EventArgs)
        Public Event VoicePlaybackFinished(sender As Object, e As EventArgs)

        Public Sub New(configuration As VoiceConfiguration)
            voiceConfig = configuration
            httpClient = New HttpClient()
            soundPlayer = New SoundPlayer()

            ' Configurar timeout
            httpClient.Timeout = TimeSpan.FromMinutes(2)

            ' Inicializar lista de vozes
            availableVoices = New List(Of ElevenLabsVoice)
        End Sub

        ' ===================================================================
        ' INICIALIZAÇÃO
        ' ===================================================================
        Public Async Function InitializeAsync() As Task(Of Boolean)
            Try
                If String.IsNullOrEmpty(voiceConfig.ElevenLabsApiKey) Then
                    Throw New InvalidOperationException("Chave da API ElevenLabs não configurada")
                End If

                ' Configurar headers do HTTP client
                httpClient.DefaultRequestHeaders.Clear()
                httpClient.DefaultRequestHeaders.Add("xi-api-key", voiceConfig.ElevenLabsApiKey)

                ' Carregar vozes disponíveis
                Await LoadAvailableVoicesAsync()

                ' Testar síntese com texto curto
                Dim testResult As Boolean = Await TestSynthesis()

                If testResult Then
                    isInitialized = True
                    Return True
                Else
                    Throw New Exception("Falha no teste de síntese ElevenLabs")
                End If

            Catch ex As Exception
                RaiseEvent SynthesisError(Me, New SynthesisErrorEventArgs($"Erro na inicialização do TTS: {ex.Message}", ex))
                Return False
            End Try
        End Function

        ' ===================================================================
        ' SÍNTESE E REPRODUÇÃO DE FALA
        ' ===================================================================
        Public Async Function SpeakAsync(text As String) As Task
            Try
                If Not isInitialized Then
                    Throw New InvalidOperationException("TTS Engine não inicializado")
                End If

                If String.IsNullOrEmpty(text) Then
                    Return
                End If

                If isSpeaking Then
                    ' Parar reprodução atual se estiver falando
                    StopSpeaking()
                End If

                ' Sintetizar texto para áudio
                Dim audioData As Byte() = Await SynthesizeTextAsync(text)

                If audioData IsNot Nothing AndAlso audioData.Length > 0 Then
                    ' Salvar áudio temporário e reproduzir
                    Await PlayAudioDataAsync(audioData)

                    RaiseEvent SynthesisComplete(Me, New SynthesisEventArgs(text, audioData.Length))
                Else
                    Throw New Exception("Dados de áudio vazios recebidos")
                End If

            Catch ex As Exception
                Dim errorMsg As String = $"Erro na síntese de fala: {ex.Message}"
                RaiseEvent SynthesisError(Me, New SynthesisErrorEventArgs(errorMsg, ex))
            End Try
        End Function

        ' ===================================================================
        ' SÍNTESE DE TEXTO PARA DADOS DE ÁUDIO
        ' ===================================================================
        Public Async Function SynthesizeTextAsync(text As String) As Task(Of Byte())
            Try
                If String.IsNullOrEmpty(text) Then
                    Return Nothing
                End If

                ' Obter ID da voz configurada
                Dim voiceId As String = GetVoiceId()

                ' Preparar requisição de síntese
                Dim synthesisRequest As New With {
                    .text = text,
                    .model_id = If(voiceConfig.UseHighQualityTTS, "eleven_multilingual_v2", "eleven_flash_v2_5"),
                    .voice_settings = New With {
                        .stability = voiceConfig.TTSStability,
                        .similarity_boost = voiceConfig.TTSSimilarityBoost,
                        .style = voiceConfig.TTSStyle,
                        .use_speaker_boost = voiceConfig.UseSpeakerBoost
                    }
                }

                Dim jsonContent As String = JsonConvert.SerializeObject(synthesisRequest)

                Using content As New StringContent(jsonContent, Encoding.UTF8, "application/json")
                    Dim url As String = $"{ELEVENLABS_BASE_URL}/text-to-speech/{voiceId}"
                    Dim response As HttpResponseMessage = Await httpClient.PostAsync(url, content)

                    If response.IsSuccessStatusCode Then
                        Dim audioData As Byte() = Await response.Content.ReadAsByteArrayAsync()
                        Return audioData
                    Else
                        Dim errorContent As String = Await response.Content.ReadAsStringAsync()
                        Throw New Exception($"Erro na API ElevenLabs: {response.StatusCode} - {errorContent}")
                    End If
                End Using

            Catch ex As Exception
                Throw New Exception($"Falha na síntese de texto: {ex.Message}", ex)
            End Try
        End Function

        ' ===================================================================
        ' REPRODUÇÃO DE ÁUDIO
        ' ===================================================================
        Private Async Function PlayAudioDataAsync(audioData As Byte()) As Task
            Try
                isSpeaking = True
                RaiseEvent VoicePlaybackStarted(Me, EventArgs.Empty)

                ' Salvar dados de áudio em arquivo temporário
                Dim tempAudioPath As String = Path.Combine(Path.GetTempPath(), $"aria_tts_{DateTime.Now:yyyyMMdd_HHmmss}.mp3")
                File.WriteAllBytes(tempAudioPath, audioData)

                ' Para MP3, usar MediaPlayer ou converter para WAV
                Await PlayAudioFileAsync(tempAudioPath)

                ' Limpar arquivo temporário
                Try
                    File.Delete(tempAudioPath)
                Catch
                    ' Ignorar erro de limpeza
                End Try

            Catch ex As Exception
                Throw New Exception($"Erro na reprodução de áudio: {ex.Message}", ex)
            Finally
                isSpeaking = False
                RaiseEvent VoicePlaybackFinished(Me, EventArgs.Empty)
            End Try
        End Function

        Private Async Function PlayAudioFileAsync(audioFilePath As String) As Task
            Try
                ' Para reprodução de MP3, você pode usar diferentes abordagens:
                ' 1. Windows Media Player (via COM)
                ' 2. Converter para WAV primeiro
                ' 3. Usar biblioteca externa como NAudio

                ' Implementação básica usando ProcessStartInfo para reprodução
                Dim processInfo As New ProcessStartInfo() With {
                    .FileName = "powershell.exe",
                    .Arguments = $"-c ""Add-Type -AssemblyName presentationCore; $player = New-Object system.windows.media.mediaplayer; $player.open('{audioFilePath}'); $player.Play(); Start-Sleep -Seconds 10""",
                    .WindowStyle = ProcessWindowStyle.Hidden,
                    .CreateNoWindow = True
                }

                Using process As Process = Process.Start(processInfo)
                    Await Task.Run(Sub() process.WaitForExit())
                End Using

            Catch ex As Exception
                ' Fallback: tentar com SoundPlayer se for WAV
                If audioFilePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) Then
                    soundPlayer.SoundLocation = audioFilePath
                    soundPlayer.PlaySync()
                Else
                    Throw New Exception($"Não foi possível reproduzir o arquivo de áudio: {ex.Message}")
                End If
            End Try
        End Function

        ' ===================================================================
        ' GERENCIAMENTO DE VOZES
        ' ===================================================================
        Private Async Function LoadAvailableVoicesAsync() As Task
            Try
                Dim response As HttpResponseMessage = Await httpClient.GetAsync($"{ELEVENLABS_BASE_URL}/voices")

                If response.IsSuccessStatusCode Then
                    Dim responseContent As String = Await response.Content.ReadAsStringAsync()
                    Dim jsonResponse As JObject = JObject.Parse(responseContent)
                    Dim voicesArray As JArray = jsonResponse("voices")

                    availableVoices.Clear()

                    For Each voiceObj As JObject In voicesArray
                        Dim voice As New ElevenLabsVoice() With {
                            .VoiceId = voiceObj("voice_id").ToString(),
                            .Name = voiceObj("name").ToString(),
                            .Category = voiceObj("category").ToString(),
                            .Gender = If(voiceObj("labels")?.Value(Of String)("gender"), "unknown"),
                            .Age = If(voiceObj("labels")?.Value(Of String)("age"), "unknown"),
                            .Accent = If(voiceObj("labels")?.Value(Of String)("accent"), "unknown"),
                            .Description = If(voiceObj("labels")?.Value(Of String)("description"), ""),
                            .PreviewUrl = If(voiceObj("preview_url")?.ToString(), "")
                        }

                        availableVoices.Add(voice)
                    Next

                Else
                    Throw New Exception($"Erro ao carregar vozes: {response.StatusCode}")
                End If

            Catch ex As Exception
                ' Se falhar, usar voz padrão
                availableVoices.Add(New ElevenLabsVoice() With {
                    .VoiceId = "21m00Tcm4TlvDq8ikWAM",
                    .Name = "Rachel",
                    .Category = "premade",
                    .Gender = "female",
                    .Age = "young",
                    .Accent = "american"
                })
            End Try
        End Function

        Private Function GetVoiceId() As String
            ' Retornar voz configurada ou padrão
            If Not String.IsNullOrEmpty(voiceConfig.SelectedVoiceId) Then
                Return voiceConfig.SelectedVoiceId
            End If

            ' Usar primeira voz disponível ou Rachel como padrão
            If availableVoices.Count > 0 Then
                Return availableVoices(0).VoiceId
            Else
                Return "21m00Tcm4TlvDq8ikWAM" ' Rachel - voz padrão
            End If
        End Function

        ' ===================================================================
        ' CONTROLE DE REPRODUÇÃO
        ' ===================================================================
        Public Sub StopSpeaking()
            Try
                If isSpeaking Then
                    soundPlayer.Stop()
                    isSpeaking = False
                    RaiseEvent VoicePlaybackFinished(Me, EventArgs.Empty)
                End If
            Catch ex As Exception
                ' Falha silenciosa
            End Try
        End Sub

        Public Sub PauseSpeaking()
            ' ElevenLabs não suporta pausa nativamente
            ' Implementar se necessário parando e retomando
            StopSpeaking()
        End Sub

        ' ===================================================================
        ' TESTE DE SÍNTESE
        ' ===================================================================
        Private Async Function TestSynthesis() As Task(Of Boolean)
            Try
                Dim testText As String = "Teste de síntese de voz"
                Dim audioData As Byte() = Await SynthesizeTextAsync(testText)

                Return audioData IsNot Nothing AndAlso audioData.Length > 0

            Catch ex As Exception
                Return False
            End Try
        End Function

        ' ===================================================================
        ' MÉTODOS UTILITÁRIOS
        ' ===================================================================
        Public Function GetAvailableVoices() As List(Of ElevenLabsVoice)
            Return New List(Of ElevenLabsVoice)(availableVoices)
        End Function

        Public Function GetVoiceByName(voiceName As String) As ElevenLabsVoice
            Return availableVoices.FirstOrDefault(Function(v) v.Name.Equals(voiceName, StringComparison.OrdinalIgnoreCase))
        End Function

        Public Async Function GetUserInfoAsync() As Task(Of String)
            Try
                Dim response As HttpResponseMessage = Await httpClient.GetAsync($"{ELEVENLABS_BASE_URL}/user")

                If response.IsSuccessStatusCode Then
                    Return Await response.Content.ReadAsStringAsync()
                Else
                    Return "Erro ao obter informações do usuário"
                End If

            Catch ex As Exception
                Return $"Erro: {ex.Message}"
            End Try
        End Function

        ' ===================================================================
        ' PROPRIEDADES
        ' ===================================================================
        Public ReadOnly Property IsInitialized As Boolean
            Get
                Return isInitialized
            End Get
        End Property

        Public ReadOnly Property IsSpeaking As Boolean
            Get
                Return isSpeaking
            End Get
        End Property

        Public ReadOnly Property VoiceCount As Integer
            Get
                Return availableVoices.Count
            End Get
        End Property

        ' ===================================================================
        ' LIMPEZA E DISPOSIÇÃO
        ' ===================================================================
        Public Sub Dispose() Implements IDisposable.Dispose
            Try
                StopSpeaking()
                soundPlayer?.Dispose()
                httpClient?.Dispose()
                isInitialized = False
            Catch
                ' Falha silenciosa na limpeza
            End Try
        End Sub

    End Class

    ' ===================================================================
    ' CLASSES DE SUPORTE
    ' ===================================================================
    Public Class ElevenLabsVoice
        Public Property VoiceId As String
        Public Property Name As String
        Public Property Category As String
        Public Property Gender As String
        Public Property Age As String
        Public Property Accent As String
        Public Property Description As String
        Public Property PreviewUrl As String

        Public Overrides Function ToString() As String
            Return $"{Name} ({Gender}, {Age}, {Accent})"
        End Function
    End Class

    ' ===================================================================
    ' CLASSES DE EVENTOS
    ' ===================================================================
    Public Class SynthesisEventArgs
        Inherits EventArgs

        Public Property Text As String
        Public Property AudioSizeBytes As Long
        Public Property Timestamp As DateTime

        Public Sub New(text As String, audioSize As Long)
            Me.Text = text
            Me.AudioSizeBytes = audioSize
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

    Public Class SynthesisErrorEventArgs
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

    Public Class SynthesisProgressEventArgs
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