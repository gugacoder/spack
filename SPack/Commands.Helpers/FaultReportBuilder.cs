using ScriptPack.Domain;
using SPack.Prompting;

namespace SPack.Commands.Helpers;

/// <summary>
/// Utiltiário para validação da estrutura dos nodos do catálogos e de
/// pipelines e suas fases.
/// </summary>
public class FaultReportBuilder
{
  private CommandLineOptions _options = null!;
  private List<INode> _nodes = new();

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
  /// Adiciona nodos para validação.
  /// </summary>
  /// <param name="nodes">
  /// Lista de nodos para análise de erros.
  /// </param>
  public void AddNodes(params INode[] nodes)
  {
    _nodes.AddRange(nodes);
  }

  /// <summary>
  /// Cria um relatório de erros ocorridos nos nodos indicados.
  /// </summary>
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
  public FaultReportEntry[] BuildFaultReport()
  {
    var pipeNodes =
        from node in _nodes.OfType<IPipeNode>()
        from item in node.DescendantsAndSelf()
        select item;

    var repoNodes = (
        from node in _nodes.Except(pipeNodes)
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
        select new FaultReportEntry(g.Key, g.ToArray())
    ).ToArray();

    return faultReport;
  }
}
