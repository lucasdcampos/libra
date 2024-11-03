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
        _programa = programa;
        
        LibraBase.ProgramaAtual = _programa;

        for(int i = 0; i < _programa.Instrucoes.Count; i++)
        {
            InterpretarInstrucao(_programa.Instrucoes[i]);
        }

        LibraBase.ProgramaAtual = null;
    }

    private Instrucao InterpretarInstrucao(Instrucao instrucao)
    {
        if (instrucao is InstrucaoSair)
        {
            var sair = (InstrucaoSair)instrucao;
            int? codigoSaida = (int?)InterpretarExpressao(sair.Expressao);

            if (codigoSaida == null)
                new ErroAcessoNulo().LancarErro();

            LibraBase.Sair((int)codigoSaida);
        }
        else if (instrucao is InstrucaoVar)
        {
            var var = (InstrucaoVar)instrucao;
            DefinirVariavel(var.Identificador, var.EhDeclaracao, false, var.Valor, var.Tipo, var.IndiceVetor);
        }
        else if (instrucao is InstrucaoConst)
        {
            var constante = (InstrucaoConst)instrucao;
            
            DefinirVariavel(constante.Identificador, true, true, constante.Expressao, TokenTipo.TokenInvalido, null); //TODO: Verificar indice
        }
        else if (instrucao is InstrucaoFuncao)
        {
            var funcao = (InstrucaoFuncao)instrucao;
            
            string identificador = funcao.Identificador;

            if(string.IsNullOrWhiteSpace(identificador))
            {
                new Erro("Identificador inválido!").LancarErro();
            }

            if(_programa.FuncaoExiste(identificador))
            {
                new ErroFuncaoJaDefinida(identificador).LancarErro();
            }
            
            var novaFuncao = new Funcao(identificador, funcao.Escopo, funcao.Parametros);

            _programa.Funcoes[identificador] = novaFuncao;

        }
        else if (instrucao is InstrucaoChamadaFuncao)
        {
            var chamada = (InstrucaoChamadaFuncao)instrucao;
            var args = chamada.Argumentos.Count;
            
            if(chamada.Identificador.StartsWith("__") && chamada.Identificador.EndsWith("__"))
            {
                string nomeFuncao = chamada.Identificador.Replace("__", "");

                MethodInfo funcaoBase = typeof(LibraBase).GetMethod(nomeFuncao, BindingFlags.Static | BindingFlags.Public);

                if (funcaoBase != null)
                {
                    var argsBase = new List<string>();
                    for(int i = 0; i < args; i++)
                    {
                        var expr = InterpretarExpressao(chamada.Argumentos[i]);

                        if (expr == null)
                            new ErroAcessoNulo().LancarErro();

                        argsBase.Add(expr.ToString());
                    }

                    funcaoBase.Invoke(null, argsBase.ToArray());
                }
                else
                {
                    new ErroFuncaoNaoDefinida(nomeFuncao).LancarErro();
                }
            }
            else
            {
                var variaveis = new List<Variavel>();
                var funcao = _programa.Funcoes[chamada.Identificador];
                var parametros = funcao.Parametros.Count;

                if(args != parametros)
                {
                    new Erro($"Função {chamada.Identificador}() esperava {parametros} argumento(s) e recebeu {args}").LancarErro();
                }

                for(int i = 0; i < chamada.Argumentos.Count; i++)
                {
                    string nomeVariavel = funcao.Parametros[i];
                    variaveis.Add(new Variavel(nomeVariavel, new Token(TokenTipo.NumeroLiteral, 0, InterpretarExpressao(chamada.Argumentos[i]).ToString())));
                }

                int retorno = InterpretarEscopo(funcao.Escopo, variaveis);

                _ultimoRetorno = retorno; // TODO: melhorar isso 
            }
        }

        else if (instrucao is InstrucaoSe)
        {
            var se = (InstrucaoSe)instrucao;

            if((int)InterpretarExpressao(se.Expressao) != 0)
            {
                InterpretarEscopo(se.Escopo);
            }
            else
            {
                if(se.SenaoEscopo != null)
                {
                    InterpretarEscopo(se.SenaoEscopo);
                }
            }
        }
        else if (instrucao is InstrucaoEnquanto)
        {
            var enquanto = (InstrucaoEnquanto)instrucao;

            while((int)InterpretarExpressao(enquanto.Expressao) != 0)
            {
                InterpretarEscopo(enquanto.Escopo);
            }
        }

        else if (instrucao is InstrucaoRomper)
        {
            return instrucao;
        }

        else if (instrucao is InstrucaoRetornar)
        {
            return instrucao;
        }

        return null;
    }

    private int InterpretarEscopo(Escopo escopo, List<Variavel> variaveis = null)
    {
        _enderecosIniciaisEscopos.Add(_programa.Variaveis.Count -1);

        if(variaveis != null)
        {
            for(int i = 0; i < variaveis.Count; i++)
            {
                _programa.Variaveis.Add(variaveis[i].Identificador, variaveis[i]);
            }
        }
        
        for(int i = 0; i < escopo.Instrucoes.Count; i++)
        {
            var instrucao = InterpretarInstrucao(escopo.Instrucoes[i]);
            if(instrucao is InstrucaoRetornar)
            {
                var retorno = (InstrucaoRetornar)instrucao;
                var resultado = InterpretarExpressao(retorno.Expressao);

                if(variaveis != null)
                {
                    for(int j = 0; j < variaveis.Count; j++)
                    {
                        _programa.Variaveis.Remove(variaveis[j].Identificador);
                    }
                }

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
        {
            var termo = (ExpressaoTermo)expressao;

            return ExtrairValorTermo(termo);
        }

        else if(expressao is ExpressaoBinaria)
        {
            var binaria = (ExpressaoBinaria)expressao;

            var esq = (ExpressaoTermo)binaria.Esquerda;
            var dir = binaria.Direita;

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

    // TODO: Só um protótipo pra testes. PRECISA DE REFATORAÇÃO!
    private void DefinirVariavel(string identificador, bool declaracao, bool constante, object valor, TokenTipo tipo, Expressao expressaoIndiceVetor)
    {
        if(string.IsNullOrWhiteSpace(identificador))
        {
            new Erro("Identificador inválido!").LancarErro();
        }

        if(declaracao && _programa.VariavelExiste(identificador))
        {
            new ErroVariavelJaDeclarada(identificador).LancarErro();
        }

        if(_programa.Variaveis.ContainsKey(identificador))
        {
            if(_programa.Variaveis[identificador].Constante)
                new ErroModificacaoConstante(identificador).LancarErro();
        }

        Token token = null;

        if (valor is Token)
            token = (Token)valor;

        else if(valor is Expressao)
        {
            var expressaoValorFinal = (Expressao)valor;
            object resultado = InterpretarExpressao(expressaoValorFinal);

            if(expressaoIndiceVetor != null)
            {
                int indiceVetor = (int)InterpretarExpressao(expressaoIndiceVetor);
                var vetor = (Token[])_programa.Variaveis[identificador].Valor;

                vetor[indiceVetor].Valor = resultado;
                return;
            }

            // declarando um Vetor (var x = [10])
            if (tipo == TokenTipo.Vetor)
            {
                Token[] tokens = new Token[(int)resultado];
                for(int i =0; i < tokens.Length; i++)
                    tokens[i] = new Token(TokenTipo.Nulo, 0);

                token = new Token(tipo, 0, tokens);
                
            }
            else
                token = new Token(TokenTipo.NumeroLiteral, 0, resultado);
        }

        var variavel = new Variavel(identificador, token, constante);

        _programa.Variaveis[identificador] = variavel;
    }

    private object ExtrairValorTermo(ExpressaoTermo termo)
    {
        if(termo.ChamadaFuncao != null)
        {
            InterpretarInstrucao(termo.ChamadaFuncao);
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
                if(!_programa.VariavelExiste((string)termo.Valor)) return 0; // TODO: Lançar erro
                return int.Parse(_programa.Variaveis[(string)termo.Valor].Valor.ToString());
            case TokenTipo.CaractereLiteral:
                return (int)termo.Token.Valor.ToString()[0];
        }

        return (int)termo.Valor;
    }

}