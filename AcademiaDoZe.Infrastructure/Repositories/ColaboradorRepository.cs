// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories;

public sealed class ColaboradorRepository : BaseRepository, IColaboradorRepository
{
    private const string BaseSelectQuery = "SELECT c.id_colaborador, c.cpf, c.nome, c.nascimento, c.telefone, c.email, c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo, l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais FROM tb_colaborador c INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro";

    public ColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }

    public async Task<Colaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"{BaseSelectQuery} WHERE c.id_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter colaborador por ID {id}: {ex.Message}", ex); }
    }

    public async Task<IEnumerable<Colaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"{BaseSelectQuery} ORDER BY c.nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var result = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
            return result;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os colaboradores: {ex.Message}", ex); }
    }

    public async Task<Colaborador> Adicionar(Colaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = FormatInsertQuery("INSERT INTO tb_colaborador (cpf, nome, nascimento, telefone, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo) VALUES (@Cpf, @Nome, @Nascimento, @Telefone, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto, @Admissao, @Tipo, @Vinculo)");
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Cpf", entity.Cpf.Valor, DbType.String);
            command.AddParameter("@Nome", entity.Nome, DbType.String);
            command.AddParameter("@Nascimento", entity.DataNascimento, DbType.Date);
            command.AddParameter("@Telefone", entity.Telefone.Valor, DbType.String);
            command.AddParameter("@Email", entity.Email.Valor, DbType.String);
            command.AddParameter("@LogradouroId", entity.Endereco.LogradouroId, DbType.Int32);
            command.AddParameter("@Numero", entity.Endereco.Numero, DbType.String);
            command.AddParameter("@Complemento", entity.Endereco.Complemento, DbType.String);
            command.AddParameter("@Senha", entity.Senha.Valor, DbType.String);
            command.AddParameter("@Foto", entity.Foto.Conteudo, DbType.Binary);
            command.AddParameter("@Admissao", entity.DataAdmissao, DbType.Date);
            command.AddParameter("@Tipo", (int)entity.Tipo, DbType.Int32);
            command.AddParameter("@Vinculo", (int)entity.Vinculo, DbType.Int32);
            var id = await command.ExecuteScalarIdAsync("ERRO_ADICIONAR_COLABORADOR", "Falha ao obter ID inserido para o colaborador.", cancellationToken);
            typeof(Entity).GetProperty(nameof(Entity.Id))?.SetValue(entity, id);
            return entity;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_ADICIONAR_COLABORADOR", $"Erro ao adicionar colaborador: {ex.Message}", ex); }
    }

    public async Task<Colaborador> Atualizar(Colaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "UPDATE tb_colaborador SET cpf = @Cpf, nome = @Nome, nascimento = @Nascimento, telefone = @Telefone, email = @Email, logradouro_id = @LogradouroId, numero = @Numero, complemento = @Complemento, senha = @Senha, foto = @Foto, admissao = @Admissao, tipo = @Tipo, vinculo = @Vinculo WHERE id_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@Cpf", entity.Cpf.Valor, DbType.String);
            command.AddParameter("@Nome", entity.Nome, DbType.String);
            command.AddParameter("@Nascimento", entity.DataNascimento, DbType.Date);
            command.AddParameter("@Telefone", entity.Telefone.Valor, DbType.String);
            command.AddParameter("@Email", entity.Email.Valor, DbType.String);
            command.AddParameter("@LogradouroId", entity.Endereco.LogradouroId, DbType.Int32);
            command.AddParameter("@Numero", entity.Endereco.Numero, DbType.String);
            command.AddParameter("@Complemento", entity.Endereco.Complemento, DbType.String);
            command.AddParameter("@Senha", entity.Senha.Valor, DbType.String);
            command.AddParameter("@Foto", entity.Foto.Conteudo, DbType.Binary);
            command.AddParameter("@Admissao", entity.DataAdmissao, DbType.Date);
            command.AddParameter("@Tipo", (int)entity.Tipo, DbType.Int32);
            command.AddParameter("@Vinculo", (int)entity.Vinculo, DbType.Int32);
            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0) throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum colaborador encontrado com ID {entity.Id} para atualização.");
            return entity;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_ATUALIZAR_COLABORADOR", $"Erro ao atualizar colaborador ID {entity.Id}: {ex.Message}", ex); }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "DELETE FROM tb_colaborador WHERE id_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_REMOVER_COLABORADOR", $"Erro ao remover colaborador ID {id}: {ex.Message}", ex); }
    }

    public async Task<Colaborador?> ObterPorCpf(Cpf cpf, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"{BaseSelectQuery} WHERE c.cpf = @Cpf";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Cpf", cpf.Valor, DbType.String);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_OBTER_POR_CPF", $"Erro ao obter colaborador por CPF {cpf.Valor}: {ex.Message}", ex); }
    }

    public async Task<Colaborador?> ObterPorEmail(Email email, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"{BaseSelectQuery} WHERE c.email = @Email";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Email", email.Valor, DbType.String);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_OBTER_POR_EMAIL", $"Erro ao obter colaborador por Email {email.Valor}: {ex.Message}", ex); }
    }

    public async Task<bool> CpfJaExiste(Cpf cpf, int? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "SELECT COUNT(1) FROM tb_colaborador WHERE cpf = @Cpf AND (@Id IS NULL OR id_colaborador <> @Id)";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Cpf", cpf.Valor, DbType.String);
            command.AddParameter("@Id", id, DbType.Int32);
            return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_VERIFICAR_CPF", $"Erro ao verificar existência de CPF: {ex.Message}", ex); }
    }

    public async Task<bool> EmailJaExiste(Email email, int? id = null, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "SELECT COUNT(1) FROM tb_colaborador WHERE email = @Email AND (@Id IS NULL OR id_colaborador <> @Id)";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Email", email.Valor, DbType.String);
            command.AddParameter("@Id", id, DbType.Int32);
            return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_VERIFICAR_EMAIL", $"Erro ao verificar existência de Email: {ex.Message}", ex); }
    }

    public async Task<IEnumerable<Colaborador>> ObterPorTipo(ColaboradorTipo tipo, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"{BaseSelectQuery} WHERE c.tipo = @Tipo ORDER BY c.nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Tipo", (int)tipo, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var result = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
            return result;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_OBTER_POR_TIPO", $"Erro ao obter colaboradores por tipo {tipo}: {ex.Message}", ex); }
    }

    public async Task<IEnumerable<Colaborador>> ObterPorVinculo(ColaboradorVinculo vinculo, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = $"{BaseSelectQuery} WHERE c.vinculo = @Vinculo ORDER BY c.nome";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Vinculo", (int)vinculo, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var result = new List<Colaborador>();
            while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
            return result;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_OBTER_POR_VINCULO", $"Erro ao obter colaboradores por vínculo {vinculo}: {ex.Message}", ex); }
    }

    public async Task<bool> TrocarSenha(int id, Senha novaSenha, CancellationToken cancellationToken = default)
    {
        try
        {
            const string query = "UPDATE tb_colaborador SET senha = @Senha WHERE id_colaborador = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            command.AddParameter("@Senha", novaSenha.Valor, DbType.String);
            return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
        }
        catch (DbException ex) { throw new InfrastructureException("ERRO_TROCAR_SENHA", $"Erro ao trocar senha do colaborador ID {id}: {ex.Message}", ex); }
    }

    public static Colaborador Map(DbDataReader reader)
    {
        try
        {
            var id = reader.GetInt32Value("id_colaborador");
            var cpf = reader.GetStringValue("cpf");
            var nome = reader.GetStringValue("nome");
            var dataNascimento = reader.GetDateOnlyValue("nascimento");
            var telefone = reader.GetStringValue("telefone");
            var email = reader.GetStringValue("email");
            var numero = reader.GetStringValue("numero");
            var complemento = reader.GetNullableString("complemento");
            var senha = reader.GetStringValue("senha");
            var dataAdmissao = reader.GetDateOnlyValue("admissao");
            var tipo = (ColaboradorTipo)reader.GetInt32Value("tipo");
            var vinculo = (ColaboradorVinculo)reader.GetInt32Value("vinculo");
            var fotoBytes = reader.GetNullableBytes("foto");
            var foto = fotoBytes is null ? Arquivo.Criar(Array.Empty<byte>()).Value! : Arquivo.Criar(fotoBytes).Value!;
            var logradouro = LogradouroRepository.Map(reader, "logradouro_nome");
            var result = Colaborador.Criar(id, nome, cpf, dataNascimento, telefone, email, logradouro, numero, complemento, senha, foto, dataAdmissao, tipo, vinculo);
            if (result.IsFailure) throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear colaborador ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            return result.Value!;
        }
        catch (InfrastructureException) { throw; }
        catch (Exception ex) { throw new InfrastructureException("ERRO_MAPEAMENTO_COLABORADOR", $"Erro ao mapear dados do colaborador: {ex.Message}", ex); }
    }
}
