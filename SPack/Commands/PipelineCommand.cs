using System.Text;
using ScriptPack.Model.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;
using SPack.Helpers;
using SPack.Prompting;
using SPack.Commands.Helpers;

namespace SPack.Commands;

/// <summary>
/// Comando de execução de scripts de migração de bases de dados.
/// </summary>
public class PipelineCommand : ICommand
{
  /// <summary>
  /// Executa o comando de migração de dados.
  /// </summary>
  public async Task RunAsync(CommandLineOptions options)
  {
    var repositoryUtilityBuilder = new RepositoryUtilityBuilder();
    repositoryUtilityBuilder.AddOptions(options);
    repositoryUtilityBuilder.AddValidators();

    var repository = await repositoryUtilityBuilder.BuildRepositoryAsync();

    // Selecionando os scripts.
    var nodeSelectorBuilder = new PackageSelectionBuilder();
    nodeSelectorBuilder.AddOptions(options);
    nodeSelectorBuilder.AddRepository(repository);

    var nodes = nodeSelectorBuilder.BuildPackageSelection();

    // Selecionando as conexões.
    var connectionSelectorBuilder = new ConnectionSelectionBuilder();
    connectionSelectorBuilder.AddOptions(options);
    nodes.ForEach(connectionSelectorBuilder.AddConnectionsFromNode);
    var connections = connectionSelectorBuilder.BuildConnectionSelection();

    // Montando pipelines.
    var pipelineBuilder = new PipelineBuilder();
    nodes.ForEach(pipelineBuilder.AddScripts);
    connections.ForEach(pipelineBuilder.AddConnection);
    var pipelines = pipelineBuilder.BuildPipelines();

    // Detectando e reportando falhas.
    var faultReportBuilder = new FaultReportBuilder();
    faultReportBuilder.AddOptions(options);
    faultReportBuilder.AddNodes(repository);

    var faultReport = faultReportBuilder.BuildFaultReport();
    if (faultReport.Length > 0)
    {
      var printer = new FaultReportPrinter();
      printer.AddOptions(options);
      printer.AddFaultReport(faultReport);
      printer.PrintFaultReport();
      return;
    }

    ReportPipelineExecutionPlan(pipelines);
  }

  /// <summary>
  /// Imprime o plano de execução de um pipeline.
  /// </summary>
  /// <param name="pipelines">
  /// Os pipelines a serem impressos.
  /// </param>
  private void ReportPipelineExecutionPlan(List<PipelineNode> pipelines)
  {
    if (!pipelines.Any())
    {
      Console.WriteLine("NENHUM PLANO DE EXECUÇÃO ENCONTRADO");
      return;
    }

    // Realizando a impressão do plano de execução.
    var connectionPool = (
        from node in pipelines
        from catalog in node.Ancestors<CatalogNode>()
        from connection in catalog.Connections
        select connection
    ).Distinct().ToArray();

    foreach (var pipeline in pipelines)
    {
      var database = DetectDatabase(pipeline.Connection, connectionPool);
      var databaseInfo =
          $"{pipeline.Connection.Name} (" +
          $"Provider={pipeline.Connection.Provider};" +
          $"Database={database}" +
          $")";

      Console.WriteLine($"+- PIPELINE {pipeline.Name}");
      foreach (var stage in pipeline.Stages)
      {
        Console.WriteLine($"   +- STAGE {stage.Name}");
        Console.WriteLine($"      +- CONNECTION {databaseInfo}");
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
  private string DetectDatabase(ConnectionNode connection,
      ConnectionNode[] connectionPool)
  {
    var factory = connection.Factory;
    if (factory?.ConnectionString.Contains("=") ?? false)
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

    if (factory?.Connection is not null && factory.Query is null)
    {
      var targetConnection = connectionPool.FirstOrDefault(
          c => c.Name == factory.Connection);
      if (targetConnection is not null)
      {
        return DetectDatabase(targetConnection, connectionPool);
      }
    }

    return string.IsNullOrEmpty(connection.DefaultDatabaseName)
        ? "(indefinido)"
        : connection.DefaultDatabaseName;
  }
}