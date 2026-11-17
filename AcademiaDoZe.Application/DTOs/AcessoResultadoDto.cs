// Ian Freitas de Sousa
namespace AcademiaDoZe.Application.DTOs;

public class AcessoResultadoDto
{
    public required string Titulo { get; init; }
    public required string Mensagem { get; init; }
    public required bool FoiEntrada { get; init; }
    public required DateTime DataHora { get; init; }
}
