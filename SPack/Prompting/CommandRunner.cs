using Humanizer;
using ScriptPack.FileSystem;
using SPack.Commands;
using SPack.Prompting.Domain;

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

      // Interpretando os argumentos de linha de comando.
      options = commandLineParser.ParseArgs(args);

      if (options.Help.On)
      {
        var help = new HelpCommand();
        await help.RunAsync(options);
        return;
      }

      // Selecionando a ação a ser executada.
      var actions = (
          from option in options.AllOptions
          where option.On && !option.Design.Long
          select option
      ).ToArray();

      if (actions.Length == 0)
      {
        throw new ArgumentException(
            "USO INCORRETO! Nenhuma ação indicada. " +
            "Use --help para mais detalhes.");
      }

      if (actions.Length > 1)
      {
        var actionNames = string.Join(", ",
            actions.Select(x => x.Name.ToLower()));
        throw new ArgumentException(
            "USO INCORRETO! Mais de uma ação indicada. " +
            $"Ações: {actionNames}. " +
            "Use --help para mais detalhes.");
      }

      var action = actions[0];

      var commandTypeName = $"SPack.Commands.{action.Name}Command";
      var commandType = Type.GetType(commandTypeName);

      if (commandType is null)
      {
        var actionName = action.Name.Kebaberize();
        throw new ArgumentException(
            $"USO INCORRETO! Ação '{actionName}' não implementada. " +
            "Use --help para mais detalhes.");
      }

      var command = Activator.CreateInstance(commandType) as ICommand;
      if (command is null)
      {
        var actionName = action.Name.Kebaberize();
        throw new ArgumentException(
            $"USO INCORRETO! Ação '{actionName}' não implementada. " +
            "Use --help para mais detalhes.");
      }

      await command.RunAsync(options);

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
        } while (cause is not null);
      }
    }
  }
}