using ScriptPack.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;

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
  /// Obtém ou define os filtros de script a serem aplicados.
  /// Um filtro é um padrão de pesquisa de pastas e arquivos virtuais na
  /// árvode de nodos do catálogo.
  /// 
  /// Por exemplo, para selecionar todos os scripts da versão 1.0.0 disponível
  /// no catálogo o filtro poderia ser: **/1.0.0.
  /// </summary>
  public List<string> ScriptFilters { get; set; } = new();

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
    var catalogOpener = new RepositoryOpener();
    var catalog = await catalogOpener.OpenRepositoryAsync(this.CatalogPath);

    // Carregando a relação de dependência entre os scripts do catálogo.
    var dependencyDetectorVisitor = new DependencyDetectorVisitor();
    await catalog.AcceptAsync(dependencyDetectorVisitor);

    //
    // Configurando as conexões.
    //
    if (ConnectionMaps?.Any() == true)
    {
      var connectionConfigurator = new ConnectionConfigurator();
      connectionConfigurator.ConfigureConnections(catalog, ConnectionMaps);
    }

    //
    // Montando pipelines.
    //
    var pipelineBuilder = new PipelineBuilder();
    var treeNodeNavigator = new TreeNodeNavigator(catalog);
    foreach (var scriptFilter in ScriptFilters)
    {
      var nodes = treeNodeNavigator.ListNodes(scriptFilter);
      pipelineBuilder.AddScriptsFromNodes(nodes);
    }

    var pipelines = pipelineBuilder.BuildPipelines();

    //
    // Detectando e reportando falhas.
    //
    var faultReport = CreateFaultReport(pipelines);
    if (faultReport.Length > 0)
    {
      PrintFaultReport(faultReport);
      return;
    }

    //
    // Realizando a migração de bases.
    //
    var databaseMigrator = new DatabaseMigrator();

    databaseMigrator.OnPipelineStart += (sender, args) =>
      Console.WriteLine($"PIPELINE {args.Phase.Name}");
    databaseMigrator.OnStageStart += (sender, args) =>
      Console.WriteLine($"+- STAGE {args.Phase.Name}");
    databaseMigrator.OnConnection += (sender, args) =>
      Console.WriteLine($"   +- BASE {args.Connection.Database}");
    databaseMigrator.OnConnectionMessage += (sender, args) =>
      Console.WriteLine($"            {args.Message}");
    databaseMigrator.OnStepStart += (sender, args) =>
      Console.WriteLine($"   +- STEP {args.Phase.Name}");
    databaseMigrator.OnMigrate += (sender, args) =>
      Console.WriteLine($"      +- {args.Script.Name}");
    databaseMigrator.OnSuccess += (sender, args) =>
      Console.WriteLine("            [ OK ]");
    databaseMigrator.OnError += (sender, args) =>
    {
      Console.WriteLine("            [ ERRO ]");
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

    foreach (var pipeline in pipelines)
    {
      await databaseMigrator.MigrateAsync(pipeline);
    }
  }

  /// <summary>
  /// Cria um relatório de erros ocorridos nos pipelines e seus scripts
  /// </summary>
  /// <param name="pipelines">
  /// Lista de objetos PipelineNode para análise de erros.
  /// </param>
  /// <returns>
  /// Uma tupla contendo o nó e um array de erros relacionados.
  /// </returns>
  private (INode Node, Fault[] Faults)[] CreateFaultReport(
      List<PipelineNode> pipelines)
  {
    var pipelineFaults =
        from pipeline in pipelines
        from node in pipeline.DescendantsAndSelf()
        from fault in node.Faults
        select (node, fault);

    var scriptFaults =
        from pipeline in pipelines
        from step in pipeline.Descendants<StepNode>()
        from script in step.Scripts
        from node in script.Ancestor<CatalogNode>()!.DescendantsAndSelf()
        from fault in node.Faults
        select (node, fault);

    var faultReport = (
        from entry in pipelineFaults.Concat(scriptFaults).Distinct()
        group entry by entry.node into g
        select (node: g.Key, faults: g.Select(x => x.fault).ToArray())
    ).ToArray();

    return faultReport;
  }

  /// <summary>
  /// Imprime um relatório de erros.
  /// </summary>
  /// <param name="faultReport">
  /// Uma matriz de tuplas contendo o nó e um array de erros relacionados.
  /// </param>
  private void PrintFaultReport((INode Node, Fault[] Faults)[] faultReport)
  {
    Console.Error.WriteLine("Foram contrados erros:");
    Console.Error.WriteLine();
    foreach (var (node, faults) in faultReport)
    {
      Console.Error.WriteLine(node.Path);
      foreach (var fault in faults)
      {
        Console.Error.WriteLine($"- {fault.Message}");
      }
      Console.Error.WriteLine();
    }
    return;
  }
}
