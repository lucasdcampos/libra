namespace Libra.Arvore
{
    public abstract class Nodo { }

    public class NodoPrograma : Nodo
    {
        public NodoPrograma(NodoEscopo escopo)
        {
            Escopo = escopo;
            Variaveis = new Dictionary<string, Variavel>();
            Funcoes = new Dictionary<string, Funcao>();
        }

        public NodoEscopo Escopo { get; private set; }
        public int CodigoSaida { get; private set; }
        public Dictionary<string, Variavel> Variaveis {get; private set; }
        public Dictionary<string, Funcao> Funcoes {get; private set; }

        public bool VariavelExiste(string ident)
        {
            return Variaveis.ContainsKey(ident);
        }
        public bool FuncaoExiste(string ident)
        {
            return Funcoes.ContainsKey(ident);
        }
    }

    public class NodoEscopo
    {
        public NodoEscopo(List<NodoInstrucao> instrucoes = null, NodoEscopo pai = null)
        {
            Variaveis = new Dictionary<string, Variavel>();
            Instrucoes = instrucoes;

            if(instrucoes == null)
            {
                Instrucoes = new List<NodoInstrucao>();
            }

            Pai = pai;
        }

        public Dictionary<string, Variavel> Variaveis {get; private set; }
        public List<NodoInstrucao> Instrucoes { get; internal set; }
        public NodoEscopo Pai { get; private set; }

        public bool VariavelExiste(string ident)
        {
            NodoEscopo escopoAtual = this;

            while(escopoAtual.Pai != null)
            {
                if(escopoAtual.Variaveis.ContainsKey(ident))
                {
                    return true;
                }

                escopoAtual = escopoAtual.Pai;
            }

            return false;
            
        }
    }
}