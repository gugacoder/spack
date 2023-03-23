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
    if (x is null && y is null) return 0;
    if (x is null) return -1;
    if (y is null) return 1;

    var version1 = ParseVersion(x);
    var version2 = ParseVersion(y);

    for (int i = 0; i < Math.Max(version1.Length, version2.Length); i++)
    {
      IComparable part1 = version1[i];
      IComparable part2 = version2[i];

      if (part1 is int)
      {
        if (part1.CompareTo(part2) is int value && value != 0)
          return value;

        continue;
      }

      if (part1 is string a1)
      {
        var a2 = (string)part2;
        // casos comuns
        if (a1 == a2) continue;
        if (a1 == "alpha") return -1;
        if (a1 == "beta") return a2 == "alpha" ? 1 : -1;
        if (a1 == "rc") return a2 == "alpha" || a2 == "beta" ? 1 : -1;
        if (a1 == "final") return 1;
        // casos especiais
        if (a1 == "branch") return -1;
        if (a1 == "trunk") return a2 == "branch" ? 1 : -1;
        if (a1 == "latest") return 1;
      }
    }

    return 0;
  }

  /// <summary>
  /// Analisa uma string que representa uma versão e retorna um array de
  /// strings com os tokens da versão.
  /// </summary>
  /// <param name="version">A string que representa a versão.</param>
  /// <returns>
  /// Um array de strings com os tokens da versão.
  /// </returns>
  /// <remarks>
  /// A string que representa a versão deve estar no formato SEMVER.
  /// Forma geral da versão:
  ///    <major>.<minor>.<patch>[-<label>]
  /// </remarks>
  private static IComparable[] ParseVersion(string version)
  {
    string[] tokens;

    version = version.Split('_')[0];

    tokens = version.Split('-');
    var label = tokens.Length > 1 ? tokens[1] : "";

    version = tokens[0];

    tokens = version.Split('.');
    var patch = tokens.Length > 2 ? int.Parse(tokens[2]) : 0;
    var minor = tokens.Length > 1 ? int.Parse(tokens[1]) : 0;
    var major = tokens.Length > 0 ? int.Parse(tokens[0]) : 0;

    return new IComparable[] { major, minor, patch, label };
  }
}