using SPack.Commands;

namespace SPack.Prompting;

/// <summary>
/// Utilitário para interpretação de argumentos de linha de comando.
/// </summary>
public class CommandRunner
{
  /// <summary>
  /// Executa um comando com base nos argumentos passados na linha de comando.
  /// </summary>
  /// <param name="args">Os argumentos passados para o programa.</param>
  public async Task RunAsync(string[] args)
  {
    CommandLineOptions? options = null;

    try
    {
      var commandLineParser = new CommandLineParser();
      options = commandLineParser.ParseArgs(args);

      var catalog = options.Catalog.On ? options.Catalog.Value : null;

      ICommand command = options switch
      {
        { Help: { On: true } }
            => new HelpCommand(),

        { Encode: { On: true } }
            => new EncodeCommand { Token = options.Encode.Value },

        { List: { On: true } }
            => new ListCommand
            {
              CatalogPath = catalog,
              SearchPattern = options.List.Value
            },

        { Show: { On: true } }
            => new ShowCommand
            {
              CatalogPath = catalog,
              SearchPattern = options.Show.Value,
              DatabaseMaps = options.Database.Items
            },

        { Validate: { On: true } }
            => new ValidateCommand
            {
              CatalogPath = catalog,
              SearchPackageCriteria = options.Package.Items,
              SearchScriptCriteria = options.Script.Items
            },

        { Pipeline: { On: true } }
            => new PipelineCommand
            {
              CatalogPath = catalog,
              SearchPackageCriteria = options.Package.Items,
              SearchScriptCriteria = options.Script.Items,
              BuiltInScripts = !options.IgnoreBuiltIn.On
            },

        { Init: { On: true } }
            => new MigrateCommand
            {
              CatalogPath = catalog,
              DatabaseMaps = options.Database.Items,
              BuiltInScripts = true
            },

        { Migrate: { On: true } }
            => new MigrateCommand
            {
              CatalogPath = catalog,
              SearchPackageCriteria = options.Package.Items,
              SearchScriptCriteria = options.Script.Items,
              DatabaseMaps = options.Database.Items,
              BuiltInScripts = !options.IgnoreBuiltIn.On
            },

        _ => throw new ArgumentException(
            "USO INCORRETO! Nenhuma ação indicada. " +
            "Use --help para mais detalhes.")
      };

      command.Verbose = options.Verbose.On;

      await command.RunAsync();

    }
    catch (Exception ex)
    {
      Environment.ExitCode = 1;
      Console.Error.WriteLine(ex.Message);
      if (options?.Verbose.On == true)
      {
        Exception? cause = ex;
        do
        {
          Console.Error.WriteLine("---");
          Console.Error.WriteLine(cause.StackTrace);
          cause = cause.InnerException;
        } while (cause != null);
      }
    }
  }
}