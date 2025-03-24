using Libra.Arvore;

namespace Libra;

public class LibraObjeto
{
    public LibraObjeto(string nome, Variavel[] propriedades, Funcao[] metodos)
    {
        Nome = nome;
        Propriedades = new();
        Metodos = new();

        for(int i = 0; i < propriedades.Length; i++)
        {
            DeclararPropriedade(propriedades[i]);
        }
        for(int i = 0; i < metodos.Length; i++)
        {
            DefinirMetodo(metodos[i]);
        }
    }

    public virtual string Nome { get; protected set; }
    public virtual Dictionary<string, Variavel> Propriedades { get; protected set; }
    public virtual Dictionary<string, Funcao> Metodos { get; protected set; }

    public virtual LibraObjeto Converter(string novoTipo)
    {
        throw new ErroConversao(Nome, novoTipo, Interpretador.LocalAtual);
    }

    protected void DeclararPropriedade(Variavel prop)
    {
        if(Propriedades.ContainsKey(prop.Identificador))
            throw new ErroVariavelJaDeclarada(prop.Identificador);
        
        Propriedades.Add(prop.Identificador, prop);
    }

    protected void DefinirMetodo(Funcao metodo)
    {
        if(Metodos.ContainsKey(metodo.Identificador))
            throw new ErroFuncaoJaDefinida(metodo.Identificador);
        
        Metodos.Add(metodo.Identificador, metodo);
    }

    public virtual object ObterValor() 
    {
        throw new Erro($"Não foi possível obter o valor de {Nome}");
    }

    public virtual LibraInt ObterTamanhoEmBytes() 
    {
        throw new Erro($"Não é possível calcular tamanho em bytes de {Nome}");
    }

    public object paraTexto(object[] args)
    {
        return ObterValor().ToString();
    }

    public LibraObjeto AcessarPropriedade(string prop)
    {
        if(!Propriedades.ContainsKey(prop))
            throw new ErroVariavelNaoDeclarada(prop);
        return Propriedades[prop].Valor;
    }

    public void AtribuirPropriedade(string ident, LibraObjeto novoValor)
    {
        if(!Propriedades.ContainsKey(ident))
            throw new ErroVariavelNaoDeclarada(ident);
        Propriedades[ident].AtualizarValor(novoValor);
    }

    public virtual LibraObjeto ChamarMetodo(ExpressaoChamadaFuncao chamada, string quemChamou = "")
    {
        if(!Metodos.ContainsKey(chamada.Identificador))
            throw new ErroFuncaoNaoDefinida(chamada.Identificador);
        var args = chamada.Argumentos.ToList<Expressao>();
        args.Insert(0, new ExpressaoVariavel(new Token(TokenTipo.Identificador, new LocalToken(), quemChamou)));
        return new Interpretador().ExecutarFuncao(Metodos[chamada.Identificador], args.ToArray());
    }

    // Inicializa um novo Objeto de acordo com o tipo especificado
    public static LibraObjeto Inicializar(string tipo)
    {
        switch(tipo)
        {
            case "Int": return new LibraInt(0);
            case "Real": return new LibraReal(0);
            case "Texto": return new LibraTexto("");
            case "Vetor": return new LibraVetor(0);
        }
        
        return new LibraNulo();
    }

    public static LibraObjeto ParaLibraObjeto(object valor)
    {
        if(valor == null)
            return new LibraNulo();
            
        if(valor is LibraObjeto valorLibraObjeto)
            return valorLibraObjeto; // o valor JÁ é LibraObjeto
        else if(valor is Variavel valorVariavel)
            return ParaLibraObjeto(valorVariavel.Valor);
        else if(valor is int valorInt)
            return new LibraInt(valorInt);
        else if(valor is double valorDouble)
            return new LibraReal(valorDouble);
        else if(valor is string valorString)
            return new LibraTexto(valorString);
        else if(valor is LibraObjeto[] valorVetor)
            return new LibraVetor(valorVetor);

        Ambiente.Msg($"Tipo: {valor.GetType()}");
        throw new ErroAcessoNulo($" Causa: Impossível converter {valor.ToString()} para Objeto");
    }

    // Cabe aos objetos que herdam essa classe implementar os operadores necessários
    public virtual LibraObjeto Soma(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) + ({outro})");
    }

    public virtual LibraObjeto Sub(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) - ({outro})");
    }

    public virtual LibraObjeto Div(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) / ({outro})");
    }

    public virtual LibraObjeto Mult(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) * ({outro})");
    }

    public virtual LibraObjeto Pot(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) ^ ({outro})");
    }

    public virtual LibraObjeto Resto(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) % ({outro})");
    }

    public virtual LibraInt Igual(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) == ({outro})");
    }

    public virtual LibraInt MaiorQue(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) > ({outro})");
    }

    public virtual LibraInt MaiorIgualQue(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) >= ({outro})");
    }

    public virtual LibraInt MenorQue(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) < ({outro})");
    }

    public virtual LibraInt MenorIgualQue(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) <= ({outro})");
    }

    public virtual LibraInt E(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) e ({outro})");
    }

    public virtual LibraInt Ou(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}) ou ({outro})");
    }
}