namespace ScriptPack.Helpers;

public class PathComparer : IComparer<string?>
{
  public int Compare(string? path1, string? path2)
  {
    return ComparePaths(path1, path2);
  }

  /// <summary>
  /// Compara dois caminhos de arquivo para determinar qual é maior em termos de
  /// precedência.
  /// </summary>
  /// <remarks>
  /// O método compara dois caminhos de arquivo, considerando que um caminho é
  /// maior se o nome do diretório raiz é maior em ordem alfabética ou se os
  /// nomes de diretórios são iguais e o nome do arquivo é maior em ordem
  /// alfabética.
  /// </remarks>
  /// <param name="path1">O primeiro caminho de arquivo a ser comparado.</param>
  /// <param name="path2">O segundo caminho de arquivo a ser comparado.</param>
  /// <returns>
  /// Um valor inteiro que indica a relação entre os dois caminhos de arquivo:
  /// -   Se o primeiro caminho é maior que o segundo, retorna um número maior
  ///     que zero.
  /// -   Se o primeiro caminho é igual ao segundo, retorna zero.
  /// -   Se o primeiro caminho é menor que o segundo, retorna um número menor
  ///     que zero.
  /// </returns>
  public static int ComparePaths(string? path1, string? path2)
  {
    if (path1 == path2)
      return 0;

    if (path1 is null)
      return -1;

    if (path2 is null)
      return 1;

    var path1Parts = path1.Split('/');
    var path2Parts = path2.Split('/');

    for (int i = 0; i < Math.Min(path1Parts.Length, path2Parts.Length); i++)
    {
      var part1 = path1Parts[i];
      var part2 = path2Parts[i];

      if (part1 == part2)
        continue;

      if (part1.Contains('.') && !part2.Contains('.'))
        return -1;

      if (!part1.Contains('.') && part2.Contains('.'))
        return 1;

      return string.Compare(part1, part2, StringComparison.OrdinalIgnoreCase);
    }

    return path1Parts.Length.CompareTo(path2Parts.Length);
  }
}