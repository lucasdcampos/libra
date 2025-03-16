namespace Libra;

public class LibraInt : LibraObjeto
{
    public int Valor { get; private set; }

    public LibraInt(int valor)
    {
        Valor = valor;
    }

    public LibraInt(bool valor)
    {
        Valor = valor ? 1 : 0;
    }

    public override string ToString()
    {
        return "Int";
    }

    public override object ObterValor()
    {
        return Valor;
    }

    public override LibraInt ObterTamanhoEmBytes()
    {
        return new LibraInt(4);
    }

    public override LibraObjeto Soma(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor + libraInt.Valor),
            (LibraReal libraReal) => new LibraReal(Valor + libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}+{outro.ToString()}")
        };
    }

    public override LibraObjeto Sub(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor - libraInt.Valor),
            (LibraReal libraReal) => new LibraReal(Valor - libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}-{outro.ToString()}")
        };
    }

    public override LibraObjeto Mult(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor * libraInt.Valor),
            (LibraReal libraReal) => new LibraReal(Valor * libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}*{outro.ToString()}")
        };
    }

    public override LibraObjeto Div(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) when libraInt.Valor != 0 => new LibraReal(Valor / libraInt.Valor),
            (LibraReal libraReal) when libraReal.Valor != 0 => new LibraReal(Valor / libraReal.Valor),
            (LibraInt libraInt) => throw new ErroDivisaoPorZero(),
            (LibraReal libraReal) => throw new ErroDivisaoPorZero(),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}/{outro.ToString()}")
        };
    }

    public override LibraObjeto Pot(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraReal(Math.Pow(Valor, libraInt.Valor)),
            (LibraReal libraReal) => new LibraReal(Math.Pow(Valor, libraReal.Valor)),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}^{outro.ToString()}")
        };
    }

    public override LibraObjeto Resto(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor % libraInt.Valor),
            (LibraReal libraReal) => new LibraReal(Valor % libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}%{outro.ToString()}")
        };
    }

    public override LibraInt MaiorQue(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor > libraInt.Valor),
            (LibraReal libraReal) => new LibraInt(Valor > libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}%{outro.ToString()}")
        };
    }

    public override LibraInt MaiorIgualQue(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor >= libraInt.Valor),
            (LibraReal libraReal) => new LibraInt(Valor >= libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}%{outro.ToString()}")
        };
    }

    public override LibraInt MenorQue(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor < libraInt.Valor),
            (LibraReal libraReal) => new LibraInt(Valor < libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}%{outro.ToString()}")
        };
    }

    public override LibraInt MenorIgualQue(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor <= libraInt.Valor),
            (LibraReal libraReal) => new LibraInt(Valor <= libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}%{outro.ToString()}")
        };
    }

    public override LibraInt Igual(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor == libraInt.Valor),
            (LibraReal libraReal) => new LibraInt(Valor == libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}%{outro.ToString()}")
        };
    }

    public override LibraInt E(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor != 0 && libraInt.Valor != 0),
            (LibraReal libraReal) => new LibraInt(Valor != 0 && libraReal.Valor != 0),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}%{outro.ToString()}")
        };
    }

    public override LibraInt Ou(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraInt(Valor != 0 || libraInt.Valor != 0),
            (LibraReal libraReal) => new LibraInt(Valor != 0 || libraReal.Valor != 0),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}%{outro.ToString()}")
        };
    }

}