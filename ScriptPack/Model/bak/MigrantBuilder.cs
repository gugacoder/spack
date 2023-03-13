// using ScriptPack.Domain;
// using ScriptPack.Helpers;

// namespace ScriptPack.Model;

// public class MigrantBuilder
// {
//   private IDrive? drive;
//   private List<ConnectionNode> connections = new();
//   private List<PipelineNode> pipelines = new();

//   public void AddDrive(IDrive drive)
//   {
//     this.drive = drive;
//   }

//   public void AddConnection(params ConnectionNode[] connections)
//   {
//     this.connections.AddRange(connections);
//   }

//   public void AddPipeline(params PipelineNode[] pipelines)
//   {
//     this.pipelines.AddRange(pipelines);
//   }

//   public List<Migrant> BuildMigrant()
//   {
//     if (this.drive == null)
//       throw new ArgumentNullException(nameof(this.drive), "Driver não informado.");

//     return pipelines.Select(pipeline =>
//        new Migrant(drive, connections.ToArray(), pipeline)).ToList();
//   }
// }
