Imports System.Text
Imports System.Security.Cryptography
Imports System.IO
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports ARIA.Security

Namespace ARIA.Security

    ''' &lt;summary&gt;
    ''' Main security management and audit system for ARIA Premium
    ''' Handles authentication, authorization, encryption, and audit logging
    ''' &lt;/summary&gt;
    Public Class SecurityManager

#Region "Properties and Fields"

        Private Shared _instance As SecurityManager
        Private Shared ReadOnly _lockObject As New Object()

        Private ReadOnly _auditLogger As AuditLogger
        Private ReadOnly _encryptionManager As EncryptionManager
        Private ReadOnly _sessionManager As SessionManager
        Private ReadOnly _dslValidator As DSLValidator

        Private _isInitialized As Boolean = False
        Private _securitySettings As SecuritySettings

        Public Shared ReadOnly Property Instance As SecurityManager
            Get
                If _instance Is Nothing Then
                    SyncLock _lockObject
                        If _instance Is Nothing Then
                            _instance = New SecurityManager()
                        End If
                    End SyncLock
                End If
                Return _instance
            End Get
        End Property

        Public ReadOnly Property IsInitialized As Boolean
            Get
                Return _isInitialized
            End Get
        End Property

        Public Property SecuritySettings As SecuritySettings
            Get
                Return _securitySettings
            End Get
            Set(value As SecuritySettings)
                _securitySettings = value
            End Set
        End Property

#End Region

#Region "Constructor"

        Private Sub New()
            _auditLogger = New AuditLogger()
            _encryptionManager = New EncryptionManager()
            _sessionManager = New SessionManager()
            _securitySettings = SecuritySettings.LoadDefault()
        End Sub

#End Region

#Region "Initialization"

        ''' &lt;summary&gt;
        ''' Initializes the security system
        ''' &lt;/summary&gt;
        ''' &lt;returns&gt;True if initialization successful&lt;/returns&gt;
        Public Async Function InitializeAsync() As Task(Of Boolean)
            Try
                ' Initialize encryption keys
                Await _encryptionManager.InitializeAsync()

                ' Load security settings
                _securitySettings = Await SecuritySettings.LoadAsync()

                ' Initialize audit logging
                Await _auditLogger.InitializeAsync()

                ' Create initial session
                _sessionManager.CreateSession()

                _isInitialized = True

                Await LogSecurityEventAsync(SecurityEventType.SystemStartup, "Security system initialized successfully")

                Return True

            Catch ex As Exception
                Await LogSecurityEventAsync(SecurityEventType.SystemError, $"Security initialization failed: {ex.Message}")
                Return False
            End Try
        End Function

#End Region

#Region "Command Security"

        ''' &lt;summary&gt;
        ''' Validates and authorizes a voice command
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Raw voice command&lt;/param&gt;
        ''' &lt;param name="userId"&gt;User identifier&lt;/param&gt;
        ''' &lt;returns&gt;Security validation result&lt;/returns&gt;
        Public Async Function ValidateCommandAsync(command As String, userId As String) As Task(Of SecurityValidationResult)
            Try
                ' Log command attempt
                Await LogSecurityEventAsync(SecurityEventType.CommandAttempt, $"Command attempted by user {userId}: {command}")

                ' Validate user session
                If Not _sessionManager.IsValidSession(userId) Then
                    Await LogSecurityEventAsync(SecurityEventType.UnauthorizedAccess, $"Invalid session for user {userId}")
                    Return New SecurityValidationResult(False, "Invalid session", SecurityRiskLevel.High)
                End If

                ' Validate command structure using DSL validator
                Dim validationResult = DSLValidator.ValidateCommand(command)

                If Not validationResult.IsValid Then
                    Await LogSecurityEventAsync(SecurityEventType.InvalidCommand, $"Invalid command structure: {validationResult.ErrorMessage}")
                    Return New SecurityValidationResult(False, validationResult.ErrorMessage, validationResult.RiskLevel)
                End If

                ' Check if command requires additional authorization
                If validationResult.RequiresConfirmation Then
                    Await LogSecurityEventAsync(SecurityEventType.ConfirmationRequired, $"Command requires confirmation: {command}")
                    Return New SecurityValidationResult(True, "Confirmation required", validationResult.RiskLevel, True, validationResult.ParsedCommand)
                End If

                ' Check rate limiting
                If IsRateLimited(userId) Then
                    Await LogSecurityEventAsync(SecurityEventType.RateLimitExceeded, $"Rate limit exceeded for user {userId}")
                    Return New SecurityValidationResult(False, "Rate limit exceeded", SecurityRiskLevel.Medium)
                End If

                ' All checks passed
                Await LogSecurityEventAsync(SecurityEventType.CommandAuthorized, $"Command authorized for user {userId}")
                Return New SecurityValidationResult(True, "Command authorized", validationResult.RiskLevel, False, validationResult.ParsedCommand)

            Catch ex As Exception
                Await LogSecurityEventAsync(SecurityEventType.SystemError, $"Command validation error: {ex.Message}")
                Return New SecurityValidationResult(False, $"Validation error: {ex.Message}", SecurityRiskLevel.Critical)
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Processes a verbal confirmation for dangerous commands
        ''' &lt;/summary&gt;
        ''' &lt;param name="confirmation"&gt;Verbal confirmation text&lt;/param&gt;
        ''' &lt;param name="originalCommand"&gt;Original command requiring confirmation&lt;/param&gt;
        ''' &lt;param name="userId"&gt;User identifier&lt;/param&gt;
        ''' &lt;returns&gt;True if confirmation is valid&lt;/returns&gt;
        Public Async Function ProcessVerbalConfirmationAsync(confirmation As String, originalCommand As String, userId As String) As Task(Of Boolean)
            Try
                ' Clean and normalize confirmation
                Dim cleanConfirmation = confirmation.Trim().ToLowerInvariant()

                ' Check for explicit positive confirmations
                Dim positiveConfirmations() As String = {"sim", "yes", "confirmo", "autorizo", "execute", "proceder", "ok"}
                Dim negativeConfirmations() As String = {"não", "no", "nao", "cancel", "cancelar", "abort", "parar", "stop"}

                Dim isPositive = positiveConfirmations.Any(Function(c) cleanConfirmation.Contains(c))
                Dim isNegative = negativeConfirmations.Any(Function(c) cleanConfirmation.Contains(c))

                If isNegative Then
                    Await LogSecurityEventAsync(SecurityEventType.ConfirmationDenied, $"User {userId} denied command: {originalCommand}")
                    Return False
                End If

                If isPositive Then
                    Await LogSecurityEventAsync(SecurityEventType.ConfirmationGranted, $"User {userId} confirmed command: {originalCommand}")
                    Return True
                End If

                ' Ambiguous response
                Await LogSecurityEventAsync(SecurityEventType.ConfirmationAmbiguous, $"Ambiguous confirmation from user {userId} for command: {originalCommand}")
                Return False

            Catch ex As Exception
                Await LogSecurityEventAsync(SecurityEventType.SystemError, $"Confirmation processing error: {ex.Message}")
                Return False
            End Try
        End Function

#End Region

#Region "Session Management"

        ''' &lt;summary&gt;
        ''' Creates a new user session
        ''' &lt;/summary&gt;
        ''' &lt;param name="userId"&gt;User identifier&lt;/param&gt;
        ''' &lt;returns&gt;Session token&lt;/returns&gt;
        Public Async Function CreateSessionAsync(userId As String) As Task(Of String)
            Try
                Dim sessionToken = _sessionManager.CreateSession(userId)
                Await LogSecurityEventAsync(SecurityEventType.SessionCreated, $"Session created for user {userId}")
                Return sessionToken
            Catch ex As Exception
                Await LogSecurityEventAsync(SecurityEventType.SystemError, $"Session creation failed: {ex.Message}")
                Throw
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Validates a user session
        ''' &lt;/summary&gt;
        ''' &lt;param name="sessionToken"&gt;Session token&lt;/param&gt;
        ''' &lt;returns&gt;True if session is valid&lt;/returns&gt;
        Public Function ValidateSession(sessionToken As String) As Boolean
            Return _sessionManager.IsValidSession(sessionToken)
        End Function

        ''' &lt;summary&gt;
        ''' Terminates a user session
        ''' &lt;/summary&gt;
        ''' &lt;param name="sessionToken"&gt;Session token&lt;/param&gt;
        Public Async Function TerminateSessionAsync(sessionToken As String) As Task
            Try
                _sessionManager.TerminateSession(sessionToken)
                Await LogSecurityEventAsync(SecurityEventType.SessionTerminated, $"Session terminated: {sessionToken}")
            Catch ex As Exception
                Await LogSecurityEventAsync(SecurityEventType.SystemError, $"Session termination failed: {ex.Message}")
                Throw
            End Try
        End Function

#End Region

#Region "Encryption Services"

        ''' &lt;summary&gt;
        ''' Encrypts sensitive data
        ''' &lt;/summary&gt;
        ''' &lt;param name="plaintext"&gt;Data to encrypt&lt;/param&gt;
        ''' &lt;returns&gt;Encrypted data&lt;/returns&gt;
        Public Function EncryptData(plaintext As String) As String
            Return _encryptionManager.Encrypt(plaintext)
        End Function

        ''' &lt;summary&gt;
        ''' Decrypts sensitive data
        ''' &lt;/summary&gt;
        ''' &lt;param name="ciphertext"&gt;Data to decrypt&lt;/param&gt;
        ''' &lt;returns&gt;Decrypted data&lt;/returns&gt;
        Public Function DecryptData(ciphertext As String) As String
            Return _encryptionManager.Decrypt(ciphertext)
        End Function

        ''' &lt;summary&gt;
        ''' Encrypts API keys for secure storage
        ''' &lt;/summary&gt;
        ''' &lt;param name="apiKey"&gt;API key to encrypt&lt;/param&gt;
        ''' &lt;returns&gt;Encrypted API key&lt;/returns&gt;
        Public Function EncryptAPIKey(apiKey As String) As String
            Return _encryptionManager.EncryptAPIKey(apiKey)
        End Function

        ''' &lt;summary&gt;
        ''' Decrypts API keys for use
        ''' &lt;/summary&gt;
        ''' &lt;param name="encryptedAPIKey"&gt;Encrypted API key&lt;/param&gt;
        ''' &lt;returns&gt;Decrypted API key&lt;/returns&gt;
        Public Function DecryptAPIKey(encryptedAPIKey As String) As String
            Return _encryptionManager.DecryptAPIKey(encryptedAPIKey)
        End Function

#End Region

#Region "Audit Logging"

        ''' &lt;summary&gt;
        ''' Logs a security event
        ''' &lt;/summary&gt;
        ''' &lt;param name="eventType"&gt;Type of security event&lt;/param&gt;
        ''' &lt;param name="message"&gt;Event description&lt;/param&gt;
        ''' &lt;param name="userId"&gt;Optional user identifier&lt;/param&gt;
        Public Async Function LogSecurityEventAsync(eventType As SecurityEventType, message As String, Optional userId As String = Nothing) As Task
            Try
                Dim auditEvent = New SecurityAuditEvent With {
                    .EventType = eventType,
                    .Message = message,
                    .UserId = userId,
                    .Timestamp = DateTime.UtcNow,
                    .SessionId = _sessionManager.CurrentSessionId,
                    .IpAddress = GetClientIPAddress(),
                    .Severity = GetEventSeverity(eventType)
                }

                Await _auditLogger.LogEventAsync(auditEvent)

            Catch ex As Exception
                ' Fail silently to avoid recursive logging errors
                Console.WriteLine($"Audit logging failed: {ex.Message}")
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Retrieves security audit events
        ''' &lt;/summary&gt;
        ''' &lt;param name="fromDate"&gt;Start date filter&lt;/param&gt;
        ''' &lt;param name="toDate"&gt;End date filter&lt;/param&gt;
        ''' &lt;param name="eventType"&gt;Optional event type filter&lt;/param&gt;
        ''' &lt;returns&gt;List of audit events&lt;/returns&gt;
        Public Async Function GetAuditEventsAsync(fromDate As DateTime, toDate As DateTime, Optional eventType As SecurityEventType? = Nothing) As Task(Of List(Of SecurityAuditEvent))
            Return Await _auditLogger.GetEventsAsync(fromDate, toDate, eventType)
        End Function

#End Region

#Region "Rate Limiting"

        Private ReadOnly _rateLimitTracker As New Dictionary(Of String, List(Of DateTime))()
        Private ReadOnly _rateLimitLock As New Object()

        ''' &lt;summary&gt;
        ''' Checks if a user has exceeded rate limits
        ''' &lt;/summary&gt;
        ''' &lt;param name="userId"&gt;User identifier&lt;/param&gt;
        ''' &lt;returns&gt;True if rate limited&lt;/returns&gt;
        Private Function IsRateLimited(userId As String) As Boolean
            SyncLock _rateLimitLock
                Dim now = DateTime.UtcNow
                Dim windowStart = now.AddMinutes(-1) ' 1-minute window

                If Not _rateLimitTracker.ContainsKey(userId) Then
                    _rateLimitTracker(userId) = New List(Of DateTime)()
                End If

                Dim userRequests = _rateLimitTracker(userId)

                ' Remove old requests outside the window
                userRequests.RemoveAll(Function(request) request < windowStart)

                ' Check if user has exceeded the limit (e.g., 10 requests per minute)
                If userRequests.Count >= _securitySettings.MaxRequestsPerMinute Then
                    Return True
                End If

                ' Add current request
                userRequests.Add(now)
                Return False
            End SyncLock
        End Function

#End Region

#Region "Private Helpers"

        Private Function GetEventSeverity(eventType As SecurityEventType) As SecurityEventSeverity
            Select Case eventType
                Case SecurityEventType.SystemStartup, SecurityEventType.SessionCreated, SecurityEventType.CommandAuthorized
                    Return SecurityEventSeverity.Information
                Case SecurityEventType.ConfirmationRequired, SecurityEventType.RateLimitExceeded
                    Return SecurityEventSeverity.Warning
                Case SecurityEventType.UnauthorizedAccess, SecurityEventType.InvalidCommand, SecurityEventType.ConfirmationDenied
                    Return SecurityEventSeverity.Error
                Case SecurityEventType.SystemError, SecurityEventType.SecurityViolation
                    Return SecurityEventSeverity.Critical
                Case Else
                    Return SecurityEventSeverity.Information
            End Select
        End Function

        Private Function GetClientIPAddress() As String
            ' In a desktop application, this would be localhost
            ' In a web application, this would get the actual client IP
            Return "127.0.0.1"
        End Function

#End Region

    End Class

#Region "Supporting Classes"

    ''' &lt;summary&gt;
    ''' Security validation result
    ''' &lt;/summary&gt;
    Public Class SecurityValidationResult
        Public Property IsValid As Boolean
        Public Property ErrorMessage As String
        Public Property RiskLevel As SecurityRiskLevel
        Public Property RequiresConfirmation As Boolean
        Public Property ParsedCommand As JObject

        Public Sub New(isValid As Boolean, errorMessage As String, riskLevel As SecurityRiskLevel, Optional requiresConfirmation As Boolean = False, Optional parsedCommand As JObject = Nothing)
            Me.IsValid = isValid
            Me.ErrorMessage = errorMessage
            Me.RiskLevel = riskLevel
            Me.RequiresConfirmation = requiresConfirmation
            Me.ParsedCommand = parsedCommand
        End Sub
    End Class

    ''' &lt;summary&gt;
    ''' Security settings configuration
    ''' &lt;/summary&gt;
    Public Class SecuritySettings
        Public Property MaxRequestsPerMinute As Integer = 10
        Public Property SessionTimeoutMinutes As Integer = 30
        Public Property RequireConfirmationForDangerousActions As Boolean = True
        Public Property EnableAuditLogging As Boolean = True
        Public Property EncryptStoredData As Boolean = True
        Public Property MaxFailedAttempts As Integer = 5

        Public Shared Function LoadDefault() As SecuritySettings
            Return New SecuritySettings()
        End Function

        Public Shared Async Function LoadAsync() As Task(Of SecuritySettings)
            Try
                Dim configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ARIA", "security_config.json")
                If File.Exists(configPath) Then
                    Dim json = Await File.ReadAllTextAsync(configPath)
                    Return JsonConvert.DeserializeObject(Of SecuritySettings)(json)
                End If
            Catch ex As Exception
                ' If loading fails, return default settings
            End Try

            Return LoadDefault()
        End Function

        Public Async Function SaveAsync() As Task
            Try
                Dim configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ARIA", "security_config.json")
                Directory.CreateDirectory(Path.GetDirectoryName(configPath))
                Dim json = JsonConvert.SerializeObject(Me, Formatting.Indented)
                Await File.WriteAllTextAsync(configPath, json)
            Catch ex As Exception
                Throw New InvalidOperationException($"Failed to save security settings: {ex.Message}", ex)
            End Try
        End Function
    End Class

    ''' &lt;summary&gt;
    ''' Security audit event
    ''' &lt;/summary&gt;
    Public Class SecurityAuditEvent
        Public Property Id As Guid = Guid.NewGuid()
        Public Property Timestamp As DateTime
        Public Property EventType As SecurityEventType
        Public Property Message As String
        Public Property UserId As String
        Public Property SessionId As String
        Public Property IpAddress As String
        Public Property Severity As SecurityEventSeverity
    End Class

#End Region

#Region "Enums"

    ''' &lt;summary&gt;
    ''' Types of security events
    ''' &lt;/summary&gt;
    Public Enum SecurityEventType
        SystemStartup
        SystemError
        SessionCreated
        SessionTerminated
        CommandAttempt
        CommandAuthorized
        InvalidCommand
        UnauthorizedAccess
        ConfirmationRequired
        ConfirmationGranted
        ConfirmationDenied
        ConfirmationAmbiguous
        RateLimitExceeded
        SecurityViolation
    End Enum

    ''' &lt;summary&gt;
    ''' Severity levels for security events
    ''' &lt;/summary&gt;
    Public Enum SecurityEventSeverity
        Information = 0
        Warning = 1
        [Error] = 2
        Critical = 3
    End Enum

#End Region

End Namespace