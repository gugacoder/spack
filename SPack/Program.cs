using SPack.Prompting;

public class Program
{
  [STAThread]
  public static async Task Main(string[] args)
  {
    var commandRunner = new CommandRunner();
    await commandRunner.RunAsync(args);
  }
}