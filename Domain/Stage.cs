namespace SPack.Domain;

/// <summary>
/// Bloco de execução de scripts dentro de um pipeline.
/// </summary>
public class Stage : IMetaNode
{
  public Stage()
  {
    this.Faults = new(this);
  }

  INode? INode.Parent { get; set; }

  /// <summary>
  /// Nome do estágio.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Sequência de passos a serem executados.
  /// </summary>
  public List<Step> Steps { get; set; } = new();

  /// <summary>
  /// Falhas ocorridas durante a execução do stage.
  /// </summary>
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    foreach (var item in Steps) yield return item;
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Steps.ForEach(item => item.Accept(visitor));
    Faults.ForEach(item => item.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await Task.WhenAll(Steps.Select(item => item.AcceptAsync(visitor)));
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }

  public override string ToString() => $"{base.ToString()} {Name}".Trim();
}
