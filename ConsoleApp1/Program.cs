using System;
using Simplificacao_Normalizacao;

public class Program {
    public static void Main(string[] args) {
        // Gramática de exemplo para teste
        string definicaoGramatica = @"
S -> AB | a
A -> aA | ε
B -> bB | A
C -> c
";

        Console.WriteLine("--- Gramática Original ---");
        var gramatica = Gramatica.FromString(definicaoGramatica);
        Console.WriteLine(gramatica);

        Console.WriteLine("\n--- Etapa 1: Removendo Produções Vazias ---");
        gramatica.RemoverProducoesVazias();
        Console.WriteLine(gramatica);

        Console.WriteLine("\n--- Etapa 2: Removendo Produções Unitárias ---");
        gramatica.RemoverProducoesUnitarias();
        Console.WriteLine(gramatica);

        Console.WriteLine("\n--- Etapa 3: Removendo Símbolos Inúteis ---");
        gramatica.RemoverSimbolosInuteis();
        Console.WriteLine(gramatica);

        Console.WriteLine("\n--- Gramática Simplificada Final ---");
        Console.WriteLine(gramatica);
    }
}