using Libra.Arvore;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Libra;

public class Interpretador
{
    public static int NivelDebug = 0;
    private Programa _programa;
    private object _ultimoRetorno = 0;
    
    public void Interpretar(Programa programa)
    {
        LibraBase.ProgramaAtual = _programa = programa;

        LibraBase.RegistrarFuncoesEmbutidas();

        InterpretarInstrucoes(_programa.Instrucoes);
        
        LibraBase.ProgramaAtual = null; // limpar o programa depois que terminar
    }

    private object InterpretarInstrucoes(Instrucao[] instrucoes)
    {
        for(int i = 0; i < instrucoes.Length; i++)
        {
            var instrucao = InterpretarInstrucao(instrucoes[i]);

            if(instrucao is InstrucaoRetornar) 
            {
                var retorno = (InstrucaoRetornar)instrucao;
                _ultimoRetorno = InterpretarExpressao(retorno.Expressao);
                return _ultimoRetorno;
            }
        }
        
        return 0;
    }

    // TODO: Muitos Else Ifs, mas não acho ser possível usar Switch Case em Tipos?
    private Instrucao InterpretarInstrucao(Instrucao instrucao)
    {
        if (instrucao is InstrucaoVar)
            InterpretarInstrucaoVar((InstrucaoVar)instrucao);

        else if (instrucao is InstrucaoChamadaFuncao)
            InterpretarChamadaFuncao((InstrucaoChamadaFuncao)instrucao);

        else if (instrucao is InstrucaoSe || instrucao is InstrucaoEnquanto)
            InterpretarCondicional(instrucao);

        else if (instrucao is InstrucaoRomper)
            return instrucao;

        else if (instrucao is InstrucaoRetornar)
            return instrucao;

        else if (instrucao is InstrucaoFuncao)
            InterpretarFuncao((InstrucaoFuncao)instrucao);

        new Erro($"Instrução Inválida: {instrucao.ToString()}");
        return null;
    }

    private void InterpretarCondicional(Instrucao instrucao)
    {
        if(instrucao is InstrucaoSe)
        {
            var instrucaoSe = (InstrucaoSe)instrucao;
            var instrucoesSe = instrucaoSe.Instrucoes;
            if((int)InterpretarExpressao(instrucaoSe.Expressao) != 0)
                InterpretarInstrucoes(instrucoesSe);
            
            if(instrucaoSe.SenaoInstrucoes != null)
                InterpretarInstrucoes(instrucaoSe.SenaoInstrucoes);
            
            return;
        }

        var enquanto = (InstrucaoEnquanto)instrucao;
    	var instrucoes = enquanto.Instrucoes;
        while((int)InterpretarExpressao(enquanto.Expressao) != 0)
            InterpretarInstrucoes(instrucoes);
        
    }

    private void InterpretarFuncao(InstrucaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if(string.IsNullOrWhiteSpace(identificador))
            new Erro("Identificador inválido!").LancarErro();
        
        if(_programa.FuncaoExiste(identificador))
            new ErroFuncaoJaDefinida(identificador).LancarErro();
        
        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros);

        _programa.Funcoes[identificador] = novaFuncao;
    }

    // REFATORAR!
    private HashSet<string> funcoesEmExecucao = new HashSet<string>();
    private object InterpretarChamadaFuncao(InstrucaoChamadaFuncao instrucaoChamada)
    {
        var chamada = instrucaoChamada.Chamada;
        var argumentos = chamada.Argumentos;

        if (!_programa.FuncaoExiste(chamada.Identificador))
                new ErroFuncaoNaoDefinida(chamada.Identificador).LancarErro();
                
        if(_programa.Funcoes[chamada.Identificador] is FuncaoEmbutida)
        {
            var f = (FuncaoEmbutida)_programa.Funcoes[chamada.Identificador];
            List<object> valoresArgumentos = new List<object>();
            foreach(var arg in argumentos)
            {
                valoresArgumentos.Add(InterpretarExpressao(arg));
            }

            _ultimoRetorno = f.Executar(valoresArgumentos.ToArray());
            return _ultimoRetorno;
        }
           
        // Verifica se a função já está em execução (evita recursão infinita)
        if (funcoesEmExecucao.Contains(chamada.Identificador))
        {
            new ErroTransbordoDePilha(chamada.Identificador).LancarErro();
            return null;
        }

        // Marca a função como em execução
        funcoesEmExecucao.Add(chamada.Identificador);

        try
        {
        
            var funcao = _programa.Funcoes[chamada.Identificador];
            var parametros = funcao.Parametros.Count;

            if (argumentos.Count != parametros)
                new Erro($"Função {chamada.Identificador}() esperava {parametros} argumento(s) e recebeu {argumentos.Count}").LancarErro();

            _programa.PilhaEscopos.EmpilharEscopo(); // empurra o novo Escopo da função

            // Adicionando os argumentos ao Escopo
            for (int i = 0; i < chamada.Argumentos.Count; i++)
            {
                string ident = funcao.Parametros[i];
                Token valor = new Token(TokenTipo.NumeroLiteral, 0, InterpretarExpressao(chamada.Argumentos[i]).ToString());
                _programa.PilhaEscopos.DefinirVariavel(ident, new Variavel(ident, valor));
            }

            object retorno = InterpretarInstrucoes(funcao.Instrucoes);

            _programa.PilhaEscopos.DesempilharEscopo(); // Removendo o Escopo da Pilha

            return _ultimoRetorno = retorno;
        }
        finally
        {
            // Marca a função como terminada
            funcoesEmExecucao.Remove(chamada.Identificador);
        }
    }

    private object InterpretarExpressao(Expressao expressao)
    {
        if(expressao is ExpressaoUnaria)
            return ExtrairValorTermo((ExpressaoUnaria)expressao);
        
        else if(expressao is ExpressaoBinaria)
        {
            var binaria = (ExpressaoBinaria)expressao;

            var esq = (ExpressaoUnaria)binaria.Esquerda;
            var dir = binaria.Direita;

            // TODO: Só aceita Inteiros! Modificar
            int a, b = 0;

            a = (int)ExtrairValorTermo(esq);

            b = (int)InterpretarExpressao(dir);
            
            switch(binaria.Operador.Tipo)
            {
                case TokenTipo.OperadorSoma: return a+b;
                case TokenTipo.OperadorSub: return a-b;
                case TokenTipo.OperadorMult: return a*b;
                case TokenTipo.OperadorDiv:
                    if(b == 0)
                        new ErroDivisaoPorZero().LancarErro();
                    return a/b;
                case TokenTipo.OperadorMaiorQue: return LibraHelper.BoolParaInt(a>b);
                case TokenTipo.OperadorMaiorIgualQue: return LibraHelper.BoolParaInt(a>=b);
                case TokenTipo.OperadorMenorQue: return LibraHelper.BoolParaInt(a<b);
                case TokenTipo.OperadorMenorIgualQue: return LibraHelper.BoolParaInt(a<=b);
                case TokenTipo.OperadorOu: return LibraHelper.BoolParaInt(a!= 0 || b != 0);
                case TokenTipo.OperadorE: return LibraHelper.BoolParaInt(a!=0 && b != 0);
                case TokenTipo.OperadorComparacao: return LibraHelper.BoolParaInt(a==b);
                case TokenTipo.OperadorDiferente: return LibraHelper.BoolParaInt(a!=b);
            }
        }

        return 0;
    }

    // TODO: Refatorar esse método (Desculpa, já era tarde da noite quando eu escrevi isso e deveria estar dormindo)
    private object InterpretarInstrucaoVar(InstrucaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            new Erro("Identificador inválido!").LancarErro();

        var expressaoValorFinal = (Expressao)i.Valor;
        object resultado = InterpretarExpressao(expressaoValorFinal);

        if(i.IndiceVetor != null)
            return AtribuirElementoVetor(i.Identificador, i.IndiceVetor, expressaoValorFinal);
        
        // declarando um Vetor (var x = [10])
        if (i.Tipo == TokenTipo.Vetor)
        {
            Token[] tokens = new Token[(int)resultado];
            for(int _ =0; _ < tokens.Length; _++)
                tokens[_] = new Token(TokenTipo.Nulo, 0);

            if(i.EhDeclaracao)
                _programa.PilhaEscopos.DefinirVariavel(i.Identificador, new Variavel(i.Identificador, new Token(i.Tipo, 0, tokens), i.Constante));
            else
                _programa.PilhaEscopos.AtualizarVariavel(i.Identificador,  new Token(i.Tipo, 0, tokens));
            return null;
        }

        // Declarando uma variável normal
        var token = new Token(i.Tipo, 0, resultado);
        var variavel = new Variavel(i.Identificador, token, i.Constante);

        if(i.EhDeclaracao)
            _programa.PilhaEscopos.DefinirVariavel(i.Identificador, variavel);
        else
            _programa.PilhaEscopos.AtualizarVariavel(i.Identificador, resultado);

        return variavel.Valor;
    }

    private object AtribuirElementoVetor(string identificador, Expressao expressaoIndice, Expressao expressao)
    {
        int indiceVetor = (int)InterpretarExpressao(expressaoIndice);
        object valor = InterpretarExpressao(expressao);

        var vetor = (Token[])_programa.PilhaEscopos.ObterVariavel(identificador).Valor;

        if(indiceVetor > vetor.Length || indiceVetor < 0)
            new ErroIndiceForaVetor().LancarErro();

        vetor[indiceVetor].Valor = valor; // TODO: Isso funciona?

        return valor;
    }

    private object ExtrairValorTermo(ExpressaoUnaria termo)
    {
        if(termo.ChamadaFuncao != null)
        {
            InterpretarInstrucao(new InstrucaoChamadaFuncao(termo.ChamadaFuncao));
            return _ultimoRetorno;
        }

        if(termo.AcessoVetor != null)
        {
            var vetor = (Token[])(_programa.PilhaEscopos.ObterVariavel(termo.AcessoVetor.Identificador).Valor);
            int indice = (int)InterpretarExpressao(termo.AcessoVetor.Expressao);

            return vetor[indice].Valor;
        }

        switch(termo.Token.Tipo)
        {
            case TokenTipo.Identificador:
                if(_programa.PilhaEscopos.ObterVariavel((string)termo.Valor) == null)
                    new ErroVariavelNaoDeclarada((string)termo.Valor).LancarErro();
                return _programa.PilhaEscopos.ObterVariavel((string)termo.Valor).Valor;
            case TokenTipo.CaractereLiteral:
                return (int)termo.Token.Valor.ToString()[0];
        }

        return termo.Valor;
    }

}