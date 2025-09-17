Imports System.Security.Cryptography
Imports System.Text
Imports System.IO
Imports System.Threading.Tasks

Namespace ARIA.Security

    ''' &lt;summary&gt;
    ''' Manages encryption and decryption of sensitive data
    ''' Uses AES-256 encryption for maximum security
    ''' &lt;/summary&gt;
    Friend Class EncryptionManager

#Region "Properties and Fields"

        Private _masterKey As Byte()
        Private _apiKeyKey As Byte()
        Private ReadOnly _keyDerivationSalt As Byte()
        Private ReadOnly _apiKeyDerivationSalt As Byte()
        Private _isInitialized As Boolean = False

        Private Const ITERATION_COUNT As Integer = 10000
        Private Const KEY_SIZE As Integer = 256 \ 8 ' 256 bits = 32 bytes
        Private Const IV_SIZE As Integer = 128 \ 8 ' 128 bits = 16 bytes

        Public ReadOnly Property IsInitialized As Boolean
            Get
                Return _isInitialized
            End Get
        End Property

#End Region

#Region "Constructor"

        Public Sub New()
            ' Generate fixed salts for key derivation (in production, these should be securely stored)
            _keyDerivationSalt = Encoding.UTF8.GetBytes("ARIA_MASTER_SALT_2024")
            _apiKeyDerivationSalt = Encoding.UTF8.GetBytes("ARIA_APIKEY_SALT_2024")
        End Sub

#End Region

#Region "Initialization"

        ''' &lt;summary&gt;
        ''' Initializes the encryption manager with master keys
        ''' &lt;/summary&gt;
        Public Async Function InitializeAsync() As Task
            Try
                ' Generate or load master keys
                _masterKey = Await GetOrCreateMasterKeyAsync()
                _apiKeyKey = Await GetOrCreateAPIKeyKeyAsync()

                _isInitialized = True

            Catch ex As Exception
                Throw New InvalidOperationException($"Failed to initialize encryption manager: {ex.Message}", ex)
            End Try
        End Function

#End Region

#Region "General Encryption"

        ''' &lt;summary&gt;
        ''' Encrypts a string using AES-256
        ''' &lt;/summary&gt;
        ''' &lt;param name="plaintext"&gt;Text to encrypt&lt;/param&gt;
        ''' &lt;returns&gt;Base64 encoded encrypted string&lt;/returns&gt;
        Public Function Encrypt(plaintext As String) As String
            If Not _isInitialized Then
                Throw New InvalidOperationException("Encryption manager not initialized")
            End If

            If String.IsNullOrEmpty(plaintext) Then
                Return String.Empty
            End If

            Try
                Using aes As Aes = Aes.Create()
                    aes.Key = _masterKey
                    aes.Mode = CipherMode.CBC
                    aes.Padding = PaddingMode.PKCS7

                    ' Generate random IV for each encryption
                    aes.GenerateIV()

                    Using encryptor As ICryptoTransform = aes.CreateEncryptor()
                        Dim plaintextBytes = Encoding.UTF8.GetBytes(plaintext)

                        Using memoryStream As New MemoryStream()
                            ' Write IV first
                            memoryStream.Write(aes.IV, 0, aes.IV.Length)

                            Using cryptoStream As New CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)
                                cryptoStream.Write(plaintextBytes, 0, plaintextBytes.Length)
                                cryptoStream.FlushFinalBlock()
                            End Using

                            Return Convert.ToBase64String(memoryStream.ToArray())
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Throw New InvalidOperationException($"Encryption failed: {ex.Message}", ex)
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Decrypts a Base64 encoded encrypted string
        ''' &lt;/summary&gt;
        ''' &lt;param name="ciphertext"&gt;Base64 encoded encrypted string&lt;/param&gt;
        ''' &lt;returns&gt;Decrypted plaintext&lt;/returns&gt;
        Public Function Decrypt(ciphertext As String) As String
            If Not _isInitialized Then
                Throw New InvalidOperationException("Encryption manager not initialized")
            End If

            If String.IsNullOrEmpty(ciphertext) Then
                Return String.Empty
            End If

            Try
                Dim ciphertextBytes = Convert.FromBase64String(ciphertext)

                Using aes As Aes = Aes.Create()
                    aes.Key = _masterKey
                    aes.Mode = CipherMode.CBC
                    aes.Padding = PaddingMode.PKCS7

                    ' Extract IV from the beginning of the ciphertext
                    Dim iv(IV_SIZE - 1) As Byte
                    Array.Copy(ciphertextBytes, 0, iv, 0, IV_SIZE)
                    aes.IV = iv

                    Using decryptor As ICryptoTransform = aes.CreateDecryptor()
                        Using memoryStream As New MemoryStream(ciphertextBytes, IV_SIZE, ciphertextBytes.Length - IV_SIZE)
                            Using cryptoStream As New CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)
                                Using reader As New StreamReader(cryptoStream, Encoding.UTF8)
                                    Return reader.ReadToEnd()
                                End Using
                            End Using
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Throw New InvalidOperationException($"Decryption failed: {ex.Message}", ex)
            End Try
        End Function

#End Region

#Region "API Key Encryption"

        ''' &lt;summary&gt;
        ''' Encrypts an API key with additional security measures
        ''' &lt;/summary&gt;
        ''' &lt;param name="apiKey"&gt;API key to encrypt&lt;/param&gt;
        ''' &lt;returns&gt;Encrypted API key&lt;/returns&gt;
        Public Function EncryptAPIKey(apiKey As String) As String
            If Not _isInitialized Then
                Throw New InvalidOperationException("Encryption manager not initialized")
            End If

            If String.IsNullOrEmpty(apiKey) Then
                Return String.Empty
            End If

            Try
                Using aes As Aes = Aes.Create()
                    aes.Key = _apiKeyKey
                    aes.Mode = CipherMode.CBC
                    aes.Padding = PaddingMode.PKCS7

                    ' Generate random IV
                    aes.GenerateIV()

                    Using encryptor As ICryptoTransform = aes.CreateEncryptor()
                        ' Add checksum for integrity
                        Dim checksumHash = ComputeChecksum(apiKey)
                        Dim dataToEncrypt = $"{apiKey}|{checksumHash}"
                        Dim plaintextBytes = Encoding.UTF8.GetBytes(dataToEncrypt)

                        Using memoryStream As New MemoryStream()
                            ' Write IV first
                            memoryStream.Write(aes.IV, 0, aes.IV.Length)

                            Using cryptoStream As New CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)
                                cryptoStream.Write(plaintextBytes, 0, plaintextBytes.Length)
                                cryptoStream.FlushFinalBlock()
                            End Using

                            Return Convert.ToBase64String(memoryStream.ToArray())
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Throw New InvalidOperationException($"API key encryption failed: {ex.Message}", ex)
            End Try
        End Function

        ''' &lt;summary&gt;
        ''' Decrypts an API key and validates integrity
        ''' &lt;/summary&gt;
        ''' &lt;param name="encryptedAPIKey"&gt;Encrypted API key&lt;/param&gt;
        ''' &lt;returns&gt;Decrypted API key&lt;/returns&gt;
        Public Function DecryptAPIKey(encryptedAPIKey As String) As String
            If Not _isInitialized Then
                Throw New InvalidOperationException("Encryption manager not initialized")
            End If

            If String.IsNullOrEmpty(encryptedAPIKey) Then
                Return String.Empty
            End If

            Try
                Dim ciphertextBytes = Convert.FromBase64String(encryptedAPIKey)

                Using aes As Aes = Aes.Create()
                    aes.Key = _apiKeyKey
                    aes.Mode = CipherMode.CBC
                    aes.Padding = PaddingMode.PKCS7

                    ' Extract IV
                    Dim iv(IV_SIZE - 1) As Byte
                    Array.Copy(ciphertextBytes, 0, iv, 0, IV_SIZE)
                    aes.IV = iv

                    Using decryptor As ICryptoTransform = aes.CreateDecryptor()
                        Using memoryStream As New MemoryStream(ciphertextBytes, IV_SIZE, ciphertextBytes.Length - IV_SIZE)
                            Using cryptoStream As New CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)
                                Using reader As New StreamReader(cryptoStream, Encoding.UTF8)
                                    Dim decryptedData = reader.ReadToEnd()

                                    ' Validate checksum
                                    Dim parts = decryptedData.Split("|"c)
                                    If parts.Length <> 2 Then
                                        Throw New InvalidOperationException("Invalid API key format")
                                    End If

                                    Dim apiKey = parts(0)
                                    Dim storedChecksum = parts(1)
                                    Dim computedChecksum = ComputeChecksum(apiKey)

                                    If storedChecksum <> computedChecksum Then
                                        Throw New InvalidOperationException("API key integrity check failed")
                                    End If

                                    Return apiKey
                                End Using
                            End Using
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                Throw New InvalidOperationException($"API key decryption failed: {ex.Message}", ex)
            End Try
        End Function

#End Region

#Region "Key Management"

        Private Async Function GetOrCreateMasterKeyAsync() As Task(Of Byte())
            Dim keyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ARIA", "keys", "master.key")

            Try
                If File.Exists(keyPath) Then
                    ' Load existing key
                    Dim encryptedKey = Await File.ReadAllTextAsync(keyPath)
                    Return DecryptKeyWithDPAPI(encryptedKey)
                Else
                    ' Generate new key
                    Dim newKey = GenerateKey()

                    ' Save encrypted with DPAPI
                    Directory.CreateDirectory(Path.GetDirectoryName(keyPath))
                    Dim encryptedKey = EncryptKeyWithDPAPI(newKey)
                    Await File.WriteAllTextAsync(keyPath, encryptedKey)

                    Return newKey
                End If

            Catch ex As Exception
                ' If key loading fails, generate a new one from machine characteristics
                Return DeriveKeyFromMachine("master")
            End Try
        End Function

        Private Async Function GetOrCreateAPIKeyKeyAsync() As Task(Of Byte())
            Dim keyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ARIA", "keys", "apikey.key")

            Try
                If File.Exists(keyPath) Then
                    Dim encryptedKey = Await File.ReadAllTextAsync(keyPath)
                    Return DecryptKeyWithDPAPI(encryptedKey)
                Else
                    Dim newKey = GenerateKey()

                    Directory.CreateDirectory(Path.GetDirectoryName(keyPath))
                    Dim encryptedKey = EncryptKeyWithDPAPI(newKey)
                    Await File.WriteAllTextAsync(keyPath, encryptedKey)

                    Return newKey
                End If

            Catch ex As Exception
                Return DeriveKeyFromMachine("apikey")
            End Try
        End Function

        Private Function GenerateKey() As Byte()
            Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
                Dim key(KEY_SIZE - 1) As Byte
                rng.GetBytes(key)
                Return key
            End Using
        End Function

        Private Function DeriveKeyFromMachine(purpose As String) As Byte()
            ' Derive key from machine characteristics as fallback
            Dim machineInfo = $"{Environment.MachineName}|{Environment.UserName}|{purpose}"
            Dim salt = If(purpose = "master", _keyDerivationSalt, _apiKeyDerivationSalt)

            Using pbkdf2 As New Rfc2898DeriveBytes(machineInfo, salt, ITERATION_COUNT)
                Return pbkdf2.GetBytes(KEY_SIZE)
            End Using
        End Function

        Private Function EncryptKeyWithDPAPI(key As Byte()) As String
            Try
                Dim encryptedKey = ProtectedData.Protect(key, Nothing, DataProtectionScope.CurrentUser)
                Return Convert.ToBase64String(encryptedKey)
            Catch ex As Exception
                ' If DPAPI fails, use simple base64 (less secure)
                Return Convert.ToBase64String(key)
            End Try
        End Function

        Private Function DecryptKeyWithDPAPI(encryptedKey As String) As Byte()
            Try
                Dim encryptedBytes = Convert.FromBase64String(encryptedKey)
                Return ProtectedData.Unprotect(encryptedBytes, Nothing, DataProtectionScope.CurrentUser)
            Catch ex As Exception
                ' If DPAPI fails, assume it's plain base64
                Return Convert.FromBase64String(encryptedKey)
            End Try
        End Function

#End Region

#Region "Utility Methods"

        Private Function ComputeChecksum(data As String) As String
            Using sha256 As SHA256 = SHA256.Create()
                Dim hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data))
                Return Convert.ToBase64String(hashBytes)
            End Using
        End Function

        ''' &lt;summary&gt;
        ''' Securely clears sensitive data from memory
        ''' &lt;/summary&gt;
        ''' &lt;param name="data"&gt;Byte array to clear&lt;/param&gt;
        Public Shared Sub SecureClear(data As Byte())
            If data IsNot Nothing Then
                Array.Clear(data, 0, data.Length)
            End If
        End Sub

#End Region

#Region "IDisposable Support"

        Private disposedValue As Boolean

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' Clear sensitive data
                    If _masterKey IsNot Nothing Then
                        SecureClear(_masterKey)
                    End If
                    If _apiKeyKey IsNot Nothing Then
                        SecureClear(_apiKeyKey)
                    End If
                End If
                disposedValue = True
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#End Region

    End Class

End Namespace