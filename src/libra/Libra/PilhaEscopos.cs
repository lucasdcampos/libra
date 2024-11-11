using Libra.Arvore;

namespace Libra; 

public class PilhaDeEscopos
{
    private Stack<Escopo> escopos = new Stack<Escopo>();

    public PilhaDeEscopos()
    {
        // Adiciona o escopo global ao iniciar
        escopos.Push(new Escopo());
    }

    // Adiciona um novo escopo à pilha (usado para blocos locais, funções, etc.)
    public void EmpilharEscopo()
    {
        escopos.Push(new Escopo());
    }

    // Remove o escopo atual da pilha, caso não seja o global
    public void DesempilharEscopo()
    {
        if (escopos.Count > 1)
            escopos.Pop();
        else
            new Erro("A pilha de escopos está vazia para remover.", 0, -1);
    }

    // Define uma variável no escopo atual
    public void DefinirVariavel(string identificador, Variavel variavel)
    {
        escopos.Peek().DefinirVariavel(identificador, variavel);
    }

    // Busca uma variável, começando pelo escopo mais interno até o global
    public Variavel? ObterVariavel(string identificador)
    {
        for(int i = 0; i < escopos.Count; i++)
        {
            if (escopos.ElementAt(i).VariavelExiste(identificador))
                return escopos.ElementAt(i)?.ObterVariavel(identificador);
        }
        new ErroVariavelNaoDeclarada(identificador).LancarErro();
        return null; // Variável não encontrada
    }

    // Atualiza o valor de uma variável existente
    public void AtualizarVariavel(string identificador, object novoValor)
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
        new ErroVariavelNaoDeclarada(identificador).LancarErro();
    }
}
