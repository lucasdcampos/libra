// VM simples temporária para testar o gerador de bytecode.
// A VM real será feita em C

using System;
using System.Collections.Generic;

namespace Libra.VM
{
    public class VM
    {
        private readonly List<InstrucaoVM> _programa;
        private readonly Stack<object> _pilha = new();
        private readonly Dictionary<string, object> _variaveis = new();
        public VM(List<InstrucaoVM> programa)
        {
            _programa = programa;
        }

        public void Executar()
        {
            for (int ip = 0; ip < _programa.Count; ip++)
            {
                var instr = _programa[ip];
                switch (instr.Op)
                {
                    case Opcode.EMPILHAR:
                        _pilha.Push(instr.Argumento);
                        break;

                    case Opcode.SOMAR:
                    case Opcode.SUBTRAIR:
                    case Opcode.MULTIPLICAR:
                    case Opcode.DIVIDIR:
                    case Opcode.POTENCIA:
                    case Opcode.RESTO:
                        ExecutarOperacao(instr.Op);
                        break;

                    case Opcode.ENCERRAR:
                        if (_pilha.Count > 0)
                            Console.WriteLine(_pilha.Peek());
                        return;

                    case Opcode.ARMAZENAR:
                        {
                            // instr.Argumento é o nome da variável
                            string nome = (string)instr.Argumento;
                            object valor = _pilha.Pop(); // remove o valor do topo da pilha
                            _variaveis[nome] = valor;    // armazena no dicionário de variáveis
                            break;
                        }

                    case Opcode.CARREGAR:
                        {
                            string nome = (string)instr.Argumento;
                            if (!_variaveis.TryGetValue(nome, out var valor))
                                throw new Exception($"Variável '{nome}' não definida.");
                            _pilha.Push(valor);
                            break;
                        }

                    case Opcode.SALTAR_SE_FALSO:
                    {
                        if (instr.Argumento == null) 
        throw new Exception($"Erro de compilação: Salto para endereço nulo no IP {ip}");

    dynamic condicao = _pilha.Pop();
                        bool falso = condicao == null || condicao.Equals(0); // 0 ou null = falso
                        if (falso)
                        {
                            ip = (int)instr.Argumento - 1; // -1 porque o for vai incrementar
                        }
                        break;
                    }

                    case Opcode.SALTAR:
                    {
                        ip = (int)instr.Argumento - 1; // pulo incondicional
                        break;
                    }

                    default:
                        throw new Exception($"Opcode desconhecido: {instr.Op}");
                }
            }
        }

        private void ExecutarOperacao(Opcode op)
        {
            if (_pilha.Count < 2)
                throw new Exception("Pilha com elementos insuficientes para operação");

            dynamic b = _pilha.Pop();
            dynamic a = _pilha.Pop();
            dynamic resultado = op switch
            {
                Opcode.SOMAR => a + b,
                Opcode.SUBTRAIR => a - b,
                Opcode.MULTIPLICAR => a * b,
                Opcode.DIVIDIR => a / b,
                Opcode.POTENCIA => Math.Pow(a, b),
                Opcode.RESTO => a % b,
                _ => throw new Exception($"Operação não implementada: {op}")
            };

            _pilha.Push(resultado);
        }

    }
}
