namespace ScriptPack.Algorithms;

/// <summary>
/// Utilitários de interpretação de caminhos de arquivos.
/// </summary>
public class PathPatternInterpreter
{
  /// <summary>
  /// Identifica o sufixo de versão a partir do caminho de arquivo.
  /// O sufixo em geral pode ser:
  /// -   trunk     Indica uma versão em desenvolvimento.
  /// -   latest    Indica a última versão disponível.
  /// -   branch    Indica uma ramificação da versão em estudo ou em correção.
  /// </summary>
  /// <param name="filePath">
  /// Caminho de arquivo.
  /// </param>
  /// <returns>
  /// Rótulo de versão do arquivo.
  /// </returns>
  public string? ExtractVersionTag(string filePath)
  {
    if (filePath == null) return null;
    var tokens = filePath.Split('/');
    foreach (var token in tokens.Reverse())
    {
      if (token == "trunk" || token == "latest") return token;
      if (token == "branches") return "branch";
    }
    return null;
  }

  /// <summary>
  /// Extrai do nome do caminho do arquivo um nome de objeto e um rótulo de
  /// agrupamento, se houver.
  /// </summary>
  /// <remarks>
  /// O rótulo de agrupamento é um prefixo que modifica sua ordem de execução.
  /// 
  /// Em geral, um de:
  /// -   -pretran  Indica que o script deve ser executado antes da transação.
  /// -   -pre      Indica que o script deve ser executado antes dos demais.
  /// -   -pos      Indica que o script deve ser executado após os demais.
  /// -   -postran  Indica que o script deve ser executado após a transação.
  /// 
  /// Por exemplo, o script:
  /// -   -pretran-01-Create-Table.sql
  /// 
  /// Será extraído como:
  /// -   ObjectName: 01-Create-Table
  /// -   Tag: -pretran
  /// </remarks>
  /// <param name="filePath">
  /// Caminho de arquivo.
  /// </param>
  /// <returns>
  /// Tupla com o nome do objeto e o rótulo de agrupamento.
  /// </returns>
  public (string ObjectName, string Tag) ExtractObjectNameAndTag(string filePath)
  {
    var fileName = Path.GetFileNameWithoutExtension(filePath);
    if (!fileName.StartsWith("-")) return (fileName, "");

    var tokens = fileName[1..].Split(' ', '-', ':', '.');
    var tag = $"-{tokens[0]}";
    var objectName = fileName[(tag.Length + 1)..].Trim();

    return (objectName, tag);
  }
}
