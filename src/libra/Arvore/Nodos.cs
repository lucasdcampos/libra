namespace Libra.Arvore
{
    public class Programa
    {
        public Programa(List<Instrucao> instrucoes)
        {
            Instrucoes = instrucoes;
            Variaveis = new Dictionary<string, Variavel>();
            Funcoes = new Dictionary<string, Funcao>();
        }

        public int CodigoSaida { get; private set; }
        public List<Instrucao> Instrucoes;
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
}