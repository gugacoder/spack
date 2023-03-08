using System.Data.Common;
using SPack.Domain;
using SPack.Library;
using SPack.Model.Algorithms;

namespace SPack.Model;

public class Migrant
{
  private readonly IDrive drive;
  private readonly Connection[] connections;
  private readonly Pipeline pipeline;

  public Migrant(IDrive drive, Connection[] connections, Pipeline pipeline)
  {
    this.drive = drive;
    this.connections = connections;
    this.pipeline = pipeline;
  }

  public async Task MigrateAsync()
  {
    foreach (var stage in this.pipeline.Stages)
    {
      await this.MigrateStageAsync(stage);
    }
  }

  private async Task MigrateStageAsync(Stage stage)
  {
    var pretran = stage.Steps.Where(s => s.Tag == Steps.PreTransaction).ToArray();
    var pre = stage.Steps.Where(s => s.Tag == Steps.Pre).ToArray();
    var main = stage.Steps.Where(s => s.Tag == Steps.Main).ToArray();
    var pos = stage.Steps.Where(s => s.Tag == Steps.Pos).ToArray();
    var postran = stage.Steps.Where(s => s.Tag == Steps.PosTransaction).ToArray();

    DbConnection? cn = null;
    try
    {
      var connector = new DbConnector(this.connections);
      var connection = this.pipeline.Connection;
      cn = await connector.CreateConnectionAsync(connection);

      await cn.OpenAsync();
      await DbConnectionOptimizations.OptimizeConnectionAsync(cn);

      foreach (var step in pretran) await ExecuteStepAsync(step, cn);

      using (var tx = await cn.BeginTransactionAsync())
      {
        foreach (var step in pre) await ExecuteStepAsync(step, cn, tx);
        foreach (var step in main) await ExecuteStepAsync(step, cn, tx);
        foreach (var step in pos) await ExecuteStepAsync(step, cn, tx);

        await tx.CommitAsync();
      }

      foreach (var step in postran) await ExecuteStepAsync(step, cn);

    }
    finally
    {
      if (cn != null) await cn.DisposeAsync();
    }
  }

  private async Task ExecuteStepAsync(Step step,
    DbConnection dbConnection, DbTransaction? dbTransaction = null)
  {
    var batcher = new SqlBatcher();
    foreach (var script in step.Scripts)
    {
      var content = await drive.ReadAllTextAsync(script.FilePath!);
      var batches = batcher.BreakInBatches(content);

      foreach (var batch in batches)
      {
        await ExecuteBatchAsync(batch, dbConnection, dbTransaction);
      }
    }
  }

  private async Task ExecuteBatchAsync(SqlBatch batch,
    DbConnection dbConnection, DbTransaction? dbTransaction = null)
  {
    using var command = dbConnection.CreateCommand();
    command.CommandText = batch.Buffer.ToString();
    command.Transaction = dbTransaction;

    // Aplicando padrões de template ao script e repassando variáveis do ScriptPack.
    // TODO: Aplicar padrões de template ao script.

    await command.ExecuteNonQueryAsync();
  }
}