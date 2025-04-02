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
            _ => throw new Erro($"Operador desconhecido: {expressao.Operador.Tipo}", expressao.Operador.Local)
        };
    }

    public LibraObjeto VisitarExpressaoVariavel(ExpressaoVariavel expressao)
    {
        var v = _programa.ObterVariavel(expressao.Identificador.Valor.ToString());
        return v.Valor;
    }

    public LibraObjeto VisitarExpressaoNovoVetor(ExpressaoNovoVetor expressao)
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

        throw new Erro("Operador unário não implementado", expressao.Operador.Local);
    }

    public LibraObjeto VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao)
    {
        string ident = expressao.Identificador;
        int indice = _interpretador.InterpretarExpressao<LibraInt>(expressao.Expressao).Valor;

        var variavel = _programa.PilhaEscopos.ObterVariavel(ident);

        if(variavel.Valor is LibraVetor vetor)
        {
            if (indice < 0 || indice >= vetor.Valor.Length)
                throw new ErroIndiceForaVetor($"{ident}[{indice.ToString()}]", Interpretador.LocalAtual);
            return vetor.Valor[indice];
        }
        if(variavel.Valor is LibraTexto texto)
        {
            if (indice < 0 || indice >= texto.Valor.Length)
                throw new ErroIndiceForaVetor($"{ident}[{indice.ToString()}]", Interpretador.LocalAtual);
            return new LibraTexto(texto.Valor[indice].ToString());
        }

        throw new ErroAcessoNulo($" {variavel.Valor} não é um Vetor");
    }

    public LibraObjeto VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao)
    {
        return _interpretador.InterpretarChamadaFuncao(expressao);
    }

    public LibraObjeto VisitarExpressaoPropriedade(ExpressaoPropriedade expressao)
    {
        var obj = LibraObjeto.ParaLibraObjeto(_programa.ObterVariavel(expressao.Identificador));
        return obj.AcessarPropriedade(expressao.Propriedade);
    }

    public LibraObjeto VisitarExpressaoChamadaMetodo(ExpressaoChamadaMetodo expressao)
    {
        var obj = LibraObjeto.ParaLibraObjeto(_programa.ObterVariavel(expressao.Identificador));

        return obj.ChamarMetodo(expressao.Chamada, expressao.Identificador);

    }
}