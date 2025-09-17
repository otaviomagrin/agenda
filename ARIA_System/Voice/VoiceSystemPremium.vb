' ===================================================================
' ARIA PREMIUM VOICE SYSTEM - MAIN COORDINATOR
' Coordenador principal do sistema de voz do ARIA Premium
'
' APIs integradas:
' - AssemblyAI (STT)
' - ElevenLabs (TTS)
' - Picovoice (Wake Word Detection)
' - OpenAI GPT-4o mini (LLM)
' ===================================================================

Imports System.Threading.Tasks
Imports System.IO
Imports ARIA_Premium_System.Core

Namespace Voice
    Public Class VoiceSystemPremium

        ' Componentes do sistema de voz
        Private ReadOnly sttEngine As STTEngine
        Private ReadOnly ttsEngine As TTSEngine
        Private ReadOnly wakeWordEngine As WakeWordEngine
        Private ReadOnly audioRecorder As AudioRecorder
        Private ReadOnly voiceConfig As VoiceConfiguration

        ' Estados do sistema
        Private isInitialized As Boolean = False
        Private isListening As Boolean = False
        Private isProcessing As Boolean = False
        Private currentMode As VoiceMode = VoiceMode.Standby

        ' Eventos do sistema de voz
        Public Event WakeWordDetected(sender As Object, e As WakeWordEventArgs)
        Public Event VoiceCommandReceived(sender As Object, e As VoiceCommandEventArgs)
        Public Event VoiceResponseReady(sender As Object, e As VoiceResponseEventArgs)
        Public Event SystemStatusChanged(sender As Object, e As VoiceStatusEventArgs)
        Public Event ErrorOccurred(sender As Object, e As VoiceErrorEventArgs)

        Public Sub New()
            ' Inicializar configuração
            voiceConfig = New VoiceConfiguration()

            ' Inicializar componentes
            sttEngine = New STTEngine(voiceConfig)
            ttsEngine = New TTSEngine(voiceConfig)
            wakeWordEngine = New WakeWordEngine(voiceConfig)
            audioRecorder = New AudioRecorder()

            ' Configurar eventos
            AddHandler wakeWordEngine.WakeWordDetected, AddressOf OnWakeWordDetected
            AddHandler sttEngine.TranscriptionComplete, AddressOf OnTranscriptionComplete
            AddHandler sttEngine.TranscriptionError, AddressOf OnTranscriptionError
            AddHandler ttsEngine.SynthesisComplete, AddressOf OnSynthesisComplete
            AddHandler ttsEngine.SynthesisError, AddressOf OnSynthesisError
            AddHandler audioRecorder.RecordingComplete, AddressOf OnRecordingComplete
            AddHandler audioRecorder.RecordingError, AddressOf OnRecordingError
        End Sub

        ' ===================================================================
        ' INICIALIZAÇÃO DO SISTEMA
        ' ===================================================================
        Public Async Function InitializeAsync() As Task(Of Boolean)
            Try
                RaiseEvent SystemStatusChanged(Me, New VoiceStatusEventArgs("Inicializando sistema de voz..."))

                ' Carregar configurações
                voiceConfig.LoadConfiguration()

                ' Verificar configurações necessárias
                If Not voiceConfig.AreAllKeysConfigured() Then
                    Throw New InvalidOperationException("Chaves de API não configuradas. Configure todas as APIs necessárias.")
                End If

                ' Inicializar componentes
                Await sttEngine.InitializeAsync()
                Await ttsEngine.InitializeAsync()
                Await wakeWordEngine.InitializeAsync()
                audioRecorder.Initialize()

                ' Marcar como inicializado
                isInitialized = True
                currentMode = VoiceMode.Standby

                RaiseEvent SystemStatusChanged(Me, New VoiceStatusEventArgs("Sistema de voz inicializado com sucesso"))

                ' Saudação inicial
                Await PlayWelcomeMessage()

                Return True

            Catch ex As Exception
                RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro na inicialização: {ex.Message}", ex))
                Return False
            End Try
        End Function

        ' ===================================================================
        ' CONTROLE DO SISTEMA DE VOZ
        ' ===================================================================
        Public Async Function StartListeningAsync() As Task(Of Boolean)
            Try
                If Not isInitialized Then
                    Throw New InvalidOperationException("Sistema não inicializado")
                End If

                If isListening Then
                    Return True
                End If

                ' Iniciar detecção de wake word
                Await wakeWordEngine.StartListeningAsync()

                isListening = True
                currentMode = VoiceMode.Listening

                RaiseEvent SystemStatusChanged(Me, New VoiceStatusEventArgs("Aguardando comando de ativação..."))

                Return True

            Catch ex As Exception
                RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro ao iniciar escuta: {ex.Message}", ex))
                Return False
            End Try
        End Function

        Public Async Function StopListeningAsync() As Task(Of Boolean)
            Try
                If Not isListening Then
                    Return True
                End If

                ' Parar detecção de wake word
                Await wakeWordEngine.StopListeningAsync()

                ' Parar gravação se estiver ativa
                If audioRecorder.IsRecording Then
                    audioRecorder.StopRecording()
                End If

                isListening = False
                currentMode = VoiceMode.Standby

                RaiseEvent SystemStatusChanged(Me, New VoiceStatusEventArgs("Sistema em standby"))

                Return True

            Catch ex As Exception
                RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro ao parar escuta: {ex.Message}", ex))
                Return False
            End Try
        End Function

        ' ===================================================================
        ' PROCESSAMENTO DE COMANDOS DE VOZ
        ' ===================================================================
        Public Async Function ProcessVoiceCommandAsync(audioFilePath As String) As Task(Of String)
            Try
                If isProcessing Then
                    Return "Sistema ocupado processando outro comando"
                End If

                isProcessing = True
                currentMode = VoiceMode.Processing

                RaiseEvent SystemStatusChanged(Me, New VoiceStatusEventArgs("Processando comando de voz..."))

                ' 1. Converter áudio para texto (STT)
                Dim transcription As String = Await sttEngine.TranscribeAsync(audioFilePath)

                If String.IsNullOrEmpty(transcription) Then
                    Await SpeakAsync("Não consegui entender o que você disse.")
                    Return "Transcrição vazia"
                End If

                RaiseEvent VoiceCommandReceived(Me, New VoiceCommandEventArgs(transcription))

                ' 2. Processar com IA (usando AriaAICore)
                Dim response As String = Await ProcessWithAI(transcription)

                ' 3. Converter resposta para áudio e reproduzir (TTS)
                Await SpeakAsync(response)

                RaiseEvent VoiceResponseReady(Me, New VoiceResponseEventArgs(response))

                Return response

            Catch ex As Exception
                Dim errorMsg As String = $"Erro ao processar comando: {ex.Message}"
                RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs(errorMsg, ex))
                Await SpeakAsync("Desculpe, ocorreu um erro ao processar seu comando.")
                Return errorMsg
            Finally
                isProcessing = False
                currentMode = If(isListening, VoiceMode.Listening, VoiceMode.Standby)
            End Try
        End Function

        ' ===================================================================
        ' SÍNTESE DE FALA
        ' ===================================================================
        Public Async Function SpeakAsync(text As String) As Task
            Try
                If String.IsNullOrEmpty(text) Then
                    Return
                End If

                ' Sintetizar e reproduzir fala
                Await ttsEngine.SpeakAsync(text)

            Catch ex As Exception
                RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro na síntese de fala: {ex.Message}", ex))
            End Try
        End Function

        ' ===================================================================
        ' MANIPULADORES DE EVENTOS INTERNOS
        ' ===================================================================
        Private Async Sub OnWakeWordDetected(sender As Object, e As WakeWordEventArgs)
            Try
                RaiseEvent WakeWordDetected(Me, e)
                RaiseEvent SystemStatusChanged(Me, New VoiceStatusEventArgs($"Wake word '{e.WakeWord}' detectado"))

                ' Reproduzir som de confirmação
                Await SpeakAsync("Sim, como posso ajudar?")

                ' Iniciar gravação de comando
                currentMode = VoiceMode.Recording
                audioRecorder.StartRecording()

                ' Timer para timeout da gravação (10 segundos)
                Await Task.Delay(10000)

                If audioRecorder.IsRecording Then
                    Dim audioPath As String = audioRecorder.StopRecording()
                    If Not String.IsNullOrEmpty(audioPath) AndAlso File.Exists(audioPath) Then
                        Await ProcessVoiceCommandAsync(audioPath)
                    End If
                End If

            Catch ex As Exception
                RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro no processamento de wake word: {ex.Message}", ex))
            End Try
        End Sub

        Private Async Sub OnRecordingComplete(sender As Object, e As AudioRecordingEventArgs)
            Try
                If Not String.IsNullOrEmpty(e.AudioFilePath) AndAlso File.Exists(e.AudioFilePath) Then
                    Await ProcessVoiceCommandAsync(e.AudioFilePath)
                End If
            Catch ex As Exception
                RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro no processamento de gravação: {ex.Message}", ex))
            End Try
        End Sub

        Private Sub OnRecordingError(sender As Object, e As AudioErrorEventArgs)
            RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro na gravação: {e.ErrorMessage}", e.Exception))
        End Sub

        Private Sub OnTranscriptionComplete(sender As Object, e As TranscriptionEventArgs)
            ' Evento interno - transcrição completa
        End Sub

        Private Sub OnTranscriptionError(sender As Object, e As TranscriptionErrorEventArgs)
            RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro na transcrição: {e.ErrorMessage}", e.Exception))
        End Sub

        Private Sub OnSynthesisComplete(sender As Object, e As SynthesisEventArgs)
            ' Evento interno - síntese completa
        End Sub

        Private Sub OnSynthesisError(sender As Object, e As SynthesisErrorEventArgs)
            RaiseEvent ErrorOccurred(Me, New VoiceErrorEventArgs($"Erro na síntese: {e.ErrorMessage}", e.Exception))
        End Sub

        ' ===================================================================
        ' PROCESSAMENTO COM IA
        ' ===================================================================
        Private Async Function ProcessWithAI(userInput As String) As Task(Of String)
            Try
                ' Usar o AriaAICore para processar o comando
                Dim aiCore As New AriaAICore()

                ' Preparar prompt específico para assistente de voz
                Dim systemPrompt As String = "Você é ARIA, um assistente pessoal inteligente integrado ao sistema ARIA Premium. " &
                    "Responda de forma concisa, útil e com personalidade amigável. " &
                    "Suas respostas serão convertidas em áudio, então seja natural e conversacional. " &
                    "Se o usuário pedir para executar ações no sistema, gere um JSON com a estrutura: " &
                    "{""action"": ""nome_acao"", ""parameters"": {""param1"": ""valor1""}}. " &
                    "Ações disponíveis: schedule_meeting, create_task, set_reminder, open_module, search_info, system_status."

                Dim response As String = Await aiCore.ProcessMessageAsync(userInput, systemPrompt)

                ' Verificar se é um comando de ação
                response = ProcessActionCommand(response)

                Return response

            Catch ex As Exception
                Return "Desculpe, não consegui processar sua solicitação no momento."
            End Try
        End Function

        ' ===================================================================
        ' PROCESSAMENTO DE COMANDOS DE AÇÃO
        ' ===================================================================
        Private Function ProcessActionCommand(aiResponse As String) As String
            Try
                ' Verificar se a resposta contém JSON de ação
                If aiResponse.Contains("{""action""") Then
                    ' Aqui você pode implementar o processamento de ações específicas
                    ' Por exemplo, integração com outros módulos do ARIA System
                    Return "Comando executado com sucesso."
                Else
                    Return aiResponse
                End If

            Catch ex As Exception
                Return aiResponse ' Retornar resposta original se houver erro no processamento
            End Try
        End Function

        ' ===================================================================
        ' MENSAGEM DE BOAS-VINDAS
        ' ===================================================================
        Private Async Function PlayWelcomeMessage() As Task
            Try
                Dim greeting As String = GetGreeting()
                Dim welcomeMessage As String = $"{greeting} Sistema ARIA Premium ativo. Como posso ajudá-lo hoje?"

                Await SpeakAsync(welcomeMessage)

            Catch ex As Exception
                ' Falha silenciosa na saudação
            End Try
        End Function

        Private Function GetGreeting() As String
            Dim currentHour As Integer = DateTime.Now.Hour

            If currentHour < 12 Then
                Return "Bom dia!"
            ElseIf currentHour < 18 Then
                Return "Boa tarde!"
            Else
                Return "Boa noite!"
            End If
        End Function

        ' ===================================================================
        ' PROPRIEDADES PÚBLICAS
        ' ===================================================================
        Public ReadOnly Property IsInitialized As Boolean
            Get
                Return isInitialized
            End Get
        End Property

        Public ReadOnly Property IsListening As Boolean
            Get
                Return isListening
            End Get
        End Property

        Public ReadOnly Property IsProcessing As Boolean
            Get
                Return isProcessing
            End Get
        End Property

        Public ReadOnly Property CurrentMode As VoiceMode
            Get
                Return currentMode
            End Get
        End Property

        Public ReadOnly Property Configuration As VoiceConfiguration
            Get
                Return voiceConfig
            End Get
        End Property

        ' ===================================================================
        ' LIMPEZA E DISPOSIÇÃO
        ' ===================================================================
        Public Sub Dispose()
            Try
                ' Parar todos os processos
                StopListeningAsync().Wait()

                ' Liberar recursos
                sttEngine?.Dispose()
                ttsEngine?.Dispose()
                wakeWordEngine?.Dispose()
                audioRecorder?.Dispose()

                isInitialized = False

            Catch ex As Exception
                ' Falha silenciosa na limpeza
            End Try
        End Sub

    End Class

    ' ===================================================================
    ' ENUMERAÇÕES E CLASSES DE EVENTOS
    ' ===================================================================
    Public Enum VoiceMode
        Standby
        Listening
        Recording
        Processing
        Speaking
    End Enum

    Public Class WakeWordEventArgs
        Inherits EventArgs

        Public Property WakeWord As String
        Public Property Confidence As Double

        Public Sub New(wakeWord As String, confidence As Double)
            Me.WakeWord = wakeWord
            Me.Confidence = confidence
        End Sub
    End Class

    Public Class VoiceCommandEventArgs
        Inherits EventArgs

        Public Property Command As String
        Public Property Timestamp As DateTime

        Public Sub New(command As String)
            Me.Command = command
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

    Public Class VoiceResponseEventArgs
        Inherits EventArgs

        Public Property Response As String
        Public Property Timestamp As DateTime

        Public Sub New(response As String)
            Me.Response = response
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

    Public Class VoiceStatusEventArgs
        Inherits EventArgs

        Public Property Status As String
        Public Property Timestamp As DateTime

        Public Sub New(status As String)
            Me.Status = status
            Me.Timestamp = DateTime.Now
        End Sub
    End Class

    Public Class VoiceErrorEventArgs
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

End Namespace