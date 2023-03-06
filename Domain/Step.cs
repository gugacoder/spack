namespace SPack.Domain;

/// <summary>
/// Bloco de execução de scripts dentro de um pipeline.
/// </summary>
public class Step : IMetaNode
{
  public Step()
  {
    this.Faults = new(this);
  }

  INode? INode.Parent { get; set; }

  /// <summary>
  /// Nome do passo.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Rótulo de filtragem de scripts incluídos na fila.
  /// </summary>
  public string Tag { get; set; } = string.Empty;

  /// <summary>
  /// Precedência de execução do estágio em relação aos demais que compa
  /// compartilham o mesmo rótulo.
  /// </summary>
  public int Precedence { get; set; } = 0;

  /// <summary>
  /// Determina se a execução dos scripts devem ser feita em uma transação
  /// quando disponível.
  /// </summary>
  public bool Transactional { get; set; } = true;

  /// <summary>
  /// Sequência de scripts a serem executados.
  /// </summary>
  public List<Script> Scripts { get; set; } = new();

  /// <summary>
  /// Falhas ocorridas durante a execução do stage.
  /// </summary>
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    foreach (var item in Scripts) yield return item;
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Scripts.ForEach(item => item.Accept(visitor));
    Faults.ForEach(item => item.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await Task.WhenAll(Scripts.Select(item => item.AcceptAsync(visitor)));
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }

  public override string ToString() => $"{base.ToString()} {Name} {Tag}".Trim();
}
