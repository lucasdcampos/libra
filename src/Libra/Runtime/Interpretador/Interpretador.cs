// O Interpretador da Libra será descontinuado em favor do novo Compilador
// Será mantido apenas para compatibilidade com versões antigas, mas não receberá mais atualizações ou correções de bugs

using Libra.Arvore;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.IO;
using System.Text.Json;

namespace Libra.Runtime;

public sealed class Interpretador : IVisitor<LibraObjeto>
{
    public LocalFonte LocalAtual => _local;
    public InterpretadorFlags Flags { get; }
    public LibraObjeto Saida => _ultimoRetorno ?? LibraObjeto.Inicializar("Nulo");
    private LocalFonte _local = new LocalFonte();
    private LibraObjeto _ultimoRetorno;
    private Ambiente _ambiente;
    private Dictionary<string, Modulo> _modulosImportados;

    public Interpretador(InterpretadorFlags flags = null, Dictionary<string, Modulo> modulosImportados = null, ILogger logger = null)
    {
        Flags = flags == null ? InterpretadorFlags.Padrao() : flags;
        _ambiente = new Ambiente(logger ?? new ConsoleLogger(), Flags.ModoSeguro);
        _modulosImportados = modulosImportados ?? new Dictionary<string, Modulo>();
    }

    public Ambiente Ambiente => _ambiente;

    public void LimparSaida()
    {
        _ultimoRetorno = null;
    }

    public LibraObjeto VisitarPrograma(Programa programa)
    {
        VisitarInstrucoes(programa.Instrucoes);

        return _ultimoRetorno;
    }

    public LibraObjeto VisitarImportar(InstrucaoImportar instrucao)
    {
        string caminho = instrucao.Caminho;
        string? arquivoCompleto = EncontrarCaminhoBiblioteca(caminho, instrucao.Local);

        string codigo;
        string chaveModulo;
        string nomeArquivo;
        string diretorioArquivo;

        if (arquivoCompleto != null)
        {
            codigo = File.ReadAllText(arquivoCompleto).ReplaceLineEndings("\n");
            chaveModulo = arquivoCompleto;
            nomeArquivo = Path.GetFileName(arquivoCompleto);
            diretorioArquivo = Path.GetDirectoryName(arquivoCompleto) ?? "";
        }
        // Fallback: biblioteca padrão embutida no assembly (ex.: playground WASM sem filesystem).
        else if (BibliotecaEmbutida.TryLer(caminho, out var codigoEmbutido, out var arquivoEmbutido))
        {
            codigo = codigoEmbutido;
            chaveModulo = "embutido:" + arquivoEmbutido;
            nomeArquivo = arquivoEmbutido;
            diretorioArquivo = "";
        }
        else
        {
            throw new ErroImportacao(caminho, instrucao.Local);
        }

        if (_modulosImportados.TryGetValue(chaveModulo, out var moduloExistente))
        {
            if (instrucao.Identificador != null)
                _ambiente.Pilha.DefinirVariavel(instrucao.Identificador, moduloExistente, "Modulo", true);
            if (instrucao.InjetarNoEscopo)
                InjetarModuloNoEscopo(moduloExistente);
            return moduloExistente;
        }

        var tokenizador = new Tokenizador(codigo, nomeArquivo, diretorioArquivo);
        var tokens = tokenizador.Tokenizar();
        var parser = new Parser(tokens.ToArray(), Flags.ForcarTiposEstaticos);
        var programa = parser.Parse();

        // Compartilha o logger do interpretador atual para que a saída das funções
        // do módulo importado seja capturada no mesmo lugar (e não caia num
        // ConsoleLogger, que quebra no browser WASM).
        var novoInterpretador = new Interpretador(Flags, _modulosImportados, _ambiente.Logger);
        
        // Evita recursão infinita adicionando um módulo vazio temporário
        var moduloTemporario = new Modulo(instrucao.Identificador ?? Path.GetFileNameWithoutExtension(caminho), new Dictionary<string, Variavel>());
        _modulosImportados[chaveModulo] = moduloTemporario;

        novoInterpretador.VisitarPrograma(programa);

        // Coleta todas as variáveis do escopo global do novo interpretador
        var escopoGlobal = novoInterpretador._ambiente.Pilha.ObterEscopoGlobal();
        var propriedades = new Dictionary<string, Variavel>();
        foreach (var par in escopoGlobal.Variaveis)
        {
            propriedades[par.Key] = par.Value;
        }

        var modulo = new Modulo(instrucao.Identificador ?? Path.GetFileNameWithoutExtension(caminho), propriedades);
        _modulosImportados[chaveModulo] = modulo;

        if (instrucao.Identificador != null)
            _ambiente.Pilha.DefinirVariavel(instrucao.Identificador, modulo, "Modulo", true);

        if (instrucao.InjetarNoEscopo)
            InjetarModuloNoEscopo(modulo);

        return modulo;
    }

    /// <summary>
    /// Injeta os elementos de um módulo (funções, constantes) no escopo global atual,
    /// permitindo chamá-los diretamente após `importar X` (sem apelido). Nomes que já
    /// existem no escopo — como as funções base — são preservados, evitando conflitos.
    /// As funções mantêm seu ambiente de definição (fecho), então dependências internas
    /// do módulo continuam resolvidas.
    /// </summary>
    private void InjetarModuloNoEscopo(Modulo modulo)
    {
        var escopoGlobal = _ambiente.Pilha.ObterEscopoGlobal();
        foreach (var par in modulo.Propriedades)
        {
            if (!escopoGlobal.Variaveis.ContainsKey(par.Key))
                escopoGlobal.Variaveis[par.Key] = par.Value;
        }
    }

    private string? TentarCaminhos(string baseDir, string nomeLogico)
    {
        // 1. Tenta o arquivo direto (se o usuário passou com .libra ou se o parser passou)
        string fullPath = Path.Combine(baseDir, nomeLogico);
        if (File.Exists(fullPath)) return Path.GetFullPath(fullPath);

        // 2. Tenta adicionar .libra se não tiver
        if (!nomeLogico.EndsWith(".libra"))
        {
            string comExt = fullPath + ".libra";
            if (File.Exists(comExt)) return Path.GetFullPath(comExt);
        }

        // 3. Tenta como diretório (Entry Points)
        string nomeDir = nomeLogico.EndsWith(".libra") 
            ? nomeLogico.Substring(0, nomeLogico.Length - 6) 
            : nomeLogico;
        
        string dirPath = Path.Combine(baseDir, nomeDir);
        if (Directory.Exists(dirPath))
        {
            string[] entryPoints = { "inicio.libra", "index.libra" };
            foreach (var ep in entryPoints)
            {
                string epPath = Path.Combine(dirPath, ep);
                if (File.Exists(epPath)) return Path.GetFullPath(epPath);
            }
        }

        return null;
    }

    private string? EncontrarCaminhoBiblioteca(string caminho, LocalFonte local)
    {
        // 1. Tentar caminho relativo ao arquivo atual
        string? res = TentarCaminhos(local.CaminhoCompleto, caminho);
        if (res != null) return res;

        // 2. Tentar na subpasta 'biblioteca' relativa ao arquivo atual
        res = TentarCaminhos(Path.Combine(local.CaminhoCompleto, "biblioteca"), caminho);
        if (res != null) return res;

        // 3. Tentar no diretório de trabalho atual (projeto raiz)
        res = TentarCaminhos(Directory.GetCurrentDirectory(), caminho);
        if (res != null) return res;

        // 4. Lógica de Pacotes (importar pacote.modulo)
        string[] partes = caminho.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length > 0)
        {
            string nomePacote = partes[0];
            string restoCaminho = string.Join("/", partes.Skip(1));
            
            // Se for apenas 'importar pacote', o restoCaminho é vazio, 
            // e o TentarCaminhos vai lidar com isso buscando por 'inicio.libra' etc na pasta do pacote.
            if (string.IsNullOrEmpty(restoCaminho)) restoCaminho = ""; 

            string pastaPacote = Path.Combine(Directory.GetCurrentDirectory(), "pacotes", nomePacote);
            if (Directory.Exists(pastaPacote))
            {
                // Tenta resolver a raiz do pacote via projeto.libra.json
                string raizPacote = "";
                string configPath = Path.Combine(pastaPacote, "projeto.libra.json");
                if (File.Exists(configPath))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(File.ReadAllText(configPath));
                        if (doc.RootElement.TryGetProperty("raiz", out var raizProp))
                            raizPacote = raizProp.GetString() ?? "";
                    } catch {}
                }

                // Lista de pastas para buscar dentro do pacote
                var pastasBuscaPacote = new List<string> { pastaPacote };
                if (!string.IsNullOrEmpty(raizPacote)) pastasBuscaPacote.Add(Path.Combine(pastaPacote, raizPacote));
                pastasBuscaPacote.Add(Path.Combine(pastaPacote, "codigo"));
                pastasBuscaPacote.Add(Path.Combine(pastaPacote, "src"));

                foreach (var pasta in pastasBuscaPacote)
                {
                    res = TentarCaminhos(pasta, restoCaminho == "" ? "." : restoCaminho);
                    if (res != null) return res;
                }
            }
        }

        // 5. Biblioteca Padrão (diretório do executável)
        res = TentarCaminhos(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "biblioteca"), caminho);
        if (res != null) return res;

        return null;
    }

    public void VisitarInstrucoes(Instrucao[] instrucoes)
    {
        for (int i = 0; i < instrucoes.Length; i++)
        {
            instrucoes[i].Aceitar(this);
        }
    }

    public LibraObjeto VisitarInstrucaoExpressao(InstrucaoExpressao instrucao)
    {
        if (instrucao.Expressao == null)
            return null;

        _ultimoRetorno = VisitarExpressao(instrucao.Expressao);

        return null;
    }

    public LibraObjeto VisitarTentar(Tentar instrucao)
    {
        try
        {
            _ambiente.Pilha.EmpilharEscopo();
            VisitarInstrucoes(instrucao.InstrucoesTentar);
            _ambiente.Pilha.DesempilharEscopo();
        }
        catch (Erro err)
        {
            _ambiente.Pilha.DesempilharEscopo(); // Desempilhando escopo do "Tentar"

            _ambiente.Pilha.EmpilharEscopo();
            _ambiente.Pilha.DefinirVariavel(instrucao.VariavelErro, new LibraTexto(err.Mensagem), TiposPadrao.Texto, true);
            VisitarInstrucoes(instrucao.InstrucoesCapturar);
            _ambiente.Pilha.DesempilharEscopo();
        }

        return null;

    }

    public LibraObjeto VisitarAtribProp(AtribuicaoPropriedade instrucao)
    {
        var alvo = VisitarExpressao(instrucao.Alvo.Alvo);
        var obj = LibraObjeto.ParaLibraObjeto(alvo);

        obj.AtribuirPropriedade(instrucao.Alvo.Propriedade, VisitarExpressao(instrucao.Expressao));

        return null;
    }

    public LibraObjeto VisitarAtribIndice(AtribuicaoIndice instrucao)
    {
        string identificador = instrucao.Identificador;
        int indice = VisitarExpressao<LibraInt>(instrucao.ExpressaoIndice).Valor;
        LibraObjeto expressao = VisitarExpressao(instrucao.Expressao);

        _ambiente.Pilha.ModificarVetor(identificador, indice, expressao);

        return null;
    }

    public LibraObjeto VisitarRetorno(Retornar instrucao)
    {
        object resultadoExpressao = VisitarExpressao(((Retornar)instrucao).Expressao);
        _ultimoRetorno = LibraObjeto.ParaLibraObjeto(resultadoExpressao);
        throw new ExcecaoRetorno(resultadoExpressao);


        return null;
    }

    public LibraObjeto VisitarSe(Se se)
    {
        if (VisitarExpressao<LibraInt>(se.Condicao).Valor != 0)
        {
            return se.Entao.Aceitar(this);
        }
        else if (se.Senao != null)
        {
            return se.Senao.Aceitar(this);
        }

        return null;
    }

    public LibraObjeto VisitarBloco(Bloco bloco)
    {
        _ambiente.Pilha.EmpilharEscopo();
        try
        {
            foreach (var instrucao in bloco.Instrucoes)
            {
                instrucao.Aceitar(this);
            }
        }
        finally
        {
            _ambiente.Pilha.DesempilharEscopo();
        }

        return null;
    }

    public LibraObjeto VisitarEnquanto(Enquanto enquanto)
    {
        while (VisitarExpressao<LibraInt>(enquanto.Expressao).Valor != 0)
        {
            _ambiente.Pilha.EmpilharEscopo();
            try
            {
                foreach (var i in ((Bloco)enquanto.Corpo).Instrucoes)
                {
                    i.Aceitar(this);
                }
            }
            catch (ExcecaoRomper)
            {
                return null;
            }
            finally
            {
                _ambiente.Pilha.DesempilharEscopo();
            }
        }

        return null;
    }

    public LibraObjeto VisitarParaCada(ParaCada instrucao)
    {
        var expr = instrucao.Vetor;

        var vetor = VisitarExpressao<LibraVetor>(expr);

        foreach (var item in vetor.Valor)
        {
            _ambiente.Pilha.EmpilharEscopo();
            try
            {
                _ambiente.DefinirGlobal(instrucao.Identificador.Valor.ToString(), item);
                VisitarInstrucoes(instrucao.Instrucoes);
            }
            catch (ExcecaoRomper)
            {
                return null;
            }
            finally
            {
                _ambiente.Pilha.DesempilharEscopo();
            }
        }

        return null;
    }

    public LibraObjeto VisitarFuncao(DefinicaoFuncao funcao)
    {
        string identificador = funcao.Identificador;

        if (string.IsNullOrWhiteSpace(identificador))
            throw new ErroIdentificadorInvalido(identificador, _local);

        var novaFuncao = new Funcao(identificador, funcao.Instrucoes, funcao.Parametros, funcao.TipoRetorno, _ambiente);

        _ambiente.Pilha.DefinirVariavel(identificador, novaFuncao, TiposPadrao.Func, true);

        return null;
    }

    public LibraObjeto ExecutarFuncaoEmbutida(FuncaoNativa funcao, Expressao[] argumentos) 
    {
        List<object> valoresArgumentos = new List<object>();

        for(int i = 0; i < argumentos.Length; i++)
        {
            valoresArgumentos.Add(VisitarExpressao(argumentos[i]).ObterValor());
        }

        var resultadoFuncao = funcao.Executar(valoresArgumentos.ToArray());
        var objeto = LibraObjeto.ParaLibraObjeto(resultadoFuncao);

        return objeto;
    }

    public LibraObjeto VisitarConstrutorClasse(string nome, Expressao[] expressoes, string quemChamou = "")
    {
        // TODO: Pode dar erro!
        Classe tipo = (Classe)_ambiente.Pilha.ObterVariavel(nome).Valor;

        // Cria um novo ambiente para o objeto, para que o 'auto' seja isolado
        var ambienteObjeto = new Ambiente(_ambiente.Logger, _ambiente.AmbienteSeguro);
        
        // TODO: Arrumar, nunca vi um código tão porcaria em toda a minha vida
        List<Variavel> vars = new();
        foreach(var i in tipo.Variaveis)
        {
            LibraObjeto valorProp;
            if (i.Expressao != null)
            {
                valorProp = VisitarExpressao(i.Expressao);
            }
            else
            {
                valorProp = LibraObjeto.Inicializar(i.TipoVar);
            }
            vars.Add(new Variavel(i.Identificador, valorProp, i.TipoVar, i.Constante));
        }

        var obj = new LibraObjeto(nome, vars.ToArray(), expressoes);
        
        // Injetar a referência 'auto' (this) no ambiente do objeto
        ambienteObjeto.Pilha.DefinirVariavel("auto", obj, nome, true);

        // Adiciona as funções ao objeto, vinculando-as ao ambiente do objeto para que tenham acesso ao 'auto'
        foreach(var i in tipo.Funcoes)
        {
            var metodo = new Funcao(i.Identificador, i.Instrucoes, i.Parametros, i.TipoRetorno, ambienteObjeto);
            obj.Propriedades[i.Identificador] = new Variavel(i.Identificador, metodo, TiposPadrao.Func, true);
        }

        // Executa o construtor (função com o mesmo nome da classe) se existir
        if (obj.Propriedades.ContainsKey(nome) && obj.Propriedades[nome].Valor is Funcao construtor)
        {
            ExecutarFuncao(construtor, expressoes);
        }
        
        return obj;
    }

    public LibraObjeto ExecutarFuncao(Funcao funcao, Expressao[] argumentos)
    {
        if(funcao is FuncaoNativa nativa)
        {
            return ExecutarFuncaoEmbutida(nativa, argumentos);
        }
        
        var qtdParametros = funcao.Parametros.Length;

        if (argumentos.Length != qtdParametros)
            throw new ErroEsperadoNArgumentos(funcao.Identificador, qtdParametros, argumentos.Length, _local);

        // 1. Avalia os argumentos no ambiente ATUAL (do chamador)
        var valoresArgumentos = new LibraObjeto[argumentos.Length];
        for (int i = 0; i < argumentos.Length; i++)
        {
            valoresArgumentos[i] = VisitarExpressao(argumentos[i]);
        }

        // 2. Troca para o ambiente onde a função foi definida
        var ambienteOriginal = _ambiente;
        if (funcao.AmbienteDefinicao != null)
            _ambiente = funcao.AmbienteDefinicao;

        _ambiente.Pilha.EmpilharEscopo(funcao.Identificador, _local);

        try 
        {
            // Se a função vier de um objeto, injetamos o 'auto' no escopo local
            if (_ambiente.Pilha.VariavelExiste("auto"))
            {
                var auto = _ambiente.Pilha.ObterVariavel("auto");
                _ambiente.Pilha.DefinirVariavel("auto", auto.Valor, auto.Tipo, true);
            }

            // 3. Define os parâmetros com os valores já calculados
            for (int i = 0; i < valoresArgumentos.Length; i++)
            {
                string ident = funcao.Parametros[i].Identificador;
                var obj = valoresArgumentos[i];
                
                if(funcao.Parametros[i].Tipo != TiposPadrao.Objeto && funcao.Parametros[i].Tipo != obj.Nome)
                    obj = obj.Converter(funcao.Parametros[i].Tipo);

                _ambiente.Pilha.DefinirVariavel(ident, obj, funcao.Parametros[i].Tipo);
            }

            VisitarInstrucoes(funcao.Instrucoes);
        }
        catch(ExcecaoRetorno retorno)
        {
            var resultado = LibraObjeto.ParaLibraObjeto(retorno.Valor);
            if(funcao.TipoRetorno != resultado.Nome && funcao.TipoRetorno != TiposPadrao.Objeto)
            {
                return resultado.Converter(funcao.TipoRetorno);
            }
            
            return resultado;
        }
        finally
        {
            _ambiente.Pilha.DesempilharEscopo(); // Removendo o Escopo da Pilha
            _ambiente = ambienteOriginal;
        }

        // Caso a função não tenha um retorno explicito
        return LibraObjeto.Inicializar(TiposPadrao.Nulo);
    }

    public LibraObjeto VisitarChamadaFuncao(ExpressaoChamadaFuncao chamada)
    {
        var argumentos = chamada.Argumentos;

        var v = _ambiente.Pilha.ObterVariavel(chamada.Identificador);

        if(v.Valor is Classe)
            return VisitarConstrutorClasse(chamada.Identificador, chamada.Argumentos.ToArray());

        return ExecutarFuncao((Funcao)v.Valor, chamada.Argumentos);
    }

    // TODO: É isso?
    public LibraObjeto VisitarClasse(DefinicaoTipo i)
    {
        _ambiente.Pilha.DefinirVariavel(i.Identificador, new Classe(i.Identificador, i.Variaveis, i.Funcoes), TiposPadrao.Objeto);

        return null;
    }

    public LibraObjeto VisitarAtribVar(AtribuicaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new ErroIdentificadorInvalido(i.Identificador, _local);

        LibraObjeto resultado = VisitarExpressao(i.Expressao);

        _ambiente.Pilha.AtualizarVariavel(i.Identificador, resultado);

        resultado.Construtor(i.Identificador);
        
        return resultado;
    }

    public LibraObjeto VisitarDeclVar(DeclaracaoVar i)
    {
        if(string.IsNullOrWhiteSpace(i.Identificador))
            throw new ErroIdentificadorInvalido(i.Identificador, _local);

        LibraObjeto resultado;
        if (i.Expressao != null)
        {
            resultado = VisitarExpressao(i.Expressao);
        }
        else
        {
            resultado = LibraObjeto.Inicializar(i.TipoVar);
        }

        _ambiente.Pilha.DefinirVariavel(i.Identificador, resultado, i.TipoVar, i.Constante);

        resultado.Construtor(i.Identificador);

        return resultado;
    }

    public object[] VisitarVetor(ExpressaoNovoVetor expressao)
    {
        int indice = VisitarExpressao<LibraInt>(expressao.Expressao).Valor;
        return new LibraObjeto[indice];
    }

    public object[] VisitarInicializacaoVetor(ExpressaoInicializacaoVetor expressao)
    {
        int tamanho = expressao.Expressoes.Count;
        var vetor = new object[tamanho];

        for(int i = 0; i < tamanho; i++)
        {
            vetor[i] = VisitarExpressao(expressao.Expressoes[i]);
        }

        return vetor;
    }

    public LibraObjeto VisitarExpressao(Expressao expressao)
    {
        if(expressao == null)
            return new LibraNulo();

        _local = expressao.Local;
        
        return LibraObjeto.ParaLibraObjeto(expressao.Aceitar(this));
    }

    public T VisitarExpressao<T>(Expressao expressao)
    {
        var resultado = VisitarExpressao(expressao);

        if (resultado is T t) return t;

        throw new ErroAcessoNulo($" Expressão retornou {resultado.GetType()} ao invés do esperado", _local);
    }

    public LibraObjeto VisitarExpressaoLiteral(ExpressaoLiteral expressao)
    {
        return LibraObjeto.ParaLibraObjeto(expressao.Valor);
    }
    
    public LibraObjeto VisitarExpressaoBinaria(ExpressaoBinaria expressao)
    {
        var a = VisitarExpressao(expressao.Esquerda);
        var b = VisitarExpressao(expressao.Direita);

        return expressao.Operador.Tipo switch
        {
            TokenTipo.OperadorSoma => a.Soma(b),
            TokenTipo.OperadorSub => a.Sub(b),
            TokenTipo.OperadorMult => a.Mult(b),
            TokenTipo.OperadorDiv => a.Div(b),
            TokenTipo.OperadorPot => a.Pot(b),
            TokenTipo.OperadorResto => a.Resto(b),
            TokenTipo.OperadorComparacao => a.Igual(b),
            TokenTipo.OperadorDiferente => new LibraInt(LibraUtil.NegarInteiroLogico(a.Igual(b).Valor)), // Vendo isso aqui meses depois, genial!
            TokenTipo.OperadorMaiorQue => a.MaiorQue(b),
            TokenTipo.OperadorMaiorIgualQue => a.MaiorIgualQue(b),
            TokenTipo.OperadorMenorQue => a.MenorQue(b),
            TokenTipo.OperadorMenorIgualQue => a.MenorIgualQue(b),
            TokenTipo.OperadorE => a.E(b),
            TokenTipo.OperadorOu => a.Ou(b),
            _ => throw new ErroOperadorInvalido(expressao.Operador.Tipo.ToString(), expressao.Operador.Local)
        };
    }

    public LibraObjeto VisitarExpressaoVariavel(ExpressaoVariavel expressao)
    {
        var v = _ambiente.Pilha.ObterVariavel(expressao.Identificador.Valor.ToString());
        return v.Valor;
    }

    public LibraObjeto VisitarExpressaoNovoVetor(ExpressaoNovoVetor expressao)
    {
        var vetor = new LibraObjeto[VisitarExpressao<LibraInt>(expressao.Expressao).Valor];
        return LibraObjeto.ParaLibraObjeto(vetor); // Converte para LibraVetor
    }

    public LibraObjeto VisitarExpressaoInicializacaoVetor(ExpressaoInicializacaoVetor expressao)
    {
        var arr = new LibraObjeto[expressao.Expressoes.Count];

        for(int i = 0; i < expressao.Expressoes.Count; i++)
        {
            arr[i] = VisitarExpressao(expressao.Expressoes[i]);
        }

        return LibraObjeto.ParaLibraObjeto(arr);
    }

    public LibraObjeto VisitarExpressaoUnaria(ExpressaoUnaria expressao)
    {
        switch(expressao.Operador.Tipo)
        {
            case TokenTipo.OperadorNeg:
                return LibraObjeto.ParaLibraObjeto(VisitarExpressao<LibraInt>(expressao.Operando).Valor);
            case TokenTipo.OperadorSub:
                return LibraObjeto.ParaLibraObjeto(VisitarExpressao(expressao.Operando).Mult(new LibraInt(-1)));
        }

        throw new ErroOperadorInvalido(expressao.Operador.Tipo.ToString(), expressao.Operador.Local);
    }

    public LibraObjeto VisitarExpressaoAcessoVetor(ExpressaoAcessoVetor expressao)
    {
        string ident = expressao.Identificador;
        int indice = VisitarExpressao<LibraInt>(expressao.Expressao).Valor;

        var variavel = _ambiente.Pilha.ObterVariavel(ident);

        if(variavel.Valor is LibraVetor vetor)
        {
            if (indice < 0 || indice >= vetor.Valor.Length)
                throw new ErroIndiceForaVetor($"{ident}[{indice.ToString()}]", _local);
            return vetor.Valor[indice];
        }
        if(variavel.Valor is LibraTexto texto)
        {
            if (indice < 0 || indice >= texto.Valor.Length)
                throw new ErroIndiceForaVetor($"{ident}[{indice.ToString()}]", _local);
            return new LibraTexto(texto.Valor[indice].ToString());
        }

        throw new ErroAcessoNulo($" {variavel.Valor} não é um Vetor");
    }

    public LibraObjeto VisitarExpressaoChamadaFuncao(ExpressaoChamadaFuncao expressao)
    {
        return VisitarChamadaFuncao(expressao);
    }

    public LibraObjeto VisitarExpressaoPropriedade(ExpressaoPropriedade expressao)
    {
        var obj = LibraObjeto.ParaLibraObjeto(expressao.Alvo.Aceitar(this));

        return obj.AcessarPropriedade(expressao.Propriedade);
    }

    public LibraObjeto VisitarExpressaoChamadaMetodo(ExpressaoChamadaMetodo expressao)
    {
        var alvo = VisitarExpressao(expressao.Alvo);
        var chamada = expressao.Chamada;

        if (!alvo.Propriedades.ContainsKey(chamada.Identificador))
            throw new ErroFuncaoNaoDefinida($"{alvo.Nome}.{chamada.Identificador}");

        var valorProp = alvo.Propriedades[chamada.Identificador].Valor;
        
        if (valorProp is not Funcao funcao)
            throw new ErroFuncaoNaoDefinida($"{alvo.Nome}.{chamada.Identificador}");

        return ExecutarFuncao(funcao, chamada.Argumentos.ToArray());
    }

    public LibraObjeto VisitarRomper(Romper instrucao)
    {
        throw new ExcecaoRomper();
    }

    public LibraObjeto VisitarContinuar(Continuar instrucao)
    {
        throw new NotImplementedException();
    }
}
