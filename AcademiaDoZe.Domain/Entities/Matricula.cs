// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Entities;

public sealed class Matricula : Entity
{
    public Aluno AlunoMatricula { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public string Objetivo { get; private set; }
    public MatriculaRestricoes RestricoesMedicas { get; private set; }
    public string ObservacoesRestricoes { get; private set; }
    public Arquivo? LaudoMedico { get; private set; }

    private Matricula(int id, Aluno alunoMatricula, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFim,
        string objetivo, MatriculaRestricoes restricoesMedicas, Arquivo? laudoMedico,
        string observacoesRestricoes)
        : base(id)
    {
        AlunoMatricula = alunoMatricula;
        Plano = plano;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Objetivo = objetivo;
        RestricoesMedicas = restricoesMedicas;
        LaudoMedico = laudoMedico;
        ObservacoesRestricoes = observacoesRestricoes;
    }

    public static Result<Matricula> Criar(int id, Aluno alunoMatricula, MatriculaPlano plano,
        DateOnly dataInicio, DateOnly dataFim, string objetivo, MatriculaRestricoes restricoesMedicas,
        Arquivo? laudoMedico, string observacoesRestricoes = "")
    {
        var notifications = new List<Notification>();

        if (alunoMatricula is null)
            notifications.Add(new Notification("AlunoMatricula", "ALUNO_OBRIGATORIO"));

        if (!Enum.IsDefined(plano))
            notifications.Add(new Notification("Plano", "PLANO_MATRICULA_INVALIDO"));

        if (dataInicio == default)
            notifications.Add(new Notification("DataInicio", "DATA_INICIO_OBRIGATORIO"));

        if (dataFim == default)
            notifications.Add(new Notification("DataFim", "DATA_FIM_OBRIGATORIO"));
        else if (dataInicio != default && dataFim < dataInicio)
            notifications.Add(new Notification("DataFim", "DATA_FIM_INVALIDA"));

        if (NormalizadoService.TextoVazioOuNulo(objetivo))
            notifications.Add(new Notification("Objetivo", "OBJETIVO_OBRIGATORIO"));
        else
            objetivo = NormalizadoService.LimparEspacos(objetivo);

        if (!Enum.IsDefined(restricoesMedicas))
            notifications.Add(new Notification("RestricoesMedicas", "RESTRICOES_INVALIDAS"));

        observacoesRestricoes = NormalizadoService.LimparEspacos(observacoesRestricoes);

        if (notifications.Count != 0)
            return Result<Matricula>.Failure(notifications);

        return Result<Matricula>.Success(
            new Matricula(id, alunoMatricula!, plano, dataInicio, dataFim, objetivo,
                restricoesMedicas, laudoMedico, observacoesRestricoes));
    }
}
