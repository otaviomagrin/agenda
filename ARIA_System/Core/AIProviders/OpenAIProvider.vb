Imports System
Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports ARIA_Premium_System.Utils

Namespace Core.AIProviders

    ''' <summary>
    ''' Provedor de IA OpenAI GPT-4o Mini - Prioridade 3
    ''' Balanceado entre custo e qualidade
    ''' </summary>
    Public Class OpenAIProvider
        Implements IAIProvider

        Private Shared ReadOnly httpClient As New HttpClient()
        Private Const ENDPOINT As String = "https://api.openai.com/v1/chat/completions"
        Private Const MODEL As String = "gpt-4o-mini"
        Private Const TIMEOUT_MS As Integer = 30000

        ' Custos por token (GPT-4o Mini)
        Private Const INPUT_COST_PER_TOKEN As Decimal = 0.00000015D ' $0.00015 por 1K tokens
        Private Const OUTPUT_COST_PER_TOKEN As Decimal = 0.0000006D ' $0.0006 por 1K tokens

        Shared Sub New()
            ' Configurar HttpClient
            httpClient.Timeout = TimeSpan.FromMilliseconds(TIMEOUT_MS)
            httpClient.DefaultRequestHeaders.Add("User-Agent", "ARIA-Premium-System/1.0")
        End Sub

        Public Function GetProviderName() As String Implements IAIProvider.GetProviderName
            Return "OpenAI GPT-4o Mini"
        End Function

        Public Async Function TestConnection() As Task(Of ConnectionResult) Implements IAIProvider.TestConnection
            Try
                Dim startTime = DateTime.Now

                ' Teste simples de conectividade
                Dim testPrompt = "Responda apenas 'Sistema operacional' para este teste."
                Dim response = Await ProcessRequest(testPrompt)

                Dim latency = DateTime.Now.Subtract(startTime).TotalMilliseconds

                If Not String.IsNullOrEmpty(response) Then
                    Logger.LogInfo($"OpenAI: Teste de conexão bem-sucedido ({latency:F0}ms)")
                    Return New ConnectionResult(True, latency)
                Else
                    Logger.LogWarning("OpenAI: Teste de conexão falhou - resposta vazia")
                    Return New ConnectionResult(False, latency, "Resposta vazia")
                End If

            Catch ex As Exception
                Logger.LogError($"OpenAI: Erro no teste de conexão: {ex.Message}", ex)
                Return New ConnectionResult(False, 0, ex.Message)
            End Try
        End Function

        Public Async Function ProcessRequest(prompt As String) As Task(Of String) Implements IAIProvider.ProcessRequest
            Try
                Logger.LogInfo("OpenAI: Processando requisição...")

                ' Verificar se API key está configurada
                Dim apiKey = ConfigManager.GetOpenAIAPIKey()
                If String.IsNullOrEmpty(apiKey) Then
                    Throw New InvalidOperationException("API Key do OpenAI não configurada")
                End If

                ' Preparar requisição
                Dim requestBody = New With {
                    .model = MODEL,
                    .messages = New Object() {
                        New With {
                            .role = "system",
                            .content = "Você é ARIA, uma assistente de voz inteligente e eficiente. " &
                                     "Forneça respostas precisas, úteis e concisas. " &
                                     "Mantenha um tom profissional mas amigável."
                        },
                        New With {
                            .role = "user",
                            .content = prompt
                        }
                    },
                    .max_tokens = 800,
                    .temperature = 0.7,
                    .top_p = 0.9,
                    .frequency_penalty = 0.0,
                    .presence_penalty = 0.0
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

                            Logger.LogInfo("OpenAI: Requisição processada com sucesso")
                            Return aiResponse

                        Else
                            Dim errorMsg = $"Erro HTTP {response.StatusCode}: {responseContent}"
                            Logger.LogError($"OpenAI: {errorMsg}")
                            Throw New HttpRequestException(errorMsg)
                        End If
                End Using

            Catch ex As Exception
                Logger.LogError($"OpenAI: Erro ao processar requisição: {ex.Message}", ex)
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Método para gerar comandos estruturados para o sistema de agenda
        ''' </summary>
        ''' <param name="voiceCommand">Comando de voz do usuário</param>
        ''' <returns>JSON estruturado para integração com agenda</returns>
        Public Async Function GenerateAgendaCommand(voiceCommand As String) As Task(Of String)
            Try
                Logger.LogInfo("OpenAI: Gerando comando estruturado para agenda...")

                Dim agendaPrompt = $"Analise este comando de voz e gere um JSON estruturado para integração com sistema de agenda.

Comando: {voiceCommand}

Retorne APENAS um JSON válido no formato:
{{
  ""action"": ""add_task|create_reminder|schedule_meeting|list_tasks|sync_agenda"",
  ""parameters"": {{
    ""title"": ""título da tarefa/reunião"",
    ""date"": ""YYYY-MM-DD"",
    ""time"": ""HH:MM"",
    ""description"": ""descrição detalhada"",
    ""priority"": ""high|medium|low"",
    ""category"": ""categoria""
  }},
  ""confidence"": 0.95
}}

Se não conseguir interpretar o comando, retorne:
{{
  ""action"": ""unknown"",
  ""message"": ""Não consegui entender o comando"",
  ""confidence"": 0.0
}}"

                Return Await ProcessRequest(agendaPrompt)

            Catch ex As Exception
                Logger.LogError($"OpenAI: Erro ao gerar comando de agenda: {ex.Message}", ex)
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
            Return False ' OpenAI requer conexão com internet
        End Function

    End Class

End Namespace