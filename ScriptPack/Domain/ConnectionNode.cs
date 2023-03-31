namespace ScriptPack.Domain;

/// <summary>
/// Configuração de uma base de dados do sistema.
/// </summary>
public class ConnectionNode : AbstractNode
{
  public ConnectionNode()
  {
    this.Faults = new();
  }

  /// <summary>
  /// Determina se a conexão deve ser usada como conexão padrão em caso de
  /// omissão.
  /// </summary>
  /// <remarks>
  /// Critério de seleção de conexão padrão:
  /// 
  /// <para>
  /// Quando nenhuma conexão padrão é indicada, a primeira conexão não-vinculada
  /// (BoundTo = null) é utilizada. Se não houver nenhuma conexão não-vinculada
  /// a primeira conexão disponível é utilizada.
  /// </para>
  /// 
  /// <para>
  /// Quando mais de uma conexão padrão é especificada a primeira conexão
  /// padrão não-vinculada (BoundTo = null) é utilizada. Se não houver nenhuma
  /// conexão padrão não-vinculada a primeira conexão padrão disponível é
  /// utilizada.
  /// </para>
  /// </remarks>
  public bool IsDefault { get; set; } = false;

  /// <summary>
  /// Nome sugerido para bases de dados implantadas por esta conexão.
  /// </summary>
  public string DefaultDatabaseName { get; set; } = "";

  /// <summary>
  /// Nome da base de dados a qual esta base se vincula.
  /// Permite a partição e uma base de dados principal em uma base de dados
  /// secundária para distribuição de dados.
  /// </summary>
  public string? BoundTo { get; set; }

  /// <summary>
  /// Driver de base de dados.
  /// </summary>
  public string Provider { get; set; } = Providers.SQLServer;

  /// <summary>
  /// Fábrica de conexão de base de dados baseado em consulta SQL.
  /// </summary>
  public ConnectionStringFactoryNode? Factory
  {
    get => Get<ConnectionStringFactoryNode>();
    set => Set(value);
  }
}
