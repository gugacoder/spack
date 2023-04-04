using System.Data.Common;
using Humanizer;
using Microsoft.Data.SqlClient;
using Npgsql;
using ScriptPack.Domain;
using ScriptPack.Helpers;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Utilitário para inicializar, otimizar e configurar a base de dados com os
/// parâmetros de contexto do ScriptPack.
/// </summary>
public class ConnectionSetup
{
  /// <summary>
  /// Contexto de execução de scripts.
  /// </summary>
  public Context Context { get; init; } = null!;

  /// <summary>
  /// Configuração da conexão destino.
  /// </summary>
  public ConnectionNode TargetConnection { get; init; } = null!;

  /// <summary>
  /// Conjunto das configurações de conexão disponíveis.
  /// </summary>
  public ConnectionPool ConnectionPool { get; init; } = null!;

  /// <summary>
  /// Verifica se a base de dados suporta o ScriptPack.
  /// </summary>
  /// <param name="cn">
  /// Conexão com o banco de dados para a qual os argumentos serão repassados.
  /// </param>
  /// <param name="tx">
  /// Transação ativa para a qual os argumentos serão repassados.
  /// </param>
  public Task<bool> CheckScriptPackSupportAsync(DbConnection cn,
      DbTransaction? tx)
  {
    var cm = cn.CreateCommand();
    cm.CommandText = @"
        SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_SCHEMA = 'scriptpack'
          AND TABLE_NAME = 'bindings' ";
    cm.Transaction = tx;
    return cm.ExecuteScalarAsync<bool>();
  }

  /// <summary>
  /// Configura a conexão com os argumentos e objetos do ScriptPack para
  /// automação de scripts.
  /// </summary>
  /// <param name="cn">
  /// Conexão com a base de dados para a qual os argumentos serão repassados.
  /// </param>
  /// <param name="tx">
  /// Transação ativa para a qual os argumentos serão repassados.
  /// </param>
  public async Task OptimizeConnectionAsync(DbConnection cn, DbTransaction? tx)
  {
    // Há apenas otimizações para SqlServer até o momento.
    if (cn is not SqlConnection)
      return;

    //
    // Nota sobre 'ARITHABORT ON':
    //
    // A configuração padrão ARITHABORT do SQL Server Management Studio é ON.
    // Quando os aplicativos clientes definem ARITHABORT como OFF, podem
    // receber planos de consulta diferentes, o que pode dificultar a solução
    // de problemas de consultas que são executadas insatisfatoriamente.
    // Em outras palavras, uma mesma consulta pode ser executada rapidamente
    // no Management Studio, mas de forma lenta no aplicativo.
    // Ao solucionar problemas de consultas usando o Management Studio, é
    // importante levar em consideração a configuração ARITHABORT do cliente.
    //
    // Fonte:
    // - https://msdn.microsoft.com/pt-br/library/ms190306.aspx
    //      

    using var cm = cn.CreateCommand();
    cm.Transaction = tx;
    cm.CommandText =
      @"SET ANSI_NULL_DFLT_ON ON;
        SET ANSI_NULLS ON;
        SET ANSI_PADDING ON;
        SET ANSI_WARNINGS ON;
        SET ARITHABORT ON;
        SET CONCAT_NULL_YIELDS_NULL ON;
        SET CURSOR_CLOSE_ON_COMMIT OFF;
        SET DEADLOCK_PRIORITY NORMAL;
        SET IMPLICIT_TRANSACTIONS OFF;
        SET LOCK_TIMEOUT -1;
        SET NOCOUNT ON;
        SET QUERY_GOVERNOR_COST_LIMIT 0;
        SET QUOTED_IDENTIFIER ON;
        SET ROWCOUNT 0;
        SET TEXTSIZE 2147483647;
        SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
        ";
    await cm.ExecuteNonQueryAsync();
  }

  /// <summary>
  /// Registra na base de dados os vínculos de bases de dados.
  /// </summary>
  /// <param name="cn">
  /// Conexão com o banco de dados para a qual os registros serão repassados.
  /// </param>
  /// <param name="tx">
  /// Transação ativa para a qual os argumentos serão repassados.
  /// </param>
  public async Task BindDatabasesAsync(DbConnection cn, DbTransaction? tx)
  {
    // As tabelas e funções usadas neste método são definidas no pacote de
    // scripts embarcados e definidos na pasta "Assets/Scripts".

    var hasScriptPackSupport = await CheckScriptPackSupportAsync(cn, tx);
    if (!hasScriptPackSupport)
    {
      return;
    }

    //
    // Motando uma lista de todas as bases vinculadas mais a base corrente.
    //
    var databases = (
        from entry in ConnectionPool.Values
        let connection = entry.Connection
        where connection.BoundTo == TargetConnection.Name
          || TargetConnection.BoundTo == connection.Name
        select (
            kind: connection.Name.Pascalize(),
            name: ConnectionPool.GetDatabaseName(connection.Name)
        )
    ).Distinct().ToList();

    databases.Add(("CurrentDb",
        ConnectionPool.GetDatabaseName(TargetConnection.Name)));

    //
    // Validando se todas as bases de dados foram identificadas
    //
    if (databases.Any(x => x.name is null))
    {
      var unindentified = databases
          .Where(x => x.name is null)
          .Select(x => x.kind).ToList();
      var message = string.Join(", ", unindentified);
      throw new InvalidOperationException(
          $"Não foi possível identificar o nome da base de dados: {message}");
    }

    //
    // Registrando os vínculos de bases de dados.
    //
    using var cm = cn.CreateCommand();
    cm.Transaction = tx;

    // Removendo indicador 'TargetDb' legado não mais utilizado nesta versão do
    // ScrptPack. A base corrente deve ser identificada por 'CurrentDb'.
    cm.CommandText = @"
        DELETE FROM scriptpack.bindings WHERE kind = 'TargetDb';";
    await cm.ExecuteNonQueryAsync();

    // Cadastrando os vínculos de bases de dados.
    cm.CommandText = @"
        DELETE FROM scriptpack.bindings WHERE kind = @kind;
        INSERT INTO scriptpack.bindings (kind, name) VALUES (@kind, @name);";
    foreach (var (kind, name) in databases)
    {
      cm.Parameters.Clear();
      cm.AddParameterWithValue("@kind", kind);
      cm.AddParameterWithValue("@name", name);
      await cm.ExecuteNonQueryAsync();
    }
  }

  /// <summary>
  /// Registra na base de dados os argumentos de contexto para serem usados
  /// internamente pelos scripts.
  /// </summary>
  /// <param name="cn">
  /// Conexão com o banco de dados para a qual os argumentos serão repassados.
  /// </param>
  /// <param name="tx">
  /// Transação ativa para a qual os argumentos serão repassados.
  /// </param>
  public async Task SetArgumentsAsync(DbConnection cn, DbTransaction? tx)
  {
    // As tabelas e funções usadas neste método são definidas no pacote de
    // scripts embarcados e definidos na pasta "Assets/Scripts".

    var hasScriptPackSupport = await CheckScriptPackSupportAsync(cn, tx);
    if (!hasScriptPackSupport)
    {
      return;
    }

    using var cm = cn.CreateCommand();
    cm.Transaction = tx;
    cm.CommandText = cn is NpgsqlConnection
        ? "SELECT scriptpack.SET_ARG(@name, @value)"
        : "EXEC scriptpack.SET_ARG @name, @value";

    foreach (var (key, value) in Context.Arguments)
    {
      cm.Parameters.Clear();
      cm.AddParameterWithValue("@name", key);
      cm.AddParameterWithValue("@value", value);
      await cm.ExecuteNonQueryAsync();
    }
  }
}
