Imports System.Threading.Tasks

Namespace Core.AIProviders

    ''' <summary>
    ''' Interface para provedores de IA
    ''' </summary>
    Public Interface IAIProvider

        ''' <summary>
        ''' Obtém o nome do provedor
        ''' </summary>
        ''' <returns>Nome do provedor</returns>
        Function GetProviderName() As String

        ''' <summary>
        ''' Testa a conexão com o provedor
        ''' </summary>
        ''' <returns>Resultado do teste de conexão</returns>
        Function TestConnection() As Task(Of ConnectionResult)

        ''' <summary>
        ''' Processa uma requisição
        ''' </summary>
        ''' <param name="prompt">Prompt/pergunta do usuário</param>
        ''' <returns>Resposta da IA</returns>
        Function ProcessRequest(prompt As String) As Task(Of String)

        ''' <summary>
        ''' Obtém o custo por token de entrada
        ''' </summary>
        ''' <returns>Custo por token de entrada</returns>
        Function GetInputCostPerToken() As Decimal

        ''' <summary>
        ''' Obtém o custo por token de saída
        ''' </summary>
        ''' <returns>Custo por token de saída</returns>
        Function GetOutputCostPerToken() As Decimal

        ''' <summary>
        ''' Verifica se o provedor está disponível offline
        ''' </summary>
        ''' <returns>True se funciona offline</returns>
        Function IsOfflineCapable() As Boolean

    End Interface

    ''' <summary>
    ''' Resultado de teste de conexão
    ''' </summary>
    Public Structure ConnectionResult
        Public IsSuccessful As Boolean
        Public LatencyMs As Double
        Public ErrorMessage As String
        Public ResponseTime As DateTime

        Public Sub New(successful As Boolean, latency As Double, Optional errorMsg As String = "")
            IsSuccessful = successful
            LatencyMs = latency
            ErrorMessage = errorMsg
            ResponseTime = DateTime.Now
        End Sub
    End Structure

End Namespace