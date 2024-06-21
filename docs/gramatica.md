$$
\begin{align}
[\text{Programa}] &\to [\text{Instrucao}]*\\
[\text{Instrucao}] &\to
\begin{cases}
    \text{sair} ([\text{Expressao}]);\\
    \text{var}\space [\text{Identificador}] =[\text{Expressao}];\\
    [\text{Identificador}] =[\text{Expressao}];\\
    \text{exibir}([\text{Texto}]);\\
    \text{se}\space ([\text{Expressao}])\space \text{entao}\space [\text{Instrucao}]*&\text{fim}
\end{cases}\\
[\text{Expressao}] &\to
\begin{cases}
    [\text{Termo}]\\
    [\text{ExpressaoBinaria}]
\end{cases}\\
[\text{Termo}] &\to
\begin{cases}
    [\text{NumeroLiteral}]\\
    [\text{Identificador}]
\end{cases}\\
[\text{ExpressaoBinaria}] &\to
\begin{cases}
    [\text{Expressao}] + [\text{Expressao}]\\
    [\text{Expressao}] - [\text{Expressao}]\\
    [\text{Expressao}] * [\text{Expressao}]\\
    [\text{Expressao}] / [\text{Expressao}]\\
    
\end{cases}\\
[\text{Texto}] &\to
\begin{cases}
    [\text{TextoLiteral}]\\
    [\text{BoolLiteral}]\\
    [\text{Expressao}]\\
\end{cases}\\
[\text{NumeroLiteral}] &\to [0-9]+\\
[\text{TextoLiteral}] &\to \text{``[A-Za-z0-9\_]``*}
\end{align}
$$

