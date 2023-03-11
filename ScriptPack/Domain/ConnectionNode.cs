using System.Text.Json.Serialization;

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
  /// Nome da configuração da base de dados.
  /// Em geral representa o nome sugerido para implantação da base de dados.
  /// </summary>
  public string Name { get; set; } = "";

  /// <summary>
  /// Descrição da base de dados.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Indica se a seção está habilitada.
  /// Se estiver desabilitada, o conteúdo da seção não será executado.
  /// </summary>
  public bool Enabled { get; set; } = true;

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
  public string Provider { get; set; } = nameof(Providers.SqlServer);

  /// <summary>
  /// Fábrica de conexão de base de dados baseado em consulta SQL.
  /// </summary>
  /// <value></value>
  public ConnectionStringFactoryNode? ConnectionStringFactory
  {
    get => Get<ConnectionStringFactoryNode>();
    set => Set(value);
  }
}
