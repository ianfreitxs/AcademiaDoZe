// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;

namespace AcademiaDoZe.Domain.Tests.Common;

public class RepositoryContractTests
{
    [Fact]
    public void Deve_Exigir_AggregateRoot_Nos_Repositorios()
    {
        var type = typeof(IRepository<>);
        Assert.True(type.IsGenericTypeDefinition);
        var parameter = type.GetGenericArguments().Single();
        Assert.Contains(typeof(IAggregateRoot), parameter.GetGenericParameterConstraints());
    }

    [Theory]
    [InlineData(typeof(ILogradouroRepository), typeof(Logradouro))]
    [InlineData(typeof(IAlunoRepository), typeof(Aluno))]
    [InlineData(typeof(IColaboradorRepository), typeof(Colaborador))]
    [InlineData(typeof(IMatriculaRepository), typeof(Matricula))]
    [InlineData(typeof(IAcessoAlunoRepository), typeof(AcessoAluno))]
    [InlineData(typeof(IAcessoColaboradorRepository), typeof(AcessoColaborador))]
    public void Deve_Ter_Repositorio_Para_AggregateRoot(Type repositoryType, Type entityType)
    {
        Assert.True(repositoryType.IsInterface);
        Assert.True(typeof(IAggregateRoot).IsAssignableFrom(entityType));
    }

    [Theory]
    [InlineData(typeof(Logradouro))]
    [InlineData(typeof(Aluno))]
    [InlineData(typeof(Colaborador))]
    [InlineData(typeof(Matricula))]
    [InlineData(typeof(AcessoAluno))]
    [InlineData(typeof(AcessoColaborador))]
    public void Deve_Marcar_AgregadoRaiz(Type type)
    {
        Assert.True(typeof(IAggregateRoot).IsAssignableFrom(type));
    }
}
