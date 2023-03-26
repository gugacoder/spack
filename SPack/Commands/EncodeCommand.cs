using ScriptPack.Helpers;
using SPack.Prompting;

namespace SPack.Commands;

/// <summary>
/// Classe responsável por codificar tokens.
/// </summary>
public class EncodeCommand : ICommand
{
  /// <summary>
  /// Codifica um token e exibe o resultado no console.
  /// </summary>
  public async Task RunAsync(CommandLineOptions options)
  {
    var token = options.Encode.Value;
    var result = Crypto.Encrypt(token);
    await Console.Out.WriteLineAsync(result);
  }
}
