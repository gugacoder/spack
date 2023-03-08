using System.Security.Cryptography;
using System.Data.Common;
using Npgsql;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace SPack.Domain;

/// <summary>
/// Drivers de base de dados suportados pelo sistema.
/// </summary>
public static class Providers
{
  /// <summary>
  /// Provedor de dados do SQL Server.
  /// </summary>
  public const string SqlServer = "System.Data.SqlClient";

  /// <summary>
  /// Provedor de dados do PostgreSQL.
  /// </summary>
  public const string PostgreSQL = "Npgsql";

  /// <summary>
  /// Provedor de dados do SQLite.
  /// </summary>
  public const string SQLite = "System.Data.SQLite";

  /// <summary>
  /// Obtém o provedor de dados a partir de um nome ou apelido.
  /// O apelido é definido pelas constantes desta classe.
  /// </summary>
  /// <param name="providerNameOrAlias">
  /// Nome ou apelido do provedor de dados.
  /// </param>
  /// <returns>
  /// Nome do provedor de dados.
  /// </returns>
  public static string GetProviderName(string providerNameOrAlias)
  {
    return providerNameOrAlias.ToLower() switch
    {
      "sqlserver" => SqlServer,
      "postgresql" => PostgreSQL,
      "sqlite" => SQLite,
      _ => providerNameOrAlias
    };
  }

  /// <summary>
  /// Obtém o provedor de dados a partir do nome do provedor de dados.
  /// </summary>
  /// <param name="providerNameOrAlias">
  /// Nome ou apelido do provedor de dados.
  /// </param>
  /// <returns>
  /// Provedor de dados.
  /// </returns>
  public static DbProviderFactory GetProviderFactory(string providerNameOrAlias)
  {
    var providerName = GetProviderName(providerNameOrAlias);
    return providerName switch
    {
      "System.Data.SqlClient" => SqlClientFactory.Instance,
      "Npgsql" => NpgsqlFactory.Instance,
      "System.Data.SQLite" => SqliteFactory.Instance,
      _ => DbProviderFactories.GetFactory(providerName)
    };
  }
}
