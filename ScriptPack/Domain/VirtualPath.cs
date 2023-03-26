using System.Text;
using Humanizer;
using ScriptPack.Domain;

namespace ScriptPack.Domain;

/// <summary>
/// Utilitário para manipulação de caminhos virtuais da árvore de nodos do
/// ScriptPack.
/// </summary>
public static class VirtualPath
{
  /// <summary>
  /// Cria o caminho virtual de um nodo.
  /// </summary>
  /// <param name="node">
  /// O nodo cujo caminho virtual deve ser criado.
  /// </param>
  /// <returns>
  /// O caminho virtual do nodo.
  /// </returns>
  public static string CreateNodePath(INode node)
  {
    if (node is RepositoryNode)
      return "/repository.index";

    var stringBuilder = new StringBuilder();

    foreach (var item in node.AncestorsAndSelf<IFileNode>())
    {
      if (item is ScriptNode) continue;

      var itemName = string.IsNullOrEmpty(item.Name) ? "(unnamed)" : item.Name;
      stringBuilder.Insert(0, $"/{itemName}");
    }

    var folder = stringBuilder.ToString();
    var filename = "";

    if (node is ScriptNode script)
    {
      filename = Path.GetFileName(script.FilePath);
    }
    else
    {
      var nodeName = node.GetType().Name[..^"Node".Length].Kebaberize();
      filename = $"{nodeName}.json";
    }

    var path = $"{folder}/{filename}";
    return path;
  }
}
