namespace ScriptPack.Domain;

/// <summary>
/// Representa uma exceção lançada quando ocorre uma falha no processamento de
/// scripts. A exceção carrega a instância da falha para ser captura e tratada
/// no decorrer da pilha de chamadas.
/// </summary>
public class FaultException : Exception
{
  /// <summary>
  /// Cria uma nova instância da classe FaultException com a falha especificada.
  /// </summary>
  /// <param name="fault">A falha ocorrida.</param>
  /// <param name="cause">A exceção que causou a falha.</param>
  public FaultException(Fault fault, Exception? cause = null)
      : base(fault.Message, cause)
  {
    Fault = fault;
  }

  /// <summary>
  /// Obtém a falha ocorrida.
  /// </summary>
  public Fault Fault { get; }
}
