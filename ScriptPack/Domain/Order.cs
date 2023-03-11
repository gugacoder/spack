namespace ScriptPack.Domain;

/// <summary>
/// Define os tipos de ordenação possíveis para os scripts de migração de base
/// de dados.
/// </summary>
public static class Orders
{
  /// <summary>
  /// O sistema detecta e ordena automaticamente os scripts pela análise de
  /// dependência no corpo do script.
  /// </summary>
  public const string Auto = "auto";

  /// <summary>
  /// O sistema executa os scripts em ordem alfabética de nome de arquivo.
  /// </summary>
  public const string Alpha = "alpha";

  /// <summary>
  /// Retorna o tipo de ordenação a partir de uma string de entrada.
  /// </summary>
  /// <param name="order">
  /// A string de entrada que representa o tipo de ordenação.
  /// </param>
  /// <returns>
  /// O tipo de ordenação correspondente à string de entrada.
  /// </returns>
  public static string GetOrder(string order)
  {
    // Este método recebe uma string como parâmetro e retorna o tipo de
    // ordenação correspondente.
    // A string de entrada pode ser "Auto" ou uma forma de escrita da palavra
    // "Alphabetic" em portugês.
    // Caso a string não corresponda a nenhum dos tipos de ordenação, o método
    // retorna "Auto".
    return order.ToLower() switch
    {
      Alpha => Alpha,
      "alfa" => Alpha,
      "alfabética" => Alpha,
      "alfabético" => Alpha,
      "alfabetica" => Alpha,
      "alfabetico" => Alpha,
      _ => Auto,
    };
  }
}
