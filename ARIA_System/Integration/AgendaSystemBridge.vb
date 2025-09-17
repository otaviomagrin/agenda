Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.IO

Namespace ARIA.Integration

    ''' &lt;summary&gt;
    ''' Bridge to integrate ARIA with the existing Electron agenda system
    ''' Provides voice command interface for agenda management operations
    ''' &lt;/summary&gt;
    Public Class AgendaSystemBridge
        Implements IDisposable

#Region "Properties and Fields"

        Private _httpClient As HttpClient
        Private _baseUrl As String = "http://localhost:3002/api/aria"
        Private _isConnected As Boolean = False
        Private _lastSyncTime As DateTime
        Private _connectionRetryCount As Integer = 0
        Private Const MAX_RETRY_COUNT As Integer = 3
        Private _disposedValue As Boolean = False

        ' Events
        Public Event ConnectionEstablished()
        Public Event ConnectionLost()
        Public Event SyncCompleted(syncResult As SyncResult)
        Public Event TaskCreated(task As AgendaTask)
        Public Event ProjectCreated(project As AgendaProject)
        Public Event ErrorOccurred(errorMessage As String)

        ' Properties
        Public ReadOnly Property IsConnected As Boolean
            Get
                Return _isConnected
            End Get
        End Property

        Public ReadOnly Property LastSyncTime As DateTime
            Get
                Return _lastSyncTime
            End Get
        End Property

        Public Property BaseUrl As String
            Get
                Return _baseUrl
            End Get
            Set(value As String)
                _baseUrl = value
            End Set
        End Property

#End Region

#Region "Constructor"

        Public Sub New(Optional baseUrl As String = "http://localhost:3001/api")
            _baseUrl = baseUrl
            InitializeHttpClient()
        End Sub

#End Region

#Region "Connection Management"

        ''' &lt;summary&gt;
        ''' Establishes connection to the Electron agenda system
        ''' &lt;/summary&gt;
        ''' &lt;returns&gt;True if connection successful&lt;/returns&gt;
        Public Async Function ConnectAsync() As Task(Of Boolean)
            Try
                ' Test connection with health check
                Dim healthResponse = Await _httpClient.GetAsync($"{_baseUrl}/health")

                If healthResponse.IsSuccessStatusCode Then
                    _isConnected = True
                    _connectionRetryCount = 0
                    RaiseEvent ConnectionEstablished()
                    Return True
                Else
                    _isConnected = False
                    Return False
                End If

            Catch ex As Exception
                _isConnected = False
                _connectionRetryCount += 1

                If _connectionRetryCount <= MAX_RETRY_COUNT Then
                    ' Retry connection after delay
                    Await Task.Delay(2000)
                    Return Await ConnectAsync()
                Else
                    RaiseEvent ErrorOccurred($"Failed to connect to agenda system: {ex.Message}")
                    Return False
                End If
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Disconnects from the agenda system
        ''' &lt;/summary&gt;
        Public Sub Disconnect()
            _isConnected = False
            RaiseEvent ConnectionLost()
        End Sub

        ''' &lt;summary&gt;
        ''' Checks if the agenda system is available
        ''' &lt;/summary&gt;
        ''' &lt;returns&gt;True if system is available&lt;/returns&gt;
        Public Async Function CheckAvailabilityAsync() As Task(Of Boolean)
            Try
                Dim response = Await _httpClient.GetAsync($"{_baseUrl}/health")
                Return response.IsSuccessStatusCode
            Catch ex As Exception
                Return False
            End Try
        End Function

#End Region

#Region "Voice Command Processing"

        ''' &lt;summary&gt;
        ''' Processes voice commands related to agenda management
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Parsed voice command&lt;/param&gt;
        ''' &lt;returns&gt;Response message&lt;/returns&gt;
        Public Async Function ProcessVoiceCommandAsync(command As VoiceCommand) As Task(Of String)
            Try
                If Not _isConnected Then
                    Dim connected = Await ConnectAsync()
                    If Not connected Then
                        Return "Não foi possível conectar ao sistema de agenda."
                    End If
                End If

                Select Case command.Action.ToLowerInvariant()
                    Case "create_task", "add_task", "nova_tarefa"
                        Return Await CreateTaskFromVoiceAsync(command)

                    Case "create_project", "add_project", "novo_projeto"
                        Return Await CreateProjectFromVoiceAsync(command)

                    Case "list_tasks", "minhas_tarefas", "listar_tarefas"
                        Return Await GetTasksAsync(command)

                    Case "list_projects", "meus_projetos", "listar_projetos"
                        Return Await GetProjectsAsync(command)

                    Case "schedule_meeting", "agendar_reuniao"
                        Return Await ScheduleMeetingAsync(command)

                    Case "check_schedule", "ver_agenda", "cronograma"
                        Return Await GetScheduleAsync(command)

                    Case "sync", "sincronizar"
                        Return Await SyncAgendaAsync()

                    Case "update_task", "atualizar_tarefa"
                        Return Await UpdateTaskAsync(command)

                    Case "complete_task", "concluir_tarefa"
                        Return Await CompleteTaskAsync(command)

                    Case "search", "buscar", "procurar"
                        Return Await SearchAgendaAsync(command)

                    Case Else
                        Return $"Comando não reconhecido: {command.Action}"
                End Select

            Catch ex As Exception
                RaiseEvent ErrorOccurred($"Erro ao processar comando: {ex.Message}")
                Return $"Erro ao processar comando: {ex.Message}"
            End Try
        End Function

#End Region

#Region "Task Management"

        ''' &lt;summary&gt;
        ''' Creates a new task from voice command
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with task details&lt;/param&gt;
        ''' &lt;returns&gt;Response message&lt;/returns&gt;
        Private Async Function CreateTaskFromVoiceAsync(command As VoiceCommand) As Task(Of String)
            Try
                Dim task = ParseTaskFromCommand(command)
                Dim response = Await CreateTaskAsync(task)

                If response.Success Then
                    RaiseEvent TaskCreated(task)
                    Return $"Tarefa '{task.Title}' criada com sucesso."
                Else
                    Return $"Erro ao criar tarefa: {response.ErrorMessage}"
                End If

            Catch ex As Exception
                Return $"Erro ao criar tarefa: {ex.Message}"
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Creates a task via API
        ''' &lt;/summary&gt;
        ''' &lt;param name="task"&gt;Task to create&lt;/param&gt;
        ''' &lt;returns&gt;API response&lt;/returns&gt;
        Public Async Function CreateTaskAsync(task As AgendaTask) As Task(Of APIResponse)
            Try
                Dim json = JsonConvert.SerializeObject(task)
                Dim content = New StringContent(json, Encoding.UTF8, "application/json")

                Dim response = Await _httpClient.PostAsync($"{_baseUrl}/tasks", content)
                Dim responseContent = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    Return New APIResponse(True, responseContent)
                Else
                    Return New APIResponse(False, $"HTTP {response.StatusCode}: {responseContent}")
                End If

            Catch ex As Exception
                Return New APIResponse(False, ex.Message)
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Gets tasks based on voice command parameters
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with filter parameters&lt;/param&gt;
        ''' &lt;returns&gt;Response with task list&lt;/returns&gt;
        Private Async Function GetTasksAsync(command As VoiceCommand) As Task(Of String)
            Try
                Dim filter = ParseTaskFilter(command)
                Dim tasks = Await GetTasksAsync(filter)

                If tasks.Count = 0 Then
                    Return "Nenhuma tarefa encontrada."
                End If

                Dim response = New StringBuilder()
                response.AppendLine($"Encontradas {tasks.Count} tarefas:")

                For Each task In tasks.Take(5) ' Limit to first 5 for voice response
                    response.AppendLine($"- {task.Title} (Prioridade: {task.Priority}, Status: {task.Status})")
                Next

                If tasks.Count > 5 Then
                    response.AppendLine($"... e mais {tasks.Count - 5} tarefas.")
                End If

                Return response.ToString()

            Catch ex As Exception
                Return $"Erro ao obter tarefas: {ex.Message}"
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Gets tasks from API with optional filter
        ''' &lt;/summary&gt;
        ''' &lt;param name="filter"&gt;Optional task filter&lt;/param&gt;
        ''' &lt;returns&gt;List of tasks&lt;/returns&gt;
        Public Async Function GetTasksAsync(Optional filter As TaskFilter = Nothing) As Task(Of List(Of AgendaTask))
            Try
                Dim url = $"{_baseUrl}/tasks"
                If filter IsNot Nothing Then
                    url += BuildFilterQuery(filter)
                End If

                Dim response = Await _httpClient.GetAsync(url)
                If response.IsSuccessStatusCode Then
                    Dim content = Await response.Content.ReadAsStringAsync()
                    Return JsonConvert.DeserializeObject(Of List(Of AgendaTask))(content)
                Else
                    Return New List(Of AgendaTask)()
                End If

            Catch ex As Exception
                RaiseEvent ErrorOccurred($"Erro ao obter tarefas: {ex.Message}")
                Return New List(Of AgendaTask)()
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Updates a task based on voice command
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with update details&lt;/param&gt;
        ''' &lt;returns&gt;Response message&lt;/returns&gt;
        Private Async Function UpdateTaskAsync(command As VoiceCommand) As Task(Of String)
            Try
                ' Parse task identifier and updates from command
                Dim taskUpdate = ParseTaskUpdate(command)

                Dim response = Await UpdateTaskAsync(taskUpdate.TaskId, taskUpdate.Updates)

                If response.Success Then
                    Return $"Tarefa atualizada com sucesso."
                Else
                    Return $"Erro ao atualizar tarefa: {response.ErrorMessage}"
                End If

            Catch ex As Exception
                Return $"Erro ao atualizar tarefa: {ex.Message}"
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Updates a task via API
        ''' &lt;/summary&gt;
        ''' &lt;param name="taskId"&gt;Task ID to update&lt;/param&gt;
        ''' &lt;param name="updates"&gt;Updates to apply&lt;/param&gt;
        ''' &lt;returns&gt;API response&lt;/returns&gt;
        Public Async Function UpdateTaskAsync(taskId As String, updates As Object) As Task(Of APIResponse)
            Try
                Dim json = JsonConvert.SerializeObject(updates)
                Dim content = New StringContent(json, Encoding.UTF8, "application/json")

                Dim response = Await _httpClient.PutAsync($"{_baseUrl}/tasks/{taskId}", content)
                Dim responseContent = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    Return New APIResponse(True, responseContent)
                Else
                    Return New APIResponse(False, $"HTTP {response.StatusCode}: {responseContent}")
                End If

            Catch ex As Exception
                Return New APIResponse(False, ex.Message)
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Marks a task as completed
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with task identifier&lt;/param&gt;
        ''' &lt;returns&gt;Response message&lt;/returns&gt;
        Private Async Function CompleteTaskAsync(command As VoiceCommand) As Task(Of String)
            Try
                Dim taskId = ParseTaskIdentifier(command)

                Dim updates = New With {.status = "completed", .completedAt = DateTime.UtcNow}
                Dim response = Await UpdateTaskAsync(taskId, updates)

                If response.Success Then
                    Return $"Tarefa marcada como concluída."
                Else
                    Return $"Erro ao concluir tarefa: {response.ErrorMessage}"
                End If

            Catch ex As Exception
                Return $"Erro ao concluir tarefa: {ex.Message}"
            End Try
        End Function

#End Region

#Region "Project Management"

        ''' &lt;summary&gt;
        ''' Creates a new project from voice command
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with project details&lt;/param&gt;
        ''' &lt;returns&gt;Response message&lt;/returns&gt;
        Private Async Function CreateProjectFromVoiceAsync(command As VoiceCommand) As Task(Of String)
            Try
                Dim project = ParseProjectFromCommand(command)
                Dim response = Await CreateProjectAsync(project)

                If response.Success Then
                    RaiseEvent ProjectCreated(project)
                    Return $"Projeto '{project.Name}' criado com sucesso."
                Else
                    Return $"Erro ao criar projeto: {response.ErrorMessage}"
                End If

            Catch ex As Exception
                Return $"Erro ao criar projeto: {ex.Message}"
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Creates a project via API
        ''' &lt;/summary&gt;
        ''' &lt;param name="project"&gt;Project to create&lt;/param&gt;
        ''' &lt;returns&gt;API response&lt;/returns&gt;
        Public Async Function CreateProjectAsync(project As AgendaProject) As Task(Of APIResponse)
            Try
                Dim json = JsonConvert.SerializeObject(project)
                Dim content = New StringContent(json, Encoding.UTF8, "application/json")

                Dim response = Await _httpClient.PostAsync($"{_baseUrl}/projects", content)
                Dim responseContent = Await response.Content.ReadAsStringAsync()

                If response.IsSuccessStatusCode Then
                    Return New APIResponse(True, responseContent)
                Else
                    Return New APIResponse(False, $"HTTP {response.StatusCode}: {responseContent}")
                End If

            Catch ex As Exception
                Return New APIResponse(False, ex.Message)
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Gets projects based on voice command
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with filter parameters&lt;/param&gt;
        ''' &lt;returns&gt;Response with project list&lt;/returns&gt;
        Private Async Function GetProjectsAsync(command As VoiceCommand) As Task(Of String)
            Try
                Dim projects = Await GetProjectsAsync()

                If projects.Count = 0 Then
                    Return "Nenhum projeto encontrado."
                End If

                Dim response = New StringBuilder()
                response.AppendLine($"Encontrados {projects.Count} projetos:")

                For Each project In projects.Take(5)
                    response.AppendLine($"- {project.Name} (Status: {project.Status})")
                Next

                If projects.Count > 5 Then
                    response.AppendLine($"... e mais {projects.Count - 5} projetos.")
                End If

                Return response.ToString()

            Catch ex As Exception
                Return $"Erro ao obter projetos: {ex.Message}"
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Gets projects from API
        ''' &lt;/summary&gt;
        ''' &lt;returns&gt;List of projects&lt;/returns&gt;
        Public Async Function GetProjectsAsync() As Task(Of List(Of AgendaProject))
            Try
                Dim response = Await _httpClient.GetAsync($"{_baseUrl}/projects")
                If response.IsSuccessStatusCode Then
                    Dim content = Await response.Content.ReadAsStringAsync()
                    Return JsonConvert.DeserializeObject(Of List(Of AgendaProject))(content)
                Else
                    Return New List(Of AgendaProject)()
                End If

            Catch ex As Exception
                RaiseEvent ErrorOccurred($"Erro ao obter projetos: {ex.Message}")
                Return New List(Of AgendaProject)()
            End Try
        End Function

#End Region

#Region "Meeting and Schedule Management"

        ''' &lt;summary&gt;
        ''' Schedules a meeting from voice command
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with meeting details&lt;/param&gt;
        ''' &lt;returns&gt;Response message&lt;/returns&gt;
        Private Async Function ScheduleMeetingAsync(command As VoiceCommand) As Task(Of String)
            Try
                Dim meeting = ParseMeetingFromCommand(command)

                ' Create as a task with type "meeting"
                Dim meetingTask = New AgendaTask With {
                    .Title = meeting.Title,
                    .Description = meeting.Description,
                    .DueDate = meeting.DateTime,
                    .Priority = "medium",
                    .Type = "meeting",
                    .Status = "pending"
                }

                Dim response = Await CreateTaskAsync(meetingTask)

                If response.Success Then
                    Return $"Reunião '{meeting.Title}' agendada para {meeting.DateTime:dd/MM/yyyy HH:mm}."
                Else
                    Return $"Erro ao agendar reunião: {response.ErrorMessage}"
                End If

            Catch ex As Exception
                Return $"Erro ao agendar reunião: {ex.Message}"
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Gets schedule information
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with date parameters&lt;/param&gt;
        ''' &lt;returns&gt;Schedule information&lt;/returns&gt;
        Private Async Function GetScheduleAsync(command As VoiceCommand) As Task(Of String)
            Try
                Dim dateFilter = ParseDateFilter(command)
                Dim filter = New TaskFilter With {
                    .StartDate = dateFilter.StartDate,
                    .EndDate = dateFilter.EndDate,
                    .Type = "meeting"
                }

                Dim tasks = Await GetTasksAsync(filter)
                Dim meetings = tasks.Where(Function(t) t.Type = "meeting").OrderBy(Function(t) t.DueDate).ToList()

                If meetings.Count = 0 Then
                    Return $"Nenhuma reunião agendada para {dateFilter.Description}."
                End If

                Dim response = New StringBuilder()
                response.AppendLine($"Agenda para {dateFilter.Description}:")

                For Each meeting In meetings
                    If meeting.DueDate.HasValue Then
                        response.AppendLine($"- {meeting.DueDate.Value:HH:mm} - {meeting.Title}")
                    End If
                Next

                Return response.ToString()

            Catch ex As Exception
                Return $"Erro ao obter agenda: {ex.Message}"
            End Try
        End Function

#End Region

#Region "Search and Sync"

        ''' &lt;summary&gt;
        ''' Searches agenda items
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Voice command with search parameters&lt;/param&gt;
        ''' &lt;returns&gt;Search results&lt;/returns&gt;
        Private Async Function SearchAgendaAsync(command As VoiceCommand) As Task(Of String)
            Try
                Dim searchTerm = ExtractSearchTerm(command)

                Dim tasks = Await GetTasksAsync()
                Dim projects = Await GetProjectsAsync()

                Dim matchingTasks = tasks.Where(Function(t)
                    t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) OrElse
                    (t.Description IsNot Nothing AndAlso t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).Take(3).ToList()

                Dim matchingProjects = projects.Where(Function(p)
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) OrElse
                    (p.Description IsNot Nothing AndAlso p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).Take(3).ToList()

                If matchingTasks.Count = 0 AndAlso matchingProjects.Count = 0 Then
                    Return $"Nenhum resultado encontrado para '{searchTerm}'."
                End If

                Dim response = New StringBuilder()
                response.AppendLine($"Resultados para '{searchTerm}':")

                If matchingTasks.Count > 0 Then
                    response.AppendLine("Tarefas:")
                    For Each task In matchingTasks
                        response.AppendLine($"- {task.Title}")
                    Next
                End If

                If matchingProjects.Count > 0 Then
                    response.AppendLine("Projetos:")
                    For Each project In matchingProjects
                        response.AppendLine($"- {project.Name}")
                    Next
                End If

                Return response.ToString()

            Catch ex As Exception
                Return $"Erro na busca: {ex.Message}"
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Synchronizes agenda data
        ''' &lt;/summary&gt;
        ''' &lt;returns&gt;Sync result message&lt;/returns&gt;
        Private Async Function SyncAgendaAsync() As Task(Of String)
            Try
                Dim response = Await _httpClient.PostAsync($"{_baseUrl}/sync/force", Nothing)

                If response.IsSuccessStatusCode Then
                    _lastSyncTime = DateTime.UtcNow
                    Dim syncResult = New SyncResult With {
                        .Success = True,
                        .Timestamp = _lastSyncTime,
                        .Message = "Sincronização concluída com sucesso."
                    }

                    RaiseEvent SyncCompleted(syncResult)
                    Return "Sincronização concluída com sucesso."
                Else
                    Return "Erro durante a sincronização."
                End If

            Catch ex As Exception
                Return $"Erro na sincronização: {ex.Message}"
            End Try
        End Function

#End Region

#Region "Command Parsing Methods"

        Private Function ParseTaskFromCommand(command As VoiceCommand) As AgendaTask
            Dim task = New AgendaTask()

            ' Extract task title from parameters
            If command.Parameters.ContainsKey("title") Then
                task.Title = command.Parameters("title").ToString()
            ElseIf command.Parameters.ContainsKey("query") Then
                task.Title = command.Parameters("query").ToString()
            Else
                task.Title = "Nova Tarefa"
            End If

            ' Extract priority
            If command.Parameters.ContainsKey("priority") Then
                task.Priority = command.Parameters("priority").ToString().ToLowerInvariant()
            Else
                task.Priority = "medium"
            End If

            ' Extract due date
            If command.Parameters.ContainsKey("dueDate") Then
                If DateTime.TryParse(command.Parameters("dueDate").ToString(), task.DueDate) Then
                    ' Date parsed successfully
                End If
            End If

            task.Status = "pending"
            task.CreatedAt = DateTime.UtcNow

            Return task
        End Function

        Private Function ParseProjectFromCommand(command As VoiceCommand) As AgendaProject
            Dim project = New AgendaProject()

            If command.Parameters.ContainsKey("name") Then
                project.Name = command.Parameters("name").ToString()
            ElseIf command.Parameters.ContainsKey("query") Then
                project.Name = command.Parameters("query").ToString()
            Else
                project.Name = "Novo Projeto"
            End If

            If command.Parameters.ContainsKey("description") Then
                project.Description = command.Parameters("description").ToString()
            End If

            project.Status = "active"
            project.CreatedAt = DateTime.UtcNow

            Return project
        End Function

        Private Function ParseTaskFilter(command As VoiceCommand) As TaskFilter
            Dim filter = New TaskFilter()

            If command.Parameters.ContainsKey("priority") Then
                filter.Priority = command.Parameters("priority").ToString()
            End If

            If command.Parameters.ContainsKey("status") Then
                filter.Status = command.Parameters("status").ToString()
            End If

            ' Parse date filters from natural language
            Dim query = command.Parameters.GetValueOrDefault("query", "").ToString().ToLowerInvariant()

            If query.Contains("hoje") Then
                filter.StartDate = DateTime.Today
                filter.EndDate = DateTime.Today.AddDays(1)
            ElseIf query.Contains("amanhã") Then
                filter.StartDate = DateTime.Today.AddDays(1)
                filter.EndDate = DateTime.Today.AddDays(2)
            ElseIf query.Contains("semana") Then
                filter.StartDate = DateTime.Today
                filter.EndDate = DateTime.Today.AddDays(7)
            End If

            Return filter
        End Function

        Private Function ParseMeetingFromCommand(command As VoiceCommand) As MeetingInfo
            Dim meeting = New MeetingInfo()

            If command.Parameters.ContainsKey("title") Then
                meeting.Title = command.Parameters("title").ToString()
            ElseIf command.Parameters.ContainsKey("query") Then
                meeting.Title = command.Parameters("query").ToString()
            Else
                meeting.Title = "Nova Reunião"
            End If

            ' Parse date and time from natural language
            If command.Parameters.ContainsKey("datetime") Then
                DateTime.TryParse(command.Parameters("datetime").ToString(), meeting.DateTime)
            Else
                meeting.DateTime = DateTime.Now.AddHours(1) ' Default to 1 hour from now
            End If

            Return meeting
        End Function

        Private Function ParseDateFilter(command As VoiceCommand) As DateFilter
            Dim filter = New DateFilter()

            Dim query = command.Parameters.GetValueOrDefault("query", "").ToString().ToLowerInvariant()

            If query.Contains("hoje") Then
                filter.StartDate = DateTime.Today
                filter.EndDate = DateTime.Today.AddDays(1)
                filter.Description = "hoje"
            ElseIf query.Contains("amanhã") Then
                filter.StartDate = DateTime.Today.AddDays(1)
                filter.EndDate = DateTime.Today.AddDays(2)
                filter.Description = "amanhã"
            ElseIf query.Contains("semana") Then
                filter.StartDate = DateTime.Today
                filter.EndDate = DateTime.Today.AddDays(7)
                filter.Description = "esta semana"
            Else
                filter.StartDate = DateTime.Today
                filter.EndDate = DateTime.Today.AddDays(1)
                filter.Description = "hoje"
            End If

            Return filter
        End Function

        Private Function ParseTaskUpdate(command As VoiceCommand) As TaskUpdateInfo
            ' This would parse task identifier and updates from natural language
            ' For simplicity, returning a basic implementation
            Return New TaskUpdateInfo With {
                .TaskId = "task_id_placeholder",
                .Updates = New With {.status = "updated"}
            }
        End Function

        Private Function ParseTaskIdentifier(command As VoiceCommand) As String
            ' This would extract task identifier from natural language
            ' For simplicity, returning a placeholder
            Return "task_id_placeholder"
        End Function

        Private Function ExtractSearchTerm(command As VoiceCommand) As String
            Return command.Parameters.GetValueOrDefault("query", "").ToString()
        End Function

        Private Function BuildFilterQuery(filter As TaskFilter) As String
            Dim queryParams = New List(Of String)()

            If Not String.IsNullOrEmpty(filter.Priority) Then
                queryParams.Add($"priority={filter.Priority}")
            End If

            If Not String.IsNullOrEmpty(filter.Status) Then
                queryParams.Add($"status={filter.Status}")
            End If

            If filter.StartDate.HasValue Then
                queryParams.Add($"startDate={filter.StartDate.Value:yyyy-MM-dd}")
            End If

            If filter.EndDate.HasValue Then
                queryParams.Add($"endDate={filter.EndDate.Value:yyyy-MM-dd}")
            End If

            If queryParams.Count > 0 Then
                Return "?" + String.Join("&", queryParams)
            End If

            Return String.Empty
        End Function

#End Region

#Region "Private Methods"

        Private Sub InitializeHttpClient()
            _httpClient = New HttpClient()
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ARIA-Voice-Assistant/1.0")
            _httpClient.Timeout = TimeSpan.FromSeconds(30)
        End Sub

#End Region

#Region "IDisposable Support"

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not _disposedValue Then
                If disposing Then
                    _httpClient?.Dispose()
                End If
                _disposedValue = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#End Region

    End Class

#Region "Supporting Classes and Structures"

    ''' &lt;summary&gt;
    ''' Voice command structure
    ''' &lt;/summary&gt;
    Public Class VoiceCommand
        Public Property Action As String
        Public Property Parameters As Dictionary(Of String, Object)

        Public Sub New()
            Parameters = New Dictionary(Of String, Object)()
        End Sub
    End Class

    ''' &lt;summary&gt;
    ''' Agenda task model
    ''' &lt;/summary&gt;
    Public Class AgendaTask
        Public Property Id As String = Guid.NewGuid().ToString()
        Public Property Title As String
        Public Property Description As String
        Public Property Priority As String
        Public Property Status As String
        Public Property Type As String = "task"
        Public Property DueDate As DateTime?
        Public Property CreatedAt As DateTime
        Public Property CompletedAt As DateTime?
        Public Property ProjectId As String
    End Class

    ''' &lt;summary&gt;
    ''' Agenda project model
    ''' &lt;/summary&gt;
    Public Class AgendaProject
        Public Property Id As String = Guid.NewGuid().ToString()
        Public Property Name As String
        Public Property Description As String
        Public Property Status As String
        Public Property CreatedAt As DateTime
        Public Property CompletedAt As DateTime?
    End Class

    ''' &lt;summary&gt;
    ''' API response wrapper
    ''' &lt;/summary&gt;
    Public Class APIResponse
        Public Property Success As Boolean
        Public Property Data As String
        Public Property ErrorMessage As String

        Public Sub New(success As Boolean, data As String)
            Me.Success = success
            If success Then
                Me.Data = data
            Else
                Me.ErrorMessage = data
            End If
        End Sub
    End Class

    ''' &lt;summary&gt;
    ''' Task filter parameters
    ''' &lt;/summary&gt;
    Public Class TaskFilter
        Public Property Priority As String
        Public Property Status As String
        Public Property Type As String
        Public Property StartDate As DateTime?
        Public Property EndDate As DateTime?
        Public Property ProjectId As String
    End Class

    ''' &lt;summary&gt;
    ''' Meeting information
    ''' &lt;/summary&gt;
    Public Class MeetingInfo
        Public Property Title As String
        Public Property Description As String
        Public Property DateTime As DateTime
        Public Property Duration As TimeSpan
        Public Property Participants As List(Of String)

        Public Sub New()
            Participants = New List(Of String)()
        End Sub
    End Class

    ''' &lt;summary&gt;
    ''' Date filter information
    ''' &lt;/summary&gt;
    Public Class DateFilter
        Public Property StartDate As DateTime?
        Public Property EndDate As DateTime?
        Public Property Description As String
    End Class

    ''' &lt;summary&gt;
    ''' Task update information
    ''' &lt;/summary&gt;
    Public Class TaskUpdateInfo
        Public Property TaskId As String
        Public Property Updates As Object
    End Class

    ''' &lt;summary&gt;
    ''' Sync result information
    ''' &lt;/summary&gt;
    Public Class SyncResult
        Public Property Success As Boolean
        Public Property Timestamp As DateTime
        Public Property Message As String
        Public Property ItemsUpdated As Integer
    End Class

#End Region

End Namespace