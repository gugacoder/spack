namespace SPack.Prompting;

/// <summary>
/// Classe que representa as opções da linha de comando para a aplicação.
/// </summary>
public class CommandLineOptions
{
  /// <summary>
  /// Representa a opção 'list' para listar o conteúdo do catálogo de scripts.
  /// </summary>
  [ArgumentAttribute]
  public Option List { get; set; } = new(DefaultValue: "**");

  /// <summary>
  /// Representa a opção 'show' para exibir o conteúdo dos objetos do catálogo
  /// de scripts.
  /// </summary>
  [ArgumentAttribute]
  public Option Show { get; set; } = new();

  /// <summary>
  /// Representa a opção 'init' para inicializar as bases de dados com os
  /// objetos básicos do esquema ScriptPack.
  /// </summary>
  [ArgumentAttribute]
  public Option Init { get; set; } = new();

  /// <summary>
  /// Representa a opção 'migrate' para executar os scripts de migração nas
  /// bases de dados.
  /// </summary>
  [ArgumentAttribute]
  public Switch Migrate { get; set; } = new();

  /// <summary>
  /// Representa a opção 'pipeline' para exibir o plano de execução dos
  /// pipelines.
  /// </summary>
  [ArgumentAttribute]
  public Switch Pipeline { get; set; } = new();

  /// <summary>
  /// Representa a opção 'validate' para verificar se há falhas nos catálogos de
  /// scripts.
  /// </summary>
  [ArgumentAttribute]
  public Switch Validate { get; set; } = new();

  /// <summary>
  /// Representa a opção 'encode' para codificar uma senha a ser usada em uma
  /// string de conexão.
  /// </summary>
  [ArgumentAttribute]
  public Option Encode { get; set; } = new();

  /// <summary>
  /// Representa a opção 'catalog' para definir o caminho da pasta ou arquivo do
  /// catálogo.
  /// </summary>
  [ArgumentAttribute(@long: true, 'c')]
  public Option Catalog { get; set; } = new(Long: true, 'c');

  /// <summary>
  /// Representa a opção 'package' para selecionar pacotes de scripts de forma
  /// simplificada.
  /// </summary>
  [ArgumentAttribute(@long: true, 'p')]
  public OptionList Package { get; set; } = new(Long: true, 'p');

  /// <summary>
  /// Representa a opção 'script' para selecionar os scripts ou pacotes de
  /// scripts que serão executados na base de dados.
  /// </summary>
  [ArgumentAttribute(@long: true, 's')]
  public OptionList Script { get; set; } = new(Long: true, 's');

  /// <summary>
  /// Representa a opção 'arg' para definir o valor de um argumento repassado
  /// para os scripts.
  /// </summary>
  [ArgumentAttribute(@long: true, 'a')]
  public OptionList Arg { get; set; } = new(Long: true, 'a');

  /// <summary>
  /// Representa a opção 'database' para configurar a base de dados que será
  /// migrada.
  /// </summary>
  [ArgumentAttribute(@long: true, 'd')]
  public OptionList Database { get; set; } = new(Long: true, 'd');

  /// <summary>
  /// Representa a opção 'ignore-built-in' para ignorar os pacotes de scripts
  /// internos.
  /// </summary>
  /// <remarks>
  /// Os scripts internos são ferramentas de automação da base de dados para
  /// scripts de migração de base de dados.
  /// </remarks>
  [ArgumentAttribute(@long: true, 'i')]
  public Switch IgnoreBuiltIn { get; set; } = new(Long: true, 'i');

  /// <summary>
  /// Representa a opção 'verbose' para mostrar informações adicionais durante a
  /// execução.
  /// </summary>
  [ArgumentAttribute(@long: true, 'v')]
  public Switch Verbose { get; set; } = new(Long: true, 'v');

  /// <summary>
  /// Representa a opção 'help' para exibir a ajuda do comando.
  /// </summary>
  [ArgumentAttribute(@long: true, 'h')]
  public Switch Help { get; set; } = new(Long: true, 'h');
};
