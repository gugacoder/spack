using System.Text;
using ScriptPack.Helpers;
using ScriptPack.Model;
using SPack.Commands.Helpers;
using SPack.Commands.Printers;
using SPack.Helpers;
using SPack.Prompting;

namespace SPack.Commands;

/// <summary>
/// Comando de execução de scripts de migração de bases de dados.
/// </summary>
public class MigrateCommand : ICommand
{
  /// <summary>
  /// Executa o comando de migração de dados.
  /// </summary>
  public async Task RunAsync(CommandLineOptions options)
  {
    var repositoryUtilityBuilder = new RepositoryUtilityBuilder();
    repositoryUtilityBuilder.AddOptions(options);
    if (!options.IgnoneDependencies.On)
    {
      repositoryUtilityBuilder.AddValidators();
    }

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

    //
    // Realizando a migração de bases.
    //
    var contextBuilder = new ContextBuilder();
    contextBuilder.AddOptions(options);
    var context = contextBuilder.BuildContext();

    foreach (var pipeline in pipelines)
    {
      var databaseMigrator = new DatabaseMigrator
      {
        Pipeline = pipeline,
        Context = context
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
        var cause = args.Exception;
        Console.Error.WriteLine($"[ERRO] {cause.Message}");
        while ((cause = cause?.InnerException) is not null)
        {
          Console.Error.WriteLine($"- {cause.Message}");
        }
        if (verbose)
        {
          Console.Error.WriteLine(args.Exception.StackTrace);
        }
        Console.Error.WriteLine();
      };
    }

    databaseMigrator.OnMigrate += (sender, args) =>
        Console.WriteLine(args.Script.Path);

    databaseMigrator.OnResultSet += (sender, args) =>
        new ResultSetPrinter().AddDbDataReader(args.Result).Print();

  }
}
