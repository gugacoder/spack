using Humanizer;
using ScriptPack.Domain;

namespace ScriptPack.Helpers;

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
      return "/index.repository";

    var parentPath = GetParentPath(node);

    if (node is ScriptNode scriptNode)
    {
      var prefix = string.IsNullOrEmpty(scriptNode.Tag)
          ? null
          : $"{scriptNode.Tag}:";
      return $"{parentPath}{prefix}{scriptNode.Name}.sql";
    }

    var name = string.IsNullOrEmpty(node.Name) ? "unnamed" : node.Name;
    var extension = node.GetType().Name[..^"Node".Length].Kebaberize();
    var path = $"{parentPath}{name}.{extension}";
    return path;
  }

  /// <summary>
  /// Obtém o caminho de pasta virtual do nodo pai.
  /// </summary>
  /// <param name="node">
  /// O nodo cujo caminho de pasta virtual do pai deve ser obtido.
  /// </param>
  /// <returns>
  /// O caminho de pasta virtual do nodo pai.
  /// </returns>
  public static string GetParentPath(INode node)
  {
    if (node.Parent == null) return "/";
    if (node.Parent.Path == "/") return "/";
    if (node.Parent is RepositoryNode) return "/";

    var parentPath = Path.GetDirectoryName(node.Parent.Path) ?? "";
    if (!parentPath.EndsWith("/")) parentPath = $"{parentPath}/";
    if (!parentPath.StartsWith("/")) parentPath = $"/{parentPath}";
    return $"{parentPath}{node.Parent.Name}/";
  }
}
