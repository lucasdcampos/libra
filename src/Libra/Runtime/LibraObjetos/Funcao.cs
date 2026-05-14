using Libra.Arvore;

namespace Libra.Runtime
{
    public class Funcao : LibraObjeto, IChamavel
    {
        public Instrucao[] Instrucoes;
        public Parametro[] Parametros;
        public string TipoRetorno;
        public string Identificador { get; }
        public Ambiente? AmbienteDefinicao;
        public Funcao(string ident, Instrucao[] instrucoes, Parametro[] parametros, string tipoRetorno = "Objeto", Ambiente? ambiente = null) : base("Func", new Variavel[0])
        {
            Instrucoes = instrucoes;
            Parametros = parametros;
            TipoRetorno = tipoRetorno;
            Identificador = ident;
            AmbienteDefinicao = ambiente;
        }

        public override LibraInt Igual(LibraObjeto outro)
        {
           return new LibraInt(0);
        }

    }

    // Definidas em Modulos/
    public class FuncaoNativa : Funcao
    {
        private readonly Func<object[], object> _implementacao;

        public FuncaoNativa(Func<object[], object> implementacao, string ident = "") : base("", null, null)
        {
            _implementacao = implementacao;
        }

        public object Executar(params object[] argumentos)
        {
            return _implementacao(argumentos);
        }
    }
}