namespace SPack.Commands;

/// <summary>
/// Interface para um comando que pode ser executado pelo aplicativo.
/// </summary>
public interface ICommand
{
  /// <summary>
  /// Executa o comando.
  /// </summary>
  /// <returns>A tarefa assíncrona resultante da execução do comando.</returns>
  Task RunAsync();
}
