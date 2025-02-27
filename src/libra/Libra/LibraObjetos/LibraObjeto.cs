namespace Libra;

public abstract class LibraObjeto
{
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
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraObjeto Sub(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraObjeto Div(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraObjeto Mult(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraObjeto Pot(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraObjeto Resto(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraInt Igual(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraInt MaiorQue(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraInt MaiorIgualQue(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraInt MenorQue(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraInt MenorIgualQue(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraInt E(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }

    public virtual LibraInt Ou(LibraObjeto outro)
    {
        throw new Erro($"Operação Inválida: ({this}), ({outro})");
    }
}