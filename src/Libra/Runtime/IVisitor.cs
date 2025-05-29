using Libra.Arvore;

namespace Libra.Runtime;

public interface IVisitor
{
    LibraObjeto VisitarExpressaoLiteral(ExpressaoLiteral expressao);
    LibraObjeto VisitarExpressaoVariavel(ExpressaoVariavel expressao);
    LibraObjeto VisitarExpressaoPropriedade(ExpressaoPropriedade expressao);
    LibraObjeto VisitarExpressaoBinaria(ExpressaoBinaria expressao);
    LibraObjeto VisitarExpressaoNovoVetor(ExpressaoNovoVetor expressao);
    LibraObjeto VisitarExpressaoInicializacaoVetor(ExpressaoInicializacaoVetor expressao);
    LibraObjeto VisitarExpressaoUnaria(ExpressaoUnaria expressao);
    LibraObjeto VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao);
    LibraObjeto VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao);
    LibraObjeto VisitarExpressaoChamadaMetodo(ExpressaoChamadaMetodo expressao);
}