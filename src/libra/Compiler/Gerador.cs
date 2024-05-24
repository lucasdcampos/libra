using Libra.Arvore;

public abstract class Gerador 
{
    protected string m_final = string.Empty;
    protected NodoPrograma m_programa;

    public abstract string Gerar(NodoPrograma programa);
    protected virtual void Escrever(string str) {}
}

public class GeradorC : Gerador
{
    public override string Gerar(NodoPrograma programa)
    {
        m_programa = programa;

        Escrever("#include <stdio.h>");
        Escrever("");
        Escrever("int main() {");

        // Eu deveria compilar em C o resultado final de uma expressão
        // ou compilar a própria expressão? Ex: compilar 10 + 2, ou compilar 12 direto?
        foreach(var instrucao in m_programa.Instrucoes)
        {
            var tipo = instrucao.Instrucao.GetType();

            if(tipo == typeof(NodoInstrucaoSair))
            {
                Escrever($"    exit({instrucao.Avaliar()});");
            }

            else if(tipo == typeof(NodoInstrucaoVar))
            {
                var variavel = (Var)instrucao.Avaliar();

                Escrever($"    double {variavel.Identificador} = {variavel.Valor};");
            }

            else if(tipo == typeof(NodoInstrucaoImprimir))
            {
                Escrever($"    printf({instrucao.Avaliar().ToString()});");
            }
        }

        Escrever("    return 0;");
        Escrever("}");

        return m_final;
    }

    protected override void Escrever(string str)
    {
        m_final += str + "\n";
    }
}