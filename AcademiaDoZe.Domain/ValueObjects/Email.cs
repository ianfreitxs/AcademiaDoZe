// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }

    private Email(string valor)
    {
        Valor = valor;
    }

    public static Result<Email> Criar(string? valor)
    {
        var textoLimpo = NormalizacaoService.LimparEspacos(valor);

        if (string.IsNullOrWhiteSpace(textoLimpo) || !ValidarFormato(textoLimpo))
            return Result<Email>.Failure("Email", "EMAIL_FORMATO");

        return Result<Email>.Success(new Email(textoLimpo));
    }

    private static bool ValidarFormato(string email)
    {
        var partes = email.Split('@');

        if (partes.Length != 2)
            return false;

        var usuario = partes[0];
        var dominio = partes[1];

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(dominio))
            return false;

        if (usuario.StartsWith('.') || usuario.EndsWith('.') || usuario.Contains(".."))
            return false;

        if (dominio.StartsWith('.') || dominio.EndsWith('.') || dominio.Contains(".."))
            return false;

        var labels = dominio.Split('.');

        if (labels.Length < 2)
            return false;

        return labels.All(l => !string.IsNullOrWhiteSpace(l));
    }

    public override string ToString() => Valor;
}