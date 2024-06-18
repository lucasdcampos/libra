```
<programa> ::= <comandos>

<comandos> ::= <comando> | <comando> <comandos>

<comando> ::= <comando_condicional>
            | <comando_repeticao>
            | <comando_exibir>
            | <comando_sair>
            | <declaracao_variavel>
            | <expressao>
            | <declaracao_funcao>
            | <tipo_valor>
            | <bytes_tipo>

<comando_condicional> ::= "se" <condicao> "entao" <comandos> "fim"

<comando_repeticao> ::= "enquanto" <condicao> <comandos> "fim"

<comando_exibir> ::= "exibir" "(" <texto> ")"

<comando_sair> ::= "sair" "(" <expressao> ")"

<declaracao_variavel> ::= "var" <identificador> "=" <expressao>

<declaracao_funcao> ::= "funcao" <identificador> "(" [<lista_parametros>] ")" <comandos> "fim"

<lista_parametros> ::= <identificador> | <identificador> "," <lista_parametros>

<condicao> ::= <expressao>
            | <expressao> <operador_relacional> <expressao>
            | <expressao> <operador_logico> <expressao>

<expressao> ::= <termo> | <termo> <operador> <expressao>

<termo> ::= <identificador> | <numero> | "(" <expressao> ")"

<operador> ::= "+" | "-" | "*" | "/"

<operador_relacional> ::= "==" | "!=" | "<" | ">" | "<=" | ">="

<operador_logico> ::= "e" | "ou"

<identificador> ::= [a-zA-Z][a-zA-Z0-9_]*

<numero> ::= [0-9]+

<texto> ::= [^"]*

<comentario> ::= "--" [^newline]*

<entrada_dados> ::= "ler" "(" <identificador> ")"

<tipo_valor> ::= "tipo" "(" <valor_ou_identificador> ")"

<valor_ou_identificador> ::= <identificador> | <numero>

<bytes_tipo> ::= "bytes" "(" <tipo_dado> ")"

<tipo_dado> ::= "numero" | "texto" | "booleano" | "array" "(" <tipo_dado> ")"
```
