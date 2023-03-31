using SPack.Prompting;

namespace SPack.Commands.Helpers;

/// <summary>
/// Impressora de falhas em nodos de um catálogo.
/// </summary>
public class FaultReportPrinter
{
  private CommandLineOptions? _options;
  private FaultReportEntry[] _faultReport = null!;

  /// <summary>
  /// Adiciona critérios de seleção a partir das opções de linha de comando.
  /// </summary>
  /// <param name="options">
  /// Opções de linha de comando.
  /// </param>
  public void AddOptions(CommandLineOptions options)
  {
    _options = options;
  }

  /// <summary>
  /// Adiciona um relatório de erros.
  /// </summary>
  /// <param name="faultReport">
  /// Uma matriz de tuplas contendo o nodo e um array de erros relacionados.
  /// </param>
  public void AddFaultReport(FaultReportEntry[] faultReport)
  {
    _faultReport = faultReport;
  }

  /// <summary>
  /// Imprime um relatório de erros.
  /// </summary>
  public void PrintFaultReport()
  {
    var verbose = _options?.Verbose.On == true;

    Console.Error.WriteLine("Foram contrados erros:");
    Console.Error.WriteLine();
    foreach (var (node, faults) in _faultReport)
    {
      Console.Error.WriteLine(node.Path);
      foreach (var fault in faults)
      {
        Console.Error.WriteLine($"- {fault.Message}");
        if (verbose) Console.Error.WriteLine(fault.Details);
      }
      Console.Error.WriteLine();
    }
    return;
  }
}
