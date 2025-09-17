Imports System
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports ARIA_Premium_System.Utils

Namespace Core.AIProviders

    ''' <summary>
    ''' Provedor de IA Claude (Anthropic) - Prioridade 2
    ''' Especializado em análises complexas e reuniões
    ''' </summary>
    Public Class ClaudeProvider
        Implements IAIProvider

        Private Shared ReadOnly httpClient As New HttpClient()
        Private Const ENDPOINT As String = "https://api.anthropic.com/v1/messages"
        Private Const MODEL As String = "claude-3-sonnet-20240229"
        Private Const TIMEOUT_MS As Integer = 45000 ' Claude pode ser mais lento

        ' Custos por token
        Private Const INPUT_COST_PER_TOKEN As Decimal = 0.000003D ' $0.000003 por token
        Private Const OUTPUT_COST_PER_TOKEN As Decimal = 0.000015D ' $0.000015 por token

        Shared Sub New()
            ' Configurar HttpClient
            httpClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS)
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ARIA-Premium-System/1.0")
            httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01")
        End Sub

        Public Function GetProviderName() As String Implements IAIProvider.GetProviderName
            Return "Claude Anthropic"
        End Function

        Public Async Function TestConnection() As Task(Of ConnectionResult) Implements IAIProvider.TestConnection
            Try
                Dim startTime = DateTime.Now

                ' Teste simples de conectividade
                Dim testPrompt = "Responda apenas 'Conectado' para este teste."
                Dim response = Await ProcessRequest(testPrompt)

                Dim latency = DateTime.Now.Subtract(startTime).TotalMilliseconds

                If Not String.IsNullOrEmpty(response) Then
                    Logger.LogInfo($"Claude: Teste de conexão bem-sucedido ({latency:F0}ms)")
                    Return New ConnectionResult(True, latency)
                Else
                    Logger.LogWarning("Claude: Teste de conexão falhou - resposta vazia")
                    Return New ConnectionResult(False, latency, "Resposta vazia")
                End If

            Catch ex As Exception
                Logger.LogError($"Claude: Erro no teste de conexão: {ex.Message}", ex)
                Return New ConnectionResult(False, 0, ex.Message)
            End Try
        End Function

        Public Async Function ProcessRequest(prompt As String) As Task(Of String) Implements IAIProvider.ProcessRequest
            Try
                Logger.LogInfo("Claude: Processando requisição...")

                ' Verificar se API key está configurada
                Dim apiKey = ConfigManager.GetClaudeAPIKey()
                If String.IsNullOrEmpty(apiKey) Then
                    Throw New InvalidOperationException("API Key do Claude não configurada")
                End If

                ' Preparar requisição (formato específico do Claude)
                Dim requestBody = New With {
                    .model = MODEL,
                    .max_tokens = 1000,
                    .temperature = 0.7,
                    .system = "Você é ARIA, uma assistente de voz inteligente especializada em análises detalhadas. " &
                             "Forneça respostas bem estruturadas, analíticas e completas. " &
                             "Para reuniões, foque em pontos-chave, decisões e próximos passos.",
                    .messages = New Object() {
                        New With {
                            .role = "user",
                            .content = prompt
                        }
                    }
                }

                Dim jsonContent = JsonConvert.SerializeObject(requestBody)

                ' Criar requisição HTTP
                Using request As New HttpRequestMessage(HttpMethod.Post, ENDPOINT)
                    request.Headers.Add("x-api-key", apiKey)
                    request.Content = New StringContent(jsonContent, Encoding.UTF8, "application/json")

                    ' Enviar requisição
                    Using response = Await httpClient.SendAsync(request)
                        Dim responseContent = Await response.Content.ReadAsStringAsync()

                        If response.IsSuccessStatusCode Then
                            ' Parse da resposta (formato específico do Claude)
                            Dim jsonResponse = JsonConvert.DeserializeObject(responseContent)
                            Dim aiResponse = jsonResponse.content(0).text.ToString()

                            Logger.LogInfo("Claude: Requisição processada com sucesso")
                            Return aiResponse

                        Else
                            Dim errorMsg = $"Erro HTTP {response.StatusCode}: {responseContent}"
                            Logger.LogError($"Claude: {errorMsg}")
                            Throw New HttpRequestException(errorMsg)
                        End If
                End Using

            Catch ex As Exception
                Logger.LogError($"Claude: Erro ao processar requisição: {ex.Message}", ex)
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Método especializado para análise de reuniões
        ''' </summary>
        ''' <param name="transcription">Transcrição da reunião</param>
        ''' <returns>Resumo estruturado da reunião</returns>
        Public Async Function AnalyzeMeeting(transcription As String) As Task(Of String)
            Try
                Logger.LogInfo("Claude: Analisando reunião...")

                Dim meetingPrompt = $"Analise esta transcrição de reunião e forneça um resumo estruturado com:

1. **PONTOS PRINCIPAIS DISCUTIDOS**
2. **DECISÕES TOMADAS**
3. **AÇÕES DEFINIDAS** (quem faz o quê e quando)
4. **PRÓXIMOS PASSOS**
5. **PENDÊNCIAS/QUESTÕES EM ABERTO**

Transcrição:
{transcription}

Formate a resposta de forma clara e profissional."

                Return Await ProcessRequest(meetingPrompt)

            Catch ex As Exception
                Logger.LogError($"Claude: Erro ao analisar reunião: {ex.Message}", ex)
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
            Return False ' Claude requer conexão com internet
        End Function

    End Class

End Namespace