using System.Data.Common;
using ScriptPack.Domain;
using ScriptPack.Helpers;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Utilitário para construção de um pool de conexões pela resolução das
/// strings de conexão.
/// </summary>
/// <remarks>
/// A string de conexão pode ser definida diretamente para um template de
/// conexão ou pode ser definido por uma consulta SQL a ser executar em uma das
/// demais conexões disponíveis. Uma consulta SQL pode tanto devolver a string
/// de conexão montada quando como suas partes em colunas separadas. É possível
/// ainda definir como string de conexão a string de conexão de outra conexão
/// disponível. Em geral estas regras de conectividade são definidas no projeto
/// do ScriptPack dentro do arquivo `Catalog.json`.
/// </remarks>
public class ConnectionPoolBuilder
{
  private readonly List<ConnectionNode> _templates = new();
  private readonly Dictionary<string, string> _connectionStrings =
      new(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  /// Adiciona um template de conexão ao pool de conexões.
  /// </summary>
  /// <param name="templates">
  /// Conjunto de templates de conexão a serem adicionados ao pool de
  /// conexões.
  /// </param>
  public void AddConnectionTemplate(params ConnectionNode[] templates)
  {
    _templates.AddRange(templates);
  }

  /// <summary>
  /// Define uma string de conexão para um template de conexão ou uma conexão
  /// nova.
  /// </summary>
  public void AddConnectionString(string connectionName, string connectionString)
  {
    _connectionStrings.Add(connectionName, connectionString);
  }

  /// <summary>
  /// Constrói o pool de conexões a partir das strings de conexão definidas.
  /// </summary>
  /// <returns>
  /// O pool de conexões construído a partir das strings de conexão definidas.
  /// </returns>
  public async Task<ConnectionPool> BuildConnectionPoolAsync()
  {
    // 1. Enumera os nomes de conexões definidas nos templates e nas strings
    //    de conexão. A comparação é case-insensitive e os nomes definidos nos
    //    templates são priorizados.
    var connectionNames = _templates
        .Select(t => t.Name)
        .Concat(_connectionStrings.Keys)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    // 2. Copia os templates para uma lista e cria templates padrão para os
    //    nomes de conexões que não possuem um template definido.
    List<ConnectionNode> templates = _templates.ToList();
    templates.AddRange(connectionNames
        .Except(_templates.Select(t => t.Name), StringComparer.OrdinalIgnoreCase)
        .Select(name => new ConnectionNode
        {
          Name = name,
          IsDefault = true
        }));

    // 3. Remove das strings de conexão os parâmetros Name e Provider.
    //    Se Name é encontrado é então comparado ao nome do template.
    //        Se forem diferentes uma exceção é lançada.
    //    Se Provider é encontrado é então comparado ao provider do template.
    //        Se forem diferentes uma exceção é lançada.
    //        Se o provedor no template for nulo ou vazio, o provedor é definido.
    ValidateAndSetConnectionStrings(templates);

    // 4. Varre a lista de templates e intancia os pares ConnectionNode e
    //    string de conexão para as strings de conexão definidas ou para as
    //    strings de conexões criadas pelas Factories associadas aos templates.
    List<ConnectionPool.Entry> connections = new();
    foreach (var connectionName in templates.Select(t => t.Name))
    {
      await GetOrCreatePoolEntriesAsync(connectionName, connections, templates);
    }

    return new(connections);
  }

  /// <summary>
  /// Valida uma coleção de strings de conexão e define o valor do provedor no
  /// modelo correspondente, caso esteja nulo ou vazio. O método remove os
  /// parâmetros "Name" e "Provider" de cada string de conexão e os compara com
  /// os valores correspondentes nos modelos. Se os valores não corresponderem,
  /// uma exceção é lançada.
  /// Se a string de conexão ou a senha de usuário estiverem encriptadas, o
  /// método as decripta antes de prosseguir.
  /// </summary>
  /// <param name="templates">
  /// A lista de modelos <see cref="ConnectionNode"/> a ser usada para
  /// comparação.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// Lançada quando o nome de uma conexão não corresponde ao nome na string de
  /// conexão, ou quando o provedor de uma conexão não corresponde ao provedor
  /// na string de conexão.
  /// </exception>
  private void ValidateAndSetConnectionStrings(List<ConnectionNode> templates)
  {
    string name;
    string connectionString;

    var entries = _connectionStrings.ToList();
    foreach (var entry in entries)
    {
      (name, connectionString) = entry;

      var template = templates.First(t =>
          string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));


      // Decriptando a própria string de conexão se encriptada
      connectionString = Crypto.Decrypt(connectionString);

      var parameters = connectionString
          .Split(';')
          .Select(p => p.Split('='))
          .ToDictionary(p => p[0], p => p[1], StringComparer.OrdinalIgnoreCase);

      //
      // Validando o nome da conexão.
      //
      if (parameters.ContainsKey("Name"))
      {
        var connectionName = parameters["Name"];
        if (!string.Equals(connectionName, name,
            StringComparison.OrdinalIgnoreCase))
        {
          throw new InvalidOperationException(
              $"O nome da conexão '{name}' não corresponde ao nome " +
              $"definido na string de conexão '{connectionName}'.");
        }
        parameters.Remove("Name");
      }

      //
      // Validando o provedor da conexão.
      //
      if (parameters.ContainsKey("Provider"))
      {
        var provider = parameters["Provider"];

        if (!string.IsNullOrEmpty(template.Provider) &&
            !string.Equals(template.Provider, provider,
                StringComparison.OrdinalIgnoreCase))
        {
          throw new InvalidOperationException(
              $"O provedor da conexão '{name}' não corresponde ao provedor " +
              $"definido na string de conexão '{provider}'.");
        }

        template.Provider = Providers.GetAlias(provider)
            ?? throw new Exception($"Provedor '{provider}' não suportado.");

        parameters.Remove("Provider");
      }

      //
      // Validando o vínculo de base.
      //
      if (parameters.ContainsKey("BoundTo"))
      {
        var boundTo = parameters["BoundTo"];

        // obtém o template de conexão de nome igual ao valor do parâmetro
        // BoundTo ignorando caixa.
        var boundTemplate = templates.FirstOrDefault(t =>
            string.Equals(t.Name, boundTo, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"A conexão '{name}' está vinculada à conexão '{boundTo}', " +
                $"mas esta não foi encontrada.");

        template.BoundTo = boundTemplate.Name;

        parameters.Remove("BoundTo");
      }

      //
      // Decriptando a senha na string de conexão se encriptada
      //
      if (parameters.ContainsKey("Password"))
      {
        parameters["Password"] = Crypto.Decrypt(parameters["Password"]);
      }
      if (parameters.ContainsKey("pwd"))
      {
        parameters["pwd"] = Crypto.Decrypt(parameters["pwd"]);
      }

      AppendCommonProperties(template, parameters);

      _connectionStrings[name] = string.Join(';',
          parameters.Select(p => $"{p.Key}={p.Value}"));
    }
  }

  /// <summary>
  /// Adiciona propriedades comuns a uma string de conexão de banco de dados, se
  /// elas não estiverem presentes.
  /// As propriedades comuns são "Application Name" e, para conexões SQL Server,
  /// "Timeout" e "TrustServerCertificate".
  /// </summary>
  /// <param name="connection">
  /// O template de conexão a ser usado para obter o provedor.
  /// </param>
  /// <param name="connectionProperties">
  /// Propriedades da string de conexão.
  /// </param>
  /// <returns>A string de conexão aprimorada.</returns>
  private void AppendCommonProperties(ConnectionNode connection,
      Dictionary<string, string> connectionProperties)
  {
    if (!connectionProperties.ContainsKey("Application Name"))
    {
      connectionProperties.Add("Application Name", "ScriptPack");
    }

    if (Providers.AreEqual(connection.Provider, Providers.SQLServer))
    {
      if (!connectionProperties.ContainsKey("Timeout"))
      {
        connectionProperties.Add("Timeout", "10");
      }
      if (!connectionProperties.ContainsKey("TrustServerCertificate"))
      {
        connectionProperties.Add("TrustServerCertificate", "true");
      }
    }
  }

  /// <summary>
  /// Obtém ou cria uma entrada no pool de conexões para a conexão especificada
  /// pelo nome. Se a entrada já existir no pool, ela será retornada. Caso
  /// contrário, uma nova entrada será criada e adicionada ao pool.
  /// Se uma conexão referenciada for encontrada no template, a entrada para a
  /// conexão referenciada também será obtida ou criada no pool. A lista de
  /// entradas de pool resultante é retornada como uma tarefa assíncrona.
  /// </summary>
  /// <param name="connectionName">
  /// O nome da conexão a ser obtida ou criada no pool.
  /// </param>
  /// <param name="pool">A lista de entradas de pool existentes.</param>
  /// <param name="templates">
  /// A lista de templates <see cref="ConnectionNode"/> a ser usada para criação
  /// das conexões.
  /// </param>
  /// <returns>
  /// Uma tarefa assíncrona que representa a operação. O resultado da tarefa é
  /// um array de entradas de pool <see cref="ConnectionPool.Entry"/> para a
  /// conexão especificada pelo nome.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Lançada quando não é possível criar uma instância de conexão para a
  /// conexão especificada porque o template não possui uma factory associada ou
  /// quando a conexão referenciada não pode ser encontrada ou criada.
  /// </exception>
  private async Task<ConnectionPool.Entry[]> GetOrCreatePoolEntriesAsync(
      string connectionName, List<ConnectionPool.Entry> pool,
      List<ConnectionNode> templates)
  {
    //
    //  Obtendo do pool se já existir.
    //
    var currentEntries = pool
        .Where(e => string.Equals(e.Connection.Name, connectionName,
            StringComparison.OrdinalIgnoreCase))
        .ToArray();
    if (currentEntries.Length > 0)
    {
      return currentEntries;
    }

    var template = templates
        .First(t => string.Equals(t.Name, connectionName,
            StringComparison.OrdinalIgnoreCase));

    //
    //  Criando uma entrada no pool a partir da string de conexão definida.
    //
    if (_connectionStrings.ContainsKey(connectionName))
    {
      var connectionString = _connectionStrings[connectionName];
      var entry = new ConnectionPool.Entry(template, connectionString);
      pool.Add(entry);
      return new[] { entry };
    }

    //
    //  Procurando as conexões referenciadas.
    //
    if (string.IsNullOrEmpty(template.Factory?.Connection))
    {
      throw new InvalidOperationException(
          $"Não foi possível criar uma instância de conexão para a " +
          $"conexão '{connectionName}' porque o template não possui uma " +
          $"factory associada.");
    }

    var targetEntries = await GetOrCreatePoolEntriesAsync(
        template.Factory.Connection, pool, templates);

    if (targetEntries.Length == 0)
    {
      throw new InvalidOperationException(
          $"Não foi possível criar uma instância de conexão para a " +
          $"conexão '{connectionName}' porque o template não possui uma " +
          $"factory associada.");
    }

    //
    //  Compartilhando uma conexão já existente se uma query não foi indicada.
    //
    if (string.IsNullOrEmpty(template.Factory.Query))
    {
      // Quando não temos Query definida é porque estamos compartilhando
      // conexões.
      return targetEntries;
    }

    //
    //  Obtendo a string de conexão a partir da execução da query.
    //
    var entries = new List<ConnectionPool.Entry>();
    foreach (var (targetConnection, targetConnectionString) in targetEntries)
    {
      var query = template.Factory.Query;

      var connectionString = await RetrieveConnectionStringFromDatabaseAsync(
          targetConnection, targetConnectionString, query);

      var entry = new ConnectionPool.Entry(template, connectionString);
      pool.Add(entry);
      entries.Add(entry);
    }
    return entries.ToArray();
  }

  /// <summary>
  /// Realiza uma consulta na base de dados para obter a string de conexão
  /// utilizada para conectar a outra base de dados.
  /// </summary>
  /// <remarks>
  /// É esperado que a consulta retorne ou um registro com uma única coluna
  /// contendo a string de conexão completa ou um registro com várias colunas
  /// contendo os parâmetros da string de conexão.
  /// </remarks>
  /// <param name="connection">
  /// A conexão para a qual se deseja obter a string de conexão.
  /// </param>
  /// <param name="query">
  /// A consulta SQL a ser realizada para obter a string de conexão que resulte
  /// ou em um registro com uma única coluna contendo a string de conexão
  /// completa ou em um registro com várias colunas contendo os parâmetros da
  /// string de conexão.
  /// </param>
  /// <returns>
  /// A string de conexão obtida a partir da consulta SQL.
  /// </returns>
  private async Task<string> RetrieveConnectionStringFromDatabaseAsync(
      ConnectionNode connection, string connectionString, string query)
  {
    try
    {
      using var cn = await ConnectAsync(connection.Provider, connectionString);

      using var cm = cn.CreateCommand();
      cm.CommandText = query;

      using var reader = await cm.ExecuteReaderAsync();
      if (!await reader.ReadAsync())
        throw new Exception($"A string de conexão não foi encontrada.");

      if (reader.FieldCount == 1)
      {
        // Se o resultado da consulta for uma única coluna, então
        // a string de conexão é o valor da primeira coluna.
        var retrievedConnectionString = reader.GetString(0)
            ?? throw new Exception(
                $"A consulta na base de dados foi executada mas a string de " +
                $"conexão com a base destino não foi retornada: " +
                $"{connection}");

        return Crypto.Decrypt(retrievedConnectionString);
      }

      // Se o resultado da consulta for mais de uma coluna, então
      // a string de conexão é a junção das colunas com o separador ";"
      // e o valor de cada coluna é o valor da coluna com o separador "=".

      var parameters = new string[reader.FieldCount];
      for (var i = 0; i < reader.FieldCount; i++)
      {
        var key = reader.GetName(i);
        var value = Crypto.Decrypt(reader.GetString(i));
        parameters[i] = string.Join("=", key, value);
      }

      return string.Join(";", parameters);
    }
    catch (Exception ex)
    {
      throw new Exception($"Falha ao obter a conexão: {connection.Name}", ex);
    }
  }

  /// <summary>
  /// Cria uma conexão assíncrona com a base de dados a partir de informações de conexão.
  /// </summary>
  /// <param name="provider">O nome do provedor de conexão.</param>
  /// <param name="connectionString">A string de conexão.</param>
  /// <returns>A conexão criada.</returns>
  private async Task<DbConnection> ConnectAsync(string provider,
      string connectionString)
  {
    var dbProviderFactory = Providers.GetFactory(provider)
        ?? throw new ArgumentException(
            $"Fábrica de conexão não encontrada: {provider}");

    var dbConnection = dbProviderFactory.CreateConnection()!;
    dbConnection.ConnectionString = connectionString;
    await dbConnection.OpenAsync();

    return dbConnection;
  }
}
