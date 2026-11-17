// Ian Freitas de Sousa
using AcademiaDoZe.Application.DTOs;

namespace AcademiaDoZe.Application.Interfaces;

public interface IAcessoColaboradorService
{
    Task<AcessoResultadoDto> RegistrarEntradaOuSaidaAsync(string cpf, CancellationToken cancellationToken = default);
}
