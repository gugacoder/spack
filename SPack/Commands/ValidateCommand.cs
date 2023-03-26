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
    var nodeSelectorBuilder = new PackageSelectionBuilder();
    nodeSelectorBuilder.AddOptions(options);
    nodeSelectorBuilder.AddValidators();

    var nodes = await nodeSelectorBuilder.BuildPackageSelectionAsync();

    // Detectando falhas.
    var faultReporter = new FaultReporter { Verbose = options.Verbose.On };
    var faultReport = faultReporter.CreateFaultReport(nodes);

    // Reportando falhas.
    if (faultReport.Length > 0)
    {
      Environment.ExitCode = 1;
      faultReporter.PrintFaultReport(faultReport);
      return;
    }

    Console.WriteLine("OK! Nenhuma falha detectada.");
  }
}
