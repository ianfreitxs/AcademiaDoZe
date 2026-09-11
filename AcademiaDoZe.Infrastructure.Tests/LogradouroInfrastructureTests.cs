// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _repository;

    public LogradouroInfrastructureTests()
    {
        _repository = new LogradouroRepository(ConnectionString, DatabaseType);
    }

    private Logradouro CriarLogradouro(string? cep = null)
    {
        var result = Logradouro.Criar(0, cep ?? GerarCep(), "Ian Freitas de Sousa", "Sousa", NomeCidade, "SC", "Brasil");
        Assert.True(result.IsSuccess);
        return result.Value!;
    }

    [Fact]
    public async Task Logradouro_Adicionar_E_ObterPorId_Sucesso()
    {
        var logradouro = CriarLogradouro();
        var inserido = await _repository.Adicionar(logradouro);
        Assert.True(inserido.Id > 0);
        var obtido = await _repository.ObterPorId(inserido.Id);
        Assert.NotNull(obtido);
        Assert.Equal(inserido.Id, obtido.Id);
        Assert.Equal("Ian Freitas de Sousa", obtido.Nome);
        Assert.Equal("Sousa", obtido.Bairro);
        Assert.Equal(NomeCidade, obtido.Cidade);
    }

    [Fact]
    public async Task Logradouro_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _repository.ObterPorId(999999);
        Assert.Null(obtido);
    }

    [Fact]
    public async Task Logradouro_ObterTodos_Sucesso()
    {
        await _repository.Adicionar(CriarLogradouro());
        var todos = await _repository.ObterTodos();
        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    [Fact]
    public async Task Logradouro_Atualizar_Sucesso()
    {
        var logradouro = await _repository.Adicionar(CriarLogradouro());
        var atualizado = Logradouro.Criar(logradouro.Id, GerarCep(), "Ian Freitas de Sousa", "Sousa", NomeCidade, "SC", "Brasil").Value!;
        var resultado = await _repository.Atualizar(atualizado);
        Assert.Equal(atualizado.Cep.Valor, resultado.Cep.Valor);
        var noBanco = await _repository.ObterPorId(logradouro.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(atualizado.Cep.Valor, noBanco.Cep.Valor);
    }

    [Fact]
    public async Task Logradouro_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = Logradouro.Criar(999999, GerarCep(), "Ian Freitas de Sousa", "Sousa", NomeCidade, "SC", "Brasil").Value!;
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _repository.Atualizar(logradouro));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Logradouro_Remover_Sucesso()
    {
        var logradouro = await _repository.Adicionar(CriarLogradouro());
        var removido = await _repository.Remover(logradouro.Id);
        Assert.True(removido);
        var noBanco = await _repository.ObterPorId(logradouro.Id);
        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Logradouro_Remover_RetornaFalseQuandoInexistente()
    {
        var removido = await _repository.Remover(999999);
        Assert.False(removido);
    }

    [Fact]
    public async Task Logradouro_ObterPorCep_E_CepJaExiste_Sucesso()
    {
        var logradouro = await _repository.Adicionar(CriarLogradouro());
        var obtido = await _repository.ObterPorCep(logradouro.Cep);
        Assert.NotNull(obtido);
        Assert.Equal(logradouro.Id, obtido.Id);
        Assert.True(await _repository.CepJaExiste(logradouro.Cep));
        Assert.False(await _repository.CepJaExiste(logradouro.Cep, logradouro.Id));
    }

    [Fact]
    public async Task Logradouro_ObterPorCidade_E_ObterPorBairro_Sucesso()
    {
        var bairro = $"Sousa_{Guid.NewGuid():N}";
        var logradouro = Logradouro.Criar(0, GerarCep(), "Ian Freitas de Sousa", bairro, NomeCidade, "SC", "Brasil").Value!;
        await _repository.Adicionar(logradouro);
        var porCidade = await _repository.ObterPorCidade(NomeCidade);
        Assert.Contains(porCidade, x => x.Id == logradouro.Id);
        var porBairro = await _repository.ObterPorBairro(NomeCidade, bairro);
        Assert.Single(porBairro);
        Assert.Equal(logradouro.Id, porBairro.First().Id);
    }
}
