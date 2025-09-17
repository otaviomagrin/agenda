Imports System
Imports System.Collections.Generic
Imports System.Threading.Tasks
Imports System.Linq
Imports ARIA_Premium_System.Core.AIProviders
Imports ARIA_Premium_System.Utils

Namespace Core

    ''' <summary>
    ''' Núcleo principal do sistema de IA com múltiplos provedores e auto-seleção
    ''' </summary>
    Public Class AriaAICore

        Private ReadOnly providers As New List(Of IAIProvider)
        Private currentProvider As IAIProvider
        Private ReadOnly connectionMonitor As New HealthMonitor()
        Private ReadOnly costTracker As New CostTracker()
        Private ReadOnly cache As New Dictionary(Of String, CacheEntry)

        ' Configurações
        Private Const CACHE_EXPIRY_HOURS As Integer = 1
        Private Const MAX_RETRY_ATTEMPTS As Integer = 3
        Private Const CONNECTION_TIMEOUT_MS As Integer = 30000

        ''' <summary>
        ''' Estrutura para cache de respostas
        ''' </summary>
        Private Structure CacheEntry
            Public Response As String
            Public Timestamp As DateTime
            Public Hits As Integer
        End Structure

        ''' <summary>
        ''' Inicializa o núcleo de IA com todos os provedores
        ''' </summary>
        Public Sub New()
            Try
                Logger.LogInfo("Inicializando AriaAICore...")

                ' Inicializar provedores em ordem de prioridade
                providers.Add(New GrokProvider()) ' Prioridade 1 - Principal
                providers.Add(New ClaudeProvider()) ' Prioridade 2 - Análises complexas
                providers.Add(New OpenAIProvider()) ' Prioridade 3 - Balanceado
                providers.Add(New GeminiProvider()) ' Prioridade 4 - Backup gratuito
                providers.Add(New OllamaProvider()) ' Prioridade 5 - Offline apenas

                Logger.LogInfo($"Inicializados {providers.Count} provedores de IA")

            Catch ex As Exception
                Logger.LogError($"Erro ao inicializar AriaAICore: {ex.Message}", ex)
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' Processa um comando/pergunta usando o melhor provedor disponível
        ''' </summary>
        ''' <param name="prompt">Comando ou pergunta do usuário</param>
        ''' <returns>Resposta da IA</returns>
        Public Async Function ProcessCommand(prompt As String) As Task(Of String)
            Try
                Logger.LogInfo($"Processando comando: {prompt.Substring(0, Math.Min(50, prompt.Length))}...")

                ' Verificar cache primeiro
                Dim cachedResponse = GetCachedResponse(prompt)
                If Not String.IsNullOrEmpty(cachedResponse) Then
                    Logger.LogInfo("Resposta obtida do cache")
                    Return cachedResponse
                End If

                ' Selecionar melhor provedor disponível
                Dim selectedProvider = Await SelectBestProvider()
                If selectedProvider Is Nothing Then
                    Throw New InvalidOperationException("Nenhum provedor de IA disponível")
                End If

                currentProvider = selectedProvider
                Logger.LogInfo($"Usando provedor: {currentProvider.GetProviderName()}")

                ' Processar comando com retry
                Dim response As String = Nothing
                Dim attempts = 0

                While attempts < MAX_RETRY_ATTEMPTS AndAlso String.IsNullOrEmpty(response)
                    Try
                        attempts += 1
                        response = Await currentProvider.ProcessRequest(prompt)

                        If Not String.IsNullOrEmpty(response) Then
                            ' Registrar custo
                            Dim cost = CalculateRequestCost(prompt, response, currentProvider)
                            costTracker.LogCost(currentProvider.GetProviderName(), cost)

                            ' Adicionar ao cache
                            AddToCache(prompt, response)

                            Logger.LogInfo($"Comando processado com sucesso (tentativa {attempts})")
                            Return response
                        End If

                    Catch ex As Exception
                        Logger.LogWarning($"Tentativa {attempts} falhou com {currentProvider.GetProviderName()}: {ex.Message}")

                        If attempts >= MAX_RETRY_ATTEMPTS Then
                            ' Tentar próximo provedor
                            selectedProvider = Await SelectNextBestProvider(currentProvider)
                            If selectedProvider IsNot Nothing Then
                                currentProvider = selectedProvider
                                attempts = 0 ' Reset tentativas para novo provedor
                                Logger.LogInfo($"Mudando para provedor: {currentProvider.GetProviderName()}")
                            End If
                        End If
                    End Try
                End While

                Throw New InvalidOperationException("Falha ao processar comando com todos os provedores disponíveis")

            Catch ex As Exception
                Logger.LogError($"Erro ao processar comando: {ex.Message}", ex)
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Seleciona o melhor provedor baseado em conectividade e latência
        ''' </summary>
        ''' <returns>Melhor provedor disponível</returns>
        Private Async Function SelectBestProvider() As Task(Of IAIProvider)
            Try
                Logger.LogInfo("Selecionando melhor provedor de IA...")

                ' Verificar se há internet para provedores online
                Dim hasInternet = Await connectionMonitor.CheckInternetConnection()

                ' Se não há internet, usar apenas Ollama (offline)
                If Not hasInternet Then
                    Logger.LogWarning("Sem conexão com internet, usando provedor offline")
                    Dim ollamaProvider = providers.FirstOrDefault(Function(p) TypeOf p Is OllamaProvider)
                    If ollamaProvider IsNot Nothing Then
                        Return ollamaProvider
                    End If
                    Return Nothing
                End If

                ' Verificar orçamento mensal
                If costTracker.IsOverBudget() Then
                    Logger.LogWarning("Orçamento mensal excedido, usando provedores gratuitos")
                    ' Usar apenas Gemini (gratuito) e Ollama
                    Dim freeProviders = providers.Where(Function(p) TypeOf p Is GeminiProvider OrElse TypeOf p Is OllamaProvider).ToList()
                    Return Await TestProvidersAndSelectBest(freeProviders)
                End If

                ' Testar todos os provedores online em paralelo
                Dim onlineProviders = providers.Where(Function(p) Not TypeOf p Is OllamaProvider).ToList()
                Return Await TestProvidersAndSelectBest(onlineProviders)

            Catch ex As Exception
                Logger.LogError($"Erro ao selecionar provedor: {ex.Message}", ex)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Testa provedores em paralelo e retorna o melhor
        ''' </summary>
        ''' <param name="providersToTest">Lista de provedores para testar</param>
        ''' <returns>Melhor provedor disponível</returns>
        Private Async Function TestProvidersAndSelectBest(providersToTest As List(Of IAIProvider)) As Task(Of IAIProvider)
            Try
                ' Criar tasks de teste para todos os provedores
                Dim testTasks = providersToTest.Select(Async Function(provider)
                    Try
                        Dim startTime = DateTime.Now
                        Dim result = Await provider.TestConnection()
                        Dim latency = DateTime.Now.Subtract(startTime).TotalMilliseconds

                        Return New With {
                            .Provider = provider,
                            .IsAvailable = result.IsSuccessful,
                            .Latency = latency,
                            .Priority = GetProviderPriority(provider)
                        }
                    Catch
                        Return New With {
                            .Provider = provider,
                            .IsAvailable = False,
                            .Latency = Double.MaxValue,
                            .Priority = Integer.MaxValue
                        }
                    End Try
                End Function).ToArray()

                ' Aguardar todos os testes
                Dim results = Await Task.WhenAll(testTasks)

                ' Selecionar melhor provedor (disponível, menor latência, maior prioridade)
                Dim bestProvider = results.
                    Where(Function(r) r.IsAvailable).
                    OrderBy(Function(r) r.Priority).
                    ThenBy(Function(r) r.Latency).
                    FirstOrDefault()

                If bestProvider IsNot Nothing Then
                    Logger.LogInfo($"Melhor provedor selecionado: {bestProvider.Provider.GetProviderName()} " &
                                 $"(Latência: {bestProvider.Latency:F0}ms)")
                    Return bestProvider.Provider
                End If

                Return Nothing

            Catch ex As Exception
                Logger.LogError($"Erro ao testar provedores: {ex.Message}", ex)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Seleciona o próximo melhor provedor após falha
        ''' </summary>
        ''' <param name="failedProvider">Provedor que falhou</param>
        ''' <returns>Próximo melhor provedor</returns>
        Private Async Function SelectNextBestProvider(failedProvider As IAIProvider) As Task(Of IAIProvider)
            Try
                ' Remover provedor que falhou da lista temporariamente
                Dim availableProviders = providers.Where(Function(p) p IsNot failedProvider).ToList()
                Return Await TestProvidersAndSelectBest(availableProviders)

            Catch ex As Exception
                Logger.LogError($"Erro ao selecionar próximo provedor: {ex.Message}", ex)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Obtém a prioridade do provedor (menor número = maior prioridade)
        ''' </summary>
        ''' <param name="provider">Provedor de IA</param>
        ''' <returns>Número da prioridade</returns>
        Private Function GetProviderPriority(provider As IAIProvider) As Integer
            Select Case provider.GetType().Name
                Case "GrokProvider"
                    Return 1 ' Maior prioridade
                Case "ClaudeProvider"
                    Return 2
                Case "OpenAIProvider"
                    Return 3
                Case "GeminiProvider"
                    Return 4
                Case "OllamaProvider"
                    Return 5 ' Menor prioridade (offline)
                Case Else
                    Return Integer.MaxValue
            End Select
        End Function

        ''' <summary>
        ''' Calcula o custo estimado de uma requisição
        ''' </summary>
        ''' <param name="prompt">Prompt enviado</param>
        ''' <param name="response">Resposta recebida</param>
        ''' <param name="provider">Provedor utilizado</param>
        ''' <returns>Custo estimado em dólares</returns>
        Private Function CalculateRequestCost(prompt As String, response As String, provider As IAIProvider) As Decimal
            Try
                ' Estimativa simples baseada no número de tokens (aproximadamente 4 caracteres por token)
                Dim inputTokens = Math.Ceiling(prompt.Length / 4.0)
                Dim outputTokens = Math.Ceiling(response.Length / 4.0)

                Dim inputCost = inputTokens * provider.GetInputCostPerToken()
                Dim outputCost = outputTokens * provider.GetOutputCostPerToken()

                Return CDec(inputCost + outputCost)

            Catch ex As Exception
                Logger.LogWarning($"Erro ao calcular custo: {ex.Message}")
                Return 0D
            End Try
        End Function

        ''' <summary>
        ''' Obtém resposta do cache se disponível e válida
        ''' </summary>
        ''' <param name="prompt">Prompt para buscar no cache</param>
        ''' <returns>Resposta em cache ou Nothing</returns>
        Private Function GetCachedResponse(prompt As String) As String
            Try
                If cache.ContainsKey(prompt) Then
                    Dim entry = cache(prompt)
                    If DateTime.Now.Subtract(entry.Timestamp).TotalHours < CACHE_EXPIRY_HOURS Then
                        entry.Hits += 1
                        cache(prompt) = entry
                        Return entry.Response
                    Else
                        ' Cache expirado, remover
                        cache.Remove(prompt)
                    End If
                End If

                Return Nothing

            Catch ex As Exception
                Logger.LogWarning($"Erro ao acessar cache: {ex.Message}")
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Adiciona resposta ao cache
        ''' </summary>
        ''' <param name="prompt">Prompt</param>
        ''' <param name="response">Resposta</param>
        Private Sub AddToCache(prompt As String, response As String)
            Try
                Dim entry As New CacheEntry With {
                    .Response = response,
                    .Timestamp = DateTime.Now,
                    .Hits = 1
                }

                cache(prompt) = entry

                ' Limpar cache se muito grande (máximo 1000 entradas)
                If cache.Count > 1000 Then
                    Dim oldestEntries = cache.OrderBy(Function(kvp) kvp.Value.Timestamp).Take(100)
                    For Each oldEntry In oldestEntries.ToList()
                        cache.Remove(oldEntry.Key)
                    Next
                End If

            Catch ex As Exception
                Logger.LogWarning($"Erro ao adicionar ao cache: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Obtém status atual do sistema de IA
        ''' </summary>
        ''' <returns>Informações de status</returns>
        Public Async Function GetSystemStatus() As Task(Of String)
            Try
                Dim status As New System.Text.StringBuilder()

                status.AppendLine("=== STATUS DO SISTEMA ARIA ===")
                status.AppendLine($"Provedor Atual: {If(currentProvider?.GetProviderName(), "Nenhum")}")
                status.AppendLine($"Cache: {cache.Count} entradas")
                status.AppendLine($"Custo Mensal: ${costTracker.GetMonthlySpend():F2}")
                status.AppendLine()

                status.AppendLine("=== PROVEDORES DISPONÍVEIS ===")
                For Each provider In providers
                    Try
                        Dim testResult = Await provider.TestConnection()
                        Dim statusIcon = If(testResult.IsSuccessful, "✅", "❌")
                        status.AppendLine($"{statusIcon} {provider.GetProviderName()}")
                    Catch
                        status.AppendLine($"❌ {provider.GetProviderName()} (Erro)")
                    End Try
                Next

                Return status.ToString()

            Catch ex As Exception
                Logger.LogError($"Erro ao obter status do sistema: {ex.Message}", ex)
                Return "Erro ao obter status do sistema"
            End Try
        End Function

        ''' <summary>
        ''' Força limpeza do cache
        ''' </summary>
        Public Sub ClearCache()
            Try
                cache.Clear()
                Logger.LogInfo("Cache limpo com sucesso")
            Catch ex As Exception
                Logger.LogError($"Erro ao limpar cache: {ex.Message}", ex)
            End Try
        End Sub

        ''' <summary>
        ''' Libera recursos utilizados
        ''' </summary>
        Public Sub Dispose()
            Try
                cache.Clear()
                Logger.LogInfo("AriaAICore finalizado")
            Catch ex As Exception
                Logger.LogError($"Erro ao finalizar AriaAICore: {ex.Message}", ex)
            End Try
        End Sub

    End Class

End Namespace