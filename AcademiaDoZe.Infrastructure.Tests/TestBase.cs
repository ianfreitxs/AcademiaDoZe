// Ian Freitas de Sousa
using AcademiaDoZe.Infrastructure.Data;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    private const DatabaseType SelectedDatabaseType = DatabaseType.Sqlite;

    public DatabaseType DatabaseType { get; }
    public string ConnectionString { get; }
    public string NomeCompleto => "Ian Freitas de Sousa";
    public string Sobrenome => "Sousa";

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;

        var solutionDirectory = FindSolutionDirectory();

        ConnectionString = DatabaseType switch
        {
            DatabaseType.SqlServer => "Server=localhost;Database=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=True;",
            DatabaseType.MySql => "Server=localhost;Database=db_academia_do_ze;User Id=root;Password=abcBolinhas12345;",
            DatabaseType.Sqlite => $"Data Source={Path.Combine(solutionDirectory, "db_academia_do_ze.db")};Cache=Shared;",
            _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
        };
    }

    public string NomeCidade => DatabaseType switch
    {
        DatabaseType.SqlServer => "SQLServer",
        DatabaseType.MySql => "MySQL",
        DatabaseType.Sqlite => "SQLite",
        _ => throw new ArgumentOutOfRangeException(nameof(DatabaseType), DatabaseType, "SGBD não suportado para testes.")
    };

    public string SenhaDoSgbd => NomeCidade switch
    {
        "SQLServer" => "SQLServer123",
        "MySQL" => "MySQL123",
        _ => "SQLite123"
    };

    public static string GerarCep()
    {
        return Interlocked.Increment(ref _counter).ToString("D8");
    }

    public static string GerarCpf()
    {
        var valor = Math.Abs(Interlocked.Increment(ref _cpfCounter)) % 900000000 + 100000000;
        var cpf = valor.ToString("D9");
        var soma = 0;

        for (var i = 0; i < 9; i++)
            soma += (cpf[i] - '0') * (10 - i);

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        cpf += digito1;
        soma = 0;

        for (var i = 0; i < 10; i++)
            soma += (cpf[i] - '0') * (11 - i);

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return cpf + digito2;
    }

    public static string GerarEmail()
    {
        return $"user_{Guid.NewGuid():N}@test.com";
    }

    public static string GerarTelefone()
    {
        var valor = Math.Abs(Interlocked.Increment(ref _telefoneCounter)) % 800000000 + 100000000;
        return $"49{valor:D9}"[..11];
    }

    private static int _counter = (int)(DateTime.UtcNow.Ticks % 70000000) + 10000000;
    private static int _cpfCounter = (int)(DateTime.UtcNow.Ticks % 70000000) + 10000000;
    private static int _telefoneCounter = (int)(DateTime.UtcNow.Ticks % 70000000) + 10000000;

    public string GerarCpfForTest()
    {
        return GerarCpf();
    }

    public string GerarEmailForTest()
    {
        return GerarEmail();
    }

    public string GerarTelefoneForTest()
    {
        return GerarTelefone();
    }

    private static string FindSolutionDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AcademiaDoZe.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? AppContext.BaseDirectory;
    }
}