using System;
using Simplificacao_Normalizacao;

public class Program
{
    public static void Main(string[] args)
    {
        string definicaoGramatica = @"
S -> A a | B
A -> A c | S d | b
B -> a
";

        Console.WriteLine("--- Gramática Original ---");
        var gramatica = Gramatica.FromString(definicaoGramatica);
        Console.WriteLine(gramatica);

        // --- ETAPA 1: SIMPLIFICAÇÃO COMPLETA ---
        Console.WriteLine("\n--- Etapa 1: Simplificação da Gramática ---");
        gramatica.Simplificar();
        Console.WriteLine("--- Gramática Simplificada ---");
        Console.WriteLine(gramatica);

        // --- ETAPA 2: MELHORIAS PARA PARSERS ---
        Console.WriteLine("\n--- Etapa 2: Removendo Recursão à Esquerda ---");
        gramatica.RemoverRecursaoEsquerda();
        gramatica.RemoverProducoesVazias(); // Limpa épsilons introduzidos pela remoção de recursão
        gramatica.RemoverProducoesUnitarias(); // Limpa produções unitárias introduzidas
        Console.WriteLine(gramatica);

        Console.WriteLine("\n--- Etapa 3: Fatorando à Esquerda ---");
        gramatica.FatorarEsquerda();
        gramatica.RemoverProducoesVazias(); // Limpa épsilons introduzidos pela fatoração
        Console.WriteLine("--- Gramática Final Pronta para Análise Preditiva ---");
        Console.WriteLine(gramatica);
        
        // --- ETAPA 3: FORMA NORMAL DE CHOMSKY ---
        Console.WriteLine("\n\n--- CONVERSÃO PARA FORMA NORMAL DE CHOMSKY ---");
        Console.WriteLine("--- (Partindo da Gramática Simplificada) ---");
        var gramaticaParaFNC = Gramatica.FromString(definicaoGramatica);
        gramaticaParaFNC.Simplificar();
        Console.WriteLine(gramaticaParaFNC);
        
        Console.WriteLine("\n--- Convertendo para FNC ---");
        gramaticaParaFNC.ConverterParaChomsky();
        Console.WriteLine("--- Gramática Final na FNC ---");
        Console.WriteLine(gramaticaParaFNC);
    }
}