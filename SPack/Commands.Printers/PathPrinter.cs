using ScriptPack.Domain;
using ScriptPack.Helpers;

namespace SPack.Commands.Printers;

/// <summary>
/// Imprime os caminhos dos nodos.
/// </summary>
public class PathPrinter : IPrinter
{
  private readonly List<INode> _nodes = new();

  /// <summary>
  /// Adiciona um nodo à lista de nodos a serem impressos.
  /// </summary>
  public PathPrinter AddNodes(List<INode> nodes)
  {
    _nodes.AddRange(nodes);
    return this;
  }

  /// <summary>
  /// Imprime os caminhos dos nodos.
  /// </summary>
  public void Print()
  {
    var paths = _nodes
        .Except(_nodes.OfType<ConnectionStringFactoryNode>())
        .Select(x => x.Path)
        .OrderBy(x => x, new PathComparer())
        .ToList();
    foreach (var node in paths)
    {
      Console.Out.WriteLine(node);
    }
    Console.Out.WriteLine($"Total: {paths.Count}");
  }
}
