using ScriptPack.Domain;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Mapeamento de conexões e strings de conexão eleitas para migração de
/// scripts.
/// </summary>
public class ConnectionPool : Dictionary<string, ConnectionPool.Entry>
{
  public record Entry(ConnectionNode Connection, string ConnectionString);

  public ConnectionPool()
    : base(StringComparer.OrdinalIgnoreCase)
  {
  }

  /// <summary>
  /// Inicializa uma nova instância da classe <see cref="ConnectionPool"/>.
  /// </summary>
  /// <param name="items">
  /// Conjunto de itens para inicializar a instância.
  /// </param>
  public ConnectionPool(IEnumerable<Entry> items)
    : base(StringComparer.OrdinalIgnoreCase)
  {
    foreach (var item in items)
    {
      this[item.Connection.Name] = item;
    }
  }

  /// <summary>
  /// Inicializa uma nova instância da classe <see cref="ConnectionPool"/>.
  /// </summary>
  /// <param name="items">
  /// Conjunto de itens para inicializar a instância.
  /// </param>
  public ConnectionPool(IEnumerable<KeyValuePair<string, Entry>> items)
    : base(items, StringComparer.OrdinalIgnoreCase)
  {
  }

  /// <summary>
  /// Determina o nome do banco de dados para a conexão especificada.
  /// </summary>
  /// <param name="connectionName">
  /// Nome da conexão para a qual o nome do banco de dados será determinado.
  /// </param>
  /// <remarks>
  /// O nome do banco de dados é determinado pela investigação da sua string
  /// de conexão. Diferentes provedores de banco de dados podem utilizar
  /// diferentes parâmetros para determinar o nome do banco de dados, como:
  /// -   "Database": Este parâmetro é utilizado em muitos provedores de banco
  ///     de dados, como SQL Server, Oracle, MySQL e PostgreSQL, para
  ///     especificar o nome do banco de dados ao qual se conectar.
  /// -   "Initial Catalog": Este parâmetro é utilizado no SQL Server para
  ///     especificar o nome do banco de dados ao qual se conectar.
  ///     É equivalente ao parâmetro "Database".
  /// -   "DefaultCatalog": Este parâmetro é utilizado em alguns provedores de
  ///     banco de dados, como IBM DB2, para especificar o catálogo padrão para
  ///     a conexão.
  /// -   "Database Name": Este parâmetro é utilizado no MySQL para especificar
  ///     o nome do banco de dados.
  /// -   "DBQ": Este parâmetro é utilizado no ODBC para especificar o nome do
  ///     arquivo de banco de dados ou o nome da fonte de dados.
  /// <returns>
  /// O nome do banco de dados para a conexão especificada.
  /// </returns>
  public string? GetDatabaseName(string connectionName)
  {
    var entry = this[connectionName];

    var databaseParameterVariations = new[]{
        "Database",
        "Initial Catalog" ,
        "DefaultCatalog",
        "Database Name" ,
        "DBQ"
    };

    var tokens = entry.ConnectionString.Split(';');
    foreach (var token in tokens)
    {
      var parts = token.Split('=');
      if (databaseParameterVariations.Contains(parts[0],
          StringComparer.OrdinalIgnoreCase))
      {
        return parts[1];
      }
    }

    return null;
  }
}
