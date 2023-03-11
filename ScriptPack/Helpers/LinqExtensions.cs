namespace ScriptPack.Helpers;

/// <summary>
/// Classe estática que contém métodos de extensão para trabalhar com coleções
/// IEnumerable usando a biblioteca LINQ.
/// </summary>
public static class LinqExtensions
{
  /// <summary>
  /// Executa a ação especificada em cada elemento de uma coleção genérica e
  /// retorna a coleção.
  /// </summary>
  /// <typeparam name="T">
  /// O tipo dos elementos na coleção.
  /// </typeparam>
  /// <param name="source">
  /// A coleção a ser iterada.
  /// </param>
  /// <param name="action">
  /// A ação a ser executada para cada elemento da coleção.
  /// </param>
  /// <returns>
  /// A coleção original.
  /// </returns>
  public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source,
      Action<T> action)
  {
    foreach (var item in source)
    {
      action(item);
    }
    return source;
  }
}
