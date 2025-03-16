using Libra.Arvore;

namespace Libra
{
    public class Funcao
    {
            public string Identificador;
            public Instrucao[] Instrucoes;
            public List<string> Parametros;

            public Funcao(string ident, Instrucao[] instrucoes, List<string> parametros)
            {
                Identificador = ident;
                Instrucoes = instrucoes;
                Parametros = parametros;
            }
    }

    // Definidas em Modulos/
    public class FuncaoNativa : Funcao
    {
        private readonly Func<object[], object> _implementacao;

        public FuncaoNativa(Func<object[], object> implementacao) : base("", null, null)
        {
            _implementacao = implementacao;
        }

        public object Executar(params object[] argumentos)
        {
            return _implementacao(argumentos);
        }
    }
}