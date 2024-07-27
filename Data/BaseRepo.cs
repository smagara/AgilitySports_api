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
    protected bool azureOffline;  // set true in appsettings for local db testing
    protected string connectionString = "";

    public BaseRepo(IConfiguration configuration)
    {
        this.configuration = configuration;
        fetchSettings();
    }

    private void fetchSettings()
    {
        // fetch azure settings
        azureClientID = configuration.GetValue<string?>("AzureSettings:ClientID") ?? "";
        azureSQLAuthURL = configuration.GetValue<string?>("AzureSettings:AzureSQLAuthURL") ?? "";
        azureOffline = bool.Parse(configuration["AzureSettings:CloudOffline"] ?? "false");

        // determine offline local or azure connection string
        connectionString = azureOffline ?
            (configuration.GetConnectionString("LocalConnection") ?? "") :
            (configuration.GetConnectionString("AzureConnection") ?? "");

        if (azureClientID == "" || azureSQLAuthURL == "")
        {
            throw new ConfigurationErrorsException("AzureSettings not set in appsettings.json");
        }

        if (connectionString == "")
        {
            throw new ConfigurationErrorsException("Connection string not set properly in appsettings.json");
        }
    }

    protected async Task GenToken(SqlConnection connection)
    {
        if (!azureOffline)
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
            Console.WriteLine("Cloud offline setting specified, using local db");
        }
    }
}
