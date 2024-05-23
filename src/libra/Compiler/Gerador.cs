using System;

public abstract class Gerador 
{
    protected string _final = string.Empty;
    protected NodoPrograma _programa;

    public Gerador(NodoPrograma programa)
    {
        _programa = programa;
    }

    public abstract string Gerar();
    protected virtual void Escrever(string str) {}
}

public class GeradorC : Gerador
{
    public GeradorC(NodoPrograma programa) : base(programa){}

    public override string Gerar()
    {
        _final += "int main() {\n";

        foreach(var instrucao in _programa.Instrucoes)
        {
            var tipo = instrucao.Instrucao.GetType();

            if(tipo == typeof(NodoInstrucaoSair))
            {
                Escrever($"    exit({instrucao.Avaliar()});\n");
            }
        }

        Escrever("    return 0;");
        Escrever("}\n");

        return _final;
    }

    protected override void Escrever(string str)
    {
        _final += str + "\n";
    }
}