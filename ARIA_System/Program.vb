Imports System
Imports System.Windows.Forms
Imports ARIA_Premium_System.Utils

''' <summary>
''' Ponto de entrada principal da aplicação ARIA Premium System
''' </summary>
Module Program

    ''' <summary>
    ''' Método principal de inicialização
    ''' </summary>
    <STAThread>
    Sub Main()
        Try
            ' Configurar aplicação Windows Forms
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            ' Inicializar sistema de logs
            Logger.Initialize()
            Logger.LogInfo("ARIA Premium System iniciando...")

            ' Carregar configurações
            ConfigManager.LoadConfiguration()

            ' Verificar pré-requisitos do sistema
            If Not CheckSystemRequirements() Then
                MessageBox.Show("Sistema não atende aos requisitos mínimos para executar ARIA.",
                              "Erro de Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Inicializar e executar aplicação principal
            Logger.LogInfo("Iniciando interface principal...")
            Application.Run(New UI.MainForm())

        Catch ex As Exception
            Logger.LogError($"Erro crítico na inicialização: {ex.Message}", ex)
            MessageBox.Show($"Erro crítico ao inicializar ARIA: {ex.Message}",
                          "Erro Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Logger.LogInfo("ARIA Premium System finalizando...")
            Logger.Dispose()
        End Try
    End Sub

    ''' <summary>
    ''' Verifica se o sistema atende aos requisitos mínimos
    ''' </summary>
    ''' <returns>True se atende aos requisitos</returns>
    Private Function CheckSystemRequirements() As Boolean
        Try
            ' Verificar versão do .NET Framework
            Dim netVersion = Environment.Version
            If netVersion.Major < 4 OrElse (netVersion.Major = 4 AndAlso netVersion.Minor < 8) Then
                MessageBox.Show("ARIA requer .NET Framework 4.8 ou superior.",
                              "Requisito não atendido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            ' Verificar memória disponível (mínimo 4GB)
            Dim totalMemory = GC.GetTotalMemory(False)
            If totalMemory < 4L * 1024 * 1024 * 1024 Then ' 4GB
                Logger.LogWarning("Sistema com pouca memória disponível. Recomendado: 8GB+")
            End If

            ' Verificar permissões de escrita
            Dim appPath = Application.StartupPath
            If Not System.IO.Directory.Exists(appPath) Then
                MessageBox.Show("Não foi possível acessar o diretório da aplicação.",
                              "Erro de Permissão", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End If

            Logger.LogInfo("Verificação de requisitos do sistema concluída com sucesso")
            Return True

        Catch ex As Exception
            Logger.LogError($"Erro ao verificar requisitos do sistema: {ex.Message}", ex)
            Return False
        End Try
    End Function

End Module