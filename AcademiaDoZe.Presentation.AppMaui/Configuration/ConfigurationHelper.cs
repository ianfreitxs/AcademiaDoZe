// Ian Freitas de Sousa
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var databaseType = AppDatabaseType.Sqlite;

        string connectionString;

        if (databaseType == AppDatabaseType.Sqlite)
        {
            var dbPath = DeviceInfo.Platform == DevicePlatform.WinUI
    ? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Downloads",
        "AcademiaDoZe",
        "AcademiaDoZe",
        "db_academia_do_ze.db")
    : Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");

            connectionString = $"Data Source={dbPath};Default Timeout=5;";
        }
        else
        {
            const string dbServer = "10.30.21.16";
            const string dbDatabase = "db_academia_do_ze";
            string dbUser = databaseType == AppDatabaseType.SqlServer ? "sa" : "root";
            const string dbPassword = "abcBolinhas12345";

            string dbComplemento = string.Empty;

            if (databaseType == AppDatabaseType.SqlServer)
            {
                dbComplemento = "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";
            }
            else if (databaseType == AppDatabaseType.MySql)
            {
                dbComplemento = "Connection Timeout=5;Default Command Timeout=30;";
            }

            connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";
        }

        services.AddSingleton(new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        });

        services.AddApplicationServices();
    }
}