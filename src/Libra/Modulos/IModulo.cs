using Libra.Arvore;
using Libra.Runtime;

namespace Libra.Modulos;

// Módulo é uma biblioteca de código interna que pode ser acessada diretamente da Libra
public interface IModulo
{
    public void RegistrarFuncoes(Ambiente ambiente);
}