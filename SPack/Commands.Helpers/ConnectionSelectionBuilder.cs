using ScriptPack.Domain;
using ScriptPack.Helpers;
using SPack.Prompting;

namespace SPack.Commands.Helpers;

/// <summary>
/// Utilitário para seleção de conexões de banco de dados.
/// </summary>
public class ConnectionSelectionBuilder
{
  private CommandLineOptions? _options;
  private readonly List<INode> _nodes = new();

  /// <summary>
  /// Adiciona conexões lidas da linha de comando.
  /// </summary>
  /// <param name="options">
  /// Opções de linha de comando.
  /// </param>
  public void AddOptions(CommandLineOptions options)
  {
    _options = options;
  }

  /// <summary>
  /// Adiciona as conexões relativas aos nodos selecionados.
  /// As conexões são lidas a partir dos catálogos na hierarquia do nodo.
  /// </summary>
  public void AddConnectionsFromNode(INode node)
  {
    _nodes.Add(node);
  }

  /// <summary>
  /// Seleciona ou monta os nodos de conexões alvo da migração de scripts.
  /// </summary>
  /// <remarks>
  /// As conexões definidas nos catálogos dos scripts selecionados são
  /// adicionadas ao pool de conexões selecionadas.
  /// As conexões definidas na linha de comando sem correspondência nos
  /// catálogos também são acrescentadas ao pool de conexões selecionadas.
  /// </remarks>
  public List<ConnectionNode> BuildConnectionSelection()
  {
    var selectedConnections = new Dictionary<string, ConnectionNode>();

    // Selecionando as conexões pré-definidas.
    var predefinedConnections = (
        from node in _nodes
        from catalog in node.Ancestors<CatalogNode>()
        from connection in catalog.Connections
        select connection
    ).Distinct();

    // Adicionando as conexões pré-definidas.
    predefinedConnections.ForEach(connection =>
        selectedConnections[connection.Name.ToLower()] = connection);

    // Cada item da opção `database` contém uma string de conexão com duas
    // propriedades adicionais customizadas pelo SPack: `Name` e `Provider`.
    // Cada string de conexão definida para uma base de dados tem a forma:
    //     Name=<NOME_DA_CONEXÃO>;Provider=<PROVEDOR>;<STRING_DE_CONEXÃO>
    // Onde:
    //     <NOME_DA_CONEXÃO>
    //         Nome da conexão.
    //         Se omitido, o nome da conexão será "Default".
    //         Se houver uma conexão com o mesmo nome definida no catálogo esta
    //         definição a sobrescreverá. Portanto, é recomendado omitir o
    //         parâmetro `Provider` caso a conexão esteja definida no catálogo.
    //     <PROVEDOR>
    //         Nome do provedor de banco de dados.
    //         Se omitido o provedor será lido da conexão definida no catálogo.
    //         Se não houver uma conexão definida no catálogo, o provedor será
    //         "SQLServer".
    //         Nomes suportados pelo ScriptPack:
    //             "SQLServer" ou "mssql"
    //             "PostgreSQL" ou "pgsql"
    //     <STRING_DE_CONEXÃO>
    //         É a string de conexão para a base de dados.
    //         A string de conexão é composta por uma lista de pares de chave e
    //         valor separados por ponto-e-vírgula (;).
    //         Exemplo:
    //             "Server=localhost;Database=MyDB;User Id=sa;Password=123"
    var databases = _options?.Database?.Items ?? Enumerable.Empty<string>();
    foreach (var database in databases)
    {
      var args = database.Split(';').ToList();
      var nameProperty = args.FirstOrDefault(p => p.StartsWith("Name="));
      var providerProperty = args.FirstOrDefault(p => p.StartsWith("Provider="));

      if (nameProperty != null) args.Remove(nameProperty);
      if (providerProperty != null) args.Remove(providerProperty);

      var connectionString = string.Join(";", args);

      var name = nameProperty?.Split('=')[1] ?? "Default";

      selectedConnections.TryGetValue(name.ToLower(), out var template);

      var provider = providerProperty?.Split('=')[1]
          ?? template?.Provider
          ?? "SQLServer";

      var connectionNode = new ConnectionNode
      {
        Name = name,
        Provider = provider,
        IsDefault = (template?.IsDefault ?? name.Like("Default")),
        Factory = new(connectionString)
      };

      // Note que a conexão definida na linha de comando sobrepõe a conexão
      // pré-definida no catálogo. Este é extamente o comportamento esperado e
      // permite que a conexão seja completamente cusomizada na linha de
      // comando.
      selectedConnections[name.ToLower()] = connectionNode;
    }

    // Filtrando as conexões caso selecionadas na linha de comando.
    if (_options?.Connection.On == true)
    {
      var connectionNames = _options.Connection.Items
          .Select(p => p.ToLower())
          .ToArray();
      selectedConnections = selectedConnections
          .Where(p => connectionNames.Contains(p.Key))
          .ToDictionary(p => p.Key, p => p.Value);
    }

    return selectedConnections.Values.ToList();
  }
}
