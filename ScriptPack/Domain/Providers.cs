using System.Data.Common;
using Npgsql;
using Microsoft.Data.SqlClient;

namespace ScriptPack.Domain;

/// <summary>
/// Drivers de base de dados suportados pelo sistema.
/// </summary>
public static class Providers
{
  /// <summary>
  /// Nome usado internamente pelo ScriptPack (Alias) para a identificação do
  /// provedor de dados do SQLServer.
  /// </summary>
  public const string SQLServer = nameof(SQLServer);

  /// <summary>
  /// Nome usado internamente pelo ScriptPack (Alias) para a identificação do
  /// provedor de dados do PostgreSQL.
  /// </summary>
  public const string PostgreSQL = nameof(PostgreSQL);

  /// <summary>
  /// Informações de mapeamento de um provedor de dados.
  /// </summary>
  /// <param name="Alias">
  /// Nome usado internamente pelo ScriptPack para a identificação do provedor
  /// de dados.
  /// </param>
  /// <param name="Name">
  /// Nome do provedor de dados usado pelo DbProviderFactory.
  /// </param>
  /// <param name="Factory">
  /// Tipo da fábrica de conexão de base de dados.
  /// </param>
  public record Info(string Alias, string Name, DbProviderFactory Factory);

  /// <summary>
  /// Relação dos provedores de dados suportados pelo ScriptPack.
  /// </summary>
  /// <remarks>
  /// Alias é o nome usado para referenciar o provedor de dados em configurações
  /// do ScriptPack.
  /// Driver é o nome do provedor de dados usado pelo DbProviderFactory.
  /// Factory é o tipo da fábrica de conexão de base de dados.
  /// </remarks>
  public static readonly Info[] All = {
      new (SQLServer, "System.Data.SqlClient", SqlClientFactory.Instance),
      new (PostgreSQL, "Npgsql", NpgsqlFactory.Instance)
  };

  /// <summary>
  /// Obtém o nome do provedor usado internamente pelo ScriptPack.
  /// </summary>
  /// <param name="providerNameOrAlias">
  /// Nome do provedor segundo o padrão de nome do DbProviderFactory ou nome
  /// interno do ScriptPack para o provedor.
  /// </param>
  /// <returns>
  /// Nome do provedor de dados.
  /// </returns>
  public static string? GetAlias(string providerNameOrAlias)
  {
    var searchName = providerNameOrAlias.ToLower();
    var instance = (
        from provider in All
        where provider.Alias.ToLower() == searchName
          || provider.Name.ToLower() == searchName
        select provider
    ).FirstOrDefault();
    return instance?.Alias;
  }

  /// <summary>
  /// Obtém o nome do provedor no padrão do DbProviderFactory.
  /// </summary>
  /// <param name="providerNameOrAlias">
  /// Nome do provedor segundo o padrão de nome do DbProviderFactory ou nome
  /// interno do ScriptPack para o provedor.
  /// </param>
  /// <returns>
  /// Nome do provedor de dados.
  /// </returns>
  public static string? GetName(string providerNameOrAlias)
  {
    var searchName = providerNameOrAlias.ToLower();
    var instance = (
        from provider in All
        where provider.Alias.ToLower() == searchName
          || provider.Name.ToLower() == searchName
        select provider
    ).FirstOrDefault();
    return instance?.Name;
  }

  /// <summary>
  /// Obtém a instância da fábrica de conexões do provedor de dados.
  /// </summary>
  /// <param name="providerNameOrAlias">
  /// Nome do provedor segundo o padrão de nome do DbProviderFactory ou nome
  /// interno do ScriptPack para o provedor.
  /// </param>
  /// <returns>
  /// Nome do provedor de dados.
  /// </returns>
  public static DbProviderFactory? GetFactory(string providerNameOrAlias)
  {
    var searchName = providerNameOrAlias.ToLower();
    var instance = (
        from provider in All
        where provider.Alias.ToLower() == searchName
          || provider.Name.ToLower() == searchName
        select provider
    ).FirstOrDefault();
    return instance?.Factory;
  }

  /// <summary>
  /// Verifica se os nomes de provedores referem-se ao mesmo provedor.
  /// </summary>
  /// <param name="providerNameOrAlias1">
  /// Nome do provedor segundo o padrão de nome do DbProviderFactory ou nome
  /// interno do ScriptPack para o provedor.
  /// </param>
  /// <param name="providerNameOrAlias2">
  /// Nome do provedor segundo o padrão de nome do DbProviderFactory ou nome
  /// interno do ScriptPack para o provedor.
  /// </param>
  public static bool AreEqual(string providerNameOrAlias1,
      string providerNameOrAlias2)
  {
    var alias1 = GetAlias(providerNameOrAlias1);
    var alias2 = GetAlias(providerNameOrAlias2);
    return alias1 == alias2;
  }
}
