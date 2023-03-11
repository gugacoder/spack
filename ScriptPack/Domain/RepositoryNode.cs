using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Representa um catálogo de produtos e seus scripts.
/// </summary>
public class RepositoryNode : AbstractNode
{
  /// <summary>
  /// Cria uma nova instância da classe RepositoryNode.
  /// </summary>
  public RepositoryNode()
  {
    this.Catalogs = new();
  }

  /// <summary>
  /// Obtém ou define a lista de produtos disponíveis no catálogo.
  /// </summary>
  [JsonIgnore]
  public NodeList<CatalogNode> Catalogs
  {
    get => Get<NodeList<CatalogNode>>();
    set => Set(value);
  }
}