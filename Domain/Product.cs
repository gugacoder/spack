using System.Collections;
using System.Text.Json.Serialization;

namespace SPack.Domain;

/// <summary>
/// Representação de um produto modificável por scripts.
/// </summary>
public class Product : IFileNode
{
  public Product()
  {
    this.Modules = new(this);
    this.Faults = new(this);
  }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  public Catalog? Parent { get; set; }

  [JsonIgnore]
  INode? INode.Parent { get => Parent; set => Parent = (Catalog?)value; }

  /// <summary>
  /// Nome do script.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Caminho virtual do nodo dentro da árvore de nodos.
  /// </summary>
  public string Path => $"{Parent?.Path}/{Name}/{Version}";

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
  /// Versão do produto segundo o padrão semântico.
  /// Referência: https://semver.org/
  /// </summary>
  public string Version { get; set; } = string.Empty;

  /// <summary>
  /// Módulos do produto.
  /// </summary>
  [JsonIgnore]
  public NodeList<Module> Modules { get; set; }

  /// <summary>
  /// Falhas ocorridas durante a execução do produto.
  /// </summary>
  [JsonIgnore]
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    foreach (var item in Modules) yield return item;
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Modules.ForEach(item => item.Accept(visitor));
    Faults.ForEach(item => item.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await Task.WhenAll(Modules.Select(item => item.AcceptAsync(visitor)));
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }

  public override string ToString() => $"{base.ToString()} {Path}";
}
