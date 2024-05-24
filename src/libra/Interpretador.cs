// Não sei se criarei um interpretador
// provavelmente não

public class Interpretador
{
    public Interpretador(NodoPrograma programa)
    {
        _programa = programa;
    }

    private NodoPrograma _programa;

    public void Interpretar()
    {
        foreach(var instrucao in _programa.Instrucoes)
        {
            var tipo = instrucao.Instrucao.GetType();

            if(tipo == typeof(NodoInstrucaoSair))
            {
                Environment.Exit(int.Parse(instrucao.Avaliar().ToString()));
            }

            else
            {
                
            }


        }
    }
}