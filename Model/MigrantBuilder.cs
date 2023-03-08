using SPack.Domain;
using SPack.Library;

namespace SPack.Model;

public class MigrantBuilder
{
  private IDrive? drive;
  private List<Connection> connections = new();
  private List<Pipeline> pipelines = new();

  public void AddDrive(IDrive drive)
  {
    this.drive = drive;
  }

  public void AddConnection(params Connection[] connections)
  {
    this.connections.AddRange(connections);
  }

  public void AddPipeline(params Pipeline[] pipelines)
  {
    this.pipelines.AddRange(pipelines);
  }

  public List<Migrant> BuildMigrant()
  {
    if (this.drive == null)
      throw new ArgumentNullException(nameof(this.drive), "Driver não informado.");

    return pipelines.Select(pipeline =>
       new Migrant(drive, connections.ToArray(), pipeline)).ToList();
  }
}
