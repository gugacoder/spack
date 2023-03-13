namespace ScriptPack.Helpers;

/// <summary>
/// Classe utilitária que implementa a interface IComparer para comparar
/// versões no formato SEMVER (Semantic Versioning).
/// </summary>
public class VersionComparer : IComparer<string?>
{
  /// <summary>
  /// Compara duas strings que representam versões e retorna um valor inteiro
  /// que indica se a primeira string é menor, igual ou maior que a segunda
  /// string em termos de versão.
  /// </summary>
  /// <param name="x">A primeira string que representa a versão.</param>
  /// <param name="y">A segunda string que representa a versão.</param>
  /// <returns>
  /// Um valor inteiro que indica se a primeira string é menor, igual ou maior
  /// que a segunda string em termos de versão.
  /// </returns>
  public int Compare(string? x, string? y)
  {
    return CompareVersions(x, y);
  }

  /// <summary>
  /// Compara duas strings que representam versões de acordo com o padrão
  /// SEMVER.
  /// </summary>
  /// <param name="x">A primeira string que representa a versão.</param>
  /// <param name="y">A segunda string que representa a versão.</param>
  /// <returns>
  /// Um valor inteiro que indica se a primeira string é menor, igual ou maior
  /// que a segunda string em termos de versão.
  /// </returns>
  public static int CompareVersions(string? x, string? y)
  {
    if (x == null && y == null) return 0;
    if (x == null) return -1;
    if (y == null) return 1;

    var version1 = x.Split('.');
    var version2 = y.Split('.');

    for (int i = 0; i < Math.Max(version1.Length, version2.Length); i++)
    {
      int part1 = 0;
      int part2 = 0;

      if (i < version1.Length && int.TryParse(version1[i], out int result1))
        part1 = result1;

      if (i < version2.Length && int.TryParse(version2[i], out int result2))
        part2 = result2;

      if (part1 != part2)
        return part1.CompareTo(part2);
    }

    return 0;
  }
}