using System.Data.Common;
using ScriptPack.Algorithms;
using ScriptPack.Domain;

namespace ScriptPack.Model;

/// <summary>
/// Responsável por executar as migrações do banco de dados seguindo as etapas
/// do pipeline de migração.
/// </summary>
public class DatabaseMigrator
{
  /// <summary>
  /// Evento acionado a cada etapa de migração bem sucedida.
  /// </summary>
  public event EventHandler<StepEventArgs>? OnMigrate;

  /// <summary>
  /// Evento acionado quando toda a migração é bem sucedida.
  /// </summary>
  public event EventHandler<StepEventArgs>? OnSuccess;

  /// <summary>
  /// Evento acionado quando há um erro em alguma etapa da migração.
  /// </summary>
  public event EventHandler<ErrorEventArgs>? OnError;

  /// <summary>
  /// Executa as migrações do banco de dados seguindo as etapas do pipeline de
  /// migração.
  /// </summary>
  /// <param name="pipeline">Pipeline de migração a ser executado.</param>
  public async Task MigrateAsync(PipelineNode pipeline)
  {
    try
    {
      var connections = (
          from step in pipeline.Descendants<StepNode>()
          from script in step.Scripts
          let catalog = script.Ancestor<CatalogNode>()
          from connection in catalog.Connections
          select connection
      ).ToArray();

      foreach (var stage in pipeline.Stages)
      {
        await this.MigrateStageAsync(stage, connections);
      }
    }
    catch (Exception ex)
    {
      pipeline.Faults.Add(Fault.EmitException(ex));
    }
  }

  /// <summary>
  /// Executa as migrações de uma etapa do pipeline de migração.
  /// </summary>
  /// <param name="stage">Etapa a ser executada.</param>
  /// <param name="connections">Conexões disponíveis.</param>
  private async Task MigrateStageAsync(StageNode stage,
      ConnectionNode[] connections)
  {
    DbConnection? cn = null;
    try
    {
      var pipeline = stage.Ancestor<PipelineNode>()!;

      //
      // Estabelecendo conexão...
      //
      var connector = new DatabaseConnector(connections);
      cn = await connector.CreateConnectionAsync(pipeline.Connection);

      await cn.OpenAsync();

      var optimizer = new ConnectionOptimizer();
      await optimizer.OptimizeConnectionAsync(cn);

      //
      //  Determinando passos...
      //
      var pretran = stage.Steps.Where(s => s.Tag == Steps.PreTransaction)
          .ToArray();
      var pre = stage.Steps.Where(s => s.Tag == Steps.Pre).ToArray();
      var main = stage.Steps.Where(s => s.Tag == Steps.Main).ToArray();
      var pos = stage.Steps.Where(s => s.Tag == Steps.Pos).ToArray();
      var postran = stage.Steps.Where(s => s.Tag == Steps.PosTransaction)
          .ToArray();

      //
      //  Executando passos...
      //
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
    catch (Exception ex)
    {
      stage.Faults.Add(Fault.EmitException(ex));
      OnError?.Invoke(this, new(ex));
      throw;
    }
    finally
    {
      if (cn != null) await cn.DisposeAsync();
    }
  }

  /// <summary>
  /// Executa uma etapa de migração de banco de dados, executando todos os
  /// scripts associados a ela, um a um.
  /// </summary>
  /// <param name="step">Definição da etapa a ser executada.</param>
  /// <param name="dbConnection">
  /// Conexão do banco de dados a ser utilizada.
  /// </param>
  /// <param name="dbTransaction">
  /// Transação do banco de dados a ser utilizada.
  /// </param>
  private async Task ExecuteStepAsync(StepNode step,
    DbConnection dbConnection, DbTransaction? dbTransaction = null)
  {
    var batcher = new BatchExtractor();
    foreach (var script in step.Scripts)
    {
      Batch[] batches;
      Batch? batch = null;

      try
      {
        OnMigrate?.Invoke(this, new(step, script));

        batches = await batcher.ExtractBatchesAsync(script);

        for (int i = 0; i < batches.Length; i++)
        {
          batch = batches[i];
          await ExecuteBatchAsync(batch, dbConnection, dbTransaction);
        }

        OnSuccess?.Invoke(this, new(step, script));
      }
      catch (Exception ex)
      {
        script.Faults.Add(Fault.EmitException(ex));
        OnError?.Invoke(this, new(ex, step, script, batch));
        throw;
      }
    }
  }

  /// <summary>
  /// Executa um lote de comandos SQL no banco de dados.
  /// </summary>
  /// <param name="batch">O lote de comandos SQL a ser executado.</param>
  /// <param name="dbConnection">A conexão com o banco de dados.</param>
  /// <param name="dbTransaction">
  /// A transação a ser utilizada na execução do lote. O valor padrão é null.
  /// </param>
  private async Task ExecuteBatchAsync(Domain.Batch batch,
    DbConnection dbConnection, DbTransaction? dbTransaction = null)
  {
    try
    {
      using var command = dbConnection.CreateCommand();
      command.CommandText = batch.Buffer.ToString();
      command.Transaction = dbTransaction;

      // TODO: Falta aplicar padrões de template ao script.

      await command.ExecuteNonQueryAsync();
    }
    catch (Exception ex)
    {
      if (batch.Index > 0)
        throw new Exception(
            $"Falha executando o {batch.Index + 1}º bloco do script.", ex);

      throw;
    }
  }

  /// <summary>
  /// Representa os argumentos de eventos para quando uma etapa do ScriptPack é
  /// executada.
  /// </summary>
  public class StepEventArgs : EventArgs
  {
    public StepEventArgs(StepNode step, ScriptNode script)
    {
      this.Step = step;
      this.Script = script;
    }

    /// <summary>
    /// A etapa que foi executada.
    /// </summary>
    public StepNode Step { get; }

    /// <summary>
    /// O script que foi executado.
    /// </summary>
    public ScriptNode Script { get; }
  }

  /// <summary>
  /// Representa os argumentos de eventos para quando ocorre um erro durante a
  /// execução de um script.
  /// </summary>
  public class ErrorEventArgs : EventArgs
  {
    public ErrorEventArgs(Exception cause, StepNode? step = null,
        ScriptNode? script = null, Batch? batch = null)
    {
      this.Step = step;
      this.Script = script;
      this.Cause = cause;
      this.Batch = batch;
    }

    /// <summary>
    /// A exceção que causou o erro.
    /// </summary>
    public Exception Cause { get; }

    /// <summary>
    /// A etapa onde ocorreu o erro.
    /// </summary>
    public StepNode? Step { get; }

    /// <summary>
    /// O script que causou o erro.
    /// </summary>
    public ScriptNode? Script { get; }

    /// <summary>
    /// O bloco de script que causou o erro.
    /// </summary>
    public Batch? Batch { get; }
  }
}