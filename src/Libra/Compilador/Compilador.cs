// Esse código tá uma porcaria, agora são 12:31 e eu vou dormir
// Darei uma ajeitada nisso depois
using Libra.Arvore;
using Libra.Runtime;

namespace Libra;

public class Compilador : IVisitor
{
    public int[] Compilar(Programa programa)
    {
        List<int> bytecode = new List<int>();
        foreach (var instrucao in programa.Instrucoes)
        {
            switch (instrucao.Tipo)
            {
                case TipoInstrucao.Expressao:
                    InstrucaoExpressao instrucaoExpressao = (InstrucaoExpressao)instrucao;
                    Expressao expressao = instrucaoExpressao.Expressao;
                    bytecode.AddRange(CompilarExpressao(expressao));
                    continue;
                default:
                    throw new NotImplementedException($"Instrução do tipo {instrucao.Tipo} não implementada.");
            }
        }
        return bytecode.ToArray();
    }

    public int[] CompilarExpressao(Expressao expressao)
    {
        List<int> bytecode = new List<int>();
        switch (expressao.TipoExpr)
        {
            case TipoExpressao.ExpressaoLiteral:
                bytecode.Add((int)LibraVM_OP.OP_EMPILHAR);
                int resultado = AvaliarExpressao(expressao);
                bytecode.Add(resultado);
                break;
            case TipoExpressao.ExpressaoBinaria:
                int a = AvaliarExpressao(((ExpressaoBinaria)expressao).Esquerda);
                int b = AvaliarExpressao(((ExpressaoBinaria)expressao).Direita);
                bytecode.Add((int)LibraVM_OP.OP_EMPILHAR);
                bytecode.Add(b);
                bytecode.Add((int)LibraVM_OP.OP_EMPILHAR);
                bytecode.Add(a);
                switch (((ExpressaoBinaria)expressao).Operador.Tipo)
                {
                    case TokenTipo.OperadorSoma:
                        bytecode.Add((int)LibraVM_OP.OP_SOMAR);
                        break;
                    case TokenTipo.OperadorSub:
                        bytecode.Add((int)LibraVM_OP.OP_SUBTRAIR);
                        break;
                    case TokenTipo.OperadorMult:
                        bytecode.Add((int)LibraVM_OP.OP_MULTIPLICAR);
                        break;
                    default:
                        throw new NotImplementedException($"Operador {((ExpressaoBinaria)expressao).Operador.Tipo} não implementado.");
                }
                break;
        }

        return bytecode.ToArray();
    }

    public int AvaliarExpressao(Expressao expressao)
    {
        if (expressao.TipoExpr != TipoExpressao.ExpressaoLiteral)
            throw new InvalidOperationException("A expressão deve ser do tipo ExpressaoLiteral para avaliação.");

        ExpressaoLiteral literal = (ExpressaoLiteral)expressao;
        if (literal.Token.Tipo != TokenTipo.NumeroLiteral)
            throw new InvalidOperationException("O token da expressão deve ser um número literal.");
        
        return Convert.ToInt32(literal.Token.Valor);
    }
    public LibraObjeto VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoBinaria(ExpressaoBinaria expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoChamadaMetodo(ExpressaoChamadaMetodo expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoInicializacaoVetor(ExpressaoInicializacaoVetor expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoLiteral(ExpressaoLiteral expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoNovoVetor(ExpressaoNovoVetor expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoPropriedade(ExpressaoPropriedade expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoUnaria(ExpressaoUnaria expressao)
    {
        throw new NotImplementedException();
    }

    public LibraObjeto VisitarExpressaoVariavel(ExpressaoVariavel expressao)
    {
        throw new NotImplementedException();
    }
}