using Libra.Arvore;

namespace Libra
{
    public class Parametro
    {
        public string Identificador;
        public LibraTipo Tipo;

        public Parametro(string ident, LibraTipo tipo = LibraTipo.Objeto)
        {
            Identificador = ident;
            Tipo = tipo;
        }
    }

    public class Funcao
    {
        public string Identificador;
        public Instrucao[] Instrucoes;
        public Parametro[] Parametros;
        public LibraTipo TipoRetorno;

        public Funcao(string ident, Instrucao[] instrucoes, Parametro[] parametros, LibraTipo tipoRetorno = LibraTipo.Objeto)
        {
            Identificador = ident;
            Instrucoes = instrucoes;
            Parametros = parametros;
            TipoRetorno = tipoRetorno;
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