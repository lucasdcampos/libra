using Libra.Arvore;

namespace Libra
{
    public class Variavel
    {
        public string Identificador { get; }
        public LibraObjeto Valor {get; private set; }
        public bool Constante { get; }
        public string Tipo { get; private set; }
        public bool TipoModificavel { get; }

        public bool Referenciada { get; internal set; }

        public Variavel(string ident, LibraObjeto valor, bool constante = false, string tipo = "Objeto", bool tipoModificavel = true)
        {
            Identificador = ident;
            Valor = valor;
            Constante = constante;
            TipoModificavel = tipoModificavel;
            Tipo = tipo;
            Referenciada = false;
        }

        public void AtualizarValor(LibraObjeto novoValor)
        {
            Referenciada = true;
            
            if (Constante)
                throw new ErroModificacaoConstante(Identificador, Interpretador.LocalAtual);

            if (novoValor.Nome != Valor.Nome && !TipoModificavel)
            {
                novoValor = novoValor.Converter(Valor.Nome);

                if(Valor.Nome != novoValor.Nome)
                {
                    if(Tipo == "Objeto")
                        Tipo = Valor.Nome;
                    else
                    {
                        throw new ErroTipoIncompativel(Identificador, Interpretador.LocalAtual);
                    }
                        
                }
                    
            }

            Valor = novoValor;
            Tipo = novoValor.Nome;
        }

        public override string ToString()
        {
            return $"{Identificador} (Valor: {Valor}, Constante: {Constante})";
        }
    }
}