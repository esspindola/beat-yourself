using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace backend.Extensions
{
    public static class FirebaseExtensions
    {
        public static IServiceCollection AddFirebaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            string? credentialsPath = configuration["Firebase:CredentialsPath"];
            string? projectId = configuration["Firebase:ProjectId"];

            if (string.IsNullOrEmpty(credentialsPath) || string.IsNullOrEmpty(projectId))
            {
                throw new InvalidOperationException("Firebase configuration is missing. Ensure 'Firebase:CredentialsPath' and 'Firebase:ProjectId' are set in appsettings.json.");
            }

            var baseDirectory = AppContext.BaseDirectory;
            var credentialsFilePath = Path.Combine(baseDirectory, credentialsPath);

            if (!File.Exists(credentialsFilePath))
            {
                throw new FileNotFoundException($"Firebase credentials file not found at: {credentialsFilePath}. Make sure the path in appsettings.json is correct and the file exists.");
            }

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    var credential = GoogleCredential.FromFile(credentialsFilePath);
                    var firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = credential,
                        ProjectId = projectId
                    });
                    Console.WriteLine($"Firebase Admin SDK initialized for project: {projectId}");
                }
                else
                {
                    Console.WriteLine($"Firebase Admin SDK already initialized for project: {FirebaseApp.DefaultInstance.Options.ProjectId}");
                }

                services.AddSingleton(sp => FirestoreDb.Create(projectId));

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error initializing Firebase Admin SDK: {ex.Message}");
                throw;
            }

            return services;
        }
    }
}