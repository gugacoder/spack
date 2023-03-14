namespace SPack.Commands;

public class MigrateCommand : ICommand
{
  public string Catalog { get; set; } = "";
  public List<string> Scripts { get; set; } = new();
  public List<string> Connections { get; set; } = new();

  public Task RunAsync()
  {
    throw new NotImplementedException();
  }
}
