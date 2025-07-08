using Google.Cloud.Firestore;
using backend.DTOs.Account;
using backend.Services;
using backend.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly FirebaseAuth _firebaseAuth;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(FirestoreDb firestoreDb, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _firestoreDb = firestoreDb;
            _configuration = configuration;
            _logger = logger;
            if (FirebaseApp.DefaultInstance == null)
            {
                throw new InvalidOperationException("Firebase Admin SDK has not been initialized. Ensure FirebaseApp.Create has been called.");
            }
            _firebaseAuth = FirebaseAuth.GetAuth(FirebaseApp.DefaultInstance);
        }

        public async Task<AuthServiceResult<string>> RegisterAsync(RegisterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return new AuthServiceResult<string> { Success = false, Message = "Email and password are required." };
            }

            try
            {
                var newUserArgs = new UserRecordArgs
                {
                    Email = request.Email,
                    EmailVerified = false,
                    Password = request.Password,
                    DisplayName = request.FullName,
                    Disabled = false
                };

                UserRecord userRecord = await _firebaseAuth.CreateUserAsync(newUserArgs);

                var userId = userRecord.Uid;

                var userProfile = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    CreatedAt = DateTime.UtcNow
                };

                var userDocRef = _firestoreDb.Collection("users").Document(userId);
                await userDocRef.SetAsync(userProfile);

                _logger.LogInformation($"User registered successfully: {request.Email} with Firebase UID: {userId}");

                return new AuthServiceResult<string> { Success = true, Data = userId };
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError(ex, $"Firebase Authentication error during registration for email: {request.Email}. Error: {ex.Message}");
                string errorMessage = ex.Message;

                if (ex.AuthErrorCode == AuthErrorCode.EmailAlreadyExists)
                {
                    errorMessage = "The email address is already in use by another account.";
                }
                else if (ex.Message.ToLower().Contains("invalid email"))
                {
                    errorMessage = "The email address is not valid.";
                }

                return new AuthServiceResult<string> { Success = false, Message = errorMessage };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred during registration for email: {request.Email}.");
                return new AuthServiceResult<string> { Success = false, Message = "An unexpected error occurred. Please try again later." };
            }
        }

        public async Task<AuthServiceResult<string>> LoginAsync(LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return new AuthServiceResult<string> { Success = false, Message = "Email and password are required." };
            }

            try
            {
                var user = await _firebaseAuth.GetUserByEmailAsync(request.Email);

                if (user == null)
                {
                    return new AuthServiceResult<string> { Success = false, Message = "Usuario no encontrado o credenciales incorrectas." };
                }

                return new AuthServiceResult<string> { Success = true, Data = user.Uid };
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError(ex, $"Firebase Authentication error during login for email: {request.Email}. Error: {ex.Message}");
                string errorMessage = ex.Message;

                if (ex.ErrorCode.ToString() == "auth/user-not-found")
                {
                    errorMessage = "User not found or incorrect credentials.";
                }
                else if (ex.ErrorCode.ToString() == "auth/wrong-password")
                {
                    errorMessage = "Incorrect password.";
                }
                else if (ex.ErrorCode.ToString() == "auth/invalid-email")
                {
                    errorMessage = "The email address is not valid.";
                }

                return new AuthServiceResult<string> { Success = false, Message = errorMessage };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred during login for email: {request.Email}.");
                return new AuthServiceResult<string> { Success = false, Message = "An unexpected error occurred. Please try again later." };
            }
        }
    }
}