Imports System.Text
Imports System.Text.RegularExpressions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Schema
Imports Newtonsoft.Json.Linq
Imports System.Threading.Tasks
Imports System.Security.Cryptography

Namespace ARIA.Security

    ''' &lt;summary&gt;
    ''' Domain Specific Language Validator for secure command processing
    ''' Validates voice commands against a JSON schema to ensure security
    ''' &lt;/summary&gt;
    Public Class DSLValidator

#Region "Properties and Fields"

        Private Shared ReadOnly _commandSchema As JSchema
        Private Shared ReadOnly _dangerousActions As String() = {
            "delete", "remove", "send_email", "make_call", "file_operation",
            "system_command", "install", "uninstall", "shutdown", "restart",
            "execute", "run", "kill", "terminate", "modify_file", "create_file"
        }

        Private Shared ReadOnly _trustedActions As String() = {
            "search", "note", "reminder", "meeting", "weather", "time",
            "calculate", "translate", "read", "summarize", "analyze",
            "schedule", "query", "information", "help", "status"
        }

        Public Shared ReadOnly Property CommandSchema As JSchema
            Get
                Return _commandSchema
            End Get
        End Property

#End Region

#Region "Constructor"

        Shared Sub New()
            _commandSchema = CreateCommandSchema()
        End Sub

#End Region

#Region "Public Methods"

        ''' &lt;summary&gt;
        ''' Validates a voice command against the security schema
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Raw voice command text&lt;/param&gt;
        ''' &lt;returns&gt;Validation result with parsed command&lt;/returns&gt;
        Public Shared Function ValidateCommand(command As String) As CommandValidationResult
            Try
                If String.IsNullOrWhiteSpace(command) Then
                    Return New CommandValidationResult(False, "Empty command", Nothing)
                End If

                ' Clean and normalize command
                Dim cleanCommand = CleanCommand(command)

                ' Parse command to structured format
                Dim parsedCommand = ParseCommandToJson(cleanCommand)

                ' Validate against schema
                Dim validationResult = ValidateAgainstSchema(parsedCommand)
                If Not validationResult.IsValid Then
                    Return New CommandValidationResult(False, validationResult.ErrorMessage, Nothing)
                End If

                ' Additional security checks
                Dim securityCheck = PerformSecurityChecks(parsedCommand)

                Return New CommandValidationResult(
                    securityCheck.IsValid,
                    securityCheck.ErrorMessage,
                    parsedCommand
                )

            Catch ex As Exception
                Return New CommandValidationResult(False, $"Validation error: {ex.Message}", Nothing)
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Validates a pre-parsed JSON command
        ''' &lt;/summary&gt;
        ''' &lt;param name="jsonCommand"&gt;JSON command object&lt;/param&gt;
        ''' &lt;returns&gt;Validation result&lt;/returns&gt;
        Public Shared Function ValidateJsonCommand(jsonCommand As String) As CommandValidationResult
            Try
                Dim commandObj = JObject.Parse(jsonCommand)
                Dim validationResult = ValidateAgainstSchema(commandObj)

                If Not validationResult.IsValid Then
                    Return New CommandValidationResult(False, validationResult.ErrorMessage, Nothing)
                End If

                Dim securityCheck = PerformSecurityChecks(commandObj)
                Return New CommandValidationResult(
                    securityCheck.IsValid,
                    securityCheck.ErrorMessage,
                    commandObj
                )

            Catch ex As JsonException
                Return New CommandValidationResult(False, $"Invalid JSON format: {ex.Message}", Nothing)
            Catch ex As Exception
                Return New CommandValidationResult(False, $"Validation error: {ex.Message}", Nothing)
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Checks if an action requires verbal confirmation
        ''' &lt;/summary&gt;
        ''' &lt;param name="action"&gt;Action to check&lt;/param&gt;
        ''' &lt;returns&gt;True if confirmation is required&lt;/returns&gt;
        Public Shared Function RequiresConfirmation(action As String) As Boolean
            If String.IsNullOrWhiteSpace(action) Then Return True

            Return _dangerousActions.Contains(action.ToLowerInvariant()) OrElse
                   action.Contains("delete") OrElse
                   action.Contains("remove") OrElse
                   action.Contains("send") OrElse
                   action.Contains("execute")
        End Function

        ''' &lt;summary&gt;
        ''' Estimates the security risk level of a command
        ''' &lt;/summary&gt;
        ''' &lt;param name="command"&gt;Command to assess&lt;/param&gt;
        ''' &lt;returns&gt;Risk level (Low, Medium, High, Critical)&lt;/returns&gt;
        Public Shared Function AssessRiskLevel(command As JObject) As SecurityRiskLevel
            Try
                Dim action = command("action")?.ToString()?.ToLowerInvariant()

                If String.IsNullOrWhiteSpace(action) Then
                    Return SecurityRiskLevel.High
                End If

                ' Critical risk actions
                If _dangerousActions.Contains(action) Then
                    Return SecurityRiskLevel.Critical
                End If

                ' Check for dangerous keywords in parameters
                Dim parameters = command("parameters")?.ToString()?.ToLowerInvariant()
                If Not String.IsNullOrEmpty(parameters) Then
                    If parameters.Contains("admin") OrElse
                       parameters.Contains("password") OrElse
                       parameters.Contains("key") OrElse
                       parameters.Contains("secret") Then
                        Return SecurityRiskLevel.High
                    End If
                End If

                ' Trusted actions are low risk
                If _trustedActions.Contains(action) Then
                    Return SecurityRiskLevel.Low
                End If

                ' Default to medium risk for unknown actions
                Return SecurityRiskLevel.Medium

            Catch ex As Exception
                ' If we can't assess, assume high risk
                Return SecurityRiskLevel.High
            End Try
        End Function

#End Region

#Region "Private Methods"

        Private Shared Function CreateCommandSchema() As JSchema
            Dim schemaJson = "{
                ""type"": ""object"",
                ""properties"": {
                    ""action"": {
                        ""type"": ""string"",
                        ""minLength"": 1,
                        ""maxLength"": 50,
                        ""pattern"": ""^[a-zA-Z_][a-zA-Z0-9_]*$""
                    },
                    ""parameters"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""query"": {""type"": ""string"", ""maxLength"": 1000},
                            ""target"": {""type"": ""string"", ""maxLength"": 500},
                            ""urgency"": {
                                ""type"": ""string"",
                                ""enum"": [""low"", ""medium"", ""high""]
                            },
                            ""context"": {""type"": ""string"", ""maxLength"": 2000},
                            ""options"": {""type"": ""object""}
                        },
                        ""additionalProperties"": false
                    },
                    ""dangerous"": {
                        ""type"": ""boolean"",
                        ""description"": ""Requires verbal confirmation""
                    },
                    ""estimated_cost"": {
                        ""type"": ""number"",
                        ""minimum"": 0,
                        ""maximum"": 10
                    },
                    ""timestamp"": {
                        ""type"": ""string"",
                        ""format"": ""date-time""
                    },
                    ""user_id"": {
                        ""type"": ""string"",
                        ""maxLength"": 100
                    }
                },
                ""required"": [""action""],
                ""additionalProperties"": false
            }"

            Return JSchema.Parse(schemaJson)
        End Function

        Private Shared Function CleanCommand(command As String) As String
            ' Remove extra whitespace and normalize
            Dim cleaned = Regex.Replace(command.Trim(), "\s+", " ")

            ' Remove potentially dangerous characters
            cleaned = Regex.Replace(cleaned, "[<>&""']", "")

            Return cleaned
        End Function

        Private Shared Function ParseCommandToJson(command As String) As JObject
            ' Simple command parsing - in a real implementation,
            ' this would use NLP or AI to parse natural language
            Dim result = New JObject()

            ' Extract action (first word typically)
            Dim words = command.Split(" "c)
            If words.Length > 0 Then
                result("action") = NormalizeAction(words(0))
            End If

            ' Extract parameters from remaining words
            Dim parameters = New JObject()
            If words.Length > 1 Then
                parameters("query") = String.Join(" ", words.Skip(1))
            End If

            result("parameters") = parameters
            result("dangerous") = RequiresConfirmation(result("action")?.ToString())
            result("estimated_cost") = 0.1 ' Default small cost
            result("timestamp") = DateTime.UtcNow.ToString("O")

            Return result
        End Function

        Private Shared Function NormalizeAction(action As String) As String
            Dim normalized = action.ToLowerInvariant()

            ' Map common synonyms to standard actions
            Select Case normalized
                Case "find", "locate", "look"
                    Return "search"
                Case "write", "create", "add"
                    Return "note"
                Case "remind", "alert"
                    Return "reminder"
                Case "record", "capture"
                    Return "meeting"
                Case "count", "compute"
                    Return "calculate"
                Case "convert"
                    Return "translate"
                Case Else
                    Return normalized
            End Select
        End Function

        Private Shared Function ValidateAgainstSchema(command As JObject) As (IsValid As Boolean, ErrorMessage As String)
            Try
                Dim isValid = command.IsValid(_commandSchema)
                If isValid Then
                    Return (True, String.Empty)
                Else
                    Dim errors = New List(Of String)()
                    command.Validate(_commandSchema, Sub(sender, args) errors.Add(args.Message))
                    Return (False, String.Join("; ", errors))
                End If
            Catch ex As Exception
                Return (False, $"Schema validation failed: {ex.Message}")
            End Try
        End Function

        Private Shared Function PerformSecurityChecks(command As JObject) As (IsValid As Boolean, ErrorMessage As String)
            Try
                Dim action = command("action")?.ToString()

                ' Check for explicitly dangerous actions
                If _dangerousActions.Contains(action?.ToLowerInvariant()) Then
                    command("dangerous") = True
                    Return (True, "Dangerous action - requires confirmation")
                End If

                ' Check parameters for suspicious content
                Dim parameters = command("parameters")?.ToString()
                If Not String.IsNullOrEmpty(parameters) Then
                    ' Look for SQL injection patterns
                    If Regex.IsMatch(parameters, "(DROP|DELETE|INSERT|UPDATE)\s+(TABLE|FROM|INTO)", RegexOptions.IgnoreCase) Then
                        Return (False, "Potential SQL injection detected")
                    End If

                    ' Look for script injection
                    If Regex.IsMatch(parameters, "&lt;script|javascript:|vbscript:", RegexOptions.IgnoreCase) Then
                        Return (False, "Potential script injection detected")
                    End If

                    ' Look for file system access patterns
                    If Regex.IsMatch(parameters, "\.\./|\.\.\\|C:\\|/etc/|/var/", RegexOptions.IgnoreCase) Then
                        Return (False, "Potential file system access detected")
                    End If
                End If

                Return (True, String.Empty)

            Catch ex As Exception
                Return (False, $"Security check failed: {ex.Message}")
            End Try
        End Function

#End Region

    End Class

#Region "Supporting Classes and Enums"

    ''' &lt;summary&gt;
    ''' Result of command validation
    ''' &lt;/summary&gt;
    Public Class CommandValidationResult
        Public Property IsValid As Boolean
        Public Property ErrorMessage As String
        Public Property ParsedCommand As JObject
        Public Property RiskLevel As SecurityRiskLevel
        Public Property RequiresConfirmation As Boolean

        Public Sub New(isValid As Boolean, errorMessage As String, parsedCommand As JObject)
            Me.IsValid = isValid
            Me.ErrorMessage = errorMessage
            Me.ParsedCommand = parsedCommand

            If parsedCommand IsNot Nothing Then
                Me.RiskLevel = DSLValidator.AssessRiskLevel(parsedCommand)
                Me.RequiresConfirmation = CBool(parsedCommand("dangerous"))
            Else
                Me.RiskLevel = SecurityRiskLevel.High
                Me.RequiresConfirmation = True
            End If
        End Sub
    End Class

    ''' &lt;summary&gt;
    ''' Security risk levels for commands
    ''' &lt;/summary&gt;
    Public Enum SecurityRiskLevel
        Low = 0
        Medium = 1
        High = 2
        Critical = 3
    End Enum

#End Region

End Namespace