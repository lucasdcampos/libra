using Libra.Arvore;

namespace Libra.Runtime;

public interface IVisitor
{
    object VisitarPrograma(Programa programa);
    
    object VisitarDeclVar(DeclaracaoVar instrucao);
    object VisitarFuncao(DefinicaoFuncao instrucao);
    object VisitarTentar(Tentar instrucao);
    object VisitarEnquanto(Enquanto instrucao);
    object VisitarSe(Se instrucao);
    object VisitarAtribIndice(AtribuicaoIndice instrucao);
    object VisitarAtribProp(AtribuicaoPropriedade instrucao);
    object VisitarAtribVar(AtribuicaoVar instrucao);
    object VisitarInstrucaoExpressao(InstrucaoExpressao instrucao);
    object VisitarRetorno(Retornar instrucao);
    object VisitarRomper(Romper instrucao);
    object VisitarContinuar(Continuar instrucao);
    object VisitarClasse(DefinicaoTipo instrucao); // TODO: Renomear para DefinicaoClasse
    object VisitarParaCada(ParaCada instrucao);
    object VisitarSenaoSe(SenaoSe instrucao);

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