namespace SPack.Commands.Printers;

/// <summary>
/// Impressora da ajuda do SPack.
/// </summary>
public class HelpPrinter : IPrinter
{
  /// <summary>
  /// Imprime a ajuda do SPack.
  /// </summary>
  public void Print()
  {
    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
    var manifest = assembly.GetManifestResourceNames()
        .First(m => m.EndsWith("HELP.info"));

    using var stream = assembly.GetManifestResourceStream(manifest);
    if (stream is null)
    {
      Console.Error.WriteLine(
          "FALHA! O arquivo de ajuda HELP.info não foi distribuído " +
          "corretamente com a aplicação.");
      return;
    }

    try
    {
      using var reader = new StreamReader(stream);
      var content = reader.ReadToEnd();
      Console.Out.WriteLine(content);
    }
    catch (Exception ex)
    {
      Console.Error.WriteLine(
          "FALHA! O arquivo de ajuda HELP.info não foi distribuído " +
          "corretamente com a aplicação.");

      Console.Error.WriteLine();
      Console.Error.WriteLine("Causa:");
      Console.Error.WriteLine(ex.StackTrace);
    }

    return;
  }
}
