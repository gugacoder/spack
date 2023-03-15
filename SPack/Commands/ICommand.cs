namespace SPack.Commands;

/// <summary>
/// Interface para um comando que pode ser executado pelo aplicativo.
/// </summary>
public interface ICommand
{
  /// <summary>
  /// Obtém ou define um valor booleano que indica se a execução deve ser
  /// verbosa ou não.
  /// </summary>
  bool Verbose { get; set; }

  /// <summary>
  /// Executa o comando.
  /// </summary>
  /// <returns>A tarefa assíncrona resultante da execução do comando.</returns>
  Task RunAsync();
}
