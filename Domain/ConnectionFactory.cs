using System.Text.Json.Serialization;

namespace SPack.Domain;

/// <summary>
/// Fábrica de conexão de base de dados baseado em consulta SQL.
/// </summary>
public class ConnectionFactory : IMetaNode
{
  public ConnectionFactory()
  {
    this.Faults = new(this);
  }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  public Connection? Parent { get; set; }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  INode? INode.Parent { get => Parent; set => Parent = (Connection?)value; }

  /// <summary>
  /// Nome da configuração da base de dados.
  /// </summary>
  public string Connection { get; set; } = string.Empty;

  /// <summary>
  /// Consulta SQL para obtenção de dados de conexão.
  /// </summary>
  public string Query { get; set; } = string.Empty;

  /// <summary>
  /// Falhas ocorridas durante a criação ou execução da conexão.
  /// </summary>
  [JsonIgnore]
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Faults.ForEach(f => f.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }
}
