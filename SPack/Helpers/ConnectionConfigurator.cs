using ScriptPack.Domain;

namespace SPack.Helpers;

/// <summary>
/// Utilitário para aplicação de configurações de conexão em catálogos.
/// </summary>
public class ConnectionConfigurator
{
  /// <summary>
  /// Aplica as configurações de conexão nos catálogos disponíveis a partir
  /// do nodo indicado. As configurações são aplicadas recursivamente em
  /// todos os catálogos disponíveis, descendentes ou ascendentes do nodo.
  /// </summary>
  public void ConfigureConnections(INode node, List<string> connectionMaps)
  {
    var catalogs = node.Root().DescendantsAndSelf<CatalogNode>();
    foreach (var catalog in catalogs)
    {
      MapConnectionsFromCatalog(catalog, connectionMaps);
    }
  }

  /// <summary>
  /// Aplica as configurações de conexão em um catálogo.
  /// </summary>
  /// <param name="catalog">
  /// O catálogo a ser configurado.
  /// </param>
  /// <param name="connectionMaps">
  /// As configurações de conexão a serem aplicadas.
  /// </param>
  private void MapConnectionsFromCatalog(CatalogNode catalog,
      List<string> connectionMaps)
  {
    var connections = catalog.Descendants<ConnectionNode>();
    foreach (var connectionMap in connectionMaps)
    {
      // A conexão é definida na forma:
      //    <nome>:<connection string>
      // Exemplo:
      //    myapp:Server=127.0.0.1;Database=MyDB;User Id=MyUser;Password=MyPass;
      var tokens = connectionMap.Split(':', 2);
      var connectionName = tokens.First().Trim();
      var connectionString = tokens.Last().Trim();

      var connection = connections.FirstOrDefault(x => x.Name == connectionName);
      if (connection == null)
      {
        Console.Error.WriteLine(
            $"[NOTA]A conexão '{connectionName}' não foi encontrada no " +
            $"catálogo: {catalog.Path}");
        continue;
      }

      connection.ConnectionStringFactory = new(connectionString);
    }
  }
}
