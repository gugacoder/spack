namespace SPack.Domain;

/// <summary>
/// Sequência dos scripts selecionados para execução.
/// </summary>
public class Pipeline : IMetaNode
{
  public Pipeline()
  {
    this.Stages = new(this);
    this.Faults = new(this);
  }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  INode? INode.Parent { get; set; }

  /// <summary>
  /// Nome do pipeline.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Conecção utilizada na execução do scripts do pipeline.
  /// </summary>
  public Connection Connection { get; set; } = null!;

  /// <summary>
  /// Blocos de scripts executados em sequência dentro do pipeline.
  /// </summary>
  public NodeList<Stage> Stages { get; set; }

  /// <summary>
  /// Falhas ocorridas durante a execução do pipeline.
  /// </summary>
  /// <returns></returns>
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    foreach (var item in Stages) yield return item;
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Stages.ForEach(item => item.Accept(visitor));
    Faults.ForEach(item => item.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await Task.WhenAll(Stages.Select(item => item.AcceptAsync(visitor)));
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }

  public override string ToString() => $"{base.ToString()} {Name}".Trim();
}
