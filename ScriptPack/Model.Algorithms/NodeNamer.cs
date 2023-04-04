using ScriptPack.Domain;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Utilitário para determinação automática de nomes nodos.
/// 
/// Em geral o nome do nodo é definido no seu arquivo *.jsonc, mas, quando
/// o arquivo não existe, o nome do nodo é determinado automaticamente.
/// </summary>
public class NodeNamer
{

  /// <summary>
  /// Algoritmo de nomeação automática de nodos.
  /// </summary>
  /// <remarks>
  /// Em geral, o nome do nodo é definido pelo próprio arquivo de configuração
  /// usado no carregamento do nodo, como -package.jsonc, -module.jsonc, etc.
  /// Quando o arquivo de configuração não existe, o nome do nodo é definido
  /// de forma automática para obter a melhor definição possível.
  /// </remarks>
  /// <param name="node">
  /// O nodo que está sendo nomeado.
  /// </param>
  /// <param name="filePath">
  /// O caminho do arquivo de configuração do nodo.
  /// </param>
  /// <returns>
  /// O nome do nodo.
  /// </returns>
  public string NameNode(IFileNode node, string filePath)
  {
    // Pacotes recebem o nome "Package-INDICE", sendo INDICE o índice do
    // pacote na lista de pacotes do catálogo.
    if (node is PackageNode)
    {
      if (filePath.EndsWith("/-package.jsonc"))
      {
        return Path.GetFileName(Path.GetDirectoryName(filePath))!;
      }

      int index = node.Parent!.Children()
          .Select((item, index) => new { item, index })
          .FirstOrDefault(x => x.item == node)?.index ?? -1;

      return (index == 0) ? "Package" : $"Package-{index}";
    }

    if (node is ModuleNode)
    {
      if (filePath.EndsWith("/-module.jsonc"))
      {
        return Path.GetFileName(Path.GetDirectoryName(filePath))!;
      }

      int index = node.Parent!.Children()
          .Select((item, index) => new { item, index })
          .FirstOrDefault(x => x.item == node)?.index ?? -1;

      return (index == 0) ? "Module" : $"Module-{index}";
    }

    if (node is VersionNode version)
    {
      return !string.IsNullOrWhiteSpace(version.Version)
          ? version.Version
          : VersionNode.UnidentifiedVersion;
    }

    if (node is ProductNode)
    {
      if (filePath.EndsWith("/-product.jsonc"))
      {
        var name = Path.GetFileName(Path.GetDirectoryName(filePath))!;
        if (name != "trunk" && name != "branches" && name != "tags")
        {
          return name;
        }
      }

      var folder = Path.GetDirectoryName(filePath)!;
      var tokens = folder
          .Split('/')
          .Where(x => !string.IsNullOrWhiteSpace(x))
          .ToArray();
      // .Select(x => x.Pasca);

      var productName = string.Join(".", tokens);
      return productName;
    }

    return Path.GetFileName(Path.GetDirectoryName(filePath))!;
  }
}
