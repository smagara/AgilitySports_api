using System.Configuration;
using Microsoft.Data.SqlClient;

namespace AgilitySportsAPI.Data;

// DB "Context".  
// Use a connection string based on the offline preference, default to azure online
// Allows dev unit testing with a local database or dev azure database
public abstract class BaseRepo
{
    protected readonly IConfiguration configuration;
    protected string azureClientID = "";
    protected string azureSQLAuthURL = "";
    protected bool isAzureMode;
    protected string databaseMode = "Azure";
    protected string connectionString = "";

    public BaseRepo(IConfiguration configuration)
    {
        this.configuration = configuration;
        fetchSettings();
    }

    private void fetchSettings()
    {
        // Prefer explicit mode selection; keep CloudOffline as backwards-compatible fallback.
        string? configuredMode = configuration["Database:Mode"];
        if (string.IsNullOrWhiteSpace(configuredMode))
        {
            bool cloudOffline = bool.Parse(configuration["AzureSettings:CloudOffline"] ?? "false");
            databaseMode = cloudOffline ? "LocalDb" : "Azure";
        }
        else
        {
            databaseMode = configuredMode.Trim();
        }

        switch (databaseMode.ToUpperInvariant())
        {
            case "AZURE":
                isAzureMode = true;
                connectionString = configuration.GetConnectionString("AzureConnection") ?? "";
                break;
            case "DOCKER":
                isAzureMode = false;
                connectionString = configuration.GetConnectionString("DockerConnection") ?? "";
                break;
            case "LOCALDB":
                isAzureMode = false;
                connectionString = configuration.GetConnectionString("LocalConnection") ?? "";
                break;
            default:
                throw new ConfigurationErrorsException("Database:Mode must be Azure, Docker, or LocalDb.");
        }

        if (isAzureMode)
        {
            azureClientID = configuration.GetValue<string?>("AzureSettings:ClientID") ?? "";
            azureSQLAuthURL = configuration.GetValue<string?>("AzureSettings:AzureSQLAuthURL") ?? "";

            if (azureClientID == "" || azureSQLAuthURL == "")
            {
                throw new ConfigurationErrorsException("AzureSettings not set in appsettings.json");
            }
        }

        if (connectionString == "")
        {
            throw new ConfigurationErrorsException("Connection string not set properly in appsettings.json");
        }
    }

    protected async Task GenToken(SqlConnection connection)
    {
        if (isAzureMode)
        {
            // Determine the Azure token to create.  Default if in dev environment for testing or managed identity for production.
            Azure.Core.TokenCredential credential;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                Console.WriteLine("Generating Default token for Azure connection");
                credential = new Azure.Identity.DefaultAzureCredential();
            }
            else
            {
                Console.WriteLine("Generating Managed Identity token for Azure connection");
                credential = new Azure.Identity.ManagedIdentityCredential(azureClientID);
            }

            var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { azureSQLAuthURL }), CancellationToken.None);
            connection.AccessToken = token.Token;
        }
        else
        {
            Console.WriteLine($"Database mode '{databaseMode}' selected; using SQL authentication/connection string without Azure token.");
        }
    }
}
