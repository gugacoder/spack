using System.Text.Json.Serialization;

namespace SPack.Domain;

/// <summary>
/// Catálogo de produtos e seus scripts.
/// </summary>
public class Repository : IMetaNode
{
  public Repository()
  {
    this.Catalogs = new(this);
    this.Faults = new(this);
  }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  INode? INode.Parent { get; set; }

  /// <summary>
  /// Lista de produtos disponíveis no Catálogo.
  /// </summary>
  [JsonIgnore]
  public NodeList<Catalog> Catalogs { get; set; }

  /// <summary>
  /// Falhas ocorridas durante a execução do catálogo.
  /// </summary>
  [JsonIgnore]
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    foreach (var item in Catalogs) yield return item;
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Catalogs.ForEach(item => item.Accept(visitor));
    Faults.ForEach(item => item.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await Task.WhenAll(Catalogs.Select(item => item.AcceptAsync(visitor)));
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }
}
