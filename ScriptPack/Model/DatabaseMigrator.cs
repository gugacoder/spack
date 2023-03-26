using System.Data.Common;
using ScriptPack.Model.Algorithms;
using ScriptPack.Domain;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace ScriptPack.Model;

/// <summary>
/// Responsável por executar as migrações do banco de dados seguindo as etapas
/// do pipeline de migração.
/// </summary>
public class DatabaseMigrator
{
  #region Eventos

  /// <summary>
  /// Evento acionado quando o pipeline de migração é iniciado.
  /// </summary>
  public event EventHandler<ConnectionEventArgs>? OnConnection;

  /// <summary>
  /// Evento invocado quando uma mensagem de conexão é recebida.
  /// </summary>
  public event EventHandler<ConnectionMessageEventArgs>? OnConnectionMessage;

  /// <summary>
  /// Evento acionado quando o pipeline de migração é iniciado.
  /// </summary>
  public event EventHandler<PhaseEventArgs<PipelineNode>>? OnPipelineStart;

  /// <summary>
  /// Evento acionado quando o pipeline de migração é finalizado.
  /// </summary>
  public event EventHandler<PhaseEventArgs<PipelineNode>>? OnPipelineEnd;

  /// <summary>
  /// Evento acionado quando uma etapa do pipeline de migração é iniciada.
  /// </summary>
  public event EventHandler<PhaseEventArgs<StageNode>>? OnStageStart;

  /// <summary>
  /// Evento acionado quando uma etapa do pipeline de migração é finalizada.
  /// </summary>
  public event EventHandler<PhaseEventArgs<StageNode>>? OnStageEnd;

  /// <summary>
  /// Evento acionado quando uma etapa de migração é iniciada.
  /// </summary>
  public event EventHandler<PhaseEventArgs<StepNode>>? OnStepStart;

  /// <summary>
  /// Evento acionado quando uma etapa de migração é finalizada.
  /// </summary>
  public event EventHandler<PhaseEventArgs<StepNode>>? OnStepEnd;

  /// <summary>
  /// Evento acionado a cada etapa de migração bem sucedida.
  /// </summary>
  public event EventHandler<ScriptEventArgs>? OnMigrate;

  /// <summary>
  /// Evento acionado quando toda a migração é bem sucedida.
  /// </summary>
  public event EventHandler<ScriptEventArgs>? OnSuccess;

  /// <summary>
  /// Evento acionado quando há um erro em alguma etapa da migração.
  /// </summary>
  public event EventHandler<ErrorEventArgs>? OnError;

  #endregion

  /// <summary>
  /// Cria uma nova instância de <cref name="DatabaseMigrator" />.
  /// </summary>
  public DatabaseMigrator()
  {
    Pipeline = null!;
    Context = null!;
  }

  /// <summary>
  /// Cria uma nova instância de <cref name="DatabaseMigrator" />.
  /// </summary>
  /// <param name="pipeline">Pipeline de migração a ser executado.</param>
  /// <param name="context">Contexto da migração, com a definição de argumentos
  /// e strings de conexão com a base de dados.</param>
  public DatabaseMigrator(PipelineNode pipeline, Context context)
  {
    Pipeline = pipeline;
    Context = context;
  }

  /// <summary>
  /// Pipeline de migração a ser executado.
  /// </summary>
  public PipelineNode Pipeline { get; set; }

  /// <summary>
  /// Contexto da migração, com a definição de argumentos e strings de conexão
  /// com a base de dados.
  /// </summary>
  public Context Context { get; set; }

  /// <summary>
  /// Executa as migrações do banco de dados seguindo as etapas do pipeline de
  /// migração.
  /// </summary>
  public async Task MigrateAsync()
  {
    try
    {
      OnPipelineStart?.Invoke(this, new(Pipeline));

      var catalogs = (
          from step in Pipeline.Descendants<StepNode>()
          from script in step.Scripts
          let catalog = script.Ancestor<CatalogNode>()
          where catalog != null
          select catalog
      ).Distinct();

      var connections = (
          from catalog in catalogs
          from connection in catalog.Connections
          select connection
      ).Distinct().ToArray();

      foreach (var stage in Pipeline.Stages)
      {
        OnStageStart?.Invoke(this, new(stage));

        await MigrateStageAsync(stage, connections);

        OnStageEnd?.Invoke(this, new(stage));
      }
    }
    catch (Exception ex)
    {
      Pipeline.Faults.Add(Fault.EmitException(ex));
    }
    finally
    {
      OnPipelineEnd?.Invoke(this, new(Pipeline));
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
      var connector = new DatabaseConnector(connections,
          Context.ConnectionStrings);
      cn = await connector.CreateConnectionAsync(pipeline.Connection);

      ConnectListeners(cn);

      await cn.OpenAsync();

      OnConnection?.Invoke(this, new(stage, cn));

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
      OnError?.Invoke(this, new(ex, stage));
      throw;
    }
    finally
    {
      if (cn != null) await cn.DisposeAsync();
    }
  }

  /// <summary>
  /// Conecta-se aos listeners da conexão do banco de dados e invoca o evento
  /// OnConnectionMessage quando uma mensagem de conexão é recebida.
  /// </summary>
  /// <param name="cn">
  /// A conexão do banco de dados à qual os listeners serão conectados.
  /// </param>
  private void ConnectListeners(DbConnection cn)
  {
    if (OnConnectionMessage is null) return;

    if (cn is SqlConnection sql)
    {
      sql.InfoMessage += (sender, e) =>
      {
        foreach (SqlError error in e.Errors)
        {
          var severe = (error.Class >= 16);
          OnConnectionMessage.Invoke(this, new(cn, severe, error.Message));
        }
      };
    }

    if (cn is NpgsqlConnection mysql)
    {
      mysql.Notice += (sender, e) =>
      {
        var severe = (e.Notice.Severity == "ERROR");
        OnConnectionMessage.Invoke(this, new(cn, severe, e.Notice.MessageText));
      };
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
    OnStepStart?.Invoke(this, new(step));
    try
    {
      var batcher = new BatchExtractor();
      foreach (var script in step.Scripts)
      {
        Batch[] batches;
        Batch? batch = null;

        try
        {
          OnMigrate?.Invoke(this, new(step, script, dbConnection));

          batches = await batcher.ExtractBatchesAsync(script);

          for (int i = 0; i < batches.Length; i++)
          {
            batch = batches[i];
            await ExecuteBatchAsync(batch, dbConnection, dbTransaction);
          }

          OnSuccess?.Invoke(this, new(step, script, dbConnection));
        }
        catch (Exception ex)
        {
          script.Faults.Add(Fault.EmitException(ex));
          OnError?.Invoke(this, new(ex, step, script, batch));
          throw;
        }
      }
    }
    finally
    {
      OnStepEnd?.Invoke(this, new(step));
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
  /// Classe que captura mensagens emitidas pela conexão.
  /// </summary>
  public class ConnectionMessageEventArgs : EventArgs
  {
    /// <summary>
    /// Cria uma nova instância da classe MessageEventArgs.
    /// </summary>
    /// <param name="message">A mensagem emitida.</param>
    /// <param name="severe">
    /// Um valor que indica se a mensagem é grave.
    /// </param>
    public ConnectionMessageEventArgs(DbConnection connection, bool severe,
        string message)
    {
      this.Connection = connection;
      this.Severe = severe;
      this.Message = message;
    }

    /// <summary>
    /// Obtém a conexão associada a esses argumentos de evento.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// Obtém um valor que indica se a mensagem é grave.
    /// </summary>
    public bool Severe { get; }

    /// <summary>
    /// Obtém a mensagem emitida.
    /// </summary>
    public string Message { get; }
  }

  /// <summary>
  /// Classe que representa os argumentos de eventos relacionados a conexões.
  /// </summary>
  public class ConnectionEventArgs : EventArgs
  {
    /// <summary>
    /// Cria uma nova instância da classe ConnectionEventArgs com uma conexão
    /// específica.
    /// </summary>
    /// <param name="stage">
    /// A etapa associada a esses argumentos de evento.
    /// </param>
    /// <param name="connection">
    /// A conexão associada a esses argumentos de evento.
    /// </param>
    public ConnectionEventArgs(StageNode stage, DbConnection connection)
    {
      this.Stage = stage;
      this.Connection = connection;
    }

    /// <summary>
    /// Obtém a etapa associada a esses argumentos de evento.
    /// </summary>
    public StageNode Stage { get; }

    /// <summary>
    /// Obtém a conexão associada a esses argumentos de evento.
    /// </summary>
    public DbConnection Connection { get; }
  }

  /// <summary>
  /// Representa os argumentos de eventos para quando uma etapa do ScriptPack é
  /// executada.
  /// </summary>
  public class PhaseEventArgs<T> : EventArgs
    where T : INode
  {
    public PhaseEventArgs(T phase)
    {
      this.Phase = phase;
    }

    /// <summary>
    /// O pipeline, a etapa ou o passo em execução
    /// </summary>
    public T Phase { get; }
  }

  /// <summary>
  /// Representa os argumentos de eventos para quando uma etapa do ScriptPack é
  /// executada.
  /// </summary>
  public class ScriptEventArgs : EventArgs
  {
    public ScriptEventArgs(StepNode step, ScriptNode script,
        DbConnection connection)
    {
      this.Step = step;
      this.Script = script;
      this.Connection = connection;
    }

    /// <summary>
    /// O pipeline, a etapa ou o passo em execução
    /// </summary>
    public INode Step { get; }

    /// <summary>
    /// A conexão do banco de dados a ser utilizada.
    /// </summary>
    public DbConnection Connection { get; set; }

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
    public ErrorEventArgs(Exception exception, INode phase,
        ScriptNode? script = null, Batch? batch = null)
    {
      this.Exception = exception;
      this.Phase = phase;
      this.Script = script;
      this.Batch = batch;
    }

    /// <summary>
    /// A exceção que causou o erro.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// O pipeline, a etapa ou o passo em execução
    /// </summary>
    public INode Phase { get; }

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