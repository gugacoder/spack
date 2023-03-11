// using System.Data.Common;
// using ScriptPack.Domain;

// namespace ScriptPack.Library;

// /// <summary>
// /// Classe responsável por criar a conexão com o banco de dados.
// /// </summary>
// public class DbConnector
// {
//   private readonly ConnectionNode[] connections;

//   /// <summary>
//   /// Construtor.
//   /// </summary>
//   /// <param name="connections">
//   /// Conexões disponíveis.
//   /// </param>
//   public DbConnector(ConnectionNode[] connections)
//   {
//     this.connections = connections;
//   }

//   /// <summary>
//   /// Cria uma conexão com o banco de dados.
//   /// </summary>
//   /// <param name="connectionName">
//   /// Nome da conexão.
//   /// </param>
//   /// <returns>
//   /// Conexão com o banco de dados.
//   /// </returns>
//   public async Task<DbConnection> CreateConnectionAsync(ConnectionNode connection)
//   {
//     var connectionStringFactory = connection.ConnectionStringFactory
//       ?? throw new Exception($"Fábrica de configuração de conexão não encontrada: {connection.Name}");

//     try
//     {
//       var connectionString = await GetConnectionStringAsync(connectionStringFactory);

//       var dbProviderFactory = Providers.GetProviderFactory(connection.Provider);
//       var dbConnection = dbProviderFactory.CreateConnection()!;
//       dbConnection.ConnectionString = connectionString;

//       return dbConnection;
//     }
//     catch (Exception ex)
//     {
//       throw new Exception($"Falha ao obter a conexão: {connection.Name}", ex);
//     }
//   }

//   /// <summary>
//   /// Obtém a string de conexão.
//   /// </summary>
//   /// <param name="connectionStringFactory">
//   /// Fábrica de configuração de conexão.
//   /// </param>
//   /// <returns>
//   /// String de conexão.
//   /// </returns>
//   private async Task<string> GetConnectionStringAsync(ConnectionStringFactoryNode connectionStringFactory)
//   {
//     if (!string.IsNullOrEmpty(connectionStringFactory.ConnectionString))
//       return connectionStringFactory.ConnectionString;

//     var queryConnectionName = connectionStringFactory.Connection;
//     if (string.IsNullOrEmpty(queryConnectionName))
//       throw new Exception($"A configuração da fábrica de conexão não é válida.");

//     var queryConnection = this.connections.FirstOrDefault(c => c.Name == queryConnectionName)
//       ?? throw new Exception($"Conexão não encontrada: {queryConnectionName}");

//     var query = connectionStringFactory.Query;
//     if (string.IsNullOrEmpty(query))
//       throw new Exception($"A configuração da fábrica de conexão não é válida.");

//     using var dbConnection = await CreateConnectionAsync(queryConnection);

//     await dbConnection.OpenAsync();

//     using var dbCommand = dbConnection.CreateCommand();
//     dbCommand.CommandText = query;

//     var connectionString = await dbCommand.ExecuteScalarAsync() as string;
//     if (string.IsNullOrEmpty(connectionString))
//       throw new Exception($"A string de conexão não foi encontrada.");

//     return connectionString;
//   }
// }
