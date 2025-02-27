using Libra.Arvore;
using Libra;
public class VisitorExpressoes : IVisitor
{
    private Interpretador _interpretador;
    private Programa _programa => Ambiente.ProgramaAtual;

    public VisitorExpressoes(Interpretador interpretador)
    {
        _interpretador = interpretador;
    }

    public LibraObjeto VisitarExpressaoLiteral(ExpressaoLiteral expressao)
    {
        return LibraObjeto.ParaLibraObjeto(expressao.Token.Valor);
    }
    
    public LibraObjeto VisitarExpressaoBinaria(ExpressaoBinaria expressao)
    {
        var a = _interpretador.InterpretarExpressao(expressao.Esquerda);
        var b = _interpretador.InterpretarExpressao(expressao.Direita);

        return expressao.Operador.Tipo switch
        {
            TokenTipo.OperadorSoma => a.Soma(b),
            TokenTipo.OperadorSub => a.Sub(b),
            TokenTipo.OperadorMult => a.Mult(b),
            TokenTipo.OperadorDiv => a.Div(b),
            TokenTipo.OperadorPot => a.Pot(b),
            TokenTipo.OperadorResto => a.Resto(b),
            TokenTipo.OperadorComparacao => a.Igual(b),
            TokenTipo.OperadorDiferente => new LibraInt(LibraUtil.NegarInteiroLogico(a.Igual(b).Valor)),
            TokenTipo.OperadorMaiorQue => a.MaiorQue(b),
            TokenTipo.OperadorMaiorIgualQue => a.MaiorIgualQue(b),
            TokenTipo.OperadorMenorQue => a.MenorQue(b),
            TokenTipo.OperadorMenorIgualQue => a.MenorIgualQue(b),
            TokenTipo.OperadorE => a.E(b),
            TokenTipo.OperadorOu => a.Ou(b),
            _ => throw new Erro($"Operador desconhecido: {expressao.Operador.Tipo}")
        };
    }

    public LibraObjeto VisitarExpressaoVariavel(ExpressaoVariavel expressao)
    {
        return LibraObjeto.ParaLibraObjeto(_programa.ObterVariavel(expressao.Identificador.Valor.ToString()));
    }

    public LibraObjeto VisitarExpressaoDeclaracaoVetor(ExpressaoDeclaracaoVetor expressao)
    {
        var vetor = new LibraObjeto[_interpretador.InterpretarExpressao<LibraInt>(expressao.Expressao).Valor];
        return LibraObjeto.ParaLibraObjeto(vetor); // Converte para LibraVetor
    }

    public LibraObjeto VisitarExpressaoInicializacaoVetor(ExpressaoInicializacaoVetor expressao)
    {
        var arr = new LibraObjeto[expressao.Expressoes.Count];

        for(int i = 0; i < expressao.Expressoes.Count; i++)
        {
            arr[i] = _interpretador.InterpretarExpressao(expressao.Expressoes[i]);
        }

        return LibraObjeto.ParaLibraObjeto(arr);
    }

    public LibraObjeto VisitarExpressaoUnaria(ExpressaoUnaria expressao)
    {
        switch(expressao.Operador.Tipo)
        {
            case TokenTipo.OperadorNeg:
                return LibraObjeto.ParaLibraObjeto(_interpretador.InterpretarExpressao<LibraInt>(expressao.Operando).Valor);
            case TokenTipo.OperadorSub:
                return LibraObjeto.ParaLibraObjeto(_interpretador.InterpretarExpressao(expressao.Operando).Mult(new LibraInt(-1)));
        }

        throw new Erro("Operador unário não implementado");
    }

    public LibraObjeto VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao)
    {
        string ident = expressao.Identificador;
        int indice = _interpretador.InterpretarExpressao<LibraInt>(expressao.Expressao).Valor;

        var variavel = _programa.PilhaEscopos.ObterVariavel(ident);

        if (variavel.Valor is not LibraVetor vetor)
            throw new ErroAcessoNulo($" `{variavel.Identificador}` não é um Vetor.");

        if (indice < 0 || indice >= vetor.Valor.Length)
            throw new ErroIndiceForaVetor();

        return vetor.Valor[indice];
    }

    public LibraObjeto VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao)
    {
        return _interpretador.InterpretarChamadaFuncao(expressao);
    }
}