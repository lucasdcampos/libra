using Libra.Arvore;

namespace Libra.Arvore;

public interface IVisitor<T>
{
    T VisitarPrograma(Programa programa);

    T VisitarDeclVar(DeclaracaoVar instrucao);
    T VisitarFuncao(DefinicaoFuncao instrucao);
    T VisitarTentar(Tentar instrucao);
    T VisitarEnquanto(Enquanto instrucao);
    T VisitarSe(Se instrucao);
    T VisitarAtribIndice(AtribuicaoIndice instrucao);
    T VisitarAtribProp(AtribuicaoPropriedade instrucao);
    T VisitarAtribVar(AtribuicaoVar instrucao);
    T VisitarInstrucaoExpressao(InstrucaoExpressao instrucao);
    T VisitarRetorno(Retornar instrucao);
    T VisitarRomper(Romper instrucao);
    T VisitarContinuar(Continuar instrucao);
    T VisitarClasse(DefinicaoTipo instrucao);
    T VisitarParaCada(ParaCada instrucao);
    T VisitarSenaoSe(SenaoSe instrucao);

    T VisitarExpressaoLiteral(ExpressaoLiteral expressao);
    T VisitarExpressaoVariavel(ExpressaoVariavel expressao);
    T VisitarExpressaoPropriedade(ExpressaoPropriedade expressao);
    T VisitarExpressaoBinaria(ExpressaoBinaria expressao);
    T VisitarExpressaoNovoVetor(ExpressaoNovoVetor expressao);
    T VisitarExpressaoInicializacaoVetor(ExpressaoInicializacaoVetor expressao);
    T VisitarExpressaoUnaria(ExpressaoUnaria expressao);
    T VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao);
    T VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao);
    T VisitarExpressaoChamadaMetodo(ExpressaoChamadaMetodo expressao);
}
