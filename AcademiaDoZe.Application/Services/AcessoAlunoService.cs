// Ian Freitas de Sousa
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _acessoRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    private readonly IMatriculaService _matriculaService;

    public AcessoAlunoService(
        Func<IAcessoAlunoRepository> acessoRepoFactory,
        Func<IAlunoRepository> alunoRepoFactory,
        IMatriculaService matriculaService)
    {
        _acessoRepoFactory = acessoRepoFactory ?? throw new ArgumentNullException(nameof(acessoRepoFactory));
        _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
        _matriculaService = matriculaService ?? throw new ArgumentNullException(nameof(matriculaService));
    }

    public async Task<AcessoResultadoDto> RegistrarEntradaOuSaidaAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cpfResult = Cpf.Criar(cpf);
        if (cpfResult.IsFailure)
        {
            throw new ArgumentException(string.Join(", ", cpfResult.Notifications.Select(n => n.Mensagem)), nameof(cpf));
        }

        var aluno = await _alunoRepoFactory().ObterPorCpf(cpfResult.Value!, cancellationToken)
            ?? throw new KeyNotFoundException("Aluno não encontrado. Confira o CPF informado.");

        var acessoRepo = _acessoRepoFactory();
        var agora = DateTime.Now;
        var estaNaAcademia = await acessoRepo.EstaNaAcademia(aluno.Id, cancellationToken);

        if (estaNaAcademia)
        {
            var ultimoAcesso = await acessoRepo.ObterUltimoAcesso(aluno.Id, cancellationToken)
                ?? throw new InvalidOperationException("Não foi possível localizar a última entrada deste aluno.");

            var resultadoSaida = AcessoAluno.Criar(0, aluno, agora);
            if (resultadoSaida.IsFailure)
            {
                throw new InvalidOperationException(string.Join(", ", resultadoSaida.Notifications.Select(n => n.Mensagem)));
            }

            await acessoRepo.Adicionar(resultadoSaida.Value!, cancellationToken);
            var permanencia = agora - ultimoAcesso.DataHora;

            return new AcessoResultadoDto
            {
                Titulo = "Saída registrada",
                Mensagem = $"Saída registrada para {aluno.Nome}.\nPermanência nesta visita: {FormatarDuracao(permanencia)}.",
                FoiEntrada = false,
                DataHora = agora
            };
        }

        var matricula = await _matriculaService.ObterMatriculaAtivaPorAlunoAsync(aluno.Id, cancellationToken);
        if (matricula == null)
        {
            throw new InvalidOperationException("Entrada não permitida: este aluno não possui matrícula ativa.");
        }

        var resultadoEntrada = AcessoAluno.Criar(0, aluno, agora);
        if (resultadoEntrada.IsFailure)
        {
            throw new InvalidOperationException(string.Join(", ", resultadoEntrada.Notifications.Select(n => n.Mensagem)));
        }

        await acessoRepo.Adicionar(resultadoEntrada.Value!, cancellationToken);

        var fimDoPlano = matricula.DataFim.ToDateTime(new TimeOnly(23, 59, 59));
        var tempoRestante = fimDoPlano > agora ? fimDoPlano - agora : TimeSpan.Zero;

        return new AcessoResultadoDto
        {
            Titulo = "Entrada registrada",
            Mensagem = $"Entrada registrada para {aluno.Nome}.\nTempo restante do plano: {FormatarDuracao(tempoRestante)}.",
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

        var dias = (int)duracao.TotalDays;
        var horas = duracao.Hours;
        var minutos = duracao.Minutes;

        if (dias > 0)
        {
            return $"{dias} dia(s), {horas} hora(s) e {minutos} minuto(s)";
        }

        return $"{horas} hora(s) e {minutos} minuto(s)";
    }
}
