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
        return Valor.ToString();
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
            (LibraInt libraInt) when libraInt.Valor != 0 => new LibraInt(Valor / libraInt.Valor),
            (LibraReal libraReal) when libraReal.Valor != 0 => new LibraReal(Valor / libraReal.Valor),
            _ => throw new Erro($"Não é possível calcular {this.ToString()}/{outro.ToString()} ou divisão por zero.")
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

}