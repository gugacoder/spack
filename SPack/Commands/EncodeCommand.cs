using ScriptPack.Helpers;

namespace SPack.Commands;

/// <summary>
/// Classe responsável por codificar tokens.
/// </summary>
public class EncodeCommand : ICommand
{
  /// <summary>
  /// Obtém ou define um valor booleano que indica se a execução deve ser
  /// verbosa ou não.
  /// </summary>
  public bool Verbose { get; set; } = false;

  /// <summary>
  /// O token a ser codificado.
  /// </summary>
  public string Token { get; set; } = "";

  /// <summary>
  /// Codifica um token e exibe o resultado no console.
  /// </summary>
  public async Task RunAsync()
  {
    var result = Crypto.Encrypt(this.Token);
    await Console.Out.WriteLineAsync(result);
  }
}
