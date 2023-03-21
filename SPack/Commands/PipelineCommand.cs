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
public class PipelineCommand : ICommand
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
    // Realizando a impressão do plano de execução.
    //
    var connectionPool = rootNode.Descendants<ConnectionNode>().ToArray();

    ReportPipelineExecutionPlan(pipelines, connectionPool);
  }

  /// <summary>
  /// Imprime o plano de execução de um pipeline.
  /// </summary>
  /// <param name="pipelines">
  /// Os pipelines a serem impressos.
  /// </param>
  /// <param name="connectionPool">
  /// A lista de conexões disponíveis.
  /// </param>
  private void ReportPipelineExecutionPlan(List<PipelineNode> pipelines,
      ConnectionNode[] connectionPool)
  {
    if (!pipelines.Any())
    {
      Console.WriteLine("NENHUM PLANO DE EXECUÇÃO ENCONTRADO");
      return;
    }

    foreach (var pipeline in pipelines.Concat(pipelines))
    {
      var database = InferDatabaseIfPossible(pipeline.Connection,
          connectionPool);

      Console.WriteLine($"+- PIPELINE {pipeline.Name}");
      foreach (var stage in pipeline.Stages)
      {
        Console.WriteLine($"   +- STAGE {stage.Name}");
        foreach (var step in stage.Steps)
        {
          Console.WriteLine($"      +- STEP {step.Name}");
          foreach (var script in step.Scripts)
          {
            var fileName = Path.GetFileName(script.Path);
            Console.WriteLine($"         +- {fileName}");
          }
        }
      }
    }
    Console.WriteLine("-- FIM DO PLANO DE EXECUÇÃO --");
  }

  /// <summary>
  /// Tenta determinar o nome da base de dados a partir da configuração de
  /// conexão.
  /// Não é possível determinar o nome da base de dados com precisão porque
  /// existem regras de seleção de base que podem ser executadas somente
  /// durante a execução do pipeline.
  /// </summary>
  /// <param name="connection">
  /// A configuração de conexão.
  /// </param>
  /// <param name="connectionPool">
  /// A lista de conexões disponíveis.
  /// </param>
  /// <returns>
  /// O nome da base de dados inferida da conexão.
  /// </returns>
  private string InferDatabaseIfPossible(ConnectionNode connection,
      ConnectionNode[] connectionPool)
  {
    var factory = connection.ConnectionStringFactory;
    if (factory?.ConnectionString != null)
    {
      var databaseName = (
          from pair in factory.ConnectionString.Split(';')
          let parts = pair.Split('=')
          let key = parts[0].Trim()
          let value = parts[1].Trim()
          where key.Equals("database", StringComparison.OrdinalIgnoreCase)
          select value
      ).FirstOrDefault();

      if (!string.IsNullOrEmpty(databaseName))
        return databaseName;
    }

    if (factory?.Connection != null && factory.Query == null)
    {
      var targetConnection = connectionPool.FirstOrDefault(
          c => c.Name == factory.Connection);
      if (targetConnection != null)
      {
        return InferDatabaseIfPossible(targetConnection, connectionPool);
      }
    }

    return string.IsNullOrEmpty(connection.DefaultDatabaseName)
        ? "(indefinido)"
        : connection.DefaultDatabaseName;
  }
}