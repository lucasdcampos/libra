using System.Text;
using Libra.Arvore;

namespace Libra; 

public class PilhaDeEscopos
{
    private Stack<Escopo> escopos = new Stack<Escopo>();

    public PilhaDeEscopos()
    {
        // Adiciona o escopo global ao iniciar
        EmpilharEscopo("", new LocalToken());
    }

    // Adiciona um novo escopo à pilha (usado para blocos locais, funções, etc.)
    public void EmpilharEscopo(string nome = "", LocalToken local = new())
    {
        if (escopos.Count < 1000)
        {
            escopos.Push(new Escopo(nome, local));
        }
            
        else
            throw new ErroTransbordoDePilha();
        
    }

    // Remove o escopo atual da pilha, caso não seja o global
    public void DesempilharEscopo()
    {
        if (escopos.Count > 1)
        {
            for(int i = 0; i < escopos.Peek()._variaveis.Count; i++)
            {
                Variavel var = escopos.Peek()._variaveis.ElementAt(i).Value;
                if(!var.Referenciada && Interpretador.Flags.MostrarAvisos)
                {
                    Ambiente.Msg($"Aviso: Variável `{var.Identificador}` foi declarada mas nunca foi usada.\n{Interpretador.LocalAtual}");
                }
            }

            escopos.Pop();
        }
    }

    // Define uma variável no escopo atual
    public void DefinirVariavel(string identificador, LibraObjeto valor, bool constante = false, LibraTipo tipo = LibraTipo.Objeto, bool tipoModificavel = true)
    {
        escopos.Peek().DefinirVariavel(identificador, valor, constante, tipo, tipoModificavel);
    }

    public string ObterCallStack()
    {
        if(escopos.Count == 0)
            return "";
    
        var sb = new StringBuilder();
        foreach(var e in escopos)
        {
            if(string.IsNullOrEmpty(e.Nome))
                continue;
            sb.Append($"    {e.Nome} ({e.Local})\n");
        }
        return sb.ToString();
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
