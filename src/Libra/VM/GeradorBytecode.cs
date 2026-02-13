using Libra.Arvore;
using Libra.Runtime;
using System.Collections.Generic;

namespace Libra.VM;

public class GeradorBytecode : IVisitor<object>
{
    public List<InstrucaoVM> Instrucoes { get; } = new();
    private Programa _programa;

    public GeradorBytecode(Programa programa)
    {
        _programa = programa;
    }

    public List<InstrucaoVM> Gerar()
    {
        _programa.Aceitar(this);
        return Instrucoes;
    }

    public object VisitarPrograma(Programa programa)
    {
        foreach (var instr in programa.Instrucoes)
        {
            instr.Aceitar(this);
        }

        Instrucoes.Add(new InstrucaoVM { Op = Opcode.ENCERRAR, Argumento = null });

        return null;
    }

    public object VisitarExpressaoLiteral(ExpressaoLiteral expressao)
    {
        switch (expressao.Valor)
        {
            case int i:
                Instrucoes.Add(new InstrucaoVM { Op = Opcode.EMPILHAR, Argumento = i });
                break;

            case string s:
                Instrucoes.Add(new InstrucaoVM { Op = Opcode.EMPILHAR, Argumento = s });
                break;

            default:
                throw new Exception($"Tipo de literal não suportado: {expressao.Valor.GetType()}");
        }

        return null;
    }

    public object VisitarExpressaoBinaria(ExpressaoBinaria expressao)
    {
        // Primeiro geramos código para a expressão da esquerda e direita
        expressao.Esquerda.Aceitar(this);
        expressao.Direita.Aceitar(this);

        // Agora adicionamos o opcode correspondente
        switch (expressao.Operador.Tipo)
        {
            case TokenTipo.OperadorSoma:
                Emitir(Opcode.SOMAR);
                break;
            case TokenTipo.OperadorSub:
                Emitir(Opcode.SUBTRAIR);
                break;
            case TokenTipo.OperadorMult:
                Emitir(Opcode.MULTIPLICAR);
                break;
            case TokenTipo.OperadorDiv:
                Emitir(Opcode.DIVIDIR);
                break;
            case TokenTipo.OperadorPot:
                Emitir(Opcode.POTENCIA);
                break;
            case TokenTipo.OperadorResto:
                Emitir(Opcode.RESTO);
                break;
            default:
                throw new Exception($"Operador binário não suportado: {expressao.Operador.Tipo}");
        }

        return null;
    }    

    public object VisitarInstrucaoExpressao(InstrucaoExpressao instrucao)
    {
        instrucao.Expressao?.Aceitar(this);
        return null;
    }

    public object VisitarDeclVar(DeclaracaoVar instrucao)
    {
        // 1. Empilha o valor da expressão (se houver)
        instrucao.Expressao?.Aceitar(this);

        // 2. Gera instrução para armazenar na variável
        Emitir(Opcode.ARMAZENAR, instrucao.Identificador);

        return null;
    }

    public object VisitarFuncao(DefinicaoFuncao instrucao) => null;
    public object VisitarTentar(Tentar instrucao) => null;

    public object VisitarEnquanto(Enquanto instrucao)
    {
        return null;
    }

    public object VisitarSe(Se instrucao)
    {
        // 1. Condição
        instrucao.Condicao.Aceitar(this);

        // 2. Salto para o Else (ou Fim)
        int indiceSaltoSenao = Instrucoes.Count;
        Emitir(Opcode.SALTAR_SE_FALSO);

        // 3. Bloco 'Entao'
        instrucao.Entao.Aceitar(this);

        if (instrucao.Senao != null)
        {
            // 4. Salto para o Fim (pular o senao)
            int indiceSaltoFim = Instrucoes.Count;
            Emitir(Opcode.SALTAR);

            // 5. Patch: Salto do SE_FALSO aponta para o início do SENAO
            var instrSenao = Instrucoes[indiceSaltoSenao];
            instrSenao.Argumento = Instrucoes.Count;
            Instrucoes[indiceSaltoSenao] = instrSenao; // Necessário se for struct

            // 6. Bloco 'Senao'
            instrucao.Senao.Aceitar(this);

            // 7. Patch: Salto do FIM (incondicional) aponta para depois do senao
            var instrFim = Instrucoes[indiceSaltoFim];
            instrFim.Argumento = Instrucoes.Count;
            Instrucoes[indiceSaltoFim] = instrFim;
        }
        else
        {
            // Sem senao: Patch do SE_FALSO aponta para o fim da instrução SE
            var instrSenao = Instrucoes[indiceSaltoSenao];
            instrSenao.Argumento = Instrucoes.Count;
            Instrucoes[indiceSaltoSenao] = instrSenao;
        }

        return null;
    }

    public object VisitarAtribIndice(AtribuicaoIndice instrucao) => null;
    public object VisitarAtribProp(AtribuicaoPropriedade instrucao) => null;
    public object VisitarAtribVar(AtribuicaoVar instrucao) => null;
    public object VisitarRetorno(Retornar instrucao) => null;
    public object VisitarRomper(Romper instrucao) => null;
    public object VisitarContinuar(Continuar instrucao) => null;
    public object VisitarClasse(DefinicaoTipo instrucao) => null;
    public object VisitarParaCada(ParaCada instrucao) => null;
    public object VisitarExpressaoVariavel(ExpressaoVariavel expressao)
    {
        // Gera instrução para carregar a variável na pilha
        Instrucoes.Add(new InstrucaoVM
        {
            Op = Opcode.CARREGAR,
            Argumento = expressao.Identificador.Valor.ToString()
        });

        return null;
    }
    public object VisitarExpressaoPropriedade(ExpressaoPropriedade expressao) => null;
    public object VisitarExpressaoNovoVetor(ExpressaoNovoVetor expressao) => null;
    public object VisitarExpressaoInicializacaoVetor(ExpressaoInicializacaoVetor expressao) => null;
    public object VisitarExpressaoUnaria(ExpressaoUnaria expressao) => null;
    public object VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao) => null;
    public object VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao) => null;
    public object VisitarExpressaoChamadaMetodo(ExpressaoChamadaMetodo expressao) => null;

    public object VisitarBloco(Bloco instrucao)
    {
        foreach (var instr in instrucao.Instrucoes)
        {
            instr.Aceitar(this);
        }
        return null;
    }

    private void Emitir(Opcode op, object? arg = null)  => Instrucoes.Add(new InstrucaoVM { Op = op, Argumento = arg });
}
