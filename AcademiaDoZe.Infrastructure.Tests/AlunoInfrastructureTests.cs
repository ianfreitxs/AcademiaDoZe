// Ian Freitas de Sousa
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public sealed class AlunoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepository;
    private readonly AlunoRepository _repository;

    public AlunoInfrastructureTests()
    {
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
        _repository = new AlunoRepository(ConnectionString, DatabaseType);
    }

    [Fact]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(_repository, _logradouroRepository, this);
        var obtido = await _repository.ObterPorId(aluno.Id);
        Assert.NotNull(obtido);
        Assert.Equal(aluno.Id, obtido.Id);
        Assert.Equal(NomeCompleto, obtido.Nome);
        Assert.Equal(Sobrenome, obtido.Endereco.Complemento);
        Assert.Equal(SenhaDoSgbd, obtido.Senha.Valor);
    }

    [Fact]
    public async Task Aluno_ObterPorId_RetornaNuloQuandoInexistente()
    {
        Assert.Null(await _repository.ObterPorId(999999));
    }

    [Fact]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        await InfrastructureTestHelpers.CriarEInserirAlunoAsync(_repository, _logradouroRepository, this);
        Assert.Contains(await _repository.ObterTodos(), x => x.Nome == NomeCompleto);
    }

    [Fact]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(_repository, _logradouroRepository, this);
        var logradouro = await _logradouroRepository.ObterPorId(aluno.Endereco.LogradouroId) ?? throw new Exception();
        var atualizado = AcademiaDoZe.Domain.Entities.Aluno.Criar(aluno.Id, NomeCompleto + " Atualizado", aluno.Cpf.Valor, aluno.DataNascimento, aluno.Telefone.Valor, aluno.Email.Valor, logradouro, "150", Sobrenome, SenhaDoSgbd, aluno.Foto).Value!;
        var resultado = await _repository.Atualizar(atualizado);
        Assert.Equal(NomeCompleto + " Atualizado", resultado.Nome);
    }

    [Fact]
    public async Task Aluno_Atualizar_Inexistente_LancaExcecao()
    {
        var logradouro = await InfrastructureTestHelpers.CriarEInserirLogradouroAsync(_logradouroRepository, NomeCidade);
        var aluno = AcademiaDoZe.Domain.Entities.Aluno.Criar(999999, NomeCompleto, GerarCpfForTest(), new DateOnly(1990, 1, 1), GerarTelefoneForTest(), GerarEmailForTest(), logradouro, "1", Sobrenome, SenhaDoSgbd, Arquivo.Criar(new byte[] { 1 }).Value!).Value!;
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _repository.Atualizar(aluno));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Aluno_Remover_Sucesso_E_Falha()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(_repository, _logradouroRepository, this);
        Assert.True(await _repository.Remover(aluno.Id));
        Assert.False(await _repository.Remover(999999));
    }

    [Fact]
    public async Task Aluno_ObterPorCpf_E_ObterPorEmail_Sucesso()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(_repository, _logradouroRepository, this);
        Assert.Equal(aluno.Id, (await _repository.ObterPorCpf(aluno.Cpf))?.Id);
        Assert.Equal(aluno.Id, (await _repository.ObterPorEmail(aluno.Email))?.Id);
    }

    [Fact]
    public async Task Aluno_CpfJaExiste_E_EmailJaExiste_ValidacaoCorreta()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(_repository, _logradouroRepository, this);
        Assert.True(await _repository.CpfJaExiste(aluno.Cpf));
        Assert.False(await _repository.CpfJaExiste(aluno.Cpf, aluno.Id));
        Assert.True(await _repository.EmailJaExiste(aluno.Email));
        Assert.False(await _repository.EmailJaExiste(aluno.Email, aluno.Id));
    }

    [Fact]
    public async Task Aluno_ObterPorNome_Sucesso()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(_repository, _logradouroRepository, this);
        Assert.Contains(await _repository.ObterPorNome(NomeCompleto), x => x.Id == aluno.Id);
    }

    [Fact]
    public async Task Aluno_TrocarSenha_Sucesso_E_Falha()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(_repository, _logradouroRepository, this);
        var novaSenha = Senha.Criar(SenhaDoSgbd + "Nova").Value!;
        Assert.True(await _repository.TrocarSenha(aluno.Id, novaSenha));
        Assert.Equal(novaSenha.Valor, (await _repository.ObterPorId(aluno.Id))?.Senha.Valor);
        Assert.False(await _repository.TrocarSenha(999999, novaSenha));
    }
}
