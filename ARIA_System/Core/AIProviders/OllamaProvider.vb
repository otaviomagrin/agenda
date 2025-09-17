Imports System
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports ARIA_Premium_System.Utils

Namespace Core.AIProviders

    ''' <summary>
    ''' Provedor de IA Ollama - Prioridade 5
    ''' Funciona offline com modelos locais
    ''' </summary>
    Public Class OllamaProvider
        Implements IAIProvider

        Private Shared ReadOnly httpClient As New HttpClient()
        Private Const ENDPOINT As String = "http://localhost:11434/api/generate"
        Private Const CHAT_ENDPOINT As String = "http://localhost:11434/api/chat"
        Private Const DEFAULT_MODEL As String = "llama3.1:8b"
        Private Const TIMEOUT_MS As Integer = 60000 ' Ollama pode ser mais lento

        ' Custos por token (Ollama é gratuito - apenas custo computacional local)
        Private Const INPUT_COST_PER_TOKEN As Decimal = 0.0D
        Private Const OUTPUT_COST_PER_TOKEN As Decimal = 0.0D

        Private availableModels As New List(Of String)

        Shared Sub New()
            ' Configurar HttpClient para conexão local
            httpClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS)
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ARIA-Premium-System/1.0")
        End Sub

        Public Function GetProviderName() As String Implements IAIProvider.GetProviderName
            Return "Ollama Local"
        End Function

        Public Async Function TestConnection() As Task(Of ConnectionResult) Implements IAIProvider.TestConnection
            Try
                Dim startTime = DateTime.Now

                ' Verificar se Ollama está rodando
                If Not Await IsOllamaRunning() Then
                    Return New ConnectionResult(False, 0, "Ollama não está rodando")
                End If

                ' Verificar modelos disponíveis
                Await LoadAvailableModels()

                If availableModels.Count = 0 Then
                    Return New ConnectionResult(False, 0, "Nenhum modelo Ollama disponível")
                End If

                ' Teste simples com modelo disponível
                Dim testPrompt = "Responda apenas 'Ollama funcionando' para este teste."
                Dim response = Await ProcessRequest(testPrompt)

                Dim latency = DateTime.Now.Subtract(startTime).TotalMilliseconds

                If Not String.IsNullOrEmpty(response) Then
                    Logger.LogInfo($"Ollama: Teste de conexão bem-sucedido ({latency:F0}ms)")
                    Return New ConnectionResult(True, latency)
                Else
                    Logger.LogWarning("Ollama: Teste de conexão falhou - resposta vazia")
                    Return New ConnectionResult(False, latency, "Resposta vazia")
                End If

            Catch ex As Exception
                Logger.LogError($"Ollama: Erro no teste de conexão: {ex.Message}", ex)
                Return New ConnectionResult(False, 0, ex.Message)
            End Try
        End Function

        Public Async Function ProcessRequest(prompt As String) As Task(Of String) Implements IAIProvider.ProcessRequest
            Try
                Logger.LogInfo("Ollama: Processando requisição offline...")

                ' Verificar se Ollama está disponível
                If Not Await IsOllamaRunning() Then
                    Throw New InvalidOperationException("Ollama não está rodando. Execute 'ollama serve' primeiro.")
                End If

                ' Carregar modelos se necessário
                If availableModels.Count = 0 Then
                    Await LoadAvailableModels()
                End If

                If availableModels.Count = 0 Then
                    Throw New InvalidOperationException("Nenhum modelo Ollama disponível. Execute 'ollama pull llama3.1:8b' primeiro.")
                End If

                ' Usar o primeiro modelo disponível (ou modelo preferido se disponível)
                Dim modelToUse = If(availableModels.Contains(DEFAULT_MODEL), DEFAULT_MODEL, availableModels.First())

                ' Preparar requisição para chat
                Dim requestBody = New With {
                    .model = modelToUse,
                    .messages = New Object() {
                        New With {
                            .role = "system",
                            .content = "Você é ARIA, uma assistente de voz inteligente funcionando offline. " &
                                     "Responda de forma útil e concisa, mesmo sem acesso à internet. " &
                                     "Baseie-se apenas no seu conhecimento interno."
                        },
                        New With {
                            .role = "user",
                            .content = prompt
                        }
                    },
                    .stream = False,
                    .options = New With {
                        .temperature = 0.7,
                        .top_p = 0.9,
                        .num_predict = 500
                    }
                }

                Dim jsonContent = JsonConvert.SerializeObject(requestBody)

                ' Criar requisição HTTP
                Using request As New HttpRequestMessage(HttpMethod.Post, CHAT_ENDPOINT)
                    request.Content = New StringContent(jsonContent, Encoding.UTF8, "application/json")

                    ' Enviar requisição
                    Using response = Await httpClient.SendAsync(request)
                        Dim responseContent = Await response.Content.ReadAsStringAsync()

                        If response.IsSuccessStatusCode Then
                            ' Parse da resposta
                            Dim jsonResponse = JsonConvert.DeserializeObject(responseContent)
                            Dim aiResponse = jsonResponse.message.content.ToString()

                            Logger.LogInfo("Ollama: Requisição processada com sucesso (offline)")
                            Return aiResponse

                        Else
                            Dim errorMsg = $"Erro HTTP {response.StatusCode}: {responseContent}"
                            Logger.LogError($"Ollama: {errorMsg}")
                            Throw New HttpRequestException(errorMsg)
                        End If
                End Using

            Catch ex As Exception
                Logger.LogError($"Ollama: Erro ao processar requisição: {ex.Message}", ex)
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Verifica se o serviço Ollama está rodando
        ''' </summary>
        ''' <returns>True se Ollama está disponível</returns>
        Private Async Function IsOllamaRunning() As Task(Of Boolean)
            Try
                Using request As New HttpRequestMessage(HttpMethod.Get, "http://localhost:11434/api/tags")
                    Using response = Await httpClient.SendAsync(request)
                        Return response.IsSuccessStatusCode
                End Using

            Catch ex As Exception
                Logger.LogDebug($"Ollama não está rodando: {ex.Message}")
                Return False
            End Try
        End Function

        ''' <summary>
        ''' Carrega lista de modelos disponíveis no Ollama
        ''' </summary>
        Private Async Function LoadAvailableModels() As Task
            Try
                Using request As New HttpRequestMessage(HttpMethod.Get, "http://localhost:11434/api/tags")
                    Using response = Await httpClient.SendAsync(request)
                        If response.IsSuccessStatusCode Then
                            Dim responseContent = Await response.Content.ReadAsStringAsync()
                            Dim jsonResponse = JsonConvert.DeserializeObject(responseContent)

                            availableModels.Clear()
                            If jsonResponse.models IsNot Nothing Then
                                For Each model In jsonResponse.models
                                    availableModels.Add(model.name.ToString())
                                Next
                            End If

                            Logger.LogInfo($"Ollama: {availableModels.Count} modelos disponíveis")
                        End If
                End Using

            Catch ex As Exception
                Logger.LogWarning($"Ollama: Erro ao carregar modelos: {ex.Message}")
            End Try
        End Function

        ''' <summary>
        ''' Obtém lista de modelos disponíveis
        ''' </summary>
        ''' <returns>Lista de nomes dos modelos</returns>
        Public Function GetAvailableModels() As List(Of String)
            Return New List(Of String)(availableModels)
        End Function

        ''' <summary>
        ''' Instala um modelo específico
        ''' </summary>
        ''' <param name="modelName">Nome do modelo para instalar</param>
        ''' <returns>True se instalação foi bem-sucedida</returns>
        Public Async Function InstallModel(modelName As String) As Task(Of Boolean)
            Try
                Logger.LogInfo($"Ollama: Instalando modelo {modelName}...")

                Dim requestBody = New With {
                    .name = modelName
                }

                Dim jsonContent = JsonConvert.SerializeObject(requestBody)

                Using request As New HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/pull")
                    request.Content = New StringContent(jsonContent, Encoding.UTF8, "application/json")

                    Using response = Await httpClient.SendAsync(request)
                        If response.IsSuccessStatusCode Then
                            Logger.LogInfo($"Ollama: Modelo {modelName} instalado com sucesso")
                            Await LoadAvailableModels() ' Recarregar lista
                            Return True
                        Else
                            Logger.LogError($"Ollama: Erro ao instalar modelo {modelName}")
                            Return False
                        End If
                End Using

            Catch ex As Exception
                Logger.LogError($"Ollama: Erro ao instalar modelo {modelName}: {ex.Message}", ex)
                Return False
            End Try
        End Function

        Public Function GetInputCostPerToken() As Decimal Implements IAIProvider.GetInputCostPerToken
            Return INPUT_COST_PER_TOKEN ' Gratuito
        End Function

        Public Function GetOutputCostPerToken() As Decimal Implements IAIProvider.GetOutputCostPerToken
            Return OUTPUT_COST_PER_TOKEN ' Gratuito
        End Function

        Public Function IsOfflineCapable() As Boolean Implements IAIProvider.IsOfflineCapable
            Return True ' Ollama funciona completamente offline
        End Function

    End Class

End Namespace