namespace SPack.Domain;

public interface INode
{
  /// <summary>
  /// Nodo pai.
  /// </summary>
  INode? Parent { get; set; }

  /// <summary>
  /// Filhos do nodo.
  /// </summary>
  IEnumerable<INode> GetChildren();
}
