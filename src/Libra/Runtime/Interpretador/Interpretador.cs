using Libra.Arvore;
using Libra.Runtime;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Libra;

public sealed class Interpretador : IVisitor
{
    public LocalFonte LocalAtual => _local;
    public InterpretadorFlags Flags { get; }
    public LibraObjeto Saida => _ultimoRetorno ?? LibraObjeto.Inicializar("Nulo");
    private LocalFonte _local = new LocalFonte();
    private LibraObjeto _ultimoRetorno;
    
    public Interpretador(InterpretadorFlags flags = null)
    {
        Flags = flags == null ? InterpretadorFlags.Padrao() : flags;
    }

    public object VisitarPrograma(Programa programa)
    {
        VisitarInstrucoes(programa.Instrucoes);

        return _ultimoRetorno;
    }

    public void VisitarInstrucoes(Instrucao[] instrucoes)
    {
        for (int i = 0; i < instrucoes.Length; i++)
        {
            instrucoes[i].Aceitar(this);
        }
    }

    public object VisitarInstrucaoExpressao(InstrucaoExpressao instrucao)
    {
        if (instrucao.Expressao == null)
            return null;

        _ultimoRetorno = VisitarExpressao(instrucao.Expressao);

        return null;
    }

    public object VisitarTentar(Tentar instrucao)
    {
        try
        {
            Ambiente.Pilha.EmpilharEscopo();
            VisitarInstrucoes(instrucao.InstrucoesTentar);
            Ambiente.Pilha.DesempilharEscopo();
        }
        catch (Erro err)
        {
            Ambiente.Pilha.DesempilharEscopo(); // Desempilhando escopo do "Tentar"

            Ambiente.Pilha.EmpilharEscopo();
            Ambiente.Pilha.DefinirVariavel(instrucao.VariavelErro, new LibraTexto(err.Mensagem), TiposPadrao.Texto, true);
            VisitarInstrucoes(instrucao.InstrucoesCapturar);
            Ambiente.Pilha.DesempilharEscopo();
        }

        return null;

    }

    public object VisitarAtribProp(AtribuicaoPropriedade instrucao)
    {
        var obj = LibraObjeto.ParaLibraObjeto(VisitarExpressao(instrucao.Alvo));

        obj.AtribuirPropriedade(instrucao.Alvo.Propriedade, VisitarExpressao(instrucao.Expressao));

        return null;
    }

    public object VisitarAtribIndice(AtribuicaoIndice instrucao)
    {
        string identificador = instrucao.Identificador;
        int indice = VisitarExpressao<LibraInt>(instrucao.ExpressaoIndice).Valor;
        LibraObjeto expressao = VisitarExpressao(instrucao.Expressao);

        Ambiente.Pilha.ModificarVetor(identificador, indice, expressao);

        return null;
    }

    public object VisitarRetorno(Retornar instrucao)
    {
        object resultadoExpressao = VisitarExpressao(((Retornar)instrucao).Expressao);
        _ultimoRetorno = LibraObjeto.ParaLibraObjeto(resultadoExpressao);
        throw new ExcecaoRetorno(resultadoExpressao);


        return null;
    }

    public object VisitarSe(Se se)
    {
        if (VisitarExpressao<LibraInt>(se.Condicao).Valor != 0)
        {
            Ambiente.Pilha.EmpilharEscopo();
            VisitarInstrucoes(se.Corpo.ToArray());
            Ambiente.Pilha.DesempilharEscopo();
            return null;
        }

        if (se.ListaSenaoSe == null || se.ListaSenaoSe.Count == 0)
            return null;

        foreach (var inst in se.ListaSenaoSe)
        {
            if (VisitarExpressao<LibraInt>(inst.Condicao).Valor != 0)
            {
                Ambiente.Pilha.EmpilharEscopo();
                VisitarInstrucoes(inst.Corpo.ToArray());
                Ambiente.Pilha.DesempilharEscopo();

                return null;
            }
        }

        return null;
    }

    public object VisitarEnquanto(Enquanto enquanto)
    {
        // TODO: Otimizar casos em que não é necessário calcular a expressão toda vez,
        // como em "enquanto 1", por exemplo.
        while (VisitarExpressao<LibraInt>(enquanto.Expressao).Valor != 0)
        {
            Ambiente.Pilha.EmpilharEscopo();
            foreach (var i in enquanto.Instrucoes)
            {
                try
                {
                    i.Aceitar(this);
                }
                catch (ExcecaoRomper e)
                {
                    Ambiente.Pilha.DesempilharEscopo();
                    return null;
                }
                // TODO: Adicionar 'continuar'
            }
            Ambiente.Pilha.DesempilharEscopo();
        }

        return null;
    }

    public object VisitarParaCada(ParaCada instrucao)
    {
        var expr = instrucao.Vetor;

        var vetor = VisitarExpressao<LibraVetor>(expr);

        foreach (var item in vetor.Valor)
        {
            Ambiente.Pilha.EmpilharEscopo();
            try
            {
                Ambiente.DefinirGlobal(instrucao.Identificador.Valor.ToString(), item);
                VisitarInstrucoes(instrucao.Instrucoes);
            }
            catch (ExcecaoRomper e)
            {
                Ambiente.Pilha.DesempilharEscopo();
                return null;
            }
            // TODO: Adicionar 'continuar'
        }

        Ambiente.Pilha.DesempilharEscopo();

        return null;
    }

    public object VisitarFuncao(DefinicaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if (string.IsNullOrWhiteSpace(identificador))
            throw new Erro("Identificador inválido!", _local);

        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros, funcao.TipoRetorno);

        Ambiente.Pilha.DefinirVariavel(identificador, novaFuncao, TiposPadrao.Func, true);

        return null;
    }

    public LibraObjeto ExecutarFuncaoEmbutida(FuncaoNativa funcao, Expressao[] argumentos) 
    {
        List<object> valoresArgumentos = new List<object>();

        for(int i = 0; i < argumentos.Length; i++)
        {
            valoresArgumentos.Add(VisitarExpressao(argumentos[i]).ObterValor());
        }

        var resultadoFuncao = funcao.Executar(valoresArgumentos.ToArray());
        var objeto = LibraObjeto.ParaLibraObjeto(resultadoFuncao);

        return objeto;
    }

    public LibraObjeto VisitarConstrutorClasse(string nome, Expressao[] expressoes, string quemChamou = "")
    {
        // TODO: Pode dar erro!
        Classe tipo = (Classe)Ambiente.Pilha.ObterVariavel(nome).Valor;

        // TODO: Arrumar, nunca vi um código tão porcaria em toda a minha vida
        List<Variavel> vars = new();
        foreach(var i in tipo.Variaveis)
        {
            vars.Add(new Variavel(i.Identificador, VisitarExpressao(i.Expressao), i.TipoVar, i.Constante));
        }
        foreach(var i in tipo.Funcoes)
        {
            vars.Add(new Variavel(
                i.Identificador,
                new Funcao(i.Identificador, i.Instrucoes, i.Parametros, i.TipoRetorno),
                TiposPadrao.Func,
                true
            ));
        }

        var obj = new LibraObjeto(nome, vars.ToArray(), expressoes);
        
        return obj;
    }

    public LibraObjeto ExecutarFuncao(Funcao funcao, Expressao[] argumentos)
    {
        if(funcao is FuncaoNativa nativa)
        {
            return ExecutarFuncaoEmbutida(nativa, argumentos);
        }
        
        var qtdParametros = funcao.Parametros.Length;

        if (argumentos.Length != qtdParametros)
            throw new ErroEsperadoNArgumentos(funcao.Identificador, qtdParametros, argumentos.Length, _local);

        Ambiente.Pilha.EmpilharEscopo(funcao.Identificador, _local); // empurra o novo Escopo da função

        try 
        {
            // Adicionando os argumentos ao Escopo
            for (int i = 0; i < argumentos.Length; i++)
            {
                string ident = funcao.Parametros[i].Identificador;
                var obj = VisitarExpressao(argumentos[i]);
                
                if(funcao.Parametros[i].Tipo != TiposPadrao.Objeto && funcao.Parametros[i].Tipo != obj.Nome)
                    obj = obj.Converter(funcao.Parametros[i].Tipo);

                Ambiente.Pilha.DefinirVariavel(ident, obj, funcao.Parametros[i].Tipo);
            }

            VisitarInstrucoes(funcao.Instrucoes);
        }
        catch(ExcecaoRetorno retorno)
        {
            var resultado = LibraObjeto.ParaLibraObjeto(retorno.Valor);
            if(funcao.TipoRetorno != resultado.Nome && funcao.TipoRetorno != TiposPadrao.Objeto)
            {
                return resultado.Converter(funcao.TipoRetorno);
            }
            
            return resultado;
        }
        finally
        {
            Ambiente.Pilha.DesempilharEscopo(); // Removendo o Escopo da Pilha
        }

        // Caso a função não tenha um retorno explicito
        return LibraObjeto.Inicializar(TiposPadrao.Nulo);
    }

    public LibraObjeto VisitarChamadaFuncao(ExpressaoChamadaFuncao chamada)
    {
        var argumentos = chamada.Argumentos;

        var v = Ambiente.Pilha.ObterVariavel(chamada.Identificador);

        if(v.Valor is Classe)
            return VisitarConstrutorClasse(chamada.Identificador, chamada.Argumentos.ToArray());

        return ExecutarFuncao((Funcao)v.Valor, chamada.Argumentos);
    }

    // TODO: É isso?
    public object VisitarClasse(DefinicaoTipo i)
    {
        Ambiente.Pilha.DefinirVariavel(i.Identificador, new Classe(i.Identificador, i.Variaveis, i.Funcoes), i.Identificador);

        return null;
    }

    public object VisitarAtribVar(AtribuicaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _local);

        LibraObjeto resultado = VisitarExpressao(i.Expressao);

        Ambiente.Pilha.AtualizarVariavel(i.Identificador, resultado);

        resultado.Construtor(i.Identificador);
        
        return resultado;
    }

    public object VisitarDeclVar(DeclaracaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new Erro("Identificador inválido!", _local);

        LibraObjeto resultado = VisitarExpressao(i.Expressao);

        Ambiente.Pilha.DefinirVariavel(i.Identificador, resultado, i.TipoVar, i.Constante);

        resultado.Construtor(i.Identificador);

        return resultado;
    }

    public object[] VisitarVetor(ExpressaoNovoVetor expressao)
    {
        int indice = VisitarExpressao<LibraInt>(expressao.Expressao).Valor;
        return new LibraObjeto[indice];
    }

    public object[] VisitarInicializacaoVetor(ExpressaoInicializacaoVetor expressao)
    {
        int tamanho = expressao.Expressoes.Count;
        var vetor = new object[tamanho];

        for(int i = 0; i < tamanho; i++)
        {
            vetor[i] = VisitarExpressao(expressao.Expressoes[i]);
        }

        return vetor;
    }

    public LibraObjeto VisitarExpressao(Expressao expressao)
    {
        if(expressao == null)
            return new LibraNulo();

        _local = expressao.Local;
        
        return LibraObjeto.ParaLibraObjeto(expressao.Aceitar(this));
    }

    public T VisitarExpressao<T>(Expressao expressao)
    {
        var resultado = VisitarExpressao(expressao);

        if (resultado is T t) return t;

        throw new ErroAcessoNulo($" Expressão retornou {resultado.GetType()} ao invés do esperado", _local);
    }

    public LibraObjeto VisitarExpressaoLiteral(ExpressaoLiteral expressao)
    {
        return expressao.Valor;
    }
    
    public LibraObjeto VisitarExpressaoBinaria(ExpressaoBinaria expressao)
    {
        var a = VisitarExpressao(expressao.Esquerda);
        var b = VisitarExpressao(expressao.Direita);

        return expressao.Operador.Tipo switch
        {
            TokenTipo.OperadorSoma => a.Soma(b),
            TokenTipo.OperadorSub => a.Sub(b),
            TokenTipo.OperadorMult => a.Mult(b),
            TokenTipo.OperadorDiv => a.Div(b),
            TokenTipo.OperadorPot => a.Pot(b),
            TokenTipo.OperadorResto => a.Resto(b),
            TokenTipo.OperadorComparacao => a.Igual(b),
            TokenTipo.OperadorDiferente => new LibraInt(LibraUtil.NegarInteiroLogico(a.Igual(b).Valor)), // Vendo isso aqui meses depois, genial!
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
        var v = Ambiente.Pilha.ObterVariavel(expressao.Identificador.Valor.ToString());
        return v.Valor;
    }

    public LibraObjeto VisitarExpressaoNovoVetor(ExpressaoNovoVetor expressao)
    {
        var vetor = new LibraObjeto[VisitarExpressao<LibraInt>(expressao.Expressao).Valor];
        return LibraObjeto.ParaLibraObjeto(vetor); // Converte para LibraVetor
    }

    public LibraObjeto VisitarExpressaoInicializacaoVetor(ExpressaoInicializacaoVetor expressao)
    {
        var arr = new LibraObjeto[expressao.Expressoes.Count];

        for(int i = 0; i < expressao.Expressoes.Count; i++)
        {
            arr[i] = VisitarExpressao(expressao.Expressoes[i]);
        }

        return LibraObjeto.ParaLibraObjeto(arr);
    }

    public LibraObjeto VisitarExpressaoUnaria(ExpressaoUnaria expressao)
    {
        switch(expressao.Operador.Tipo)
        {
            case TokenTipo.OperadorNeg:
                return LibraObjeto.ParaLibraObjeto(VisitarExpressao<LibraInt>(expressao.Operando).Valor);
            case TokenTipo.OperadorSub:
                return LibraObjeto.ParaLibraObjeto(VisitarExpressao(expressao.Operando).Mult(new LibraInt(-1)));
        }

        throw new Erro("Operador unário não implementado", expressao.Operador.Local);
    }

    public LibraObjeto VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao)
    {
        string ident = expressao.Identificador;
        int indice = VisitarExpressao<LibraInt>(expressao.Expressao).Valor;

        var variavel = Ambiente.Pilha.ObterVariavel(ident);

        if(variavel.Valor is LibraVetor vetor)
        {
            if (indice < 0 || indice >= vetor.Valor.Length)
                throw new ErroIndiceForaVetor($"{ident}[{indice.ToString()}]", _local);
            return vetor.Valor[indice];
        }
        if(variavel.Valor is LibraTexto texto)
        {
            if (indice < 0 || indice >= texto.Valor.Length)
                throw new ErroIndiceForaVetor($"{ident}[{indice.ToString()}]", _local);
            return new LibraTexto(texto.Valor[indice].ToString());
        }

        throw new ErroAcessoNulo($" {variavel.Valor} não é um Vetor");
    }

    public LibraObjeto VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao)
    {
        return VisitarChamadaFuncao(expressao);
    }

    public LibraObjeto VisitarExpressaoPropriedade(ExpressaoPropriedade expressao)
    {
        var obj = LibraObjeto.ParaLibraObjeto(expressao.Alvo.Aceitar(this));

        return obj.AcessarPropriedade(expressao.Propriedade);
    }

    public LibraObjeto VisitarExpressaoChamadaMetodo(ExpressaoChamadaMetodo expressao)
    {
        var obj = LibraObjeto.ParaLibraObjeto(expressao.Alvo.Aceitar(this));

        return obj.ChamarMetodo(expressao.Chamada);

    }

    public object VisitarRomper(Romper instrucao)
    {
        throw new ExcecaoRomper();
    }

    public object VisitarContinuar(Continuar instrucao)
    {
        throw new NotImplementedException();
    }

    public object VisitarSenaoSe(SenaoSe instrucao)
    {
        throw new NotImplementedException();
    }
}