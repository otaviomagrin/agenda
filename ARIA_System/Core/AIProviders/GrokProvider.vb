Imports System
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports ARIA_Premium_System.Utils

Namespace Core.AIProviders

    ''' <summary>
    ''' Provedor de IA Grok (xAI) - Prioridade 1
    ''' </summary>
    Public Class GrokProvider
        Implements IAIProvider

        Private Shared ReadOnly httpClient As New HttpClient()
        Private Const ENDPOINT As String = "https://api.x.ai/v1/chat/completions"
        Private Const MODEL As String = "grok-beta"
        Private Const TIMEOUT_MS As Integer = 30000

        ' Custos por token (estimados)
        Private Const INPUT_COST_PER_TOKEN As Decimal = 0.000005D ' $0.000005 por token
        Private Const OUTPUT_COST_PER_TOKEN As Decimal = 0.000015D ' $0.000015 por token

        Shared Sub New()
            ' Configurar HttpClient
            httpClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS)
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ARIA-Premium-System/1.0")
        End Sub

        Public Function GetProviderName() As String Implements IAIProvider.GetProviderName
            Return "Grok xAI"
        End Function

        Public Async Function TestConnection() As Task(Of ConnectionResult) Implements IAIProvider.TestConnection
            Try
                Dim startTime = DateTime.Now

                ' Teste simples de conectividade
                Dim testPrompt = "Responda apenas 'OK' para este teste de conectividade."
                Dim response = Await ProcessRequest(testPrompt)

                Dim latency = DateTime.Now.Subtract(startTime).TotalMilliseconds

                If Not String.IsNullOrEmpty(response) Then
                    Logger.LogInfo($"Grok: Teste de conexão bem-sucedido ({latency:F0}ms)")
                    Return New ConnectionResult(True, latency)
                Else
                    Logger.LogWarning("Grok: Teste de conexão falhou - resposta vazia")
                    Return New ConnectionResult(False, latency, "Resposta vazia")
                End If

            Catch ex As Exception
                Logger.LogError($"Grok: Erro no teste de conexão: {ex.Message}", ex)
                Return New ConnectionResult(False, 0, ex.Message)
            End Try
        End Function

        Public Async Function ProcessRequest(prompt As String) As Task(Of String) Implements IAIProvider.ProcessRequest
            Try
                Logger.LogInfo("Grok: Processando requisição...")

                ' Verificar se API key está configurada
                Dim apiKey = ConfigManager.GetGrokAPIKey()
                If String.IsNullOrEmpty(apiKey) Then
                    Throw New InvalidOperationException("API Key do Grok não configurada")
                End If

                ' Preparar requisição
                Dim requestBody = New With {
                    .model = MODEL,
                    .messages = New Object() {
                        New With {
                            .role = "system",
                            .content = "Você é ARIA, uma assistente de voz inteligente e prestativa. " &
                                     "Responda de forma concisa, clara e útil. Use dados recentes quando disponível."
                        },
                        New With {
                            .role = "user",
                            .content = prompt
                        }
                    },
                    .max_tokens = 1000,
                    .temperature = 0.7,
                    .top_p = 0.9
                }

                Dim jsonContent = JsonConvert.SerializeObject(requestBody)

                ' Criar requisição HTTP
                Using request As New HttpRequestMessage(HttpMethod.Post, ENDPOINT)
                    request.Headers.Add("Authorization", $"Bearer {apiKey}")
                    request.Content = New StringContent(jsonContent, Encoding.UTF8, "application/json")

                    ' Enviar requisição
                    Using response = Await httpClient.SendAsync(request)
                        Dim responseContent = Await response.Content.ReadAsStringAsync()

                        If response.IsSuccessStatusCode Then
                            ' Parse da resposta
                            Dim jsonResponse = JsonConvert.DeserializeObject(responseContent)
                            Dim aiResponse = jsonResponse.choices(0).message.content.ToString()

                            Logger.LogInfo("Grok: Requisição processada com sucesso")
                            Return aiResponse

                        Else
                            Dim errorMsg = $"Erro HTTP {response.StatusCode}: {responseContent}"
                            Logger.LogError($"Grok: {errorMsg}")
                            Throw New HttpRequestException(errorMsg)
                        End If
                End Using

            Catch ex As Exception
                Logger.LogError($"Grok: Erro ao processar requisição: {ex.Message}", ex)
                Throw
            End Try
        End Function

        Public Function GetInputCostPerToken() As Decimal Implements IAIProvider.GetInputCostPerToken
            Return INPUT_COST_PER_TOKEN
        End Function

        Public Function GetOutputCostPerToken() As Decimal Implements IAIProvider.GetOutputCostPerToken
            Return OUTPUT_COST_PER_TOKEN
        End Function

        Public Function IsOfflineCapable() As Boolean Implements IAIProvider.IsOfflineCapable
            Return False ' Grok requer conexão com internet
        End Function

    End Class

End Namespace