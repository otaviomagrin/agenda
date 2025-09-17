Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Linq
Imports Newtonsoft.Json
Imports ARIA_Premium_System.Utils

Namespace Utils

    ''' <summary>
    ''' Sistema de monitoramento de custos para APIs do ARIA Premium System
    ''' Controla orçamento mensal, switching automático e alertas
    ''' </summary>
    Public Class CostTracker

        ' Configurações de budget (valores em USD)
        Private Shared ReadOnly MONTHLY_BUDGET As Decimal = 29.0D
        Private Shared ReadOnly ALERT_THRESHOLD As Double = 0.8 ' 80%
        Private Shared ReadOnly CRITICAL_THRESHOLD As Double = 0.9 ' 90%

        ' Custos por token/request para cada provider
        Private Shared ReadOnly providerCosts As New Dictionary(Of String, ProviderCostConfig) From {
            {"Grok", New ProviderCostConfig With {
                .CostPerInputToken = 0.000015D,
                .CostPerOutputToken = 0.000075D,
                .MonthlyAllocation = 5.0D,
                .HasFreeAlternative = False
            }},
            {"Claude", New ProviderCostConfig With {
                .CostPerInputToken = 0.000003D,
                .CostPerOutputToken = 0.000015D,
                .MonthlyAllocation = 6.0D,
                .HasFreeAlternative = False
            }},
            {"OpenAI", New ProviderCostConfig With {
                .CostPerInputToken = 0.00000015D,
                .CostPerOutputToken = 0.0000006D,
                .MonthlyAllocation = 3.0D,
                .HasFreeAlternative = False
            }},
            {"Gemini", New ProviderCostConfig With {
                .CostPerInputToken = 0.0D,
                .CostPerOutputToken = 0.0D,
                .MonthlyAllocation = 0.0D,
                .HasFreeAlternative = True
            }},
            {"AssemblyAI", New ProviderCostConfig With {
                .CostPerSecond = 0.000416D, ' ~$1.50/hour
                .MonthlyAllocation = 8.0D,
                .HasFreeAlternative = False
            }},
            {"ElevenLabs", New ProviderCostConfig With {
                .CostPerCharacter = 0.00003D,
                .MonthlyAllocation = 5.0D,
                .HasFreeAlternative = False
            }},
            {"Picovoice", New ProviderCostConfig With {
                .CostPerRequest = 0.0025D,
                .MonthlyAllocation = 2.0D,
                .HasFreeAlternative = False
            }},
            {"Ollama", New ProviderCostConfig With {
                .CostPerInputToken = 0.0D,
                .CostPerOutputToken = 0.0D,
                .MonthlyAllocation = 0.0D,
                .HasFreeAlternative = True
            }}
        }

        ' Cache de custos em memória
        Private Shared dailyCosts As New Dictionary(Of String, Decimal)
        Private Shared monthlyCosts As New Dictionary(Of String, Decimal)
        Private Shared isInEmergencyMode As Boolean = False

        ' Arquivo de persistência
        Private Shared ReadOnly costDataPath As String = Path.Combine(Application.StartupPath, "data", "cost_tracking.json")

        ' Eventos para notificações
        Public Shared Event BudgetAlertTriggered(threshold As Double, currentSpend As Decimal, monthlyBudget As Decimal)
        Public Shared Event EmergencyModeActivated(reason As String)
        Public Shared Event ProviderSwitched(fromProvider As String, toProvider As String, reason As String)

        ''' <summary>
        ''' Configuração de custos por provider
        ''' </summary>
        Public Class ProviderCostConfig
            Public Property CostPerInputToken As Decimal = 0.0D
            Public Property CostPerOutputToken As Decimal = 0.0D
            Public Property CostPerSecond As Decimal = 0.0D
            Public Property CostPerCharacter As Decimal = 0.0D
            Public Property CostPerRequest As Decimal = 0.0D
            Public Property MonthlyAllocation As Decimal = 0.0D
            Public Property HasFreeAlternative As Boolean = False
        End Class

        ''' <summary>
        ''' Estrutura de dados para persistência
        ''' </summary>
        Private Class CostData
            Public Property DailyCosts As Dictionary(Of String, Decimal)
            Public Property MonthlyCosts As Dictionary(Of String, Decimal)
            Public Property LastUpdate As DateTime
            Public Property CurrentMonth As String
        End Class

        ''' <summary>
        ''' Inicializa o sistema de monitoramento de custos
        ''' </summary>
        Public Shared Sub Initialize()
            Try
                Logger.LogInfo("Inicializando sistema de monitoramento de custos...", "CostTracker")

                ' Criar diretório de dados se não existir
                Dim dataDir = Path.GetDirectoryName(costDataPath)
                If Not Directory.Exists(dataDir) Then
                    Directory.CreateDirectory(dataDir)
                End If

                ' Carregar dados existentes
                LoadCostData()

                ' Verificar se é um novo mês
                CheckAndResetMonthlyData()

                Logger.LogInfo("Sistema de monitoramento de custos inicializado com sucesso", "CostTracker")

            Catch ex As Exception
                Logger.LogError($"Erro ao inicializar sistema de custos: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Registra custo de uso de IA por tokens
        ''' </summary>
        Public Shared Sub LogAIUsage(provider As String, inputTokens As Integer, outputTokens As Integer)
            Try
                If Not providerCosts.ContainsKey(provider) Then
                    Logger.LogWarning($"Provider {provider} não encontrado na configuração de custos", "CostTracker")
                    Return
                End If

                Dim config = providerCosts(provider)
                Dim cost = (inputTokens * config.CostPerInputToken) + (outputTokens * config.CostPerOutputToken)

                LogCost(provider, cost, $"Tokens: {inputTokens}in/{outputTokens}out")

            Catch ex As Exception
                Logger.LogError($"Erro ao registrar uso de IA: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Registra custo de STT por tempo
        ''' </summary>
        Public Shared Sub LogSTTUsage(provider As String, durationSeconds As Double)
            Try
                If Not providerCosts.ContainsKey(provider) Then Return

                Dim config = providerCosts(provider)
                Dim cost = durationSeconds * config.CostPerSecond

                LogCost(provider, cost, $"Audio: {durationSeconds:F1}s")

            Catch ex As Exception
                Logger.LogError($"Erro ao registrar uso de STT: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Registra custo de TTS por caracteres
        ''' </summary>
        Public Shared Sub LogTTSUsage(provider As String, characterCount As Integer)
            Try
                If Not providerCosts.ContainsKey(provider) Then Return

                Dim config = providerCosts(provider)
                Dim cost = characterCount * config.CostPerCharacter

                LogCost(provider, cost, $"Characters: {characterCount}")

            Catch ex As Exception
                Logger.LogError($"Erro ao registrar uso de TTS: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Registra custo de wake word por requests
        ''' </summary>
        Public Shared Sub LogWakeWordUsage(provider As String, requestCount As Integer)
            Try
                If Not providerCosts.ContainsKey(provider) Then Return

                Dim config = providerCosts(provider)
                Dim cost = requestCount * config.CostPerRequest

                LogCost(provider, cost, $"Requests: {requestCount}")

            Catch ex As Exception
                Logger.LogError($"Erro ao registrar uso de wake word: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Registra custo genérico
        ''' </summary>
        Private Shared Sub LogCost(provider As String, cost As Decimal, details As String)
            Try
                If cost <= 0 Then Return

                Dim today = DateTime.Now.ToString("yyyy-MM-dd")
                Dim dailyKey = $"{provider}_{today}"
                Dim monthlyKey = provider

                ' Atualizar custos diários
                If dailyCosts.ContainsKey(dailyKey) Then
                    dailyCosts(dailyKey) += cost
                Else
                    dailyCosts(dailyKey) = cost
                End If

                ' Atualizar custos mensais
                If monthlyCosts.ContainsKey(monthlyKey) Then
                    monthlyCosts(monthlyKey) += cost
                Else
                    monthlyCosts(monthlyKey) = cost
                End If

                ' Log detalhado
                Logger.LogInfo($"Custo registrado - {provider}: ${cost:F6} ({details})", "CostTracker")

                ' Verificar thresholds
                CheckBudgetThresholds()

                ' Salvar dados
                SaveCostData()

            Catch ex As Exception
                Logger.LogError($"Erro ao registrar custo: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Verifica se os thresholds de orçamento foram excedidos
        ''' </summary>
        Private Shared Sub CheckBudgetThresholds()
            Try
                Dim currentMonthlySpend = GetCurrentMonthlySpend()
                Dim spendPercentage = currentMonthlySpend / MONTHLY_BUDGET

                ' Verificar threshold crítico (90%)
                If spendPercentage >= CRITICAL_THRESHOLD AndAlso Not isInEmergencyMode Then
                    ActivateEmergencyMode("Orçamento mensal excedeu 90%")

                    ' Verificar threshold de alerta (80%)
                ElseIf spendPercentage >= ALERT_THRESHOLD Then
                    RaiseEvent BudgetAlertTriggered(spendPercentage, currentMonthlySpend, MONTHLY_BUDGET)
                    Logger.LogWarning($"Alerta de orçamento: {spendPercentage:P1} do budget mensal utilizado (${currentMonthlySpend:F2}/${MONTHLY_BUDGET:F2})", "CostTracker")
                End If

            Catch ex As Exception
                Logger.LogError($"Erro ao verificar thresholds: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Ativa modo de emergência com providers gratuitos
        ''' </summary>
        Private Shared Sub ActivateEmergencyMode(reason As String)
            Try
                isInEmergencyMode = True
                RaiseEvent EmergencyModeActivated(reason)
                Logger.LogCritical($"MODO EMERGÊNCIA ATIVADO: {reason}", Nothing, "CostTracker")

                ' Forçar uso apenas de providers gratuitos
                ConfigManager.GetConfiguration().Budget.AutoSwitchToFree = True
                ConfigManager.SaveConfiguration()

            Catch ex As Exception
                Logger.LogError($"Erro ao ativar modo emergência: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Obtém o melhor provider considerando custos
        ''' </summary>
        Public Shared Function GetOptimalProvider(providerList As List(Of String)) As String
            Try
                ' Se em modo emergência, usar apenas providers gratuitos
                If isInEmergencyMode Then
                    Dim freeProviders = providerList.Where(Function(p) providerCosts.ContainsKey(p) AndAlso providerCosts(p).HasFreeAlternative).ToList()
                    If freeProviders.Any() Then
                        Return freeProviders.First()
                    End If
                End If

                ' Verificar alocação de budget por provider
                Dim optimalProvider = providerList.FirstOrDefault()
                Dim lowestUsagePercentage = Double.MaxValue

                For Each provider In providerList
                    If providerCosts.ContainsKey(provider) Then
                        Dim config = providerCosts(provider)
                        Dim currentSpend = GetProviderMonthlySpend(provider)
                        Dim usagePercentage = If(config.MonthlyAllocation > 0, currentSpend / config.MonthlyAllocation, 0)

                        If usagePercentage < lowestUsagePercentage Then
                            lowestUsagePercentage = usagePercentage
                            optimalProvider = provider
                        End If
                    End If
                Next

                Return optimalProvider

            Catch ex As Exception
                Logger.LogError($"Erro ao obter provider ótimo: {ex.Message}", ex, "CostTracker")
                Return providerList.FirstOrDefault()
            End Try
        End Function

        ''' <summary>
        ''' Obtém gasto mensal atual total
        ''' </summary>
        Public Shared Function GetCurrentMonthlySpend() As Decimal
            Return monthlyCosts.Values.Sum()
        End Function

        ''' <summary>
        ''' Obtém gasto mensal de um provider específico
        ''' </summary>
        Public Shared Function GetProviderMonthlySpend(provider As String) As Decimal
            Return If(monthlyCosts.ContainsKey(provider), monthlyCosts(provider), 0.0D)
        End Function

        ''' <summary>
        ''' Obtém gasto diário de um provider específico
        ''' </summary>
        Public Shared Function GetProviderDailySpend(provider As String) As Decimal
            Dim today = DateTime.Now.ToString("yyyy-MM-dd")
            Dim dailyKey = $"{provider}_{today}"
            Return If(dailyCosts.ContainsKey(dailyKey), dailyCosts(dailyKey), 0.0D)
        End Function

        ''' <summary>
        ''' Obtém relatório completo de custos
        ''' </summary>
        Public Shared Function GetCostReport() As Dictionary(Of String, Object)
            Try
                Dim currentSpend = GetCurrentMonthlySpend()
                Dim remainingBudget = MONTHLY_BUDGET - currentSpend
                Dim usagePercentage = currentSpend / MONTHLY_BUDGET

                Dim providerBreakdown As New Dictionary(Of String, Object)
                For Each provider In providerCosts.Keys
                    Dim monthlySpend = GetProviderMonthlySpend(provider)
                    Dim allocation = providerCosts(provider).MonthlyAllocation
                    Dim allocationUsage = If(allocation > 0, monthlySpend / allocation, 0)

                    providerBreakdown(provider) = New With {
                        .MonthlySpend = monthlySpend,
                        .Allocation = allocation,
                        .AllocationUsage = allocationUsage,
                        .DailySpend = GetProviderDailySpend(provider)
                    }
                Next

                Return New Dictionary(Of String, Object) From {
                    {"TotalMonthlySpend", currentSpend},
                    {"MonthlyBudget", MONTHLY_BUDGET},
                    {"RemainingBudget", remainingBudget},
                    {"UsagePercentage", usagePercentage},
                    {"IsEmergencyMode", isInEmergencyMode},
                    {"AlertThreshold", ALERT_THRESHOLD},
                    {"CriticalThreshold", CRITICAL_THRESHOLD},
                    {"ProviderBreakdown", providerBreakdown},
                    {"LastUpdate", DateTime.Now}
                }

            Catch ex As Exception
                Logger.LogError($"Erro ao gerar relatório de custos: {ex.Message}", ex, "CostTracker")
                Return New Dictionary(Of String, Object)
            End Try
        End Function

        ''' <summary>
        ''' Reseta dados mensais se necessário
        ''' </summary>
        Private Shared Sub CheckAndResetMonthlyData()
            Try
                Dim currentMonth = DateTime.Now.ToString("yyyy-MM")
                Dim dataDir = Path.GetDirectoryName(costDataPath)
                Dim monthFile = Path.Combine(dataDir, $"current_month.txt")

                Dim lastMonth = ""
                If File.Exists(monthFile) Then
                    lastMonth = File.ReadAllText(monthFile).Trim()
                End If

                If lastMonth <> currentMonth Then
                    Logger.LogInfo($"Novo mês detectado ({currentMonth}), resetando dados mensais", "CostTracker")

                    ' Reset dados mensais
                    monthlyCosts.Clear()
                    isInEmergencyMode = False

                    ' Salvar mês atual
                    File.WriteAllText(monthFile, currentMonth)

                    ' Arquivar dados do mês anterior se existir
                    If Not String.IsNullOrEmpty(lastMonth) Then
                        ArchiveMonthlyData(lastMonth)
                    End If
                End If

            Catch ex As Exception
                Logger.LogError($"Erro ao verificar/resetar dados mensais: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Arquiva dados mensais
        ''' </summary>
        Private Shared Sub ArchiveMonthlyData(month As String)
            Try
                Dim archiveDir = Path.Combine(Path.GetDirectoryName(costDataPath), "archive")
                If Not Directory.Exists(archiveDir) Then
                    Directory.CreateDirectory(archiveDir)
                End If

                Dim archiveFile = Path.Combine(archiveDir, $"costs_{month}.json")
                Dim archiveData = New With {
                    .Month = month,
                    .MonthlyCosts = monthlyCosts,
                    .TotalSpend = monthlyCosts.Values.Sum(),
                    .ArchivedAt = DateTime.Now
                }

                File.WriteAllText(archiveFile, JsonConvert.SerializeObject(archiveData, Formatting.Indented))
                Logger.LogInfo($"Dados do mês {month} arquivados com sucesso", "CostTracker")

            Catch ex As Exception
                Logger.LogError($"Erro ao arquivar dados mensais: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Carrega dados de custo do arquivo
        ''' </summary>
        Private Shared Sub LoadCostData()
            Try
                If File.Exists(costDataPath) Then
                    Dim jsonContent = File.ReadAllText(costDataPath)
                    Dim data = JsonConvert.DeserializeObject(Of CostData)(jsonContent)

                    If data IsNot Nothing Then
                        If data.DailyCosts IsNot Nothing Then
                            dailyCosts = data.DailyCosts
                        End If
                        If data.MonthlyCosts IsNot Nothing Then
                            monthlyCosts = data.MonthlyCosts
                        End If
                    End If

                    Logger.LogInfo("Dados de custo carregados com sucesso", "CostTracker")
                End If

            Catch ex As Exception
                Logger.LogError($"Erro ao carregar dados de custo: {ex.Message}", ex, "CostTracker")
                ' Inicializar com dados vazios em caso de erro
                dailyCosts = New Dictionary(Of String, Decimal)
                monthlyCosts = New Dictionary(Of String, Decimal)
            End Try
        End Sub

        ''' <summary>
        ''' Salva dados de custo no arquivo
        ''' </summary>
        Private Shared Sub SaveCostData()
            Try
                Dim data As New CostData With {
                    .DailyCosts = dailyCosts,
                    .MonthlyCosts = monthlyCosts,
                    .LastUpdate = DateTime.Now,
                    .CurrentMonth = DateTime.Now.ToString("yyyy-MM")
                }

                Dim jsonContent = JsonConvert.SerializeObject(data, Formatting.Indented)
                File.WriteAllText(costDataPath, jsonContent)

            Catch ex As Exception
                Logger.LogError($"Erro ao salvar dados de custo: {ex.Message}", ex, "CostTracker")
            End Try
        End Sub

        ''' <summary>
        ''' Força reset do modo emergência (use com cuidado)
        ''' </summary>
        Public Shared Sub ResetEmergencyMode()
            isInEmergencyMode = False
            Logger.LogWarning("Modo emergência resetado manualmente", "CostTracker")
        End Sub

    End Class

End Namespace