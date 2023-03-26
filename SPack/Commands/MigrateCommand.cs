using System.Text;
using ScriptPack.Model;
using SPack.Commands.Helpers;
using SPack.Helpers;
using SPack.Prompting;

namespace SPack.Commands;

/// <summary>
/// Comando de execução de scripts de migração de bases de dados.
/// </summary>
public class MigrateCommand : ICommand
{
  /// <summary>
  /// Obtém ou define um valor booleano que indica se a execução deve ser
  /// verbosa ou não.
  /// </summary>
  public bool Verbose { get; set; } = false;

  /// <summary>
  /// Obtém ou define a codificação dos arquivos de script.
  /// </summary>
  public Encoding? Encoding { get; set; }

  /// <summary>
  /// Obtém ou define o caminho da pasta ou arquivo do catálogo.
  /// </summary>
  public string? CatalogPath { get; set; }

  /// <summary>
  /// Obtém ou define os pacotes a serem carregados.
  /// Cada pacote tem a forma:
  ///   PRODUTO[/VERSÃO[/MÓDULO[/PACOTE]]]
  /// Exemplo:
  ///   MyProduct/1.0.0/MyModule/MyPackage
  /// </summary>
  public List<string> SearchPackageCriteria { get; set; } = new();

  /// <summary>
  /// Obtém ou define os filtros de script a serem aplicados.
  /// Um filtro é um padrão de pesquisa de pastas e arquivos virtuais na
  /// árvode de nodos do catálogo.
  /// 
  /// Por exemplo, para selecionar todos os scripts da versão 1.0.0 disponível
  /// no catálogo o filtro poderia ser: **/1.0.0.
  /// </summary>
  public List<string> SearchScriptCriteria { get; set; } = new();

  /// <summary>
  /// Obtém ou define os mapas de configuração de conexão.
  /// Cada entrada no mapa tem a forma:
  ///    [nome]:[connection string]
  /// Exemplo:
  ///    myapp:Server=127.0.0.1;Database=MyDB;User Id=MyUser;Password=MyPass;
  /// </summary>
  public List<string>? DatabaseMaps { get; set; } = new();

  /// <summary>
  /// Obtém ou define um valor booleano que indica se os scripts internos
  /// devem ser incluídos na execução.
  /// </summary>
  /// <remarks>
  /// Os scripts internos são scripts que acompanham o aplicativo e que
  /// adicionam objetos de automação do ScriptPack para scripts de migração
  /// de base de dados.
  /// </remarks>
  public bool BuiltInScripts { get; set; } = false;

  /// <summary>
  /// Executa o comando de migração de dados.
  /// </summary>
  public async Task RunAsync(CommandLineOptions options)
  {
    // Selecionando os scripts.
    var nodeSelectorBuilder = new PackageSelectionBuilder();
    nodeSelectorBuilder.AddOptions(options);
    nodeSelectorBuilder.AddValidators();
    var nodes = await nodeSelectorBuilder.BuildPackageSelectionAsync();

    // Selecionando as conexões.
    var connectionSelectorBuilder = new ConnectionSelectionBuilder();
    connectionSelectorBuilder.AddOptions(options);
    nodes.ForEach(connectionSelectorBuilder.AddConnectionsFromNode);
    var connections = connectionSelectorBuilder.BuildConnectionSelection();

    // Montando pipelines.
    var pipelineBuilder = new PipelineBuilder();
    nodes.ForEach(pipelineBuilder.AddScriptsFromNode);
    connections.ForEach(pipelineBuilder.AddConnection);
    var pipelines = pipelineBuilder.BuildPipelines();

    // Detectando e reportando falhas.
    var faultReporter = new FaultReporter();
    var faultReport = faultReporter.CreateFaultReport(pipelines);
    if (faultReport.Length > 0)
    {
      faultReporter.PrintFaultReport(faultReport);
      return;
    }

    //
    // Realizando a migração de bases.
    //
    foreach (var pipeline in pipelines)
    {
      var databaseMigrator = new DatabaseMigrator
      {
        Pipeline = pipeline,
        Context = new()
      };
      RegisterListeners(databaseMigrator, options.Verbose.On);
      await databaseMigrator.MigrateAsync();
    }
  }

  /// <summary>
  /// Imprime um relatório de erros ocorridos nos pipelines e seus scripts.
  /// </summary>
  /// <param name="databaseMigrator">
  /// Objeto DatabaseMigrator para registro dos eventos.
  /// </param>
  /// <param name="verbose">
  /// Indica se a execução deve ser verbosa ou não.
  /// </param>
  private void RegisterListeners(DatabaseMigrator databaseMigrator, bool verbose)
  {
    if (verbose)
    {
      databaseMigrator.OnPipelineStart += (sender, args) =>
          Console.WriteLine($"[PIPELINE] {args.Phase.Name}");
      databaseMigrator.OnStageStart += (sender, args) =>
          Console.WriteLine($"[STAGE] {args.Phase.Name}");
      databaseMigrator.OnConnection += (sender, args) =>
          Console.WriteLine($"[DATEBASE] {args.Connection.Database}");
      databaseMigrator.OnConnectionMessage += (sender, args) =>
          Console.WriteLine($"{args.Message}");
      databaseMigrator.OnStepStart += (sender, args) =>
          Console.WriteLine($"[STEP] {args.Phase.Name}");
      databaseMigrator.OnError += (sender, args) =>
      {
        if (args.Script != null)
        {
          Console.Error.WriteLine($"[ERRO] {args.Exception.Message}");
          if (Verbose)
          {
            Console.Error.WriteLine(args.Exception.StackTrace);
          }
          Console.Error.WriteLine();
        }
        else
        {
          Console.Error.WriteLine($"[ERRO] {args.Exception.Message}");
          if (Verbose)
          {
            Console.Error.WriteLine(args.Exception.StackTrace);
          }
          Console.Error.WriteLine();
        }
      };
    }
    
    databaseMigrator.OnMigrate += (sender, args) =>
        Console.WriteLine(args.Script.Path);

    databaseMigrator.OnResultSet += (sender, args) =>
        ResultSetPrinter.PrintResultSet(args.Result);

  }
}
