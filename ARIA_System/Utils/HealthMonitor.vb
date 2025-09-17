Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Net.NetworkInformation
Imports System.Threading.Tasks
Imports System.Diagnostics
Imports System.IO
Imports Newtonsoft.Json
Imports ARIA_Premium_System.Utils

Namespace Utils

    ''' <summary>
    ''' Sistema de monitoramento de saúde para o ARIA Premium System
    ''' Monitora conectividade, latência e disponibilidade de todos os serviços
    ''' </summary>
    Public Class HealthMonitor

        ' Cliente HTTP compartilhado para testes
        Private Shared ReadOnly httpClient As New HttpClient() With {
            .Timeout = TimeSpan.FromSeconds(10)
        }

        ' URLs para teste de conectividade
        Private Shared ReadOnly connectivityTestUrls As String() = {
            "https://8.8.8.8",
            "https://1.1.1.1",
            "https://www.google.com",
            "https://www.microsoft.com"
        }

        ' Cache de resultados de health check
        Private Shared lastHealthReport As HealthReport
        Private Shared lastHealthCheckTime As DateTime = DateTime.MinValue
        Private Shared ReadOnly healthCheckCacheMinutes As Integer = 2

        ''' <summary>
        ''' Estrutura de relatório de saúde
        ''' </summary>
        Public Class HealthReport
            Public Property Timestamp As DateTime = DateTime.Now
            Public Property InternetConnectivity As ConnectivityStatus
            Public Property AIProviders As Dictionary(Of String, ServiceStatus)
            Public Property VoiceServices As Dictionary(Of String, ServiceStatus)
            Public Property LocalServices As Dictionary(Of String, ServiceStatus)
            Public Property SystemResources As SystemResourceStatus
            Public Property OverallStatus As HealthStatus

            Public Sub New()
                AIProviders = New Dictionary(Of String, ServiceStatus)
                VoiceServices = New Dictionary(Of String, ServiceStatus)
                LocalServices = New Dictionary(Of String, ServiceStatus)
                SystemResources = New SystemResourceStatus()
            End Sub
        End Class

        ''' <summary>
        ''' Status de conectividade
        ''' </summary>
        Public Class ConnectivityStatus
            Public Property IsConnected As Boolean
            Public Property Latency As Double ' em millisegundos
            Public Property PublicIP As String
            Public Property LastCheck As DateTime
            Public Property FailedAttempts As Integer
        End Class

        ''' <summary>
        ''' Status de um serviço
        ''' </summary>
        Public Class ServiceStatus
            Public Property IsAvailable As Boolean
            Public Property ResponseTime As Double ' em millisegundos
            Public Property LastSuccessfulCheck As DateTime
            Public Property LastErrorMessage As String
            Public Property FailureCount As Integer
            Public Property Endpoint As String
        End Class

        ''' <summary>
        ''' Status de recursos do sistema
        ''' </summary>
        Public Class SystemResourceStatus
            Public Property CPUUsage As Double
            Public Property MemoryUsage As Double
            Public Property DiskSpaceAvailable As Long
            Public Property ProcessMemoryUsage As Long
            Public Property ThreadCount As Integer
        End Class

        ''' <summary>
        ''' Enumeração de status geral de saúde
        ''' </summary>
        Public Enum HealthStatus
            Healthy
            Degraded
            Critical
            Offline
        End Enum

        ''' <summary>
        ''' Eventos para notificações
        ''' </summary>
        Public Shared Event ServiceStatusChanged(serviceName As String, status As ServiceStatus)
        Public Shared Event ConnectivityLost()
        Public Shared Event ConnectivityRestored()
        Public Shared Event SystemResourceAlert(resourceType As String, usage As Double)

        ''' <summary>
        ''' Inicializa o sistema de monitoramento
        ''' </summary>
        Public Shared Sub Initialize()
            Try
                Logger.LogInfo("Inicializando sistema de monitoramento de saúde...", "HealthMonitor")

                ' Configurar cliente HTTP
                httpClient.DefaultRequestHeaders.Add("User-Agent", "ARIA-HealthMonitor/1.0")

                Logger.LogInfo("Sistema de monitoramento de saúde inicializado com sucesso", "HealthMonitor")

            Catch ex As Exception
                Logger.LogError($"Erro ao inicializar sistema de monitoramento: {ex.Message}", ex, "HealthMonitor")
            End Try
        End Sub

        ''' <summary>
        ''' Executa verificação completa de saúde
        ''' </summary>
        Public Shared Async Function CheckAllServices() As Task(Of HealthReport)
            Try
                ' Verificar cache
                If lastHealthReport IsNot Nothing AndAlso
                   DateTime.Now.Subtract(lastHealthCheckTime).TotalMinutes < healthCheckCacheMinutes Then
                    Logger.LogDebug("Retornando health check do cache", "HealthMonitor")
                    Return lastHealthReport
                End If

                Logger.LogInfo("Iniciando verificação completa de saúde...", "HealthMonitor")
                Dim report As New HealthReport()

                ' Verificar conectividade com internet
                report.InternetConnectivity = Await CheckInternetConnectivity()

                ' Verificar AI providers
                Await CheckAIProviders(report)

                ' Verificar serviços de voz
                Await CheckVoiceServices(report)

                ' Verificar serviços locais
                CheckLocalServices(report)

                ' Verificar recursos do sistema
                CheckSystemResources(report)

                ' Determinar status geral
                report.OverallStatus = DetermineOverallStatus(report)

                ' Atualizar cache
                lastHealthReport = report
                lastHealthCheckTime = DateTime.Now

                Logger.LogInfo($"Verificação de saúde concluída - Status: {report.OverallStatus}", "HealthMonitor")
                Return report

            Catch ex As Exception
                Logger.LogError($"Erro durante verificação de saúde: {ex.Message}", ex, "HealthMonitor")
                Return New HealthReport With {.OverallStatus = HealthStatus.Critical}
            End Try
        End Function

        ''' <summary>
        ''' Verifica conectividade com a internet
        ''' </summary>
        Public Shared Async Function CheckInternetConnectivity() As Task(Of ConnectivityStatus)
            Dim status As New ConnectivityStatus()
            Dim sw = Stopwatch.StartNew()

            Try
                ' Tentar ping para DNS público
                Using ping As New Ping()
                    Dim reply = Await ping.SendPingAsync("8.8.8.8", 5000)
                    status.IsConnected = reply.Status = IPStatus.Success
                    status.Latency = If(reply.Status = IPStatus.Success, reply.RoundtripTime, 0)
                End Using

                ' Se ping falhou, tentar HTTP
                If Not status.IsConnected Then
                    For Each url In connectivityTestUrls
                        Try
                            Dim response = Await httpClient.GetAsync(url)
                            If response.IsSuccessStatusCode Then
                                status.IsConnected = True
                                status.Latency = sw.ElapsedMilliseconds
                                Exit For
                            End If
                        Catch
                            ' Continuar para próximo URL
                        End Try
                    Next
                End If

                ' Obter IP público se conectado
                If status.IsConnected Then
                    status.PublicIP = Await GetPublicIP()
                    status.FailedAttempts = 0
                Else
                    status.FailedAttempts += 1
                End If

                status.LastCheck = DateTime.Now

                ' Notificar mudanças de conectividade
                If lastHealthReport IsNot Nothing Then
                    Dim wasConnected = lastHealthReport.InternetConnectivity?.IsConnected = True
                    If wasConnected AndAlso Not status.IsConnected Then
                        RaiseEvent ConnectivityLost()
                        Logger.LogWarning("Conectividade com internet perdida", "HealthMonitor")
                    ElseIf Not wasConnected AndAlso status.IsConnected Then
                        RaiseEvent ConnectivityRestored()
                        Logger.LogInfo("Conectividade com internet restaurada", "HealthMonitor")
                    End If
                End If

            Catch ex As Exception
                Logger.LogError($"Erro ao verificar conectividade: {ex.Message}", ex, "HealthMonitor")
                status.IsConnected = False
                status.FailedAttempts += 1
            Finally
                sw.Stop()
            End Try

            Return status
        End Function

        ''' <summary>
        ''' Verifica status dos AI providers
        ''' </summary>
        Private Shared Async Function CheckAIProviders(report As HealthReport) As Task
            Dim config = ConfigManager.GetConfiguration()

            ' Lista de providers para verificar
            Dim providers = New List(Of (Name As String, Endpoint As String, Key As String)) From {
                ("Grok", config.AIProviders.Grok.Endpoint, config.AIProviders.Grok.APIKey),
                ("Claude", config.AIProviders.Claude.Endpoint, config.AIProviders.Claude.APIKey),
                ("OpenAI", config.AIProviders.OpenAI.Endpoint, config.AIProviders.OpenAI.APIKey),
                ("Gemini", config.AIProviders.Gemini.Endpoint, config.AIProviders.Gemini.APIKey)
            }

            For Each provider In providers
                Try
                    Dim status = Await TestAIProvider(provider.Name, provider.Endpoint, provider.Key)
                    report.AIProviders(provider.Name) = status

                    ' Notificar mudanças de status
                    NotifyServiceStatusChange(provider.Name, status)

                Catch ex As Exception
                    Logger.LogError($"Erro ao verificar provider {provider.Name}: {ex.Message}", ex, "HealthMonitor")
                    report.AIProviders(provider.Name) = New ServiceStatus With {
                        .IsAvailable = False,
                        .LastErrorMessage = ex.Message,
                        .Endpoint = provider.Endpoint
                    }
                End Try
            Next
        End Function

        ''' <summary>
        ''' Testa um AI provider específico
        ''' </summary>
        Private Shared Async Function TestAIProvider(name As String, endpoint As String, apiKey As String) As Task(Of ServiceStatus)
            Dim status As New ServiceStatus With {.Endpoint = endpoint}
            Dim sw = Stopwatch.StartNew()

            Try
                If String.IsNullOrEmpty(apiKey) Then
                    status.IsAvailable = False
                    status.LastErrorMessage = "API Key não configurada"
                    Return status
                End If

                ' Teste básico de conectividade HTTP
                Dim testUrl = If(endpoint.EndsWith("/"), endpoint.TrimEnd("/"c), endpoint)

                Using request As New HttpRequestMessage(HttpMethod.Get, testUrl)
                    request.Headers.Add("Authorization", $"Bearer {apiKey}")

                    Dim response = Await httpClient.SendAsync(request)
                    status.ResponseTime = sw.ElapsedMilliseconds

                    ' Considerar sucesso se não for erro de autorização crítico
                    status.IsAvailable = response.StatusCode <> HttpStatusCode.Forbidden AndAlso
                                       response.StatusCode <> HttpStatusCode.InternalServerError

                    If status.IsAvailable Then
                        status.LastSuccessfulCheck = DateTime.Now
                        status.FailureCount = 0
                    Else
                        status.FailureCount += 1
                        status.LastErrorMessage = $"HTTP {response.StatusCode}"
                    End If
                End Using

            Catch ex As Exception
                status.IsAvailable = False
                status.FailureCount += 1
                status.LastErrorMessage = ex.Message
                status.ResponseTime = sw.ElapsedMilliseconds
            Finally
                sw.Stop()
            End Try

            Return status
        End Function

        ''' <summary>
        ''' Verifica serviços de voz
        ''' </summary>
        Private Shared Async Function CheckVoiceServices(report As HealthReport) As Task
            Dim config = ConfigManager.GetConfiguration()

            ' AssemblyAI
            Try
                Dim assemblyStatus = Await TestHTTPService("AssemblyAI", "https://api.assemblyai.com/v2/", config.Voice.AssemblyAI.APIKey)
                report.VoiceServices("AssemblyAI") = assemblyStatus
                NotifyServiceStatusChange("AssemblyAI", assemblyStatus)
            Catch ex As Exception
                Logger.LogError($"Erro ao verificar AssemblyAI: {ex.Message}", ex, "HealthMonitor")
            End Try

            ' ElevenLabs
            Try
                Dim elevenStatus = Await TestHTTPService("ElevenLabs", "https://api.elevenlabs.io/v1/", config.Voice.ElevenLabs.APIKey)
                report.VoiceServices("ElevenLabs") = elevenStatus
                NotifyServiceStatusChange("ElevenLabs", elevenStatus)
            Catch ex As Exception
                Logger.LogError($"Erro ao verificar ElevenLabs: {ex.Message}", ex, "HealthMonitor")
            End Try
        End Function

        ''' <summary>
        ''' Testa serviço HTTP genérico
        ''' </summary>
        Private Shared Async Function TestHTTPService(name As String, endpoint As String, apiKey As String) As Task(Of ServiceStatus)
            Dim status As New ServiceStatus With {.Endpoint = endpoint}
            Dim sw = Stopwatch.StartNew()

            Try
                If String.IsNullOrEmpty(apiKey) Then
                    status.IsAvailable = False
                    status.LastErrorMessage = "API Key não configurada"
                    Return status
                End If

                Using request As New HttpRequestMessage(HttpMethod.Get, endpoint)
                    request.Headers.Add("Authorization", $"Bearer {apiKey}")

                    Dim response = Await httpClient.SendAsync(request)
                    status.ResponseTime = sw.ElapsedMilliseconds
                    status.IsAvailable = response.IsSuccessStatusCode OrElse response.StatusCode = HttpStatusCode.Unauthorized

                    If status.IsAvailable Then
                        status.LastSuccessfulCheck = DateTime.Now
                        status.FailureCount = 0
                    Else
                        status.FailureCount += 1
                        status.LastErrorMessage = $"HTTP {response.StatusCode}"
                    End If
                End Using

            Catch ex As Exception
                status.IsAvailable = False
                status.FailureCount += 1
                status.LastErrorMessage = ex.Message
            Finally
                sw.Stop()
            End Try

            Return status
        End Function

        ''' <summary>
        ''' Verifica serviços locais
        ''' </summary>
        Private Shared Sub CheckLocalServices(report As HealthReport)
            ' Ollama
            Try
                Dim ollamaStatus = TestOllamaLocal()
                report.LocalServices("Ollama") = ollamaStatus
                NotifyServiceStatusChange("Ollama", ollamaStatus)
            Catch ex As Exception
                Logger.LogError($"Erro ao verificar Ollama: {ex.Message}", ex, "HealthMonitor")
            End Try

            ' Whisper (se instalado)
            Try
                Dim whisperStatus = TestWhisperLocal()
                report.LocalServices("Whisper") = whisperStatus
                NotifyServiceStatusChange("Whisper", whisperStatus)
            Catch ex As Exception
                Logger.LogError($"Erro ao verificar Whisper: {ex.Message}", ex, "HealthMonitor")
            End Try
        End Sub

        ''' <summary>
        ''' Testa Ollama local
        ''' </summary>
        Private Shared Function TestOllamaLocal() As ServiceStatus
            Dim status As New ServiceStatus With {.Endpoint = "http://localhost:11434"}

            Try
                ' Verificar se processo Ollama está rodando
                Dim ollamaProcesses = Process.GetProcessesByName("ollama")
                If ollamaProcesses.Length > 0 Then
                    status.IsAvailable = True
                    status.LastSuccessfulCheck = DateTime.Now
                    status.FailureCount = 0
                    status.ResponseTime = 0 ' Local, sem latência de rede
                Else
                    status.IsAvailable = False
                    status.LastErrorMessage = "Processo Ollama não encontrado"
                    status.FailureCount += 1
                End If

            Catch ex As Exception
                status.IsAvailable = False
                status.LastErrorMessage = ex.Message
                status.FailureCount += 1
            End Try

            Return status
        End Function

        ''' <summary>
        ''' Testa Whisper local
        ''' </summary>
        Private Shared Function TestWhisperLocal() As ServiceStatus
            Dim status As New ServiceStatus With {.Endpoint = "Local"}

            Try
                ' Verificar se bibliotecas de Whisper estão disponíveis
                ' Isso seria específico da implementação do Whisper
                status.IsAvailable = True ' Assumir disponível por enquanto
                status.LastSuccessfulCheck = DateTime.Now
                status.FailureCount = 0

            Catch ex As Exception
                status.IsAvailable = False
                status.LastErrorMessage = ex.Message
                status.FailureCount += 1
            End Try

            Return status
        End Function

        ''' <summary>
        ''' Verifica recursos do sistema
        ''' </summary>
        Private Shared Sub CheckSystemResources(report As HealthReport)
            Try
                Dim currentProcess = Process.GetCurrentProcess()

                ' Uso de CPU (aproximado)
                report.SystemResources.CPUUsage = GetCPUUsage()

                ' Uso de memória
                Dim totalMemory = GC.GetTotalMemory(False)
                report.SystemResources.ProcessMemoryUsage = totalMemory
                report.SystemResources.MemoryUsage = (totalMemory / (1024.0 * 1024.0 * 1024.0)) ' GB

                ' Contagem de threads
                report.SystemResources.ThreadCount = currentProcess.Threads.Count

                ' Espaço em disco
                Dim appDrive = New DriveInfo(Path.GetPathRoot(Application.StartupPath))
                report.SystemResources.DiskSpaceAvailable = appDrive.AvailableSpace

                ' Alertas de recursos
                CheckResourceAlerts(report.SystemResources)

            Catch ex As Exception
                Logger.LogError($"Erro ao verificar recursos do sistema: {ex.Message}", ex, "HealthMonitor")
            End Try
        End Sub

        ''' <summary>
        ''' Obtém uso aproximado de CPU
        ''' </summary>
        Private Shared Function GetCPUUsage() As Double
            Try
                Dim currentProcess = Process.GetCurrentProcess()
                Return currentProcess.TotalProcessorTime.TotalMilliseconds / Environment.TickCount * 100
            Catch
                Return 0.0
            End Try
        End Function

        ''' <summary>
        ''' Verifica alertas de recursos
        ''' </summary>
        Private Shared Sub CheckResourceAlerts(resources As SystemResourceStatus)
            ' Alerta de CPU
            If resources.CPUUsage > 80 Then
                RaiseEvent SystemResourceAlert("CPU", resources.CPUUsage)
                Logger.LogWarning($"Alto uso de CPU detectado: {resources.CPUUsage:F1}%", "HealthMonitor")
            End If

            ' Alerta de memória
            If resources.MemoryUsage > 2.0 Then ' > 2GB
                RaiseEvent SystemResourceAlert("Memory", resources.MemoryUsage)
                Logger.LogWarning($"Alto uso de memória detectado: {resources.MemoryUsage:F1}GB", "HealthMonitor")
            End If

            ' Alerta de espaço em disco
            If resources.DiskSpaceAvailable < (1024L * 1024L * 1024L) Then ' < 1GB
                RaiseEvent SystemResourceAlert("Disk", resources.DiskSpaceAvailable)
                Logger.LogWarning($"Pouco espaço em disco: {resources.DiskSpaceAvailable / (1024.0 * 1024.0 * 1024.0):F1}GB", "HealthMonitor")
            End If
        End Sub

        ''' <summary>
        ''' Determina status geral do sistema
        ''' </summary>
        Private Shared Function DetermineOverallStatus(report As HealthReport) As HealthStatus
            ' Sem internet = crítico se nenhum serviço local disponível
            If Not report.InternetConnectivity.IsConnected Then
                Dim hasLocalServices = report.LocalServices.Values.Any(Function(s) s.IsAvailable)
                Return If(hasLocalServices, HealthStatus.Degraded, HealthStatus.Offline)
            End If

            ' Contar serviços funcionais
            Dim totalServices = report.AIProviders.Count + report.VoiceServices.Count
            Dim workingServices = report.AIProviders.Values.Count(Function(s) s.IsAvailable) +
                                report.VoiceServices.Values.Count(Function(s) s.IsAvailable)

            If totalServices = 0 Then Return HealthStatus.Critical

            Dim healthPercentage = workingServices / totalServices

            ' Determinar status baseado na porcentagem de serviços funcionais
            If healthPercentage >= 0.8 Then
                Return HealthStatus.Healthy
            ElseIf healthPercentage >= 0.5 Then
                Return HealthStatus.Degraded
            Else
                Return HealthStatus.Critical
            End If
        End Function

        ''' <summary>
        ''' Notifica mudanças de status de serviço
        ''' </summary>
        Private Shared Sub NotifyServiceStatusChange(serviceName As String, status As ServiceStatus)
            RaiseEvent ServiceStatusChanged(serviceName, status)

            If status.IsAvailable Then
                Logger.LogInfo($"Serviço {serviceName} está operacional (latência: {status.ResponseTime}ms)", "HealthMonitor")
            Else
                Logger.LogWarning($"Serviço {serviceName} indisponível: {status.LastErrorMessage}", "HealthMonitor")
            End If
        End Sub

        ''' <summary>
        ''' Obtém IP público
        ''' </summary>
        Private Shared Async Function GetPublicIP() As Task(Of String)
            Try
                Dim response = Await httpClient.GetStringAsync("https://api.ipify.org")
                Return response.Trim()
            Catch
                Return "Unknown"
            End Try
        End Function

        ''' <summary>
        ''' Verifica um serviço específico rapidamente
        ''' </summary>
        Public Shared Async Function QuickServiceCheck(serviceName As String) As Task(Of ServiceStatus)
            Try
                Select Case serviceName.ToLower()
                    Case "grok"
                        Dim config = ConfigManager.GetConfiguration()
                        Return Await TestAIProvider("Grok", config.AIProviders.Grok.Endpoint, config.AIProviders.Grok.APIKey)
                    Case "claude"
                        Dim config = ConfigManager.GetConfiguration()
                        Return Await TestAIProvider("Claude", config.AIProviders.Claude.Endpoint, config.AIProviders.Claude.APIKey)
                    Case "openai"
                        Dim config = ConfigManager.GetConfiguration()
                        Return Await TestAIProvider("OpenAI", config.AIProviders.OpenAI.Endpoint, config.AIProviders.OpenAI.APIKey)
                    Case "gemini"
                        Dim config = ConfigManager.GetConfiguration()
                        Return Await TestAIProvider("Gemini", config.AIProviders.Gemini.Endpoint, config.AIProviders.Gemini.APIKey)
                    Case "ollama"
                        Return TestOllamaLocal()
                    Case Else
                        Return New ServiceStatus With {.IsAvailable = False, .LastErrorMessage = "Serviço não reconhecido"}
                End Select

            Catch ex As Exception
                Return New ServiceStatus With {.IsAvailable = False, .LastErrorMessage = ex.Message}
            End Try
        End Function

        ''' <summary>
        ''' Limpa cache de health check
        ''' </summary>
        Public Shared Sub ClearCache()
            lastHealthReport = Nothing
            lastHealthCheckTime = DateTime.MinValue
            Logger.LogDebug("Cache de health check limpo", "HealthMonitor")
        End Sub

        ''' <summary>
        ''' Finaliza o sistema de monitoramento
        ''' </summary>
        Public Shared Sub Shutdown()
            Try
                httpClient.Dispose()
                Logger.LogInfo("Sistema de monitoramento de saúde finalizado", "HealthMonitor")
            Catch ex As Exception
                Logger.LogError($"Erro ao finalizar sistema de monitoramento: {ex.Message}", ex, "HealthMonitor")
            End Try
        End Sub

    End Class

End Namespace