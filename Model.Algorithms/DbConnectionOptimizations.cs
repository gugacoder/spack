using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace SPack.Model.Algorithms;

/// <summary>
/// Otimizações para conexões com o banco de dados.
/// </summary>
public static class DbConnectionOptimizations
{
  /// <summary>
  /// Otimiza a conexão com o banco de dados.
  /// </summary>
  /// <param name="dbConnection">
  /// Conexão com o banco de dados.
  /// </param>
  /// <param name="stopToken">
  /// Token de cancelamento.
  /// </param>
  public static async Task OptimizeConnectionAsync(
    this DbConnection dbConnection, CancellationToken stopToken = default)
  {
    // Há apenas otimizações para SqlServer até o momento.
    if (dbConnection is SqlConnection sqlConnection)
    {
      //
      // Nota sobre 'ARITHABORT ON':
      //
      // A configuração padrão ARITHABORT de SQL Server Management Studio é ON.
      // Os aplicativos cliente que definem ARITHABORT como OFF podem receber planos de
      // consulta diferentes, dificultando a solução de problemas de consultas executadas
      // insatisfatoriamente. Ou seja, a mesma consulta pode ser executada rapidamente no
      // Management Studio, mas lentamente no aplicativo. Ao solucionar problemas de
      // consultas com Management Studio, sempre faça a correspondência com a configuração
      // ARITHABORT do cliente.
      //
      // Referência:
      // -   https://msdn.microsoft.com/pt-br/library/ms190306.aspx
      //

      using var command = sqlConnection.CreateCommand();
      command.CommandType = System.Data.CommandType.Text;
      command.CommandText =
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
      await command.ExecuteNonQueryAsync(stopToken);
    }
  }
}
