// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Senha
{
    public string Valor { get; }

    private Senha(string valor)
    {
        Valor = valor;
    }

    public static Result<Senha> Criar(string valor)
    {
        var textoLimpo = NormalizadoService.LimparEspacos(valor);

        if (NormalizadoService.TextoVazioOuNulo(textoLimpo))
            return Result<Senha>.Failure("Senha", "SENHA_OBRIGATORIA");

        return Result<Senha>.Success(new Senha(textoLimpo));
    }

    public override string ToString() => Valor;
}
