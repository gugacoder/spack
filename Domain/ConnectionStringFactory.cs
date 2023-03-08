using System.Text.Json.Serialization;

namespace SPack.Domain;

/// <summary>
/// Fábrica de conexão de base de dados baseado em consulta SQL.
/// </summary>
public class ConnectionStringFactory : IMetaNode
{
  public ConnectionStringFactory()
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
  /// String de conexão com a base de dados.
  /// Quando informado, as propriedades <see cref="Connection"/> e
  /// <see cref="Query"/> são ignoradas.
  /// </summary>
  public string ConnectionString { get; set; } = string.Empty;

  /// <summary>
  /// Nome da configuração da base de dados, quando usando uma consulta na base
  /// de dados para produzir a string de conexão.
  /// 
  /// Propriedade utilizada em conjunto com a propriedade <see cref="Query"/>.
  /// 
  /// Esta propriedade é ignorada quando a propriedade
  /// <see cref="ConnectionString"/> é informada.
  /// </summary>
  public string Connection { get; set; } = string.Empty;

  /// <summary>
  /// Consulta SQL para obtenção de dados de conexão, quando usando uma consulta
  /// na base de dados para produzir a string de conexão.
  /// 
  /// Propriedade utilizada em conjunto com a propriedade <see cref="Connection"/>.
  /// 
  /// Esta propriedade é ignorada quando a propriedade
  /// <see cref="ConnectionString"/> é informada.
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

  public override string ToString()
  {
    return string.IsNullOrEmpty(ConnectionString)
      ? $"{base.ToString()} Using a query over the connection {Connection}"
      : $"{base.ToString()} {ConnectionString}";
  }
}
