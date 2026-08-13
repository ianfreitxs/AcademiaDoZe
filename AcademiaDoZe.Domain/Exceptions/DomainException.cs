// Ian Freitas de Sousa
namespace AcademiaDoZe.Domain.Exceptions;

public sealed class DomainException(string message) : Exception(message);
