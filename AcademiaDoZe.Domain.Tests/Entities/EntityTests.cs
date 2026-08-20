// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Exceptions;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class EntityTests
{
    [Fact]
    public void Deve_Criar_Entidade_ComIdValido()
    {
        var result = Helpers.GetValidLogradouro(5);
        Assert.Equal(5, result.Id);
    }

    [Fact]
    public void Deve_Falhar_Entidade_ComIdNegativo()
    {
        Assert.Throws<DomainException>(() => AcademiaDoZe.Domain.Entities.Logradouro.Criar(-1, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil"));
    }
}
