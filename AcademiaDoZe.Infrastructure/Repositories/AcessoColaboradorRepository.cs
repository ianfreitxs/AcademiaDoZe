// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : BaseRepository, IAcessoColaboradorRepository
{
    private readonly string _connectionString;
    private readonly DatabaseType _databaseType;

    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType)
        : base(connectionString, databaseType)
    {
        _connectionString = connectionString;
        _databaseType = databaseType;
    }

    private const string BaseSelectQuery = "SELECT id_acesso, pessoa_id, data_hora FROM tb_acesso WHERE pessoa_tipo = 1";

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        var registros = await ConsultarRegistrosAsync(
            $"{BaseSelectQuery} AND id_acesso = @Id",
            command => command.AddParameter("@Id", id, DbType.Int32),
            cancellationToken);

        return registros.Count == 0 ? null : await MapAsync(registros[0], cancellationToken);
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        var registros = await ConsultarRegistrosAsync($"{BaseSelectQuery} ORDER BY data_hora, id_acesso", null, cancellationToken);
        var acessos = new List<AcessoColaborador>(registros.Count);

        foreach (var registro in registros)
        {
            acessos.Add(await MapAsync(registro, cancellationToken));
        }

        return acessos;
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            var query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (1, @PessoaId, @DataHora)");
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_ACESSO_COLABORADOR", "Falha ao obter o ID do acesso do colaborador.", cancellationToken);
            typeof(Entity).GetProperty(nameof(Entity.Id))?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_COLABORADOR", $"Erro ao registrar acesso do colaborador: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        try
        {
            const string query = "UPDATE tb_acesso SET pessoa_id = @PessoaId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@PessoaId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            var alterados = await command.ExecuteNonQueryAsync(cancellationToken);
            if (alterados == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Acesso do colaborador com ID {entity.Id} não encontrado.");
            }

            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_COLABORADOR", $"Erro ao atualizar acesso do colaborador ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = 1";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_COLABORADOR", $"Erro ao remover acesso do colaborador ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var filtros = new List<string>();
        if (colaboradorId.HasValue)
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
            if (colaboradorId.HasValue)
            {
                command.AddParameter("@PessoaId", colaboradorId.Value, DbType.Int32);
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

        var acessos = new List<AcessoColaborador>(registros.Count);
        foreach (var registro in registros)
        {
            acessos.Add(await MapAsync(registro, cancellationToken));
        }
        return acessos;
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var acessos = await ObterAcessosPorColaboradorPeriodo(colaboradorId, null, null, cancellationToken);
        return acessos.OrderByDescending(a => a.DataHora).ThenByDescending(a => a.Id).FirstOrDefault();
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        var acessos = (await ObterAcessosPorColaboradorPeriodo(colaboradorId, data, data, cancellationToken))
            .OrderBy(a => a.DataHora)
            .ThenBy(a => a.Id)
            .ToList();

        var total = TimeSpan.Zero;
        for (var indice = 0; indice + 1 < acessos.Count; indice += 2)
        {
            var entrada = acessos[indice].DataHora;
            var saida = acessos[indice + 1].DataHora;
            if (saida >= entrada)
            {
                total += saida - entrada;
            }
        }

        if (acessos.Count % 2 == 1)
        {
            var entradaAberta = acessos[^1].DataHora;
            if (DateTime.Now >= entradaAberta)
            {
                total += DateTime.Now - entradaAberta;
            }
        }

        return total;
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
            throw new InfrastructureException("ERRO_CONSULTAR_ACESSOS_COLABORADOR", $"Erro ao consultar acessos de colaboradores: {ex.Message}", ex);
        }
    }

    private async Task<AcessoColaborador> MapAsync((int Id, int PessoaId, DateTime DataHora) registro, CancellationToken cancellationToken)
    {
        var colaborador = await new ColaboradorRepository(_connectionString, _databaseType).ObterPorId(registro.PessoaId, cancellationToken)
            ?? throw new InfrastructureException("COLABORADOR_ACESSO_NAO_ENCONTRADO", $"Colaborador ID {registro.PessoaId} associado ao acesso {registro.Id} não foi encontrado.");

        var resultado = AcessoColaborador.Criar(registro.Id, colaborador, registro.DataHora);
        if (resultado.IsFailure)
        {
            throw new InfrastructureException("ERRO_MAPEAR_ACESSO_COLABORADOR", string.Join(", ", resultado.Notifications.Select(n => n.Mensagem)));
        }

        return resultado.Value!;
    }
}
