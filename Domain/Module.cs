using System.Collections;
using System.Text.Json.Serialization;

namespace SPack.Domain;

/// <summary>
/// Representação de um módulo de produto.
/// </summary>
public class Module : IFileNode
{
  public Module()
  {
    this.Packages = new(this);
    this.Faults = new(this);
  }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  public Product? Parent { get; set; }

  [JsonIgnore]
  INode? INode.Parent { get => Parent; set => Parent = (Product?)value; }

  /// <summary>
  /// Nome do módulo.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Caminho virtual do nodo dentro da árvore de nodos.
  /// </summary>
  public string Path => $"{Parent?.Path}/{Name}";

  /// <summary>
  /// Nome do nó.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Caminho relativo do arquivo referente.
  /// </summary>
  [JsonIgnore]
  public string? FilePath { get; set; } = string.Empty;

  /// <summary>
  /// Indica se a seção está habilitada.
  /// Se estiver desabilitada, o conteúdo da seção não será executado.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Precedência do módulo em relação aos demais
  /// </summary>
  public int Precedence { get; set; } = 0;

  /// <summary>
  /// Scripts do pacote.
  /// </summary>
  [JsonIgnore]
  public NodeList<Package> Packages { get; set; }

  /// <summary>
  /// Falhas ocorridas durante a execução do módulo.
  /// </summary>
  [JsonIgnore]
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    foreach (var item in Packages) yield return item;
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Packages.ForEach(item => item.Accept(visitor));
    Faults.ForEach(item => item.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await Task.WhenAll(Packages.Select(item => item.AcceptAsync(visitor)));
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }

  public override string ToString() => $"{base.ToString()} {Path}";
}
