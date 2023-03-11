// using System.Data.Common;
// using ScriptPack.Domain;
// using ScriptPack.Helpers;
// using ScriptPack.Model.Algorithms;

// namespace ScriptPack.Model;

// public class Migrant
// {
//   private readonly IDrive drive;
//   private readonly ConnectionNode[] connections;
//   private readonly PipelineNode pipeline;

//   public event EventHandler<MigrantEventArgs>? OnMigrate;
//   public event EventHandler<MigrantEventArgs>? OnMigrateSuccess;
//   public event EventHandler<MigrantErrorEventArgs>? OnMigrateError;

//   public Migrant(IDrive drive, ConnectionNode[] connections, PipelineNode pipeline)
//   {
//     this.drive = drive;
//     this.connections = connections;
//     this.pipeline = pipeline;
//   }

//   public async Task MigrateAsync()
//   {
//     try
//     {
//       foreach (var stage in this.pipeline.Stages)
//       {
//         await this.MigrateStageAsync(stage);
//       }
//     }
//     catch (Exception ex)
//     {
//       this.pipeline.Faults.Add(FaultNode.EmitException(ex));
//     }
//   }

//   private async Task MigrateStageAsync(StageNode stage)
//   {
//     DbConnection? cn = null;
//     try
//     {
//       var pretran = stage.Steps.Where(s => s.Tag == Steps.PreTransaction).ToArray();
//       var pre = stage.Steps.Where(s => s.Tag == Steps.Pre).ToArray();
//       var main = stage.Steps.Where(s => s.Tag == Steps.Main).ToArray();
//       var pos = stage.Steps.Where(s => s.Tag == Steps.Pos).ToArray();
//       var postran = stage.Steps.Where(s => s.Tag == Steps.PosTransaction).ToArray();

//       var connector = new DbConnector(this.connections);
//       var connection = this.pipeline.Connection;
//       cn = await connector.CreateConnectionAsync(connection);

//       await cn.OpenAsync();
//       await DbConnectionOptimizations.OptimizeConnectionAsync(cn);

//       foreach (var step in pretran) await ExecuteStepAsync(step, cn);

//       using (var tx = await cn.BeginTransactionAsync())
//       {
//         foreach (var step in pre) await ExecuteStepAsync(step, cn, tx);
//         foreach (var step in main) await ExecuteStepAsync(step, cn, tx);
//         foreach (var step in pos) await ExecuteStepAsync(step, cn, tx);

//         await tx.CommitAsync();
//       }

//       foreach (var step in postran) await ExecuteStepAsync(step, cn);

//     }
//     catch (Exception ex)
//     {
//       stage.Faults.Add(FaultNode.EmitException(ex));
//       OnMigrateError?.Invoke(this, new(stage, ex));
//       throw;
//     }
//     finally
//     {
//       if (cn != null) await cn.DisposeAsync();
//     }
//   }

//   private async Task ExecuteStepAsync(StepNode step,
//     DbConnection dbConnection, DbTransaction? dbTransaction = null)
//   {
//     try
//     {
//       var batcher = new SqlBatcher();
//       foreach (var script in step.Scripts)
//       {
//         Batch[] batches;
//         Batch? batch = null;

//         try
//         {
//           OnMigrate?.Invoke(this, new(step, script));

//           var content = await drive.ReadAllTextAsync(script.FilePath!);
//           batches = batcher.BreakInBatches(content);

//           for (int i = 0; i < batches.Length; i++)
//           {
//             batch = batches[i];
//             await ExecuteBatchAsync(batch, dbConnection, dbTransaction);
//           }

//           OnMigrateSuccess?.Invoke(this, new(step, script));
//         }
//         catch (Exception ex)
//         {
//           script.Faults.Add(FaultNode.EmitException(ex));
//           OnMigrateError?.Invoke(this, new(step, script, batch, ex));
//           throw;
//         }
//       }
//     }
//     catch (Exception ex)
//     {
//       step.Faults.Add(FaultNode.EmitException(ex));
//       OnMigrateError?.Invoke(this, new(step, ex));
//       throw;
//     }
//   }

//   private async Task ExecuteBatchAsync(Domain.Batch batch,
//     DbConnection dbConnection, DbTransaction? dbTransaction = null)
//   {
//     try
//     {
//       using var command = dbConnection.CreateCommand();
//       command.CommandText = batch.Buffer.ToString();
//       command.Transaction = dbTransaction;

//       // Aplicando padrões de template ao script e repassando variáveis do ScriptPack.
//       // TODO: Aplicar padrões de template ao script.

//       await command.ExecuteNonQueryAsync();
//     }
//     catch (Exception ex)
//     {
//       if (batch.Index > 0)
//         throw new Exception($"Falha executando o {batch.Index + 1}º bloco do script.", ex);

//       throw;
//     }
//   }
// }