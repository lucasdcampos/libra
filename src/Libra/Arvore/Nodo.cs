namespace Libra.Arvore;

public abstract class Nodo
{
    public abstract T Aceitar<T>(IVisitor<T> visitor);
}
