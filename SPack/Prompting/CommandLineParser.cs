using Humanizer;
using SPack.Commands;

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
    var designs = ArgumentDesign.ExtractArgumentDesigns(options);

    for (var i = 0; i < args.Length; i++)
    {
      var arg = args[i];
      try
      {
        var design = designs.SingleOrDefault(x => x.IsMatch(arg));
        if (design is null)
          throw new ArgumentException($"Argumento desconhecido: {arg}");

        var option = design.Option;

        //
        // Ativando o uso da opção.
        //

        option.On = true;

        if (option is Switch)
          continue;

        //
        // A opção existe um valor. Verificando se o valor foi informado.
        //
        var argValue = (args.Length > (i + 1) && !args[i + 1].StartsWith("-"))
            ? args[++i]
            : option.DefaultValue;

        if (argValue is null)
          throw new IndexOutOfRangeException(
              $"USO INCORRETO! Valor do argumento não informado: {arg}");

        //
        // Realizando o parsing do argumento
        //

        if (option is Option opt)
        {
          opt.GetType().GetProperty("Value")?.SetValue(opt, argValue);
          continue;
        }

        if (option is OptionList optList)
        {
          var items = argValue
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
