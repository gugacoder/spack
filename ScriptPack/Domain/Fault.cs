using System.Text;
using ScriptPack.Helpers;

namespace ScriptPack.Domain;

/// <summary>
/// Classe que representa uma falha na execução de scripts.
/// </summary>
public class Fault
{
  /// <summary>
  /// Constantes utilizadas como dicas para identificar a natureza da falha.
  /// </summary>
  public static class Hints
  {
    /// <summary>
    /// Indica que a falha foi causada por uma exceção.
    /// </summary>
    public const string Exception = nameof(Exception);

    /// <summary>
    /// Indica que a falha foi causada por uma dependência cíclica.
    /// </summary>
    public const string CircularDependency = nameof(CircularDependency);
  }

  /// <summary>
  /// Dica para identificar a natureza da falha.
  /// </summary>
  public string Hint { get; set; } = "";

  /// <summary>
  /// Mensagem de erro da falha.
  /// </summary>
  public string Message { get; set; } = "";

  /// <summary>
  /// Detalhes adicionais da falha, como o stack trace.
  /// </summary>
  public string? Details { get; set; }

  /// <summary>
  /// Cria um objeto FaultNode com uma mensagem de erro indicando que houve uma
  /// dependência cíclica entre dois scripts.
  /// </summary>
  /// <param name="a">ScriptNode que representa o primeiro script.</param>
  /// <param name="b">ScriptNode que representa o segundo script.</param>
  /// <returns>Objeto FaultNode com a mensagem de erro.</returns>
  public static Fault EmitCircularDependency(ScriptNode a, ScriptNode b)
  {
    return new()
    {
      Hint = Hints.CircularDependency,
      Message = $"O script {a.Name} tem dependência cíclica com {b.Name}"
    };
  }

  /// <summary>
  /// Retorna um objeto FaultNode para uma exceção. 
  /// </summary>
  /// <param name="exception">
  /// A exceção que ocorreu. 
  /// </param>
  /// <param name="message">
  /// Uma mensagem opcional para a falha. 
  /// </param>
  /// <returns>
  /// Um objeto FaultNode contendo informações sobre a falha. 
  /// </returns>
  public static Fault EmitException(Exception exception,
      string? message = null)
  {
    return new()
    {
      Hint = Hints.Exception,
      Message = message ?? exception.GetStackMessage(),
      Details = exception.GetStackTrace()
    };
  }
}
