namespace ScriptPack.Domain;

public class Steps
{
  /// <summary>
  /// Passo ocorrido antes da abertura da transação.
  /// </summary>
  public const string PreTransaction = "-pretran";

  /// <summary>
  /// Passo ocorrido antes da execução dos scripts principais.
  /// </summary>
  public const string Pre = "-pre";

  /// <summary>
  /// Passo de execução dos scripts principais.
  /// </summary>
  public const string Main = "";

  /// <summary>
  /// Passo ocorrido após a execução dos scripts principais.
  /// </summary>
  public const string Pos = "-pos";

  /// <summary>
  /// Passo ocorrido após o fechamento da transação.
  /// </summary>
  public const string PosTransaction = "-postran";

  /// <summary>
  /// Todos os passos disponíveis em ordem de execução.
  /// </summary>
  public static readonly string[] AllSteps = new string[]
  {
    PreTransaction,
    Pre,
    Main,
    Pos,
    PosTransaction
  };

  /// <summary>
  /// Obtém um nome amigável para o passo.
  /// </summary>
  /// <param name="step">
  /// O passo.
  /// </param>
  /// <returns>
  /// O nome amigável.
  /// </returns>
  public static string NameStep(string step)
  {
    return step switch
    {
      PreTransaction => "Pré-Transação",
      PosTransaction => "Pós-Transação",
      Pre => "Pré",
      Pos => "Pós",
      _ => "Principal"
    };
  }

  /// <summary>
  /// Verifica se o passo é transacional.
  /// Um passo transacional é executado dentro de um contexto de transação.
  /// Um passo não transacional é executado antes ou depois da transação.
  /// </summary>
  /// <param name="tag">
  /// O passo.
  /// </param>
  /// <returns>
  /// Verdadeiro se o passo é transacional e falso caso contrário.
  /// </returns>
  public static bool IsTransactional(string tag)
  {
    return !tag.Contains("tran");
  }
}
