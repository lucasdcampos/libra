using Libra.Arvore;

public class Interpretador
{
    public void Interpretar(NodoPrograma programa)
    {
        foreach(var instrucao in programa.Instrucoes)
        {
            if(instrucao == null)
                Erro.ErroGenerico("Instrução inválida!");

            if(instrucao.GetType() == typeof(NodoInstrucaoExibir))
            {
                InterpretarInstrucaoExibir((NodoInstrucaoExibir)instrucao);
            }

            else if(instrucao.GetType() == typeof(NodoInstrucaoExibir))
            {
                InterpretarInstrucaoExibir((NodoInstrucaoExibir)instrucao);
            }

        }
    }

    private void InterpretarInstrucaoExibir(NodoInstrucaoExibir exibir)
    {
        Console.WriteLine(exibir.Avaliar());
    }

}