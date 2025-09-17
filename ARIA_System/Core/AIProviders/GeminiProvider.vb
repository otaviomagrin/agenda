Imports System
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports ARIA_Premium_System.Utils

Namespace Core.AIProviders

    ''' <summary>
    ''' Provedor de IA Google Gemini - Prioridade 4
    ''' Backup gratuito quando orçamento excedido
    ''' </summary>
    Public Class GeminiProvider
        Implements IAIProvider

        Private Shared ReadOnly httpClient As New HttpClient()
        Private Const ENDPOINT As String = "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent"
        Private Const TIMEOUT_MS As Integer = 35000

        ' Custos por token (Gemini tem tier gratuito)
        Private Const INPUT_COST_PER_TOKEN As Decimal = 0.0D ' Gratuito até limite
        Private Const OUTPUT_COST_PER_TOKEN As Decimal = 0.0D ' Gratuito até limite

        Shared Sub New()
            ' Configurar HttpClient
            httpClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS)
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ARIA-Premium-System/1.0")
        End Sub

        Public Function GetProviderName() As String Implements IAIProvider.GetProviderName
            Return "Google Gemini"
        End Function

        Public Async Function TestConnection() As Task(Of ConnectionResult) Implements IAIProvider.TestConnection
            Try
                Dim startTime = DateTime.Now

                ' Teste simples de conectividade
                Dim testPrompt = "Responda apenas 'Gemini ativo' para este teste."
                Dim response = Await ProcessRequest(testPrompt)

                Dim latency = DateTime.Now.Subtract(startTime).TotalMilliseconds

                If Not String.IsNullOrEmpty(response) Then
                    Logger.LogInfo($"Gemini: Teste de conexão bem-sucedido ({latency:F0}ms)")
                    Return New ConnectionResult(True, latency)
                Else
                    Logger.LogWarning("Gemini: Teste de conexão falhou - resposta vazia")
                    Return New ConnectionResult(False, latency, "Resposta vazia")
                End If

            Catch ex As Exception
                Logger.LogError($"Gemini: Erro no teste de conexão: {ex.Message}", ex)
                Return New ConnectionResult(False, 0, ex.Message)
            End Try
        End Function

        Public Async Function ProcessRequest(prompt As String) As Task(Of String) Implements IAIProvider.ProcessRequest
            Try
                Logger.LogInfo("Gemini: Processando requisição...")

                ' Verificar se API key está configurada
                Dim apiKey = ConfigManager.GetGeminiAPIKey()
                If String.IsNullOrEmpty(apiKey) Then
                    Throw New InvalidOperationException("API Key do Gemini não configurada")
                End If

                ' Preparar requisição (formato específico do Gemini)
                Dim requestBody = New With {
                    .contents = New Object() {
                        New With {
                            .parts = New Object() {
                                New With {
                                    .text = $"Você é ARIA, uma assistente de voz inteligente. " &
                                           "Responda de forma clara e útil. " &
                                           "Pergunta: {prompt}"
                                }
                            }
                        }
                    },
                    .generationConfig = New With {
                        .temperature = 0.7,
                        .topK = 40,
                        .topP = 0.95,
                        .maxOutputTokens = 800
                    },
                    .safetySettings = New Object() {
                        New With {
                            .category = "HARM_CATEGORY_HARASSMENT",
                            .threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        },
                        New With {
                            .category = "HARM_CATEGORY_HATE_SPEECH",
                            .threshold = "BLOCK_MEDIUM_AND_ABOVE"
                        }
                    }
                }

                Dim jsonContent = JsonConvert.SerializeObject(requestBody)
                Dim urlWithKey = $"{ENDPOINT}?key={apiKey}"

                ' Criar requisição HTTP
                Using request As New HttpRequestMessage(HttpMethod.Post, urlWithKey)
                    request.Content = New StringContent(jsonContent, Encoding.UTF8, "application/json")

                    ' Enviar requisição
                    Using response = Await httpClient.SendAsync(request)
                        Dim responseContent = Await response.Content.ReadAsStringAsync()

                        If response.IsSuccessStatusCode Then
                            ' Parse da resposta (formato específico do Gemini)
                            Dim jsonResponse = JsonConvert.DeserializeObject(responseContent)

                            If jsonResponse.candidates IsNot Nothing AndAlso jsonResponse.candidates.Count > 0 Then
                                Dim aiResponse = jsonResponse.candidates(0).content.parts(0).text.ToString()

                                Logger.LogInfo("Gemini: Requisição processada com sucesso")
                                Return aiResponse
                            Else
                                Throw New InvalidOperationException("Resposta do Gemini não contém conteúdo válido")
                            End If

                        Else
                            Dim errorMsg = $"Erro HTTP {response.StatusCode}: {responseContent}"
                            Logger.LogError($"Gemini: {errorMsg}")
                            Throw New HttpRequestException(errorMsg)
                        End If
                End Using

            Catch ex As Exception
                Logger.LogError($"Gemini: Erro ao processar requisição: {ex.Message}", ex)
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Verifica se ainda está dentro do limite gratuito
        ''' </summary>
        ''' <returns>True se ainda pode usar gratuitamente</returns>
        Public Function IsWithinFreeLimit() As Boolean
            Try
                ' Verificar uso mensal (Gemini tem limite de requests por minuto/dia)
                ' Implementação simplificada - em produção, verificar quotas reais
                Dim monthlyRequests = CostTracker.GetMonthlyRequestCount("Gemini")
                Return monthlyRequests < 1000 ' Limite estimado

            Catch ex As Exception
                Logger.LogWarning($"Gemini: Erro ao verificar limite gratuito: {ex.Message}")
                Return True ' Assumir que está dentro do limite em caso de erro
            End Try
        End Function

        Public Function GetInputCostPerToken() As Decimal Implements IAIProvider.GetInputCostPerToken
            ' Verificar se ainda está no tier gratuito
            If IsWithinFreeLimit() Then
                Return 0.0D
            Else
                Return 0.000001D ' Custo após exceder limite gratuito
            End If
        End Function

        Public Function GetOutputCostPerToken() As Decimal Implements IAIProvider.GetOutputCostPerToken
            ' Verificar se ainda está no tier gratuito
            If IsWithinFreeLimit() Then
                Return 0.0D
            Else
                Return 0.000002D ' Custo após exceder limite gratuito
            End If
        End Function

        Public Function IsOfflineCapable() As Boolean Implements IAIProvider.IsOfflineCapable
            Return False ' Gemini requer conexão com internet
        End Function

    End Class

End Namespace