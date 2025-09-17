Imports System.IO
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace ARIA.Security

    ''' &lt;summary&gt;
    ''' Handles audit logging for security events
    ''' &lt;/summary&gt;
    Friend Class AuditLogger

#Region "Properties and Fields"

        Private ReadOnly _logDirectory As String
        Private ReadOnly _lockObject As New Object()
        Private _isInitialized As Boolean = False

        Public ReadOnly Property IsInitialized As Boolean
            Get
                Return _isInitialized
            End Get
        End Property

#End Region

#Region "Constructor"

        Public Sub New()
            _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ARIA", "logs", "security")
        End Sub

#End Region

#Region "Initialization"

        ''' &lt;summary&gt;
        ''' Initializes the audit logger
        ''' &lt;/summary&gt;
        Public Async Function InitializeAsync() As Task
            Try
                ' Create log directory if it doesn't exist
                If Not Directory.Exists(_logDirectory) Then
                    Directory.CreateDirectory(_logDirectory)
                End If

                ' Create initial log file if needed
                Dim logFile = GetCurrentLogFile()
                If Not File.Exists(logFile) Then
                    Await File.WriteAllTextAsync(logFile, "")
                End If

                _isInitialized = True

            Catch ex As Exception
                Throw New InvalidOperationException($"Failed to initialize audit logger: {ex.Message}", ex)
            End Try
        End Function

#End Region

#Region "Logging Methods"

        ''' &lt;summary&gt;
        ''' Logs a security audit event
        ''' &lt;/summary&gt;
        ''' &lt;param name="auditEvent"&gt;Event to log&lt;/param&gt;
        Public Async Function LogEventAsync(auditEvent As SecurityAuditEvent) As Task
            Try
                If Not _isInitialized Then
                    Throw New InvalidOperationException("Audit logger not initialized")
                End If

                Dim logEntry = CreateLogEntry(auditEvent)
                Dim logFile = GetCurrentLogFile()

                SyncLock _lockObject
                    File.AppendAllText(logFile, logEntry + Environment.NewLine)
                End SyncLock

                ' Also log to daily file for easier searching
                Await LogToDailyFileAsync(auditEvent)

            Catch ex As Exception
                ' Silent fail to avoid recursive logging
                Console.WriteLine($"Audit logging failed: {ex.Message}")
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Retrieves audit events within a date range
        ''' &lt;/summary&gt;
        ''' &lt;param name="fromDate"&gt;Start date&lt;/param&gt;
        ''' &lt;param name="toDate"&gt;End date&lt;/param&gt;
        ''' &lt;param name="eventType"&gt;Optional event type filter&lt;/param&gt;
        ''' &lt;returns&gt;List of audit events&lt;/returns&gt;
        Public Async Function GetEventsAsync(fromDate As DateTime, toDate As DateTime, Optional eventType As SecurityEventType? = Nothing) As Task(Of List(Of SecurityAuditEvent))
            Dim events = New List(Of SecurityAuditEvent)()

            Try
                ' Get all log files in the date range
                Dim logFiles = GetLogFilesInRange(fromDate, toDate)

                For Each logFile In logFiles
                    If File.Exists(logFile) Then
                        Dim fileEvents = Await ParseLogFileAsync(logFile, fromDate, toDate, eventType)
                        events.AddRange(fileEvents)
                    End If
                Next

                ' Sort by timestamp
                events.Sort(Function(x, y) x.Timestamp.CompareTo(y.Timestamp))

            Catch ex As Exception
                Throw New InvalidOperationException($"Failed to retrieve audit events: {ex.Message}", ex)
            End Try

            Return events
        End Function

#End Region

#Region "Private Methods"

        Private Function CreateLogEntry(auditEvent As SecurityAuditEvent) As String
            Dim logObject = New JObject From {
                {"id", auditEvent.Id.ToString()},
                {"timestamp", auditEvent.Timestamp.ToString("O")},
                {"eventType", auditEvent.EventType.ToString()},
                {"severity", auditEvent.Severity.ToString()},
                {"message", auditEvent.Message},
                {"userId", auditEvent.UserId},
                {"sessionId", auditEvent.SessionId},
                {"ipAddress", auditEvent.IpAddress}
            }

            Return logObject.ToString(Formatting.None)
        End Function

        Private Function GetCurrentLogFile() As String
            Dim fileName = $"security_{DateTime.UtcNow:yyyy-MM}.log"
            Return Path.Combine(_logDirectory, fileName)
        End Function

        Private Function GetDailyLogFile(date As DateTime) As String
            Dim fileName = $"security_daily_{date:yyyy-MM-dd}.log"
            Return Path.Combine(_logDirectory, "daily", fileName)
        End Function

        Private Async Function LogToDailyFileAsync(auditEvent As SecurityAuditEvent) As Task
            Try
                Dim dailyLogDir = Path.Combine(_logDirectory, "daily")
                If Not Directory.Exists(dailyLogDir) Then
                    Directory.CreateDirectory(dailyLogDir)
                End If

                Dim dailyLogFile = GetDailyLogFile(auditEvent.Timestamp)
                Dim logEntry = CreateLogEntry(auditEvent)

                Await File.AppendAllTextAsync(dailyLogFile, logEntry + Environment.NewLine)

            Catch ex As Exception
                ' Silent fail
            End Try
        End Function

        Private Function GetLogFilesInRange(fromDate As DateTime, toDate As DateTime) As List(Of String)
            Dim logFiles = New List(Of String)()

            Try
                ' Get monthly log files
                Dim current = New DateTime(fromDate.Year, fromDate.Month, 1)
                While current <= toDate
                    Dim monthlyFile = Path.Combine(_logDirectory, $"security_{current:yyyy-MM}.log")
                    If File.Exists(monthlyFile) Then
                        logFiles.Add(monthlyFile)
                    End If
                    current = current.AddMonths(1)
                End While

                ' Get daily log files
                Dim dailyDir = Path.Combine(_logDirectory, "daily")
                If Directory.Exists(dailyDir) Then
                    current = fromDate.Date
                    While current <= toDate.Date
                        Dim dailyFile = Path.Combine(dailyDir, $"security_daily_{current:yyyy-MM-dd}.log")
                        If File.Exists(dailyFile) Then
                            logFiles.Add(dailyFile)
                        End If
                        current = current.AddDays(1)
                    End While
                End If

            Catch ex As Exception
                ' Return what we have
            End Try

            Return logFiles
        End Function

        Private Async Function ParseLogFileAsync(logFile As String, fromDate As DateTime, toDate As DateTime, eventType As SecurityEventType?) As Task(Of List(Of SecurityAuditEvent))
            Dim events = New List(Of SecurityAuditEvent)()

            Try
                Dim lines = Await File.ReadAllLinesAsync(logFile)

                For Each line In lines
                    If String.IsNullOrWhiteSpace(line) Then Continue For

                    Try
                        Dim logObject = JObject.Parse(line)
                        Dim auditEvent = ParseLogEntry(logObject)

                        ' Apply filters
                        If auditEvent.Timestamp >= fromDate AndAlso auditEvent.Timestamp <= toDate Then
                            If Not eventType.HasValue OrElse auditEvent.EventType = eventType.Value Then
                                events.Add(auditEvent)
                            End If
                        End If

                    Catch ex As Exception
                        ' Skip malformed log entries
                        Continue For
                    End Try
                Next

            Catch ex As Exception
                ' Skip problematic files
            End Try

            Return events
        End Function

        Private Function ParseLogEntry(logObject As JObject) As SecurityAuditEvent
            Return New SecurityAuditEvent With {
                .Id = Guid.Parse(logObject("id").ToString()),
                .Timestamp = DateTime.Parse(logObject("timestamp").ToString()),
                .EventType = [Enum].Parse(GetType(SecurityEventType), logObject("eventType").ToString()),
                .Severity = [Enum].Parse(GetType(SecurityEventSeverity), logObject("severity").ToString()),
                .Message = logObject("message")?.ToString(),
                .UserId = logObject("userId")?.ToString(),
                .SessionId = logObject("sessionId")?.ToString(),
                .IpAddress = logObject("ipAddress")?.ToString()
            }
        End Function

#End Region

    End Class

End Namespace