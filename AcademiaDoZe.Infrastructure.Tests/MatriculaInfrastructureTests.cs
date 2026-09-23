// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public sealed class MatriculaInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepository;
    private readonly AlunoRepository _alunoRepository;
    private readonly MatriculaRepository _repository;

    public MatriculaInfrastructureTests()
    {
        _logradouroRepository = new LogradouroRepository(ConnectionString, DatabaseType);
        _alunoRepository = new AlunoRepository(ConnectionString, DatabaseType);
        _repository = new MatriculaRepository(ConnectionString, DatabaseType);
    }

    private async Task<Matricula> CriarEInserirMatriculaAsync(
        Aluno aluno,
        MatriculaPlano plano = MatriculaPlano.Mensal,
        DateOnly? dataInicio = null,
        MatriculaRestricoes restricoes = MatriculaRestricoes.None,
        string? observacoes = null,
        Arquivo? laudo = null)
    {
        var inicio = dataInicio ?? DateOnly.FromDateTime(DateTime.Today);

        if (restricoes != MatriculaRestricoes.None && laudo is null)
            laudo = Arquivo.Criar(new byte[] { 1, 2, 3, 4 }).Value!;

        var result = Matricula.Criar(
            0,
            aluno,
            plano,
            inicio,
            NomeCompleto,
            restricoes,
            laudo,
            observacoes ?? NomeCidade);

        if (result.IsFailure)
            throw new InvalidOperationException(
                string.Join(", ", result.Notifications.Select(n => n.Mensagem)));

        return await _repository.Adicionar(result.Value!);
    }

    [Fact]
    public async Task Matricula_Adicionar_E_ObterPorId_Sucesso()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        var inserida = await CriarEInserirMatriculaAsync(
            aluno,
            MatriculaPlano.Mensal,
            restricoes: MatriculaRestricoes.Diabetes | MatriculaRestricoes.PressaoAlta,
            observacoes: NomeCidade,
            laudo: Arquivo.Criar(new byte[] { 100, 101, 102 }).Value!);

        var obtida = await _repository.ObterPorId(inserida.Id);

        Assert.NotNull(obtida);
        Assert.Equal(inserida.Id, obtida.Id);
        Assert.Equal(aluno.Id, obtida.AlunoId);
        Assert.Equal(MatriculaPlano.Mensal, obtida.Plano);
        Assert.Equal(NomeCompleto, obtida.Objetivo);
        Assert.Equal(NomeCidade, obtida.ObservacoesRestricoes);
        Assert.Equal(inserida.RestricoesMedicas, obtida.RestricoesMedicas);
        Assert.NotNull(obtida.LaudoMedico);
        Assert.Equal(inserida.LaudoMedico!.Conteudo, obtida.LaudoMedico!.Conteudo);
    }

    [Fact]
    public async Task Matricula_RestricoesMedicas_ComVariacoesMultiplaEscolha_PersisteEObtemCorretamente()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        var laudo = Arquivo.Criar(new byte[] { 10, 20, 30, 40, 50 }).Value!;

        var restricoes =
            MatriculaRestricoes.PressaoAlta |
            MatriculaRestricoes.Labirintite |
            MatriculaRestricoes.ProblemasRespiratorios |
            MatriculaRestricoes.RemedioContinuo;

        var matricula = await CriarEInserirMatriculaAsync(
            aluno,
            MatriculaPlano.Semestral,
            restricoes: restricoes,
            observacoes: NomeCidade,
            laudo: laudo);

        var obtida = await _repository.ObterPorId(matricula.Id);

        Assert.NotNull(obtida);
        Assert.Equal(restricoes, obtida.RestricoesMedicas);
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.PressaoAlta));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Labirintite));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.ProblemasRespiratorios));
        Assert.True(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.RemedioContinuo));
        Assert.False(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Diabetes));
        Assert.False(obtida.RestricoesMedicas.HasFlag(MatriculaRestricoes.Alergias));
        Assert.Equal(NomeCidade, obtida.ObservacoesRestricoes);
        Assert.NotNull(obtida.LaudoMedico);
        Assert.Equal(laudo.Conteudo, obtida.LaudoMedico!.Conteudo);
    }

    [Fact]
    public async Task Matricula_ObterPorId_RetornaNuloQuandoInexistente()
    {
        Assert.Null(await _repository.ObterPorId(999999));
    }

    [Fact]
    public async Task Matricula_ObterTodos_Sucesso()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        var inserida = await CriarEInserirMatriculaAsync(aluno);
        var todas = await _repository.ObterTodos();

        Assert.Contains(todas, x => x.Id == inserida.Id);
    }

    [Fact]
    public async Task Matricula_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        var result = Matricula.Criar(
            999999,
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today),
            NomeCompleto,
            MatriculaRestricoes.None,
            null,
            NomeCidade);

        var ex = await Assert.ThrowsAsync<InfrastructureException>(
            () => _repository.Atualizar(result.Value!));

        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }

    [Fact]
    public async Task Matricula_Atualizar_Sucesso()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        var inserida = await CriarEInserirMatriculaAsync(
            aluno,
            MatriculaPlano.Mensal,
            restricoes: MatriculaRestricoes.Alergias);

        var restricoes =
            MatriculaRestricoes.Alergias |
            MatriculaRestricoes.Diabetes |
            MatriculaRestricoes.Labirintite;

        var laudo = Arquivo.Criar(new byte[] { 99, 88, 77 }).Value!;

        var atualizado = Matricula.Criar(
            inserida.Id,
            aluno,
            MatriculaPlano.Anual,
            inserida.DataInicio,
            NomeCompleto,
            restricoes,
            laudo,
            NomeCidade).Value!;

        var resultado = await _repository.Atualizar(atualizado);
        var noBanco = await _repository.ObterPorId(inserida.Id);

        Assert.Equal(MatriculaPlano.Anual, resultado.Plano);
        Assert.Equal(NomeCompleto, resultado.Objetivo);
        Assert.Equal(restricoes, resultado.RestricoesMedicas);
        Assert.NotNull(noBanco);
        Assert.Equal(MatriculaPlano.Anual, noBanco.Plano);
        Assert.Equal(NomeCompleto, noBanco.Objetivo);
        Assert.Equal(restricoes, noBanco.RestricoesMedicas);
    }

    [Fact]
    public async Task Matricula_Remover_Sucesso()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        var inserida = await CriarEInserirMatriculaAsync(aluno);

        Assert.True(await _repository.Remover(inserida.Id));
        Assert.Null(await _repository.ObterPorId(inserida.Id));
    }

    [Fact]
    public async Task Matricula_Remover_RetornaFalseQuandoInexistente()
    {
        Assert.False(await _repository.Remover(999999));
    }

    [Fact]
    public async Task Matricula_ObterPorAluno_FiltragemCorreta()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        await CriarEInserirMatriculaAsync(aluno);

        var matriculas = await _repository.ObterPorAluno(aluno.Id);

        Assert.NotEmpty(matriculas);
        Assert.All(
            matriculas,
            matricula => Assert.Equal(aluno.Id, matricula.AlunoId));
    }

    [Fact]
    public async Task Matricula_ObterMatriculaAtivaPorAluno_E_PossuiMatriculaAtiva()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        Assert.False(await _repository.PossuiMatriculaAtiva(aluno.Id));

        await CriarEInserirMatriculaAsync(
            aluno,
            MatriculaPlano.Mensal,
            DateOnly.FromDateTime(DateTime.Today));

        Assert.True(await _repository.PossuiMatriculaAtiva(aluno.Id));

        var ativa = await _repository.ObterMatriculaAtivaPorAluno(aluno.Id);

        Assert.NotNull(ativa);
        Assert.Equal(aluno.Id, ativa.AlunoId);
    }

    [Fact]
    public async Task Matricula_ObterAtivas_FiltragemCorreta()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        await CriarEInserirMatriculaAsync(
            aluno,
            MatriculaPlano.Semestral,
            DateOnly.FromDateTime(DateTime.Today));

        var ativasGeral = await _repository.ObterAtivas();
        var ativasPorAluno = await _repository.ObterAtivas(aluno.Id);

        Assert.NotEmpty(ativasGeral);
        Assert.NotEmpty(ativasPorAluno);

        Assert.All(
            ativasPorAluno,
            matricula => Assert.Equal(aluno.Id, matricula.AlunoId));
    }

    [Fact]
    public async Task Matricula_ObterVencendoEmDias_RetornaMatriculasProximasDoVencimento()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        var inicio = DateOnly.FromDateTime(DateTime.Today.AddDays(-25));

        var inserida = await CriarEInserirMatriculaAsync(
            aluno,
            MatriculaPlano.Mensal,
            inicio);

        var vencendo = await _repository.ObterVencendoEmDias(30);

        Assert.Contains(
            vencendo,
            matricula => matricula.Id == inserida.Id);
    }

    [Fact]
    public async Task Matricula_ObterPorPlano_FiltragemCorreta()
    {
        var aluno = await InfrastructureTestHelpers.CriarEInserirAlunoAsync(
            _alunoRepository,
            _logradouroRepository,
            this);

        var inserida = await CriarEInserirMatriculaAsync(
            aluno,
            MatriculaPlano.Trimestral);

        var trimestrais = await _repository.ObterPorPlano(
            MatriculaPlano.Trimestral);

        Assert.Contains(
            trimestrais,
            matricula =>
                matricula.Id == inserida.Id &&
                matricula.Plano == MatriculaPlano.Trimestral);
    }
}