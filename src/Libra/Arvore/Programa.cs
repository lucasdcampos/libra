namespace Libra.Arvore
{
    public class Programa
    {
        public Programa(Instrucao[] instrucoes)
        {
            Instrucoes = instrucoes;
            Funcoes = new Dictionary<string, Funcao>();
            Classes = new Dictionary<string, Classe>();
            PilhaEscopos = new PilhaDeEscopos();
        }

        public string Saida { get; internal set; }
        public int CodigoSaida { get; private set; }
        public PilhaDeEscopos PilhaEscopos { get; private set;}
        public Instrucao[] Instrucoes;
        public Dictionary<string, Funcao> Funcoes {get; private set; }
        public Dictionary<string, Classe> Classes {get; private set; }

        public bool FuncaoExiste(string ident)
        {
            return Funcoes.ContainsKey(ident);
        }

        public bool ClasseExiste(string ident)
        {
            return Classes.ContainsKey(ident);
        }

        public Variavel ObterVariavel(string identificador)
        {
            return PilhaEscopos.ObterVariavel(identificador);
        }

        public void Sair(int codigo)
        {
            CodigoSaida = codigo;
        }
    }
}