// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public static class InfrastructureTestHelpers
{
    public static async Task<Logradouro> CriarEInserirLogradouroAsync(LogradouroRepository repository, string cidade)
    {
        var result = Logradouro.Criar(0, TestBase.GerarCep(), "Ian Freitas de Sousa", "Sousa", cidade, "SC", "Brasil");
        if (result.IsFailure)
            throw new InvalidOperationException(string.Join(", ", result.Notifications.Select(n => n.Mensagem)));

        return await repository.Adicionar(result.Value!);
    }

    public static async Task<Colaborador> CriarEInserirColaboradorAsync(ColaboradorRepository repository, LogradouroRepository logradouroRepository, TestBase baseTeste)
    {
        var logradouro = await CriarEInserirLogradouroAsync(logradouroRepository, baseTeste.NomeCidade);

        var result = Colaborador.Criar(
            0,
            baseTeste.NomeCompleto,
            baseTeste.GerarCpfForTest(),
            new DateOnly(1995, 5, 15),
            baseTeste.GerarTelefoneForTest(),
            baseTeste.GerarEmailForTest(),
            logradouro,
            "200",
            baseTeste.Sobrenome,
            baseTeste.SenhaDoSgbd,
            Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!,
            new DateOnly(2023, 1, 1),
            ColaboradorTipo.Instrutor,
            ColaboradorVinculo.CLT);

        if (result.IsFailure)
            throw new InvalidOperationException(string.Join(", ", result.Notifications.Select(n => n.Mensagem)));

        return await repository.Adicionar(result.Value!);
    }

    public static async Task<Aluno> CriarEInserirAlunoAsync(AlunoRepository repository, LogradouroRepository logradouroRepository, TestBase baseTeste)
    {
        var logradouro = await CriarEInserirLogradouroAsync(logradouroRepository, baseTeste.NomeCidade);

        var result = Aluno.Criar(
            0,
            baseTeste.NomeCompleto,
            baseTeste.GerarCpfForTest(),
            new DateOnly(1998, 3, 20),
            baseTeste.GerarTelefoneForTest(),
            baseTeste.GerarEmailForTest(),
            logradouro,
            "100",
            baseTeste.Sobrenome,
            baseTeste.SenhaDoSgbd,
            Arquivo.Criar(new byte[] { 4, 5, 6 }).Value!);

        if (result.IsFailure)
            throw new InvalidOperationException(string.Join(", ", result.Notifications.Select(n => n.Mensagem)));

        return await repository.Adicionar(result.Value!);
    }
}