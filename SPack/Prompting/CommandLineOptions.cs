using SPack.Prompting.Domain;

namespace SPack.Prompting;

/// <summary>
/// Classe que representa as opções da linha de comando para a aplicação.
/// </summary>
public class CommandLineOptions
{
  public CommandLineOptions()
  {
    this.AllOptions = InitializeAllOptions();
  }

  /// <summary>
  /// Obtém a lista de todas as opções declaradas.
  /// </summary>
  public List<IArgument> AllOptions { get; }

  /// <summary>
  /// Representa a opção 'list' para listar o conteúdo do catálogo de scripts.
  /// </summary>
  [Argument(DefaultValue = "")]
  public Option List { get; } = new();

  /// <summary>
  /// Representa a opção 'show' para exibir o conteúdo dos objetos do catálogo
  /// de scripts.
  /// </summary>
  [Argument]
  public Option Show { get; } = new();

  /// <summary>
  /// Representa a opção 'init' para inicializar as bases de dados com os
  /// objetos básicos do esquema ScriptPack.
  /// </summary>
  [Argument]
  public Option Init { get; } = new();

  /// <summary>
  /// Representa a opção 'migrate' para executar os scripts de migração nas
  /// bases de dados.
  /// </summary>
  [Argument]
  public Switch Migrate { get; } = new();

  /// <summary>
  /// Representa a opção 'pipeline' para exibir o plano de execução dos
  /// pipelines.
  /// </summary>
  [Argument]
  public Switch Pipeline { get; } = new();

  /// <summary>
  /// Representa a opção 'validate' para verificar se há falhas nos catálogos de
  /// scripts.
  /// </summary>
  [Argument]
  public Switch Validate { get; } = new();

  /// <summary>
  /// Representa a opção 'encode' para codificar uma senha a ser usada em uma
  /// string de conexão.
  /// </summary>
  [Argument]
  public Option Encode { get; } = new();

  /// <summary>
  /// Representa a opção 'catalog' para definir o caminho da pasta ou arquivo do
  /// catálogo.
  /// </summary>
  [Argument(@long: true, 'c', DefaultValue = ".")]
  public Option Catalog { get; } = new();

  /// <summary>
  /// Representa a opção 'package' para selecionar pacotes de scripts de forma
  /// simplificada.
  /// </summary>
  [Argument(@long: true, 'p')]
  public OptionList Package { get; } = new();

  /// <summary>
  /// Representa a opção 'script' para selecionar os scripts ou pacotes de
  /// scripts que serão executados na base de dados.
  /// </summary>
  [Argument(@long: true, 's')]
  public OptionList Search { get; } = new();

  /// <summary>
  /// Representa a opção 'arg' para definir o valor de um argumento repassado
  /// para os scripts.
  /// </summary>
  [Argument(@long: true, 'a')]
  public OptionList Arg { get; } = new();

  /// <summary>
  /// Representa a opção 'database' para configurar a base de dados que será
  /// migrada.
  /// </summary>
  [Argument(@long: true, 'd')]
  public OptionList Database { get; } = new();

  /// <summary>
  /// Representa a opção 'built-in' para acrescentar automaticamente o catálogo
  /// interno do ScriptPack contendo a coleção de objetos de automação de
  /// scripts.
  /// </summary>
  [Argument(@long: true, 'b')]
  public Switch BuiltIn { get; } = new();

  /// <summary>
  /// Representa a opção 'encoding' para definir o tipo de codificação dos
  /// scripts.
  /// </summary>
  [Argument(@long: true, 'e')]
  public Option Encoding { get; } = new();

  /// <summary>
  /// Representa a opção 'verbose' para mostrar informações adicionais durante a
  /// execução.
  /// </summary>
  [Argument(@long: true, 'v')]
  public Switch Verbose { get; } = new();

  /// <summary>
  /// Representa a opção 'help' para exibir a ajuda do comando.
  /// </summary>
  [Argument(@long: true, 'h')]
  public Switch Help { get; } = new();

  /// <summary>
  /// Inicializa as opções e associa as opções às suas definições.
  /// </summary>
  /// <returns>
  /// A lista de todas as opções declaradas.
  /// </returns>
  private List<IArgument> InitializeAllOptions()
  {
    var arguments = (
        from p in GetType().GetProperties()
        from design in p.GetCustomAttributes(true).OfType<ArgumentAttribute>()
        let argument = p.GetValue(this) as IArgument
        where argument is not null
        select (argument, design)
    ).ToArray();

    foreach (var (argument, design) in arguments)
    {
      argument.Design = design;
    }

    return arguments.Select(e => e.argument).ToList();
  }
}