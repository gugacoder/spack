using System.Collections;
using System.Text.Json.Serialization;

namespace SPack.Domain;

/// <summary>
/// Pacote de scripts.
/// </summary>
public class Package : IFileNode
{
  public Package()
  {
    this.Scripts = new(this);
    this.Faults = new(this);
  }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  public Module? Parent { get; set; }

  [JsonIgnore]
  INode? INode.Parent { get => Parent; set => Parent = (Module?)value; }

  /// <summary>
  /// Nome do script.
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
  /// Indica se a seção está habilitada.
  /// Se estiver desabilitada, o conteúdo da seção não será executado.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Precedência do pacote em relação aos demais
  /// </summary>
  /// <value></value>
  public int Precedence { get; set; } = 0;

  /// <summary>
  /// Nomes de configurações de conexões que devem ser utilizadas para execução
  /// do pacote.
  /// </summary>
  public List<string> Connections { get; set; } = new();

  /// <summary>
  /// Caminho relativo do arquivo referente.
  /// </summary>
  [JsonIgnore]
  public string? FilePath { get; set; } = string.Empty;

  /// <summary>
  /// Scripts do pacote.
  /// </summary>
  [JsonIgnore]
  public NodeList<Script> Scripts { get; set; }

  /// <summary>
  /// Falhas ocorridas durante a criação ou execução do pacote.
  /// </summary>
  [JsonIgnore]
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

  public override string ToString() => $"{base.ToString()} {Path}";
}
