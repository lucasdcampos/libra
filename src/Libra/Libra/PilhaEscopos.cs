using Libra.Arvore;

namespace Libra; 

public class PilhaDeEscopos
{
    private Stack<Escopo> escopos = new Stack<Escopo>();

    public PilhaDeEscopos()
    {
        // Adiciona o escopo global ao iniciar
        EmpilharEscopo();
    }

    // Adiciona um novo escopo à pilha (usado para blocos locais, funções, etc.)
    public void EmpilharEscopo()
    {
        if (escopos.Count < 1000)
        {
            escopos.Push(new Escopo());
        }
            
        else
            throw new ErroTransbordoDePilha();
        
    }

    // Remove o escopo atual da pilha, caso não seja o global
    public void DesempilharEscopo()
    {
        if (escopos.Count > 1)
        {
            escopos.Pop();
        }
    }

    // Define uma variável no escopo atual
    public void DefinirVariavel(string identificador, LibraObjeto valor, bool constante = false, LibraTipo tipo = LibraTipo.Objeto, bool tipoModificavel = true)
    {
        escopos.Peek().DefinirVariavel(identificador, valor, constante, tipo, tipoModificavel);
    }

    // Busca uma variável, começando pelo escopo mais interno até o global
    public Variavel? ObterVariavel(string identificador)
    {
        for(int i = 0; i < escopos.Count; i++)
        {
            if (escopos.ElementAt(i).VariavelExiste(identificador))
                return escopos.ElementAt(i)?.ObterVariavel(identificador);
        }

        throw new ErroVariavelNaoDeclarada(identificador);
        
        return null;
    }

    // Atualiza o valor de uma variável existente
    public void AtualizarVariavel(string identificador, LibraObjeto novoValor)
    {
        for(int i = 0; i < escopos.Count; i++)
        {
            var escopo = escopos.ElementAt(i);

            if (escopo.VariavelExiste(identificador))
            {
                escopo.AtualizarVariavel(identificador, novoValor);
                return;
            }
        }
        throw new ErroVariavelNaoDeclarada(identificador);
    }

    public void ModificarVetor(string identificador, int indice, LibraObjeto novoValor)
    {
        for(int i = 0; i < escopos.Count; i++)
        {
            var escopo = escopos.ElementAt(i);

            if (escopo.VariavelExiste(identificador))
            {
                escopo.ModificarVetor(identificador, indice, novoValor);
                return;
            }
        }
        throw new ErroVariavelNaoDeclarada(identificador);
    }
}
