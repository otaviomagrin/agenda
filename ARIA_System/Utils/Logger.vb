Imports System
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Concurrent

Namespace Utils

    ''' <summary>
    ''' Sistema de logging avançado para o ARIA Premium System
    ''' Suporte a múltiplos níveis de log, arquivos rotativos e buffer assíncrono
    ''' </summary>
    Public Class Logger

        ' Enumeração dos níveis de log
        Public Enum LogLevel
            Debug = 0
            Info = 1
            Warning = 2
            [Error] = 3
            Critical = 4
        End Enum

        ' Configurações do logger
        Private Shared ReadOnly logDirectory As String = Path.Combine(Application.StartupPath, "logs")
        Private Shared ReadOnly maxLogFileSize As Long = 10 * 1024 * 1024 ' 10MB
        Private Shared ReadOnly maxLogFiles As Integer = 10
        Private Shared currentLogLevel As LogLevel = LogLevel.Info
        Private Shared logToConsole As Boolean = True
        Private Shared logToFile As Boolean = True

        ' Buffer assíncrono para melhor performance
        Private Shared ReadOnly logQueue As New ConcurrentQueue(Of LogEntry)
        Private Shared ReadOnly logProcessor As New CancellationTokenSource()
        Private Shared logTask As Task

        ' Lock para operações thread-safe
        Private Shared ReadOnly logLock As New Object()
        Private Shared isInitialized As Boolean = False

        ''' <summary>
        ''' Estrutura de entrada de log
        ''' </summary>
        Private Structure LogEntry
            Public Level As LogLevel
            Public Message As String
            Public Exception As Exception
            Public Timestamp As DateTime
            Public ThreadId As Integer
            Public Category As String

            Public Sub New(level As LogLevel, message As String, Optional exception As Exception = Nothing, Optional category As String = "")
                Me.Level = level
                Me.Message = message
                Me.Exception = exception
                Me.Timestamp = DateTime.Now
                Me.ThreadId = Thread.CurrentThread.ManagedThreadId
                Me.Category = category
            End Sub
        End Structure

        ''' <summary>
        ''' Inicializa o sistema de logging
        ''' </summary>
        Public Shared Sub Initialize()
            If isInitialized Then Return

            SyncLock logLock
                If isInitialized Then Return

                Try
                    ' Criar diretório de logs se não existir
                    If Not Directory.Exists(logDirectory) Then
                        Directory.CreateDirectory(logDirectory)
                    End If

                    ' Rotacionar logs antigos se necessário
                    RotateLogFiles()

                    ' Iniciar processamento assíncrono de logs
                    logTask = Task.Run(AddressOf ProcessLogQueue, logProcessor.Token)

                    isInitialized = True
                    LogInfo("Sistema de logging inicializado com sucesso")

                Catch ex As Exception
                    Console.WriteLine($"Erro ao inicializar sistema de logging: {ex.Message}")
                End Try
            End SyncLock
        End Sub

        ''' <summary>
        ''' Define o nível mínimo de log
        ''' </summary>
        Public Shared Sub SetLogLevel(level As LogLevel)
            currentLogLevel = level
            LogInfo($"Nível de log alterado para: {level}")
        End Sub

        ''' <summary>
        ''' Define se deve logar no console
        ''' </summary>
        Public Shared Sub SetConsoleLogging(enabled As Boolean)
            logToConsole = enabled
        End Sub

        ''' <summary>
        ''' Define se deve logar em arquivo
        ''' </summary>
        Public Shared Sub SetFileLogging(enabled As Boolean)
            logToFile = enabled
        End Sub

        ''' <summary>
        ''' Log de nível Debug
        ''' </summary>
        Public Shared Sub LogDebug(message As String, Optional category As String = "")
            Log(LogLevel.Debug, message, Nothing, category)
        End Sub

        ''' <summary>
        ''' Log de nível Info
        ''' </summary>
        Public Shared Sub LogInfo(message As String, Optional category As String = "")
            Log(LogLevel.Info, message, Nothing, category)
        End Sub

        ''' <summary>
        ''' Log de nível Warning
        ''' </summary>
        Public Shared Sub LogWarning(message As String, Optional category As String = "")
            Log(LogLevel.Warning, message, Nothing, category)
        End Sub

        ''' <summary>
        ''' Log de nível Error
        ''' </summary>
        Public Shared Sub LogError(message As String, Optional exception As Exception = Nothing, Optional category As String = "")
            Log(LogLevel.Error, message, exception, category)
        End Sub

        ''' <summary>
        ''' Log de nível Critical
        ''' </summary>
        Public Shared Sub LogCritical(message As String, Optional exception As Exception = Nothing, Optional category As String = "")
            Log(LogLevel.Critical, message, exception, category)
        End Sub

        ''' <summary>
        ''' Log específico para AI Providers
        ''' </summary>
        Public Shared Sub LogAI(message As String, provider As String, Optional level As LogLevel = LogLevel.Info)
            Log(level, message, Nothing, $"AI.{provider}")
        End Sub

        ''' <summary>
        ''' Log específico para Voice System
        ''' </summary>
        Public Shared Sub LogVoice(message As String, component As String, Optional level As LogLevel = LogLevel.Info)
            Log(level, message, Nothing, $"Voice.{component}")
        End Sub

        ''' <summary>
        ''' Log específico para Security
        ''' </summary>
        Public Shared Sub LogSecurity(message As String, Optional level As LogLevel = LogLevel.Warning)
            Log(level, message, Nothing, "Security")
        End Sub

        ''' <summary>
        ''' Log específico para Performance
        ''' </summary>
        Public Shared Sub LogPerformance(message As String, executionTime As TimeSpan)
            Log(LogLevel.Info, $"{message} - Tempo: {executionTime.TotalMilliseconds:F2}ms", Nothing, "Performance")
        End Sub

        ''' <summary>
        ''' Método principal de logging
        ''' </summary>
        Private Shared Sub Log(level As LogLevel, message As String, exception As Exception, category As String)
            ' Verificar se deve logar baseado no nível
            If level < currentLogLevel Then Return

            ' Garantir inicialização
            If Not isInitialized Then Initialize()

            ' Criar entrada de log
            Dim entry As New LogEntry(level, message, exception, category)

            ' Adicionar à fila para processamento assíncrono
            logQueue.Enqueue(entry)
        End Sub

        ''' <summary>
        ''' Processa a fila de logs assincronamente
        ''' </summary>
        Private Shared Async Sub ProcessLogQueue()
            While Not logProcessor.Token.IsCancellationRequested
                Try
                    If logQueue.TryDequeue(entry) Then
                        Dim entry As LogEntry
                        Await ProcessLogEntry(entry)
                    Else
                        ' Aguardar um pouco se não há entradas
                        Await Task.Delay(100, logProcessor.Token)
                    End If
                Catch ex As OperationCanceledException
                    ' Normal quando cancelando
                    Exit While
                Catch ex As Exception
                    ' Evitar crash do processador de logs
                    Console.WriteLine($"Erro no processador de logs: {ex.Message}")
                End Try
            End While
        End Sub

        ''' <summary>
        ''' Processa uma entrada de log individual
        ''' </summary>
        Private Shared Async Function ProcessLogEntry(entry As LogEntry) As Task
            Dim formattedMessage = FormatLogMessage(entry)

            ' Log no console se habilitado
            If logToConsole Then
                WriteToConsole(formattedMessage, entry.Level)
            End If

            ' Log em arquivo se habilitado
            If logToFile Then
                Await WriteToFile(formattedMessage)
            End If
        End Function

        ''' <summary>
        ''' Formata a mensagem de log
        ''' </summary>
        Private Shared Function FormatLogMessage(entry As LogEntry) As String
            Dim sb As New StringBuilder()

            ' Timestamp
            sb.Append($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] ")

            ' Nível
            sb.Append($"[{entry.Level.ToString().ToUpper().PadRight(8)}] ")

            ' Thread ID
            sb.Append($"[T{entry.ThreadId:D3}] ")

            ' Categoria
            If Not String.IsNullOrEmpty(entry.Category) Then
                sb.Append($"[{entry.Category}] ")
            End If

            ' Mensagem
            sb.Append(entry.Message)

            ' Exception se existir
            If entry.Exception IsNot Nothing Then
                sb.AppendLine()
                sb.Append($"Exception: {entry.Exception.GetType().Name}: {entry.Exception.Message}")
                If Not String.IsNullOrEmpty(entry.Exception.StackTrace) Then
                    sb.AppendLine()
                    sb.Append($"StackTrace: {entry.Exception.StackTrace}")
                End If
            End If

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Escreve no console com cores
        ''' </summary>
        Private Shared Sub WriteToConsole(message As String, level As LogLevel)
            Dim originalColor = Console.ForegroundColor

            Try
                Select Case level
                    Case LogLevel.Debug
                        Console.ForegroundColor = ConsoleColor.Gray
                    Case LogLevel.Info
                        Console.ForegroundColor = ConsoleColor.White
                    Case LogLevel.Warning
                        Console.ForegroundColor = ConsoleColor.Yellow
                    Case LogLevel.Error
                        Console.ForegroundColor = ConsoleColor.Red
                    Case LogLevel.Critical
                        Console.ForegroundColor = ConsoleColor.Magenta
                End Select

                Console.WriteLine(message)

            Finally
                Console.ForegroundColor = originalColor
            End Try
        End Sub

        ''' <summary>
        ''' Escreve em arquivo
        ''' </summary>
        Private Shared Async Function WriteToFile(message As String) As Task
            Try
                Dim logFilePath = GetCurrentLogFilePath()

                ' Verificar se precisa rotacionar
                If File.Exists(logFilePath) AndAlso New FileInfo(logFilePath).Length > maxLogFileSize Then
                    RotateLogFiles()
                    logFilePath = GetCurrentLogFilePath()
                End If

                ' Escrever no arquivo
                Using writer As New StreamWriter(logFilePath, True, Encoding.UTF8)
                    Await writer.WriteLineAsync(message)
                    Await writer.FlushAsync()
                End Using

            Catch ex As Exception
                ' Fallback para console em caso de erro de arquivo
                Console.WriteLine($"Erro ao escrever log em arquivo: {ex.Message}")
                Console.WriteLine($"Log original: {message}")
            End Try
        End Function

        ''' <summary>
        ''' Obtém o caminho do arquivo de log atual
        ''' </summary>
        Private Shared Function GetCurrentLogFilePath() As String
            Dim today = DateTime.Now.ToString("yyyy-MM-dd")
            Return Path.Combine(logDirectory, $"aria_system_{today}.log")
        End Function

        ''' <summary>
        ''' Rotaciona arquivos de log antigos
        ''' </summary>
        Private Shared Sub RotateLogFiles()
            Try
                Dim logFiles = Directory.GetFiles(logDirectory, "aria_system_*.log")
                Array.Sort(logFiles)

                ' Remover arquivos mais antigos se exceder o limite
                If logFiles.Length >= maxLogFiles Then
                    For i = 0 To logFiles.Length - maxLogFiles
                        Try
                            File.Delete(logFiles(i))
                        Catch ex As Exception
                            Console.WriteLine($"Erro ao deletar log antigo {logFiles(i)}: {ex.Message}")
                        End Try
                    Next
                End If

            Catch ex As Exception
                Console.WriteLine($"Erro ao rotacionar logs: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Força o flush de todos os logs pendentes
        ''' </summary>
        Public Shared Async Function FlushLogs() As Task
            ' Processar todos os logs na fila
            While logQueue.Count > 0
                If logQueue.TryDequeue(entry) Then
                    Dim entry As LogEntry
                    Await ProcessLogEntry(entry)
                End If
            End While
        End Function

        ''' <summary>
        ''' Finaliza o sistema de logging
        ''' </summary>
        Public Shared Async Sub Shutdown()
            Try
                LogInfo("Finalizando sistema de logging...")

                ' Processar logs pendentes
                Await FlushLogs()

                ' Cancelar processamento assíncrono
                logProcessor.Cancel()

                ' Aguardar finalização da task
                If logTask IsNot Nothing Then
                    Try
                        Await logTask
                    Catch ex As OperationCanceledException
                        ' Normal durante shutdown
                    End Try
                End If

                isInitialized = False

            Catch ex As Exception
                Console.WriteLine($"Erro ao finalizar sistema de logging: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Obtém estatísticas do sistema de logging
        ''' </summary>
        Public Shared Function GetStatistics() As Dictionary(Of String, Object)
            Return New Dictionary(Of String, Object) From {
                {"LogLevel", currentLogLevel.ToString()},
                {"LogToConsole", logToConsole},
                {"LogToFile", logToFile},
                {"QueueSize", logQueue.Count},
                {"LogDirectory", logDirectory},
                {"IsInitialized", isInitialized}
            }
        End Function

    End Class

End Namespace