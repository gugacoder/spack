using ScriptPack.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;
using SPack.Helpers;

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
  public List<string>? ConnectionMaps { get; set; } = new();

  /// <summary>
  /// Executa o comando de migração de dados.
  /// </summary>
  public async Task RunAsync()
  {
    //
    // Abrindo o catálogo.
    //
    var repositoryOpener = new RepositoryCreator { DetectDependencies = true };
    var repositoryNavigator =
        await repositoryOpener.CreateRepositoryNavigatorAsync(CatalogPath);

    var rootNode = repositoryNavigator.RootNode;

    //
    // Selecionando nodos.
    //
    var nodeSelector = new NodeSelector();
    nodeSelector.SearchPackageCriteria = SearchPackageCriteria;
    nodeSelector.SearchScriptCriteria = SearchScriptCriteria;
    var selectedNodes = nodeSelector.SelectNodes(rootNode);

    //
    // Montando pipelines.
    //
    var pipelineBuilder = new PipelineBuilder();
    pipelineBuilder.AddScriptsFromNodes(selectedNodes);
    var pipelines = pipelineBuilder.BuildPipelines();

    //
    // Detectando e reportando falhas.
    //
    var faultReporter = new FaultReporter();
    var faultReport = faultReporter.CreateFaultReport(pipelines);
    if (faultReport.Length > 0)
    {
      faultReporter.PrintFaultReport(faultReport);
      return;
    }

    //
    // Configurando as conexões.
    //
    if (ConnectionMaps?.Any() == true)
    {
      var connectionConfigurator = new ConnectionConfigurator();
      connectionConfigurator.ConfigureConnections(rootNode, ConnectionMaps);
    }

    //
    // Realizando a migração de bases.
    //
    var databaseMigrator = new DatabaseMigrator();

    RegisterListeners(databaseMigrator);

    foreach (var pipeline in pipelines)
    {
      await databaseMigrator.MigrateAsync(pipeline);
    }
  }

  /// <summary>
  /// Imprime um relatório de erros ocorridos nos pipelines e seus scripts.
  /// </summary>
  /// <param name="databaseMigrator">
  /// Objeto DatabaseMigrator para registro dos eventos.
  /// </param>
  private void RegisterListeners(DatabaseMigrator databaseMigrator)
  {
    databaseMigrator.OnPipelineStart += (sender, args) =>
      Console.WriteLine($"+- PIPELINE {args.Phase.Name}");
    databaseMigrator.OnStageStart += (sender, args) =>
      Console.WriteLine($"   +- STAGE {args.Phase.Name}");
    databaseMigrator.OnConnection += (sender, args) =>
      Console.WriteLine($"      +- BASE {args.Connection.Database}");
    databaseMigrator.OnConnectionMessage += (sender, args) =>
      Console.WriteLine($"               {args.Message}");
    databaseMigrator.OnStepStart += (sender, args) =>
      Console.WriteLine($"      +- STEP {args.Phase.Name}");
    databaseMigrator.OnMigrate += (sender, args) =>
      Console.WriteLine($"         +- {args.Script.Name}");
    databaseMigrator.OnSuccess += (sender, args) =>
      Console.WriteLine("               [ OK ]");
    databaseMigrator.OnError += (sender, args) =>
    {
      Console.WriteLine("               [ ERRO ]");
      if (args.Script != null)
      {
        Console.Error.WriteLine($"Causa: {args.Exception.Message}");
        if (Verbose)
        {
          Console.Error.WriteLine(args.Exception.StackTrace);
        }
        Console.Error.WriteLine();
      }
      else
      {
        Console.Error.WriteLine($"Causa: {args.Exception.Message}");
        if (Verbose)
        {
          Console.Error.WriteLine(args.Exception.StackTrace);
        }
        Console.Error.WriteLine();
      }
    };
  }
}
