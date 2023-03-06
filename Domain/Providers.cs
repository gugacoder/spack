namespace SPack.Domain;

/// <summary>
/// Drivers de base de dados suportados pelo sistema.
/// </summary>
public static class Providers
{
  /// <summary>
  /// Linguagem SQL do SQLServer.
  /// </summary>
  public const string SqlServer = nameof(SqlServer);

  /// <summary>
  /// Linguagem SQL do PostgreSQL.
  /// </summary>
  public const string PostgreSQL = nameof(PostgreSQL);
}
