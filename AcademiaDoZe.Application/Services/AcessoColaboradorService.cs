// Ian Freitas de Sousa
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _acessoRepoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;

    public AcessoColaboradorService(
        Func<IAcessoColaboradorRepository> acessoRepoFactory,
        Func<IColaboradorRepository> colaboradorRepoFactory)
    {
        _acessoRepoFactory = acessoRepoFactory ?? throw new ArgumentNullException(nameof(acessoRepoFactory));
        _colaboradorRepoFactory = colaboradorRepoFactory ?? throw new ArgumentNullException(nameof(colaboradorRepoFactory));
    }

    public async Task<AcessoResultadoDto> RegistrarEntradaOuSaidaAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure)
        {
            throw new ArgumentException(string.Join(", ", cpfResult.Notifications.Select(n => n.Mensagem)), nameof(cpf));
        }

        var colaborador = await _colaboradorRepoFactory().ObterPorCpf(cpfResult.Value!, cancellationToken)
            ?? throw new KeyNotFoundException("Colaborador não encontrado. Confira o CPF informado.");

        var acessoRepo = _acessoRepoFactory();
        var agora = DateTime.Now;
        var hoje = DateOnly.FromDateTime(agora);
        var historico = (await acessoRepo.ObterAcessosPorColaboradorPeriodo(colaborador.Id, null, null, cancellationToken))
            .OrderBy(a => a.DataHora)
            .ThenBy(a => a.Id)
            .ToList();
        var estaNaAcademia = historico.Count % 2 == 1;

        if (estaNaAcademia)
        {
            var ultimaEntrada = historico[^1];
            var resultadoSaida = AcessoColaborador.Criar(0, colaborador, agora);
            if (resultadoSaida.IsFailure)
            {
                throw new InvalidOperationException(string.Join(", ", resultadoSaida.Notifications.Select(n => n.Mensagem)));
            }

            await acessoRepo.Adicionar(resultadoSaida.Value!, cancellationToken);
            var permanencia = agora - ultimaEntrada.DataHora;
            var totalDia = await acessoRepo.ObterHorasTrabalhadasNoDia(colaborador.Id, hoje, cancellationToken);

            return new AcessoResultadoDto
            {
                Titulo = "Saída registrada",
                Mensagem = $"Saída registrada para {colaborador.Nome}.\nTempo desta permanência: {FormatarDuracao(permanencia)}.\nTotal trabalhado hoje: {FormatarDuracao(totalDia)}.",
                FoiEntrada = false,
                DataHora = agora
            };
        }

        var totalAntesDaEntrada = await acessoRepo.ObterHorasTrabalhadasNoDia(colaborador.Id, hoje, cancellationToken);
        var limiteHoras = colaborador.Vinculo == ColaboradorVinculo.CLT ? 8 : 6;
        var limiteDiario = TimeSpan.FromHours(limiteHoras);

        if (totalAntesDaEntrada >= limiteDiario)
        {
            throw new InvalidOperationException($"Entrada não permitida: {colaborador.Nome} já atingiu o limite diário de {limiteHoras} horas ({colaborador.Vinculo}). Total registrado hoje: {FormatarDuracao(totalAntesDaEntrada)}.");
        }

        var resultadoEntrada = AcessoColaborador.Criar(0, colaborador, agora);
        if (resultadoEntrada.IsFailure)
        {
            throw new InvalidOperationException(string.Join(", ", resultadoEntrada.Notifications.Select(n => n.Mensagem)));
        }

        await acessoRepo.Adicionar(resultadoEntrada.Value!, cancellationToken);

        return new AcessoResultadoDto
        {
            Titulo = "Entrada registrada",
            Mensagem = $"Entrada registrada para {colaborador.Nome}.\nVínculo: {colaborador.Vinculo}.\nHoras já trabalhadas hoje: {FormatarDuracao(totalAntesDaEntrada)}.\nLimite diário: {limiteHoras} horas.",
            FoiEntrada = true,
            DataHora = agora
        };
    }

    private static string FormatarDuracao(TimeSpan duracao)
    {
        if (duracao < TimeSpan.Zero)
        {
            duracao = TimeSpan.Zero;
        }

        var horas = (int)duracao.TotalHours;
        return $"{horas} hora(s) e {duracao.Minutes} minuto(s)";
    }
}
