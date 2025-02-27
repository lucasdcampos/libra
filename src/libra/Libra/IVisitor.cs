namespace Libra.Arvore;

public interface IVisitor
{
    LibraObjeto VisitarExpressaoLiteral(ExpressaoLiteral expressao);
    LibraObjeto VisitarExpressaoVariavel(ExpressaoVariavel expressao);
    LibraObjeto VisitarExpressaoBinaria(ExpressaoBinaria expressao);
    LibraObjeto VisitarExpressaoDeclaracaoVetor(ExpressaoDeclaracaoVetor expressao);
    LibraObjeto VisitarExpressaoInicializacaoVetor(ExpressaoInicializacaoVetor expressao);
    LibraObjeto VisitarExpressaoUnaria(ExpressaoUnaria expressao);
    LibraObjeto VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao);
    LibraObjeto VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao);
}