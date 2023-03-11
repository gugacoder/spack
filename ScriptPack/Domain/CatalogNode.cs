using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Representa um catálogo de produtos e seus scripts associados.
/// </summary>
public class CatalogNode : AbstractFileNode
{
  public CatalogNode()
  {
    this.Products = new NodeList<ProductNode>();
    this.Connections = new NodeList<ConnectionNode>();
  }

  /// <summary>
  /// As bases de dados utilizadas pelo sistema.
  /// </summary>
  [JsonIgnore]
  public NodeList<ConnectionNode> Connections
  {
    get => Get<NodeList<ConnectionNode>>();
    set => Set(value);
  }

  /// <summary>
  /// A lista de produtos disponíveis no catálogo.
  /// </summary>
  [JsonIgnore]
  public NodeList<ProductNode> Products
  {
    get => Get<NodeList<ProductNode>>();
    set => Set(value);
  }
}
