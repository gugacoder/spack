using ScriptPack.Domain;

namespace SPack.Helpers;

/// <summary>
/// Utiltiário para validação da estrutura dos nodos do catálogos e de
/// pipelines e suas fases.
/// </summary>
public class FaultReporter
{
  /// <summary>
  /// Obtém ou define um valor booleano que indica se a execução deve ser
  /// verbosa ou não.
  /// </summary>
  public bool Verbose { get; set; } = false;

  /// <summary>
  /// Representação de uma entrada de falhas no relatórios de falhas.
  /// </summary>
  /// <param name="Node">
  /// Nodo que contém as falhas.
  /// </param>
  /// <param name="Faults">
  /// Lista de falhas ocorridas no nodo.
  /// </param>
  public record ReportEntry(INode Node, Fault[] Faults);

  /// <summary>
  /// Cria um relatório de erros ocorridos nos nodos indicados.
  /// </summary>
  /// <param name="nodes">
  /// Lista de nodos para análise de erros.
  /// </param>
  /// <returns>
  /// Uma tupla contendo o nodo e um array de erros relacionados.
  /// </returns>
  /// <remarks>
  /// O ScriptPack contém duas árvores de nodos distinta:
  /// -   A árvore componente do repositório, que contém catálogos, produtos,
  ///     versões, módulos, pacotes, scripts, conexões e outros nodos
  ///     relacionados ao projeto de migração de base de dados.
  /// -   A árvore de pipelines, que contém pipelines, etapas e passos.
  /// Este algoritmo é capaz de analisas as duas estruturas de árvore.
  /// Se a coleção de nodos contiver apenas nodos componentes do repositório
  /// apenas a estrutura de árvore de nodos do repositório será analisada.
  /// Se a coleção de nodos contiver nodos de pipelines, a estrutura de árvore
  /// de nodos de pipeline será analisada assim como a estrutura de árvore
  /// diretamente relacionada aos scripts componentes dos passos do pipeline.
  /// </remarks>
  public ReportEntry[] CreateFaultReport(IEnumerable<INode> nodes)
  {
    var pipeNodes =
        from node in nodes.OfType<IPipeNode>()
        from item in node.DescendantsAndSelf()
        select item;

    var repoNodes = (
        from node in nodes.Except(pipeNodes)
        from item in node.DescendantsAndSelf()
        select item
    ).Union(
        from step in pipeNodes.OfType<StepNode>()
        from script in step.Scripts
        select script
    );

    var faultReport = (
        from node in pipeNodes.Union(repoNodes)
        from fault in node.Faults
        group fault by node into g
        select new ReportEntry(g.Key, g.ToArray())
    ).ToArray();

    return faultReport;
  }

  /// <summary>
  /// Imprime um relatório de erros.
  /// </summary>
  /// <param name="faultReport">
  /// Uma matriz de tuplas contendo o nodo e um array de erros relacionados.
  /// </param>
  public void PrintFaultReport(ReportEntry[] faultReport)
  {
    Console.Error.WriteLine("Foram contrados erros:");
    Console.Error.WriteLine();
    foreach (var (node, faults) in faultReport)
    {
      Console.Error.WriteLine(node.Path);
      foreach (var fault in faults)
      {
        Console.Error.WriteLine($"- {fault.Message}");
        if (Verbose) Console.Error.WriteLine(fault.Details);
      }
      Console.Error.WriteLine();
    }
    return;
  }
}
