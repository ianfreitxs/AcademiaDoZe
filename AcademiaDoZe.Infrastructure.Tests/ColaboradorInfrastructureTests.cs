// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;
using Xunit;

namespace AcademiaDoZe.Infrastructure.Tests;

public sealed class ColaboradorInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepository;
    private readonly ColaboradorRepository _repository;

    public ColaboradorInfrastructureTests()
    {
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
        _repository = new ColaboradorRepository(ConnectionString, DatabaseType);
    }

    [Fact]
    public async Task Colaborador_Adicionar_E_ObterPorId_Sucesso()
    {
        var colaborador = await InfrastructureTestHelpers.CriarEInserirColaboradorAsync(_repository, _logradouroRepository, this);
        var obtido = await _repository.ObterPorId(colaborador.Id);
        Assert.NotNull(obtido);
        Assert.Equal(colaborador.Id, obtido.Id);
        Assert.Equal(NomeCompleto, obtido.Nome);
        Assert.Equal(Sobrenome, obtido.Endereco.Complemento);
        Assert.Equal(SenhaDoSgbd, obtido.Senha.Valor);
    }

    [Fact]
    public async Task Colaborador_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var resultado = await _repository.ObterPorId(999999);
        Assert.Null(resultado);
    }

    [Fact]
    public async Task Colaborador_ObterTodos_Sucesso()
    {
        await InfrastructureTestHelpers.CriarEInserirColaboradorAsync(_repository, _logradouroRepository, this);
        var resultado = await _repository.ObterTodos();
        Assert.Contains(resultado, x => x.Nome == NomeCompleto);
    }

    [Fact]
    public async Task Colaborador_Atualizar_Sucesso()
    {
        var colaborador = await InfrastructureTestHelpers.CriarEInserirColaboradorAsync(_repository, _logradouroRepository, this);
        var logradouro = colaborador.Endereco.LogradouroId;
        var novoNome = NomeCompleto + " Atualizado";
        var atualizado = AcademiaDoZe.Domain.Entities.Colaborador.Criar(colaborador.Id, novoNome, colaborador.Cpf.Valor, colaborador.DataNascimento, colaborador.Telefone.Valor, colaborador.Email.Valor, await _logradouroRepository.ObterPorId(logradouro) ?? throw new Exception(), "300", Sobrenome, SenhaDoSgbd, colaborador.Foto, colaborador.DataAdmissao, ColaboradorTipo.Administrador, ColaboradorVinculo.CLT).Value!;
        var resultado = await _repository.Atualizar(atualizado);
        Assert.Equal(novoNome, resultado.Nome);
        Assert.Equal(ColaboradorTipo.Administrador, resultado.Tipo);
    }

    [Fact]
    public async Task Colaborador_Atualizar_Inexistente_LancaExcecao()
    {
        var logradouro = await InfrastructureTestHelpers.CriarEInserirLogradouroAsync(_logradouroRepository, NomeCidade);
        var colaborador = AcademiaDoZe.Domain.Entities.Colaborador.Criar(999999, NomeCompleto, GerarCpfForTest(), new DateOnly(1990, 1, 1), GerarTelefoneForTest(), GerarEmailForTest(), logradouro, "1", Sobrenome, SenhaDoSgbd, Arquivo.Criar(new byte[] { 1 }).Value!, new DateOnly(2020, 1, 1), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).Value!;
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _repository.Atualizar(colaborador));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Colaborador_Remover_Sucesso()
    {
        var colaborador = await InfrastructureTestHelpers.CriarEInserirColaboradorAsync(_repository, _logradouroRepository, this);
        Assert.True(await _repository.Remover(colaborador.Id));
        Assert.Null(await _repository.ObterPorId(colaborador.Id));
    }

    [Fact]
    public async Task Colaborador_Remover_Inexistente_RetornaFalse()
    {
        Assert.False(await _repository.Remover(999999));
    }

    [Fact]
    public async Task Colaborador_ObterPorCpf_E_ObterPorEmail_Sucesso()
    {
        var colaborador = await InfrastructureTestHelpers.CriarEInserirColaboradorAsync(_repository, _logradouroRepository, this);
        Assert.Equal(colaborador.Id, (await _repository.ObterPorCpf(colaborador.Cpf))?.Id);
        Assert.Equal(colaborador.Id, (await _repository.ObterPorEmail(colaborador.Email))?.Id);
    }

    [Fact]
    public async Task Colaborador_CpfJaExiste_E_EmailJaExiste_ValidacaoCorreta()
    {
        var colaborador = await InfrastructureTestHelpers.CriarEInserirColaboradorAsync(_repository, _logradouroRepository, this);
        Assert.True(await _repository.CpfJaExiste(colaborador.Cpf));
        Assert.False(await _repository.CpfJaExiste(colaborador.Cpf, colaborador.Id));
        Assert.True(await _repository.EmailJaExiste(colaborador.Email));
        Assert.False(await _repository.EmailJaExiste(colaborador.Email, colaborador.Id));
    }

    [Fact]
    public async Task Colaborador_ObterPorTipo_E_ObterPorVinculo_Sucesso()
    {
        var colaborador = await InfrastructureTestHelpers.CriarEInserirColaboradorAsync(_repository, _logradouroRepository, this);
        Assert.Contains(await _repository.ObterPorTipo(colaborador.Tipo), x => x.Id == colaborador.Id);
        Assert.Contains(await _repository.ObterPorVinculo(colaborador.Vinculo), x => x.Id == colaborador.Id);
    }

    [Fact]
    public async Task Colaborador_TrocarSenha_Sucesso_E_Falha()
    {
        var colaborador = await InfrastructureTestHelpers.CriarEInserirColaboradorAsync(_repository, _logradouroRepository, this);
        var novaSenha = Senha.Criar(SenhaDoSgbd + "Nova").Value!;
        Assert.True(await _repository.TrocarSenha(colaborador.Id, novaSenha));
        Assert.Equal(novaSenha.Valor, (await _repository.ObterPorId(colaborador.Id))?.Senha.Valor);
        Assert.False(await _repository.TrocarSenha(999999, novaSenha));
    }
}
