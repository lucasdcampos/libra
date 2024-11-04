using Libra.Arvore;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Libra;

public class Interpretador
{
    public static int NivelDebug = 0;
    private Programa _programa;
    private int _enderecoInicialEscopo;
    private List<int> _enderecosIniciaisEscopos = new List<int>();
    private int _ultimoRetorno = 0;

    public void Interpretar(Programa programa)
    {
        LibraBase.ProgramaAtual = _programa = programa;
        
        for(int i = 0; i < _programa.Instrucoes.Count; i++)
            InterpretarInstrucao(_programa.Instrucoes[i]);
        
        LibraBase.ProgramaAtual = null; // limpar o programa depois que terminar
    }

    private Instrucao InterpretarInstrucao(Instrucao instrucao)
    {
        if (instrucao is InstrucaoVar)
        {
            var var = (InstrucaoVar)instrucao;
            DeclararVariavel(var.Identificador, var.EhDeclaracao, var.Constante, var.Valor, var.Tipo, var.IndiceVetor);
        }

        else if(instrucao is InstrucaoSair)
            InterpretarInstrucaoSair((InstrucaoSair)instrucao);

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

    private void InterpretarInstrucaoSair(InstrucaoSair instrucao)
    {
        int? codigoSaida = (int?)InterpretarExpressao(instrucao.Expressao);

        if (codigoSaida == null)
            new ErroAcessoNulo().LancarErro();

        LibraBase.Sair((int)codigoSaida);
    }

    private void InterpretarCondicional(Instrucao instrucao)
    {
        if(instrucao is InstrucaoSe)
        {
            var se = (InstrucaoSe)instrucao;

            if((int)InterpretarExpressao(se.Expressao) != 0)
                InterpretarEscopo(se.Escopo);
            
            if(se.SenaoEscopo != null)
                InterpretarEscopo(se.SenaoEscopo);
            
            return;
        }

        var enquanto = (InstrucaoEnquanto)instrucao;

        while((int)InterpretarExpressao(enquanto.Expressao) != 0)
            InterpretarEscopo(enquanto.Escopo);
        
    }

    private void InterpretarFuncao(InstrucaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if(string.IsNullOrWhiteSpace(identificador))
            new Erro("Identificador inválido!").LancarErro();
        
        if(_programa.FuncaoExiste(identificador))
            new ErroFuncaoJaDefinida(identificador).LancarErro();
        
        var novaFuncao = new Funcao(identificador, funcao.Escopo, funcao.Parametros);

        _programa.Funcoes[identificador] = novaFuncao;
    }

    private object InterpretarChamadaFuncao(InstrucaoChamadaFuncao instrucaoChamada)
    {
        var chamada = instrucaoChamada.Chamada;

        int qtdArgumentos = chamada.Argumentos.Count;

        if(chamada.Identificador.StartsWith("__") && chamada.Identificador.EndsWith("__"))
        {
            string nomeFuncao = chamada.Identificador.Replace("__", "");
            return ChamarFuncaoInterna(nomeFuncao, chamada);
        }

        if(!_programa.FuncaoExiste(chamada.Identificador))
            new ErroFuncaoNaoDefinida(chamada.Identificador).LancarErro();
            
        var variaveis = new List<Variavel>();
        var funcao = _programa.Funcoes[chamada.Identificador];
        var parametros = funcao.Parametros.Count;

        if(qtdArgumentos != parametros)
            new Erro($"Função {chamada.Identificador}() esperava {parametros} argumento(s) e recebeu {qtdArgumentos}").LancarErro();

        for(int i = 0; i < chamada.Argumentos.Count; i++)
        {
            string nomeVariavel = funcao.Parametros[i];
            variaveis.Add(new Variavel(nomeVariavel, new Token(TokenTipo.NumeroLiteral, 0, InterpretarExpressao(chamada.Argumentos[i]).ToString())));
        }

        int retorno = InterpretarEscopo(funcao.Escopo, variaveis);

        return _ultimoRetorno = retorno; // TODO: melhorar isso
        
    }

    private object ChamarFuncaoInterna(string nomeFuncao, ExpressaoChamadaFuncao chamada)
    {
        MethodInfo funcaoBase = typeof(LibraBase).GetMethod(nomeFuncao, BindingFlags.Static | BindingFlags.Public);
        int qtdArgumentos = chamada.Argumentos.Count;

        if(funcaoBase == null)
        {
            new ErroFuncaoNaoDefinida(nomeFuncao).LancarErro();
            return null;
        }
            
        var argsBase = new List<string>();
        for(int i = 0; i < qtdArgumentos; i++)
        {
            var expr = InterpretarExpressao(chamada.Argumentos[i]);
            if (expr == null)
                new ErroAcessoNulo().LancarErro();

            argsBase.Add(expr.ToString());
        }

        return funcaoBase.Invoke(null, argsBase.ToArray());
    }

    private int InterpretarEscopo(Escopo escopo, List<Variavel> variaveis = null)
    {
        _enderecosIniciaisEscopos.Add(_programa.Variaveis.Count -1);

        if(variaveis != null)
            for(int i = 0; i < variaveis.Count; i++)
                _programa.Variaveis[variaveis[i].Identificador] = variaveis[i];
                
        for(int i = 0; i < escopo.Instrucoes.Count; i++)
        {
            var instrucao = InterpretarInstrucao(escopo.Instrucoes[i]);
            if(instrucao is InstrucaoRetornar)
            {
                var retorno = (InstrucaoRetornar)instrucao;
                var resultado = InterpretarExpressao(retorno.Expressao);

                if(variaveis != null)
                    for(int j = 0; j < variaveis.Count; j++)
                        _programa.Variaveis.Remove(variaveis[j].Identificador);
                    
                return (int)resultado;
            }
        }

        int enderecoInicialUltimoEscopo = _enderecosIniciaisEscopos.Last();

        // limpando a memória depois da finalização do escopo
        for (int i = _programa.Variaveis.Count - 1; i > enderecoInicialUltimoEscopo; i--)
        {
            var variavelAtual = _programa.Variaveis.ElementAt(i).Key;
            _programa.Variaveis.Remove(variavelAtual);
        }

        _enderecosIniciaisEscopos.RemoveAt(_enderecosIniciaisEscopos.Count - 1);
        
        return 0;
    }

    private object InterpretarExpressao(Expressao expressao)
    {
        if(expressao is ExpressaoTermo)
            return ExtrairValorTermo((ExpressaoTermo)expressao);
        
        else if(expressao is ExpressaoBinaria)
        {
            var binaria = (ExpressaoBinaria)expressao;

            var esq = (ExpressaoTermo)binaria.Esquerda;
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

    private object DeclararVariavel(string identificador, bool declaracao, bool constante, object valor, TokenTipo tipo, Expressao expressaoIndiceVetor)
    {
        if(string.IsNullOrWhiteSpace(identificador))
            new Erro("Identificador inválido!").LancarErro();

        if(declaracao && _programa.VariavelExiste(identificador))
            new ErroVariavelJaDeclarada(identificador).LancarErro();

        if(_programa.Variaveis.ContainsKey(identificador))
            if(_programa.Variaveis[identificador].Constante)
                new ErroModificacaoConstante(identificador).LancarErro();

        if (valor is Token)
            new Variavel(identificador, (Token)valor, constante);

        var expressaoValorFinal = (Expressao)valor;
        object resultado = InterpretarExpressao(expressaoValorFinal);

        if(expressaoIndiceVetor != null)
            return AtribuirElementoVetor(identificador, expressaoIndiceVetor, expressaoValorFinal);
        
        // declarando um Vetor (var x = [10])
        if (tipo == TokenTipo.Vetor)
        {
            Token[] tokens = new Token[(int)resultado];
            for(int i =0; i < tokens.Length; i++)
                tokens[i] = new Token(TokenTipo.Nulo, 0);

            _programa.Variaveis[identificador] = new Variavel(identificador, new Token(tipo, 0, tokens), constante);
            return null;
        }

        // Declarando uma variável normal
        var token = new Token(TokenTipo.NumeroLiteral, 0, resultado);
        var variavel = new Variavel(identificador, token, constante);
        _programa.Variaveis[identificador] = variavel;

        return variavel.Valor;
    }

    private object AtribuirElementoVetor(string identificador, Expressao expressaoIndice, Expressao expressao)
    {
        int indiceVetor = (int)InterpretarExpressao(expressaoIndice);
        object valor = InterpretarExpressao(expressao);

        var vetor = (Token[])_programa.Variaveis[identificador].Valor;

        if(indiceVetor > vetor.Length || indiceVetor < 0)
            new ErroIndiceForaVetor().LancarErro();

        vetor[indiceVetor].Valor = valor;

        return valor;
    }

    private object ExtrairValorTermo(ExpressaoTermo termo)
    {
        if(termo.ChamadaFuncao != null)
        {
            InterpretarInstrucao(new InstrucaoChamadaFuncao(termo.ChamadaFuncao));
            return _ultimoRetorno;
        }

        if(termo.AcessoVetor != null)
        {
            var vetor = (Token[])(_programa.Variaveis[termo.AcessoVetor.Identificador].Valor);
            int indice = (int)InterpretarExpressao(termo.AcessoVetor.Expressao);

            return vetor[indice].Valor;
        }

        switch(termo.Token.Tipo)
        {
            case TokenTipo.Identificador:
                if(!_programa.VariavelExiste((string)termo.Valor))
                    new ErroVariavelNaoDeclarada((string)termo.Valor).LancarErro();
                return int.Parse(_programa.Variaveis[(string)termo.Valor].Valor.ToString());
            case TokenTipo.CaractereLiteral:
                return (int)termo.Token.Valor.ToString()[0];
        }

        return (int)termo.Valor;
    }

}