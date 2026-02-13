using Libra.Arvore;
using Libra.Runtime;

namespace Libra.Arvore;

public abstract class Nodo<T>
{
    public LocalFonte Local { get; protected set; }
    public abstract T Aceitar(IVisitor visitor);
}