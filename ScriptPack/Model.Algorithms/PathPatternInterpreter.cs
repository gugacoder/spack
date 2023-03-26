namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Utilitários de interpretação de caminhos de arquivos.
/// </summary>
internal class PathPatternInterpreter
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
    if (filePath is null) return null;
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
    var name = Path.GetFileNameWithoutExtension(filePath);

    if (IsRetroCompatibleName(name))
      return ExtractRetroCompatibleObjectNameAndTag(name);

    if (!name.StartsWith("-")) return (name, "");

    var tokens = name[1..].Split(' ', '-', ':', '.');
    var tag = $"-{tokens[0]}";
    var objectName = name[(tag.Length + 1)..].Trim();

    return (objectName, tag);
  }

  /// <summary>
  /// Identifica se o nome do arquivo é compatível com a versão anterior do
  /// ScriptPack.
  /// </summary>
  /// <param name="name">
  /// Nome do arquivo.
  /// </param>
  /// <returns>
  /// Verdadeiro se o nome do arquivo é compatível com a versão anterior do
  /// ScriptPack.
  /// </returns>
  /// <remarks>
  /// As regras de compatibilidade são:
  /// -   Pré-Script recebia o prefixo "pre-script"
  /// -   Pós-Script recebia o prefixo "pos-script"
  /// -   Pré-Transaction recebia o prefixo "pre-transaction"
  /// -   Pós-Transaction recebia o prefixo "pos-transaction"
  /// Era possível acrescentar um sufixo depois do prefixo, separado por um
  /// ponto, como:
  ///     pre-script.01-Create-Table.sql
  /// </remarks>
  /// <example>
  /// -   pre-script.01-Create-Table.sql
  /// -   pos-script.01-Create-Table.sql
  /// -   pre-transaction.01-Create-Table.sql
  /// -   pos-transaction.01-Create-Table.sql
  /// </example>
  private bool IsRetroCompatibleName(string name)
  {
    return name.StartsWith("pre-script.")
        || name.StartsWith("pos-script.")
        || name.StartsWith("pre-transaction.")
        || name.StartsWith("pos-transaction.");
  }

  /// <summary>
  /// Extrai do nome do caminho do arquivo um nome de objeto e um rótulo de
  /// agrupamento, se houver, para o caso de compatibilidade com a versão
  /// anterior do ScriptPack.
  /// </summary>
  /// <param name="name">
  /// Nome do arquivo.
  /// </param>
  /// <returns>
  /// Tupla com o nome do objeto e o rótulo de agrupamento.
  /// </returns>
  private (string ObjectName, string Tag)
      ExtractRetroCompatibleObjectNameAndTag(string name)
  {
    // Nomes sufixados:
    if (name.StartsWith("pre-script.sql")) return ("script", "-pre");
    if (name.StartsWith("pos-script.sql")) return ("script", "-pos");
    if (name.StartsWith("pre-transaction.sql")) return ("script", "-pretran");
    if (name.StartsWith("pos-transaction.sql")) return ("script", "-postran");
    // Nomes não sufixados:
    if (name.StartsWith("pre-script.")) return (name[11..], "-pre");
    if (name.StartsWith("pos-script.")) return (name[11..], "-pos");
    if (name.StartsWith("pre-transaction.")) return (name[16..], "-pretran");
    if (name.StartsWith("pos-transaction.")) return (name[16..], "-postran");

    throw new Exception("Nome de arquivo não compatível com a versão anterior.");
  }
}
