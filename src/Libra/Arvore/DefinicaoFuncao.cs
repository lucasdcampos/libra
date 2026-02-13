namespace Libra.Arvore;

public class DefinicaoFuncao : Instrucao
    {
        public DefinicaoFuncao(LocalFonte local, string identificador, Instrucao[] instrucoes, Parametro[] parametros = null, string tipoRetorno = "Objeto")
        {
            Instrucoes = instrucoes;
            Identificador = identificador;
            Parametros = parametros;
            TipoRetorno = tipoRetorno;
            Local = local;
        }

        public Instrucao[] Instrucoes { get; private set; }
        public string Identificador {get; private set; }
        public Parametro[] Parametros { get; private set; }
        public string TipoRetorno { get; private set; }

    public override T Aceitar<T>(IVisitor<T> visitor)
        {
            return visitor.VisitarFuncao(this);
        }
    }