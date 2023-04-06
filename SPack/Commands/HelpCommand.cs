using SPack.Commands.Printers;
using SPack.Prompting;

namespace SPack.Commands;

/// <summary>
/// A classe de impressão de ajuda para a aplicação ScriptPack.
/// </summary>
/// <remarks>
/// A classe Helper tem como objetivo prover funcionalidades de ajuda para a
/// aplicação ScriptPack. Essas funcionalidades são destinadas a auxiliar o
/// usuário em tarefas como consultar informações de uso, visualizar exemplos de
/// comandos e assim por diante.
/// A classe Helper possui um método assíncrono chamado PrintHelpAsync, que
/// busca o conteúdo do arquivo de ajuda "HELP.info" embarcado na aplicação e o
/// imprime no console para que o usuário possa visualizá-lo. Caso o arquivo não
/// esteja disponível, a classe imprime uma mensagem de erro informando o
/// problema.
/// Para utilizar as funcionalidades oferecidas pela classe Helper, basta
/// instanciá-la em seu código e chamar o método PrintHelpAsync.
/// </remarks>
public class HelpCommand : ICommand
{
  /// <summary>
  /// Obtém ou define um valor booleano que indica se a execução deve ser
  /// verbosa ou não.
  /// </summary>
  public bool Verbose { get; set; } = false;

  /// <summary>
  /// Imprime o arquivo de ajuda HELP.info na saída padrão.
  /// </summary>
  /// <remarks>
  /// O arquivo de ajuda HELP.info é incluído como recurso na aplicação.
  /// </remarks>
  /// <exception cref="Exception">
  /// Lançada quando o arquivo de ajuda não pode ser encontrado ou lido.
  /// </exception>
  public Task RunAsync(CommandLineOptions options)
  {
    new HelpPrinter().Print();
    return Task.CompletedTask;
  }
}
