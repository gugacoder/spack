using System.Text.Json.Serialization;

namespace SPack.Domain;

/// <summary>
/// Configuração de uma base de dados do sistema.
/// </summary>
public class Connection : IMetaNode
{
  public Connection()
  {
    this.Faults = new(this);
  }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  public Catalog? Parent { get; set; }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  INode? INode.Parent { get => Parent; set => Parent = (Catalog?)value; }

  /// <summary>
  /// Nome da configuração da base de dados.
  /// Em geral representa o nome sugerido para implantação da base de dados.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Nome sugerido para bases de dados implantadas por esta conexão.
  /// </summary>
  public string DefaultDatabaseName { get; set; } = string.Empty;

  /// <summary>
  /// Nome da base de dados a qual esta base se vincula.
  /// Permite a partição e uma base de dados principal em uma base de dados
  /// secundária para distribuição de dados.
  /// </summary>
  public string? BoundTo { get; set; }

  /// <summary>
  /// Descrição da base de dados.
  /// </summary>
  /// <value></value>
  public string? Description { get; set; }

  /// <summary>
  /// Driver de base de dados.
  /// </summary>
  /// <value></value>
  public string Provider { get; set; } = Providers.SqlServer;

  /// <summary>
  /// Indica se a seção está habilitada.
  /// Se estiver desabilitada, o conteúdo da seção não será executado.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Fábrica de conexão de base de dados baseado em consulta SQL.
  /// </summary>
  /// <value></value>
  public ConnectionFactory? Factory { get; set; }

  /// <summary>
  /// Falhas ocorridas durante a criação ou execução da conexão.
  /// </summary>
  [JsonIgnore]
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    if (Factory != null) yield return Factory;
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Factory?.Accept(visitor);
    Faults.ForEach(f => f.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await (Factory?.AcceptAsync(visitor) ?? Task.CompletedTask);
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }

  public override string ToString() => $"{base.ToString()} {Name}";
}
