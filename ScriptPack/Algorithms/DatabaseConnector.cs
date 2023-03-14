using System.Data.Common;
using ScriptPack.Domain;

namespace ScriptPack.Algorithms;

/// <summary>
/// Classe responsável por criar strings de conexão e conexões com bases de dados.
/// </summary>
public class DatabaseConnector
{
  private readonly ConnectionNode[] _availableConnections;

  /// <summary>
  /// Construtor da classe DatabaseConnector.
  /// </summary>
  /// <param name="availableConnections">
  /// Lista de ConnectionNode contendo informações de conexão com as bases de dados.
  /// </param>
  public DatabaseConnector(ConnectionNode[] availableConnections)
  {
    this._availableConnections = availableConnections;
  }

  /// <summary>
  /// Cria uma conexão assíncrona com a base de dados a partir de informações de conexão.
  /// </summary>
  /// <param name="connection">Informações de conexão com a base de dados.</param>
  /// <returns>A conexão criada.</returns>
  public async Task<DbConnection> CreateConnectionAsync(ConnectionNode connection)
  {
    var connectionString = await CreateConnectionStringAsync(connection);

    var dbProviderFactory = Providers.GetProviderFactory(connection.Provider);
    var dbConnection = dbProviderFactory.CreateConnection()!;
    dbConnection.ConnectionString = connectionString;

    return dbConnection;
  }

  /// <summary>
  /// Cria uma string de conexão assíncrona a partir de informações de conexão.
  /// </summary>
  /// <param name="connection">
  /// Informações de conexão com a base de dados.
  /// </param>
  /// <returns>A string de conexão criada.</returns>
  public async Task<string> CreateConnectionStringAsync(
      ConnectionNode connection)
  {
    //
    //  Usando a string de conexão da própria fábrica de conexão.
    //
    var connectionStringFactory = connection.ConnectionStringFactory
        ?? throw new Exception(
              $"Fábrica de configuração de conexão não encontrada: " +
              $"{connection.Name}");

    var connectionString = connectionStringFactory.ConnectionString;
    if (connectionString == null)
    {
      //
      //  Usando a string de conexão de outra fábrica de conexão.
      //
      var targetConnectionName = connectionStringFactory.Connection
          ?? throw new Exception(
                $"A configuração da fábrica de conexão não é válida: " +
                $"{connection.Name}");

      var targetConnection = _availableConnections
          .FirstOrDefault(c => c.Name == targetConnectionName)
          ?? throw new Exception(
                $"Fábrica de configuração de conexão não encontrada: " +
                $"{targetConnectionName}");

      if (string.IsNullOrEmpty(connectionStringFactory.Query))
        return await CreateConnectionStringAsync(targetConnection);

      //
      //  Como existe uma consulta SQL expecificada, vamos usá-la para obter
      //  a string de conexão.
      //
      connectionString = await RetrieveConnectionStringFromDatabaseAsync(
          targetConnection, connectionStringFactory.Query);
    }

    if (connectionString == null)
      throw new Exception(
          $"A string de conexão não foi encontrada: " +
          $"{connection.Name}");

    connectionString = AppendCommonProperties(connection, connectionString);

    return connectionString;
  }

  /// <summary>
  /// Adiciona propriedades comuns a uma string de conexão de banco de dados, se
  /// elas não estiverem presentes.
  /// As propriedades comuns são "Application Name" e, para conexões SQL Server,
  /// "Timeout" e "TrustServerCertificate".
  /// </summary>
  /// <param name="connectionString">
  /// A string de conexão a ser aprimorada.
  /// </param>
  /// <returns>A string de conexão aprimorada.</returns>
  private string AppendCommonProperties(ConnectionNode connection,
      string connectionString)
  {
    var parameters = connectionString.Split(';').ToList();

    if (!parameters.Any(p => p.StartsWith("Application Name=")))
    {
      parameters.Add("Application Name=ScriptPack");
    }

    if (Providers.GetProviderName(connection.Provider) == Providers.SqlServer)
    {
      if (!parameters.Any(p => p.StartsWith("Timeout=")))
      {
        parameters.Add("Timeout=10");
      }
      if (!parameters.Any(p => p.StartsWith("TrustServerCertificate=")))
      {
        parameters.Add("TrustServerCertificate=true");
      }
    }

    return string.Join(";", parameters);
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
      ConnectionNode connection, string query)
  {
    try
    {
      using var dbConnection = await CreateConnectionAsync(connection);

      await dbConnection.OpenAsync();

      using var dbCommand = dbConnection.CreateCommand();
      dbCommand.CommandText = query;

      using var reader = await dbCommand.ExecuteReaderAsync();
      if (!await reader.ReadAsync())
        throw new Exception($"A string de conexão não foi encontrada.");

      if (reader.FieldCount == 1)
      {
        // Se o resultado da consulta for uma única coluna, então
        // a string de conexão é o valor da primeira coluna.
        return reader.GetString(0)
            ?? throw new Exception(
                $"A consulta na base de dados foi executada mas a string de " +
                $"conexão com a base destino não foi retornada: " +
                $"{connection}");
      }

      // Se o resultado da consulta for mais de uma coluna, então
      // a string de conexão é a junção das colunas com o separador ";"
      // e o valor de cada coluna é o valor da coluna com o separador "=".

      var parameters = new string[reader.FieldCount];
      for (var i = 0; i < reader.FieldCount; i++)
      {
        var key = reader.GetName(i);
        var value = reader.GetString(i);
        parameters[i] = string.Join("=", key, value);
      }

      return string.Join(";", parameters);
    }
    catch (Exception ex)
    {
      throw new Exception($"Falha ao obter a conexão: {connection.Name}", ex);
    }
  }
}
