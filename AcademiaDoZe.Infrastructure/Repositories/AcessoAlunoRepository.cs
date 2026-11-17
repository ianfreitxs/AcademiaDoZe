// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : BaseRepository, IAcessoAlunoRepository
{
    private readonly string _connectionString;
    private readonly DatabaseType _databaseType;

    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
        _connectionString = connectionString;
        _databaseType = databaseType;
    }

    private const string BaseSelectQuery = "SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 0";

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        var registros = await ConsultarRegistrosAsync(
            $"{BaseSelectQuery} AND id_acesso = @Id",
            command => command.AddParameter("@Id", id, DbType.Int32),
            cancellationToken);

        return registros.Count == 0 ? null : await MapAsync(registros[0], cancellationToken);
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        var registros = await ConsultarRegistrosAsync($"{BaseSelectQuery} ORDER BY data_hora, id_acesso", null, cancellationToken);
        var acessos = new List<AcessoAluno>(registros.Count);

        foreach (var registro in registros)
        {
            acessos.Add(await MapAsync(registro, cancellationToken));
        }

        return acessos;
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            var query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (0, @PessoaId, @DataHora)");
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_ALUNO", "Falha ao obter o ID do acesso do aluno.", cancellationToken);
            typeof(Entity).GetProperty(nameof(Entity.Id))?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_ALUNO", $"Erro ao registrar acesso do aluno: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            const string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = 0";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            var alterados = await command.ExecuteNonQueryAsync(cancellationToken);
            if (alterados == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Acesso do aluno com ID {entity.Id} não encontrado.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_ALUNO", $"Erro ao atualizar acesso do aluno ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 0";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_ALUNO", $"Erro ao remover acesso do aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var filtros = new List<string>();
        if (alunoId.HasValue)
        {
            filtros.Add("pessoa_id = @PessoaId");
        }
        if (inicio.HasValue)
        {
            filtros.Add("data_hora >= @Inicio");
        }
        if (fim.HasValue)
        {
            filtros.Add("data_hora < @FimExclusive");
        }

        var query = BaseSelectQuery + (filtros.Count > 0 ? " AND " + string.Join(" AND ", filtros) : string.Empty) + " ORDER BY data_hora, id_acesso";
        var registros = await ConsultarRegistrosAsync(query, command =>
        {
            if (alunoId.HasValue)
            {
                command.AddParameter("@PessoaId", alunoId.Value, DbType.Int32);
            }
            if (inicio.HasValue)
            {
                command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            }
            if (fim.HasValue)
            {
                command.AddParameter("@FimExclusive", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            }
        }, cancellationToken);

        var acessos = new List<AcessoAluno>(registros.Count);
        foreach (var registro in registros)
        {
            acessos.Add(await MapAsync(registro, cancellationToken));
        }
        return acessos;
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        var acessos = await ObterAcessosPorAlunoPeriodo(alunoId, null, null, cancellationToken);
        return acessos.OrderByDescending(a => a.DataHora).ThenByDescending(a => a.Id).FirstOrDefault();
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "SELECT COUNT(1) FROM tb_acesso WHERE pessoa_tipo = 0 AND pessoa_id = @PessoaId";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaId", alunoId, DbType.Int32);
            var quantidade = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
            return quantidade % 2 == 1;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_ACESSO_ALUNO", $"Erro ao verificar presença do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        if (mes is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(mes));
        }

        var acessos = await ObterTodos(cancellationToken);
        var entradas = acessos
            .GroupBy(a => a.AlunoId)
            .SelectMany(grupo => grupo.OrderBy(a => a.DataHora).ThenBy(a => a.Id).Where((acesso, indice) => indice % 2 == 0))
            .Where(a => a.DataHora.Month == mes);

        return entradas
            .GroupBy(a => new TimeOnly(a.DataHora.Hour, 0))
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Count());
    }

    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        if (mes is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(mes));
        }

        var acessos = await ObterTodos(cancellationToken);
        var permanencias = new List<TimeSpan>();

        foreach (var grupo in acessos.GroupBy(a => a.AlunoId))
        {
            var eventos = grupo.OrderBy(a => a.DataHora).ThenBy(a => a.Id).ToList();
            for (var indice = 0; indice + 1 < eventos.Count; indice += 2)
            {
                var entrada = eventos[indice];
                var saida = eventos[indice + 1];
                if (entrada.DataHora.Month == mes && saida.DataHora >= entrada.DataHora)
                {
                    permanencias.Add(saida.DataHora - entrada.DataHora);
                }
            }
        }

        if (permanencias.Count == 0)
        {
            return new Dictionary<int, TimeSpan>();
        }

        var mediaTicks = (long)permanencias.Average(p => p.Ticks);
        return new Dictionary<int, TimeSpan> { [mes] = TimeSpan.FromTicks(mediaTicks) };
    }

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        if (dias < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(dias));
        }

        var acessos = await ObterTodos(cancellationToken);
        var dataLimite = DateTime.Now.AddDays(-dias);
        var alunosComAcesso = acessos
            .Where(a => a.DataHora >= dataLimite)
            .Select(a => a.AlunoId)
            .ToHashSet();

        return (await new AlunoRepository(_connectionString, _databaseType).ObterTodos(cancellationToken))
            .Where(aluno => !alunosComAcesso.Contains(aluno.Id))
            .ToList();
    }

    private async Task<List<(int Id, int PessoaId, DateTime DataHora)>> ConsultarRegistrosAsync(string query, Action<DbCommand>? configurar, CancellationToken cancellationToken)
    {
        try
        {
            await using var command = await CreateCommandAsync(query, cancellationToken);
            configurar?.Invoke(command);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var registros = new List<(int Id, int PessoaId, DateTime DataHora)>();

            while (await reader.ReadAsync(cancellationToken))
            {
                registros.Add((
                    Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture),
                    Convert.ToInt32(reader.GetValue(1), CultureInfo.InvariantCulture),
                    Convert.ToDateTime(reader.GetValue(2), CultureInfo.InvariantCulture)));
            }

            return registros;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_CONSULTAR_ACESSOS_ALUNO", $"Erro ao consultar acessos de alunos: {ex.Message}", ex);
        }
    }

    private async Task<AcessoAluno> MapAsync((int Id, int PessoaId, DateTime DataHora) registro, CancellationToken cancellationToken)
    {
        var aluno = await new AlunoRepository(_connectionString, _databaseType).ObterPorId(registro.PessoaId, cancellationToken)
            ?? throw new InfrastructureException("ALUNO_ACESSO_NAO_ENCONTRADO", $"Aluno ID {registro.PessoaId} associado ao acesso {registro.Id} não foi encontrado.");

        var resultado = AcessoAluno.Criar(registro.Id, aluno, registro.DataHora);
        if (resultado.IsFailure)
        {
            throw new InfrastructureException("ERRO_MAPEAR_ACESSO_ALUNO", string.Join(", ", resultado.Notifications.Select(n => n.Mensagem)));
        }

        return resultado.Value!;
    }
}
