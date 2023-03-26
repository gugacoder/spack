using SPack.Prompting;

namespace SPack.Commands;

/// <summary>
/// Interface para um comando que pode ser executado pelo aplicativo.
/// </summary>
public interface ICommand
{
  /// <summary>
  /// Executa o comando.
  /// </summary>
  /// <param name="options">As opções de linha de comando.</param>
  /// <returns>A tarefa assíncrona resultante da execução do comando.</returns>
  Task RunAsync(CommandLineOptions options);
}
