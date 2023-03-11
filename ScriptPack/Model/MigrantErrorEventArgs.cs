// using ScriptPack.Domain;

// namespace ScriptPack.Model;

// public class MigrantErrorEventArgs : EventArgs
// {
//   public MigrantErrorEventArgs(PipelineNode pipeline, Exception exception)
//   {
//     this.Pipeline = pipeline;
//     this.Exception = exception;
//   }

//   public MigrantErrorEventArgs(StageNode stage, Exception exception)
//   {
//     this.Stage = stage;
//     this.Pipeline = stage.Parent!;
//     this.Exception = exception;
//   }

//   public MigrantErrorEventArgs(StepNode step, Exception exception)
//   {
//     this.Step = step;
//     this.Stage = step.Parent;
//     this.Pipeline = step.Parent!.Parent!;
//     this.Exception = exception;
//   }

//   public MigrantErrorEventArgs(StepNode step, ScriptNode script, Batch? batch, Exception exception)
//   {
//     this.Step = step;
//     this.Stage = step.Parent;
//     this.Pipeline = step.Parent!.Parent!;
//     this.Script = script;
//     this.Batch = batch;
//     this.Exception = exception;
//   }

//   public PipelineNode Pipeline { get; }
//   public StageNode? Stage { get; }
//   public StepNode? Step { get; }

//   public ScriptNode? Script { get; }
//   public Batch? Batch { get; }

//   public Exception Exception { get; }
// }
