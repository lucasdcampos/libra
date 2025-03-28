namespace Libra.Arvore
{
    public class Programa
    {
        public Programa(Instrucao[] instrucoes)
        {
            Instrucoes = instrucoes;
            PilhaEscopos = new PilhaDeEscopos();
        }

        public string Saida { get; internal set; }
        public int CodigoSaida { get; private set; }
        public PilhaDeEscopos PilhaEscopos { get; private set;}
        public Instrucao[] Instrucoes;

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