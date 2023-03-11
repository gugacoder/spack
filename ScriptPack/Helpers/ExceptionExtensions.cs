using System.Text;

namespace ScriptPack.Helpers;

/// <summary>
/// Fornece métodos de extensão para a classe Exception.
/// </summary>
public static class ExceptionExtensions
{
  /// <summary>
  /// Retorna uma string com a mensagem de exceção e as mensagens de exceção
  /// interna.
  /// </summary>
  /// <param name="exception">
  /// A exceção.
  /// </param>
  /// <returns>
  /// Uma string com a mensagem de exceção e as mensagens de exceção interna.
  /// </returns>
  public static string GetStackMessage(this Exception exception)
  {
    var message = new StringBuilder();
    while (exception != null)
    {
      message.Append("- ").AppendLine(exception.Message);
      exception = exception.InnerException!;
    }
    return message.ToString();
  }

  /// <summary>
  /// Retorna uma string contendo a mensagem de exceção e a pilha de chamadas
  /// dos métodos que levaram à exceção,
  /// bem como as mensagens de exceção interna, se houver.
  /// </summary>
  /// <param name="exception">
  /// A exceção.
  /// </param>
  /// <returns>
  /// Uma string contendo a mensagem de exceção e a pilha de chamadas dos
  /// métodos que levaram à exceção,
  /// bem como as mensagens de exceção interna, se houver.
  /// </returns>
  public static string GetStackTrace(this Exception exception)
  {
    var stackTrace = new StringBuilder();
    while (exception != null)
    {
      stackTrace.Append((stackTrace.Length == 0) ? "Fault: " : "Cause: ");
      stackTrace.AppendLine(exception.Message);
      stackTrace.Append("Of Type: ");
      stackTrace.AppendLine(exception.GetType().Name);
      stackTrace.AppendLine(exception.StackTrace);
      exception = exception.InnerException!;
    }
    return stackTrace.ToString();
  }
}
