using System.Text.RegularExpressions;

namespace ScriptPack.Helpers;

/// <summary>
/// Conjunto de utilitários para strings.
/// </summary>
public static class StringExtensions
{
  /// <summary>
  /// Checa se a string corresponde ao padrão informado.
  /// O padrão pode conter os caracteres * e ?.
  /// </summary>
  /// <param name="string">
  /// String a ser checada.
  /// </param>
  /// <param name="searchPattern">
  /// Padrão de busca.
  /// O padrão pode conter os caracteres * e ?.
  /// </param>
  /// <returns>
  /// <c>true</c> se a string corresponde ao padrão informado;
  /// <c>false</c> caso contrário.
  /// </returns>
  public static bool Like(this string @string, string searchPattern)
  {
    var pattern = "^" + Regex.Escape(searchPattern)
        .Replace("\\*", ".*").Replace("\\?", ".") + "$";
    return Regex.IsMatch(@string, pattern, RegexOptions.IgnoreCase);
  }
}
