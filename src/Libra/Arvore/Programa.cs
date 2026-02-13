using Libra.Runtime; // TODO: remover

namespace Libra.Arvore
{
    public class Programa : Nodo
    {
        public Programa(Instrucao[] instrucoes)
        {
            Instrucoes = instrucoes;
            PilhaEscopos = new PilhaDeEscopos();
        }

        public int CodigoSaida { get; private set; }
        public PilhaDeEscopos PilhaEscopos { get; private set; }
        public Instrucao[] Instrucoes;

        public Variavel ObterVariavel(string identificador)
        {
            return PilhaEscopos.ObterVariavel(identificador);
        }

        public void Sair(int codigo)
        {
            CodigoSaida = codigo;
        }

        public override T Aceitar<T>(IVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }
    }
}