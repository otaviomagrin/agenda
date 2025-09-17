Imports System.Threading.Tasks
Imports System.Collections.Concurrent

Namespace ARIA.Security

    ''' &lt;summary&gt;
    ''' Manages user sessions and authentication state
    ''' &lt;/summary&gt;
    Friend Class SessionManager

#Region "Properties and Fields"

        Private ReadOnly _activeSessions As New ConcurrentDictionary(Of String, UserSession)()
        Private ReadOnly _sessionsByUser As New ConcurrentDictionary(Of String, List(Of String))()
        Private _currentSessionId As String
        Private ReadOnly _sessionTimeout As TimeSpan = TimeSpan.FromMinutes(30)

        Public ReadOnly Property CurrentSessionId As String
            Get
                Return _currentSessionId
            End Get
        End Property

        Public ReadOnly Property ActiveSessionCount As Integer
            Get
                CleanupExpiredSessions()
                Return _activeSessions.Count
            End Get
        End Property

#End Region

#Region "Session Creation"

        ''' &lt;summary&gt;
        ''' Creates a new session for a user
        ''' &lt;/summary&gt;
        ''' &lt;param name="userId"&gt;User identifier&lt;/param&gt;
        ''' &lt;returns&gt;Session token&lt;/returns&gt;
        Public Function CreateSession(Optional userId As String = Nothing) As String
            If String.IsNullOrEmpty(userId) Then
                userId = "default_user"
            End If

            Dim sessionToken = GenerateSessionToken()
            Dim session = New UserSession With {
                .SessionId = sessionToken,
                .UserId = userId,
                .CreatedAt = DateTime.UtcNow,
                .LastActivity = DateTime.UtcNow,
                .IsActive = True,
                .IpAddress = "127.0.0.1",
                .UserAgent = "ARIA Desktop Application"
            }

            _activeSessions.TryAdd(sessionToken, session)

            ' Track sessions by user
            _sessionsByUser.AddOrUpdate(userId,
                New List(Of String) From {sessionToken},
                Function(key, existingList)
                    existingList.Add(sessionToken)
                    Return existingList
                End Function)

            _currentSessionId = sessionToken

            Return sessionToken
        End Function

        ''' &lt;summary&gt;
        ''' Creates a session with additional parameters
        ''' &lt;/summary&gt;
        ''' &lt;param name="userId"&gt;User identifier&lt;/param&gt;
        ''' &lt;param name="ipAddress"&gt;Client IP address&lt;/param&gt;
        ''' &lt;param name="userAgent"&gt;Client user agent&lt;/param&gt;
        ''' &lt;returns&gt;Session token&lt;/returns&gt;
        Public Function CreateSession(userId As String, ipAddress As String, userAgent As String) As String
            Dim sessionToken = GenerateSessionToken()
            Dim session = New UserSession With {
                .SessionId = sessionToken,
                .UserId = userId,
                .CreatedAt = DateTime.UtcNow,
                .LastActivity = DateTime.UtcNow,
                .IsActive = True,
                .IpAddress = ipAddress,
                .UserAgent = userAgent
            }

            _activeSessions.TryAdd(sessionToken, session)

            ' Track sessions by user
            _sessionsByUser.AddOrUpdate(userId,
                New List(Of String) From {sessionToken},
                Function(key, existingList)
                    existingList.Add(sessionToken)
                    Return existingList
                End Function)

            _currentSessionId = sessionToken

            Return sessionToken
        End Function

#End Region

#Region "Session Validation"

        ''' &lt;summary&gt;
        ''' Validates if a session is active and not expired
        ''' &lt;/summary&gt;
        ''' &lt;param name="sessionToken"&gt;Session token to validate&lt;/param&gt;
        ''' &lt;returns&gt;True if session is valid&lt;/returns&gt;
        Public Function IsValidSession(sessionToken As String) As Boolean
            If String.IsNullOrEmpty(sessionToken) Then
                Return False
            End If

            If _activeSessions.TryGetValue(sessionToken, ByRef Dim session) Then
                If session.IsActive AndAlso Not IsSessionExpired(session) Then
                    ' Update last activity
                    session.LastActivity = DateTime.UtcNow
                    Return True
                Else
                    ' Session expired, remove it
                    TerminateSession(sessionToken)
                    Return False
                End If
            End If

            Return False
        End Function

        ''' &lt;summary&gt;
        ''' Gets session information
        ''' &lt;/summary&gt;
        ''' &lt;param name="sessionToken"&gt;Session token&lt;/param&gt;
        ''' &lt;returns&gt;Session information or Nothing if not found&lt;/returns&gt;
        Public Function GetSession(sessionToken As String) As UserSession
            If _activeSessions.TryGetValue(sessionToken, ByRef Dim session) Then
                If session.IsActive AndAlso Not IsSessionExpired(session) Then
                    session.LastActivity = DateTime.UtcNow
                    Return session
                End If
            End If

            Return Nothing
        End Function

        ''' &lt;summary&gt;
        ''' Updates the last activity time for a session
        ''' &lt;/summary&gt;
        ''' &lt;param name="sessionToken"&gt;Session token&lt;/param&gt;
        Public Sub UpdateActivity(sessionToken As String)
            If _activeSessions.TryGetValue(sessionToken, ByRef Dim session) Then
                session.LastActivity = DateTime.UtcNow
            End If
        End Sub

#End Region

#Region "Session Termination"

        ''' &lt;summary&gt;
        ''' Terminates a specific session
        ''' &lt;/summary&gt;
        ''' &lt;param name="sessionToken"&gt;Session token to terminate&lt;/param&gt;
        Public Sub TerminateSession(sessionToken As String)
            If _activeSessions.TryRemove(sessionToken, ByRef Dim session) Then
                session.IsActive = False
                session.EndedAt = DateTime.UtcNow

                ' Remove from user session list
                If _sessionsByUser.TryGetValue(session.UserId, ByRef Dim userSessions) Then
                    userSessions.Remove(sessionToken)
                    If userSessions.Count = 0 Then
                        _sessionsByUser.TryRemove(session.UserId, ByRef userSessions)
                    End If
                End If

                ' Update current session if this was it
                If _currentSessionId = sessionToken Then
                    _currentSessionId = Nothing
                End If
            End If
        End Sub

        ''' &lt;summary&gt;
        ''' Terminates all sessions for a specific user
        ''' &lt;/summary&gt;
        ''' &lt;param name="userId"&gt;User identifier&lt;/param&gt;
        Public Sub TerminateUserSessions(userId As String)
            If _sessionsByUser.TryRemove(userId, ByRef Dim userSessions) Then
                For Each sessionToken In userSessions
                    If _activeSessions.TryRemove(sessionToken, ByRef Dim session) Then
                        session.IsActive = False
                        session.EndedAt = DateTime.UtcNow

                        If _currentSessionId = sessionToken Then
                            _currentSessionId = Nothing
                        End If
                    End If
                Next
            End If
        End Sub

        ''' &lt;summary&gt;
        ''' Terminates all active sessions
        ''' &lt;/summary&gt;
        Public Sub TerminateAllSessions()
            Dim sessionTokens = _activeSessions.Keys.ToList()

            For Each sessionToken In sessionTokens
                TerminateSession(sessionToken)
            Next

            _activeSessions.Clear()
            _sessionsByUser.Clear()
            _currentSessionId = Nothing
        End Sub

#End Region

#Region "Session Management"

        ''' &lt;summary&gt;
        ''' Gets all active sessions for a user
        ''' &lt;/summary&gt;
        ''' &lt;param name="userId"&gt;User identifier&lt;/param&gt;
        ''' &lt;returns&gt;List of active sessions&lt;/returns&gt;
        Public Function GetUserSessions(userId As String) As List(Of UserSession)
            Dim sessions = New List(Of UserSession)()

            If _sessionsByUser.TryGetValue(userId, ByRef Dim sessionTokens) Then
                For Each sessionToken In sessionTokens
                    If _activeSessions.TryGetValue(sessionToken, ByRef Dim session) Then
                        If session.IsActive AndAlso Not IsSessionExpired(session) Then
                            sessions.Add(session)
                        End If
                    End If
                Next
            End If

            Return sessions
        End Function

        ''' &lt;summary&gt;
        ''' Gets all active sessions
        ''' &lt;/summary&gt;
        ''' &lt;returns&gt;List of all active sessions&lt;/returns&gt;
        Public Function GetAllActiveSessions() As List(Of UserSession)
            CleanupExpiredSessions()
            Return _activeSessions.Values.Where(Function(s) s.IsActive).ToList()
        End Function

        ''' &lt;summary&gt;
        ''' Cleans up expired sessions
        ''' &lt;/summary&gt;
        Public Sub CleanupExpiredSessions()
            Dim expiredSessions = _activeSessions.Values.
                Where(Function(s) IsSessionExpired(s)).
                Select(Function(s) s.SessionId).
                ToList()

            For Each sessionId In expiredSessions
                TerminateSession(sessionId)
            Next
        End Sub

#End Region

#Region "Private Methods"

        Private Function GenerateSessionToken() As String
            Using rng As New System.Security.Cryptography.RNGCryptoServiceProvider()
                Dim tokenBytes(31) As Byte ' 256 bits
                rng.GetBytes(tokenBytes)
                Return Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "")
            End Using
        End Function

        Private Function IsSessionExpired(session As UserSession) As Boolean
            Return DateTime.UtcNow - session.LastActivity > _sessionTimeout
        End Function

#End Region

    End Class

#Region "Supporting Classes"

    ''' &lt;summary&gt;
    ''' Represents a user session
    ''' &lt;/summary&gt;
    Public Class UserSession
        Public Property SessionId As String
        Public Property UserId As String
        Public Property CreatedAt As DateTime
        Public Property LastActivity As DateTime
        Public Property EndedAt As DateTime?
        Public Property IsActive As Boolean
        Public Property IpAddress As String
        Public Property UserAgent As String

        Public ReadOnly Property Duration As TimeSpan
            Get
                Dim endTime = If(EndedAt.HasValue, EndedAt.Value, DateTime.UtcNow)
                Return endTime - CreatedAt
            End Get
        End Property

        Public ReadOnly Property IdleTime As TimeSpan
            Get
                Return DateTime.UtcNow - LastActivity
            End Get
        End Property
    End Class

#End Region

End Namespace