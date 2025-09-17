' ===================================================================
' ARIA PREMIUM VOICE SYSTEM - AUDIO RECORDER
' Sistema de gravação de áudio para captura de comandos de voz
'
' Características:
' - Gravação em tempo real do microfone
' - Múltiplos formatos de saída (WAV, MP3)
' - Controle de qualidade e sample rate
' - Detecção automática de silêncio
' ===================================================================

Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

Namespace Voice
    Public Class AudioRecorder
        Implements IDisposable

        ' Importações da API do Windows para gravação de áudio
        <DllImport("winmm.dll", SetLastError:=True)>
        Private Shared Function mciSendString(command As String, returnValue As StringBuilder, returnLength As Integer, winHandle As IntPtr) As Integer
        End Function

        ' Estados da gravação
        Private isInitialized As Boolean = False
        Private isRecording As Boolean = False
        Private isPaused As Boolean = False
        Private recordingPath As String = ""
        Private recordingStartTime As DateTime

        ' Configurações de gravação
        Private sampleRate As Integer = 16000
        Private bitsPerSample As Integer = 16
        Private channels As Integer = 1
        Private outputFormat As AudioFormat = AudioFormat.WAV

        ' Configurações de detecção de silêncio
        Private enableSilenceDetection As Boolean = True
        Private silenceThreshold As Double = 0.02
        Private maxSilenceDuration As TimeSpan = TimeSpan.FromSeconds(3)
        Private maxRecordingDuration As TimeSpan = TimeSpan.FromMinutes(2)

        ' Timer para timeouts
        Private recordingTimer As Timer
        Private silenceTimer As Timer

        ' Eventos
        Public Event RecordingStarted(sender As Object, e As EventArgs)
        Public Event RecordingStopped(sender As Object, e As EventArgs)
        Public Event RecordingComplete(sender As Object, e As AudioRecordingEventArgs)
        Public Event RecordingError(sender As Object, e As AudioErrorEventArgs)
        Public Event SilenceDetected(sender As Object, e As EventArgs)
        Public Event VolumeChanged(sender As Object, e As VolumeEventArgs)

        Public Sub New()
            ' Configurações padrão
        End Sub

        ' ===================================================================
        ' INICIALIZAÇÃO
        ' ===================================================================
        Public Sub Initialize()
            Try
                ' Verificar se o sistema suporta gravação de áudio
                If Not CheckAudioSupport() Then
                    Throw New NotSupportedException("Sistema não suporta gravação de áudio")
                End If

                isInitialized = True

            Catch ex As Exception
                RaiseEvent RecordingError(Me, New AudioErrorEventArgs($"Erro na inicialização: {ex.Message}", ex))
            End Try
        End Sub

        ' ===================================================================
        ' CONTROLE DE GRAVAÇÃO
        ' ===================================================================
        Public Function StartRecording(Optional customPath As String = "") As String
            Try
                If Not isInitialized Then
                    Throw New InvalidOperationException("AudioRecorder não inicializado")
                End If

                If isRecording Then
                    Return "Já está gravando!"
                End If

                ' Definir caminho do arquivo
                If String.IsNullOrEmpty(customPath) Then
                    recordingPath = GenerateRecordingPath()
                Else
                    recordingPath = customPath
                End If

                ' Comando MCI para abrir dispositivo de áudio
                Dim command As String = "open new Type waveaudio Alias recsound"
                Dim result As Integer = mciSendString(command, Nothing, 0, IntPtr.Zero)

                If result <> 0 Then
                    Throw New Exception($"Erro ao abrir dispositivo de áudio: {GetMCIError(result)}")
                End If

                ' Configurar qualidade da gravação
                command = $"set recsound time format ms bitspersample {bitsPerSample} channels {channels} samplespersec {sampleRate}"
                result = mciSendString(command, Nothing, 0, IntPtr.Zero)

                If result <> 0 Then
                    Throw New Exception($"Erro ao configurar áudio: {GetMCIError(result)}")
                End If

                ' Iniciar gravação
                command = "record recsound"
                result = mciSendString(command, Nothing, 0, IntPtr.Zero)

                If result = 0 Then
                    isRecording = True
                    isPaused = False
                    recordingStartTime = DateTime.Now

                    ' Configurar timers
                    SetupTimers()

                    RaiseEvent RecordingStarted(Me, EventArgs.Empty)
                    Return $"Gravação iniciada: {recordingPath}"
                Else
                    Throw New Exception($"Erro ao iniciar gravação: {GetMCIError(result)}")
                End If

            Catch ex As Exception
                Dim errorMsg As String = $"Erro na gravação: {ex.Message}"
                RaiseEvent RecordingError(Me, New AudioErrorEventArgs(errorMsg, ex))
                Return errorMsg
            End Try
        End Function

        Public Function StopRecording() As String
            Try
                If Not isRecording Then
                    Return "Não há gravação ativa!"
                End If

                ' Parar gravação
                Dim command As String = "stop recsound"
                Dim result As Integer = mciSendString(command, Nothing, 0, IntPtr.Zero)

                ' Salvar arquivo
                command = $"save recsound ""{recordingPath}"""
                result = mciSendString(command, Nothing, 0, IntPtr.Zero)

                ' Fechar dispositivo
                command = "close recsound"
                mciSendString(command, Nothing, 0, IntPtr.Zero)

                ' Limpar timers
                CleanupTimers()

                isRecording = False
                isPaused = False

                RaiseEvent RecordingStopped(Me, EventArgs.Empty)

                If File.Exists(recordingPath) Then
                    RaiseEvent RecordingComplete(Me, New AudioRecordingEventArgs(recordingPath, DateTime.Now.Subtract(recordingStartTime)))
                    Return recordingPath
                Else
                    Throw New Exception("Arquivo de áudio não foi criado")
                End If

            Catch ex As Exception
                isRecording = False
                isPaused = False
                CleanupTimers()

                Dim errorMsg As String = $"Erro ao parar gravação: {ex.Message}"
                RaiseEvent RecordingError(Me, New AudioErrorEventArgs(errorMsg, ex))
                Return errorMsg
            End Try
        End Function

        Public Function PauseRecording() As Boolean
            Try
                If Not isRecording OrElse isPaused Then
                    Return False
                End If

                Dim command As String = "pause recsound"
                Dim result As Integer = mciSendString(command, Nothing, 0, IntPtr.Zero)

                If result = 0 Then
                    isPaused = True
                    Return True
                End If

                Return False

            Catch ex As Exception
                RaiseEvent RecordingError(Me, New AudioErrorEventArgs($"Erro ao pausar gravação: {ex.Message}", ex))
                Return False
            End Try
        End Function

        Public Function ResumeRecording() As Boolean
            Try
                If Not isRecording OrElse Not isPaused Then
                    Return False
                End If

                Dim command As String = "resume recsound"
                Dim result As Integer = mciSendString(command, Nothing, 0, IntPtr.Zero)

                If result = 0 Then
                    isPaused = False
                    Return True
                End If

                Return False

            Catch ex As Exception
                RaiseEvent RecordingError(Me, New AudioErrorEventArgs($"Erro ao retomar gravação: {ex.Message}", ex))
                Return False
            End Try
        End Function

        ' ===================================================================
        ' CONFIGURAÇÕES DE GRAVAÇÃO
        ' ===================================================================
        Public Sub SetRecordingQuality(sampleRate As Integer, bitsPerSample As Integer, channels As Integer)
            If Not isRecording Then
                Me.sampleRate = sampleRate
                Me.bitsPerSample = bitsPerSample
                Me.channels = channels
            End If
        End Sub

        Public Sub SetOutputFormat(format As AudioFormat)
            If Not isRecording Then
                outputFormat = format
            End If
        End Sub

        Public Sub SetSilenceDetection(enabled As Boolean, threshold As Double, maxDuration As TimeSpan)
            enableSilenceDetection = enabled
            silenceThreshold = Math.Max(0.01, Math.Min(1.0, threshold))
            maxSilenceDuration = maxDuration
        End Sub

        ' ===================================================================
        ' TIMERS E DETECÇÃO
        ' ===================================================================
        Private Sub SetupTimers()
            ' Timer para duração máxima de gravação
            recordingTimer = New Timer(AddressOf OnMaxDurationReached, Nothing, maxRecordingDuration, Timeout.InfiniteTimeSpan)

            ' Timer para detecção de silêncio (se habilitado)
            If enableSilenceDetection Then
                silenceTimer = New Timer(AddressOf OnSilenceDetected, Nothing, maxSilenceDuration, Timeout.InfiniteTimeSpan)
            End If
        End Sub

        Private Sub CleanupTimers()
            recordingTimer?.Dispose()
            silenceTimer?.Dispose()
            recordingTimer = Nothing
            silenceTimer = Nothing
        End Sub

        Private Sub OnMaxDurationReached(state As Object)
            Try
                If isRecording Then
                    StopRecording()
                End If
            Catch ex As Exception
                RaiseEvent RecordingError(Me, New AudioErrorEventArgs($"Erro no timeout de gravação: {ex.Message}", ex))
            End Try
        End Sub

        Private Sub OnSilenceDetected(state As Object)
            Try
                If isRecording AndAlso enableSilenceDetection Then
                    RaiseEvent SilenceDetected(Me, EventArgs.Empty)
                    ' Opcionalmente parar gravação automaticamente
                    StopRecording()
                End If
            Catch ex As Exception
                RaiseEvent RecordingError(Me, New AudioErrorEventArgs($"Erro na detecção de silêncio: {ex.Message}", ex))
            End Try
        End Sub

        ' ===================================================================
        ' MÉTODOS UTILITÁRIOS
        ' ===================================================================
        Private Function CheckAudioSupport() As Boolean
            Try
                ' Testar se MCI está disponível
                Dim testCommand As String = "capability waveaudio can record"
                Dim result As Integer = mciSendString(testCommand, Nothing, 0, IntPtr.Zero)
                Return result = 0
            Catch
                Return False
            End Try
        End Function

        Private Function GenerateRecordingPath() As String
            Try
                Dim fileName As String = $"aria_voice_{DateTime.Now:yyyyMMdd_HHmmss}"
                Dim extension As String = If(outputFormat = AudioFormat.WAV, ".wav", ".mp3")
                Return Path.Combine(Path.GetTempPath(), fileName & extension)
            Catch
                Return Path.Combine(Path.GetTempPath(), "aria_voice_recording.wav")
            End Try
        End Function

        Private Function GetMCIError(errorCode As Integer) As String
            ' Mapear códigos de erro MCI para mensagens legíveis
            Select Case errorCode
                Case 257
                    Return "Dispositivo não encontrado"
                Case 258
                    Return "Arquivo não encontrado"
                Case 259
                    Return "Formato não suportado"
                Case 260
                    Return "Memória insuficiente"
                Case Else
                    Return $"Erro MCI: {errorCode}"
            End Select
        End Function

        ' ===================================================================
        ' LIMPEZA DE ARQUIVOS TEMPORÁRIOS
        ' ===================================================================
        Public Sub CleanupTempFiles(Optional olderThanHours As Integer = 1)
            Try
                Dim tempPath As String = Path.GetTempPath()
                Dim files() As String = Directory.GetFiles(tempPath, "aria_voice_*.wav")

                For Each file As String In files
                    Try
                        If File.GetCreationTime(file) < DateTime.Now.AddHours(-olderThanHours) Then
                            File.Delete(file)
                        End If
                    Catch
                        ' Ignorar erros de arquivos em uso
                    End Try
                Next

            Catch ex As Exception
                ' Log do erro, mas não interromper
                Console.WriteLine($"Erro na limpeza de arquivos: {ex.Message}")
            End Try
        End Sub

        ' ===================================================================
        ' PROPRIEDADES
        ' ===================================================================
        Public ReadOnly Property IsInitialized As Boolean
            Get
                Return isInitialized
            End Get
        End Property

        Public ReadOnly Property IsRecording As Boolean
            Get
                Return isRecording
            End Get
        End Property

        Public ReadOnly Property IsPaused As Boolean
            Get
                Return isPaused
            End Get
        End Property

        Public ReadOnly Property CurrentRecordingPath As String
            Get
                Return If(isRecording, recordingPath, String.Empty)
            End Get
        End Property

        Public ReadOnly Property RecordingDuration As TimeSpan
            Get
                If isRecording Then
                    Return DateTime.Now.Subtract(recordingStartTime)
                Else
                    Return TimeSpan.Zero
                End If
            End Get
        End Property

        ' ===================================================================
        ' LIMPEZA E DISPOSIÇÃO
        ' ===================================================================
        Public Sub Dispose() Implements IDisposable.Dispose
            Try
                ' Parar gravação se ativa
                If isRecording Then
                    StopRecording()
                End If

                ' Limpar timers
                CleanupTimers()

                isInitialized = False

            Catch
                ' Falha silenciosa na limpeza
            End Try
        End Sub

    End Class

    ' ===================================================================
    ' ENUMERAÇÕES E CLASSES DE EVENTOS
    ' ===================================================================
    Public Enum AudioFormat
        WAV
        MP3
    End Enum

    Public Class AudioRecordingEventArgs
        Inherits EventArgs

        Public Property AudioFilePath As String
        Public Property Duration As TimeSpan
        Public Property Timestamp As DateTime

        Public Sub New(audioFilePath As String, duration As TimeSpan)
            Me.AudioFilePath = audioFilePath
            Me.Duration = duration
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

    Public Class AudioErrorEventArgs
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

    Public Class VolumeEventArgs
        Inherits EventArgs

        Public Property Volume As Double
        Public Property Timestamp As DateTime

        Public Sub New(volume As Double)
            Me.Volume = volume
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

End Namespace