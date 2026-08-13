// Ian Freitas de Sousa
namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Endereco { get; }

    public Email(string endereco)
    {
        Endereco = endereco;
    }
}
