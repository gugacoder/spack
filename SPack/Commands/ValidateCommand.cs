using System.Text;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;
using SPack.Commands.Helpers;
using SPack.Helpers;
using SPack.Prompting;

namespace SPack.Commands;

/// <summary>
/// Comando de execução de scripts de migração de bases de dados.
/// </summary>
public class ValidateCommand : ICommand
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

    var nodeSelectorBuilder = new PackageSelectionBuilder();
    nodeSelectorBuilder.AddOptions(options);
    nodeSelectorBuilder.AddRepository(repository);

    var nodes = nodeSelectorBuilder.BuildPackageSelection();

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

    Console.WriteLine("OK! Nenhuma falha detectada.");
  }
}
