namespace Libra;

public class LibraReal : LibraObjeto
{
    public double Valor { get; private set; }

    public LibraReal(double valor) : base("Real", new Variavel[0])
    {
        Valor = valor;
    }

    public override object ObterValor()
    {
        return Valor;
    }

    public override LibraInt ObterTamanhoEmBytes()
    {
        return new LibraInt(8);
    }

    public override LibraObjeto Soma(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraReal(Valor + libraInt.Valor),
            (LibraReal libraReal) => new LibraReal(Valor + libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}+{outro.ToString()}")
        };
    }

    public override LibraObjeto Sub(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraReal(Valor - libraInt.Valor),
            (LibraReal libraReal) => new LibraReal(Valor - libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}-{outro.ToString()}")
        };
    }

    public override LibraObjeto Mult(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraReal(Valor * libraInt.Valor),
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
            (LibraReal libraReal) => new LibraReal(Valor * libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}^{outro.ToString()}")
        };
    }

    public override LibraObjeto Resto(LibraObjeto outro)
    {
        return (outro) switch
        {
            (LibraInt libraInt) => new LibraReal(Valor % libraInt.Valor),
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
            (LibraNulo libraNulo) => new LibraInt(0),
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