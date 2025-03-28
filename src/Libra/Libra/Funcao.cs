using Libra.Arvore;

namespace Libra
{
    public class Parametro
    {
        public string Identificador;
        public string Tipo;

        public Parametro(string ident, string tipo = "Objeto")
        {
            Identificador = ident;
            Tipo = tipo;
        }
    }

    public class Funcao : Variavel, IChamavel
    {
        public Instrucao[] Instrucoes;
        public Parametro[] Parametros;
        public string TipoRetorno;

        public Funcao(string ident, Instrucao[] instrucoes, Parametro[] parametros, string tipoRetorno = "Objeto") : base(ident, null, true, "Funcao", false)
        {
            Instrucoes = instrucoes;
            Parametros = parametros;
            TipoRetorno = tipoRetorno;
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