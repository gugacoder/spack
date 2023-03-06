namespace SPack.Model.Algorithms;

/// <summary>
/// Utilitários de interpretação de caminhos de arquivos.
/// </summary>
public static class FilePathParser
{
  /// <summary>
  /// Identifica o sufixo de versão de um caminho de arquivo.
  /// </summary>
  /// <param name="filePath">
  /// Caminho de arquivo.
  /// </param>
  /// <returns>
  /// Rótulo de versão do arquivo.
  /// </returns>
  public static string? ParseVersionTag(string filePath)
  {
    if (filePath == null) return null;
    var tokens = filePath.Split('/');
    foreach (var token in tokens.Reverse())
    {
      if (token == "trunk" || token == "branches" || token == "latest")
        return token;
    }
    return null;
  }

  /// <summary>
  /// Identifica o nome do objeto e o rótulo de agrupamento de um caminho de arquivo.
  /// </summary>
  /// <param name="filePath">
  /// Caminho de arquivo.
  /// </param>
  /// <returns>
  /// Tupla com o nome do objeto e o rótulo de agrupamento.
  /// </returns>
  public static (string ObjectName, string Tag) ParseScriptTag(string filePath)
  {
    var fileName = Path.GetFileNameWithoutExtension(filePath);
    if (!fileName.StartsWith("-")) return (fileName, string.Empty);

    var tokens = fileName[1..].Split(' ', '-', ':', '.');
    var tag = $"-{tokens[0]}";
    var objectName = fileName[(tag.Length + 1)..].Trim();

    return (objectName, tag);
  }
}
