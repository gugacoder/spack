using ScriptPack.Model;
using SPack.Prompting;

namespace SPack.Commands.Helpers;

/// <summary>
/// Classe responsável por construir o objeto <see cref="Context"/> a partir das
/// opções de linha de comando especificadas pelo usuário. As opções são
/// definidas por um objeto <see cref="CommandLineOptions"/> passado para o
/// método <see cref="AddOptions"/>.
/// </summary>
public class ContextBuilder
{
  private CommandLineOptions _options = null!;

  /// <summary>
  /// Adiciona as opções de linha de comando especificadas pelo usuário para
  /// construir o contexto.
  /// </summary>
  /// <param name="options">
  /// O objeto <see cref="CommandLineOptions"/> que contém as opções de linha de
  /// comando.
  /// </param>
  public void AddOptions(CommandLineOptions options)
  {
    _options = options;
  }

  /// <summary>
  /// Constrói um objeto <see cref="Context"/> a partir das opções de linha de
  /// comando adicionadas através do método <see cref="AddOptions"/>. Retorna o
  /// objeto <see cref="Context"/> resultante.
  /// </summary>
  /// <returns>
  /// O objeto <see cref="Context"/> resultante construído a partir das opções
  /// de linha de comando.
  /// </returns>
  public Context BuildContext()
  {
    var context = new Context();

    if (_options.Arg.On)
    {
      _options.Arg.Items.ForEach(arg =>
      {
        var tokens = arg.Split('=', 2);
        context.Arguments[tokens[0]] = tokens[1];
      });
    }

    if (_options.Database.On)
    {
      _options.Database.Items.ForEach(database =>
      {
        var tokens = database.Split(';');
        var connectionName = tokens
            .Where(t => t.StartsWith("Name=", StringComparison.OrdinalIgnoreCase))
            .Select(t => t.Split('=', 2)[1])
            .FirstOrDefault() ?? "Default";
        context.ConnectionStrings[connectionName] = database;
      });
    }

    return context;
  }
}
