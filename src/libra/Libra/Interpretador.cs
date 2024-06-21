using Libra.Arvore;
using System;

namespace Libra;

public class Interpretador
{
    public void Interpretar(NodoPrograma programa)
    {
        InterpretarInstrucoes(programa.Instrucoes);
    }

    private void InterpretarInstrucoes(List<NodoInstrucao> instrucoes)
    {
        foreach (var instrucao in instrucoes)
        {
            var tipo = instrucao.GetType();

            if (tipo == typeof(NodoInstrucaoSair))
            {
                Environment.Exit(int.Parse(instrucao.Avaliar().ToString()));
            }

            else if (tipo == typeof(NodoInstrucaoVar))
            {
                // A variável já está sendo definida no Parser (É, eu deveria defini-la aqui, mas whatever)
            }

            else if (tipo == typeof(NodoInstrucaoExibir))
            {
                Console.WriteLine(instrucao.Avaliar().ToString());
            }

            else if (tipo == typeof(NodoInstrucaoTipo))
            {
                Console.WriteLine(instrucao.Avaliar().ToString());
            }

            else if (tipo == typeof(NodoInstrucaoSe))
            {
                var se = (NodoInstrucaoSe)instrucao;
                var resultado = instrucao.Avaliar().ToString();
                
                if (resultado == "True")
                {
                    InterpretarInstrucoes(se.Escopo);
                }

            }
        }
    }

}