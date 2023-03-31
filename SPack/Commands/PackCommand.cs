using System.Text;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Model;
using SPack.Commands.Helpers;
using SPack.Helpers;
using SPack.Prompting;

namespace SPack.Commands;

/// <summary>
/// Comando de empacotamento de scripts de migração de bases de dados.
/// </summary>
/// <remarks>
/// O empacotamento de scripts é o processo de geração do arquivo compactado
/// que contém os scripts de migração de bases de dados.
/// O arquivo compactado pode ser usado para migração de scripts em logo quando
/// repassado para o comando SPack ou qualquer outro método de execução do
/// algoritmo do ScriptPack.
/// </remarks>
public class PackCommand : ICommand
{
  /// <summary>
  /// Executa o comando de empacotamento de scripts.
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
    //
    // Estamos montando pipelines para garantir que somente os scripts que
    // passam por todas as regras de filtragem na montagem de pipelines sejam
    // empacotados.
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
    // Tudo OK, vamos empacotar os scripts.
    //
    var packageFile = Path.GetFullPath(options.Pack.Value);
    if (Path.GetExtension(packageFile) == "")
    {
      packageFile += ".scriptpack";
    }

    var password = options.Password.On ? options.Password.Value : null;

    var allScripts = repository.Descendants<ScriptNode>().ToList();

    var scriptPacker = new ScriptPacker();
    scriptPacker.AddTargetFile(packageFile);
    scriptPacker.AddPassword(password);
    allScripts.ForEach(scriptPacker.AddScript);

    await scriptPacker.PackScriptsAsync();

    Console.WriteLine("Pacote de scripts criado!");
    Console.WriteLine($"  {packageFile}");
  }
}
