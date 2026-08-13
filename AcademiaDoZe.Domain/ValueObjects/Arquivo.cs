// Ian Freitas de Sousa
namespace AcademiaDoZe.Domain.ValueObjects;

public record Arquivo
{
    public string Nome { get; }
    public string Extensao { get; }
    public byte[] Conteudo { get; }

    public Arquivo(string nome, string extensao, byte[] conteudo)
    {
        Nome = nome;
        Extensao = extensao;
        Conteudo = conteudo;
    }
}
