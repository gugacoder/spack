using Humanizer;
using ScriptPack.Helpers;
using SPack.Commands;
using SPack.Prompting.Domain;

namespace SPack.Prompting;

/// <summary>
/// Utilitário para interpretação de argumentos de linha de comando.
/// </summary>
public class CommandLineParser
{
  /// <summary>
  /// Analisa os argumentos passados para o programa e os mapeia para as ações,
  /// opções e parâmetros correspondentes.
  /// </summary>
  /// <param name="args">Os argumentos passados para o programa.</param>
  /// <param name="options">
  /// Um objeto com as opções, ações e argumentos esperados.
  /// </param>
  /// <remarks>
  /// Os argumentos podem ser passados de duas formas:
  /// 1. Com a opção completa, precedida de dois hífens, como "--help".
  /// 2. Com a opção abreviada, precedida de um hífen, como "-h".
  /// </remarks>
  /// <exception cref="Exception">
  /// Lançada quando um argumento desconhecido é passado.
  /// </exception>
  public CommandLineOptions ParseArgs(string[] args)
  {
    if (args.Length == 0)
      throw new ArgumentException("USO INCORRETO! Nenhum argumento informado.");

    var options = new CommandLineOptions();

    for (var i = 0; i < args.Length; i++)
    {
      var arg = args[i];
      try
      {
        var option = options.AllOptions.SingleOrDefault(
            o => o.Design.IsMatch(o, arg));

        if (option is null)
          throw new ArgumentException($"Argumento desconhecido: {arg}");

        //
        // Ativando o uso da opção.
        //

        option.On = true;

        if (option is Switch)
          continue; // Nada mais a fazer com esta opção.

        //
        // A opção existe um valor. Verificando se o valor foi informado.
        //
        var optionValue =
            (args.Length > (i + 1) && !args[i + 1].StartsWith("-"))
                ? args[++i]
                : option.Design.DefaultValue;

        if (optionValue is null)
        {
          throw new IndexOutOfRangeException(
              $"USO INCORRETO! Valor do argumento não informado: {arg}");
        }

        //
        // Realizando o parsing do argumento
        //

        if (option is Option opt)
        {
          opt.Value = optionValue;
          continue;
        }

        if (option is OptionList optList)
        {
          var items = optionValue
              .Split(',')
              .Select(x => x.Trim())
              .Where(x => !string.IsNullOrWhiteSpace(x))
              .ToList();
          optList.Items.AddRange(items);
          continue;
        }
      }
      catch (ArgumentException) { throw; }
      catch (IndexOutOfRangeException) { throw; }
      catch (Exception ex)
      {
        throw new ArgumentException(
            $"USO INCORRETO! Valor do argumento não informado: {arg}", ex);
      }
    }

    return options;
  }
}
