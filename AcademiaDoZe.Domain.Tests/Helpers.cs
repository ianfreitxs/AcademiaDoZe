// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests;

public static class Helpers
{
    public static Logradouro GetValidLogradouro(int id = 1) => Logradouro.Criar(id, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    public static Arquivo GetValidArquivo() => Arquivo.Criar([1, 2, 3]).Value!;
    public static Aluno GetValidAluno(DateOnly? nascimento = null) => Aluno.Criar(1, "João da Silva", "529.982.247-25", nascimento ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-20)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo()).Value!;
    public static Colaborador GetValidColaborador() => Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).Value!;
}
