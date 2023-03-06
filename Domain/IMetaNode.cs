namespace SPack.Domain;

public interface IMetaNode : INode
{
  /// <summary>
  /// Falhas ocorridas durante a criação ou execução do nodo.
  /// </summary>
  NodeList<Fault> Faults { get; set; }
}
