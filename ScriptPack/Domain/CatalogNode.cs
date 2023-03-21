using System.Text.Json.Serialization;
using ScriptPack.FileSystem;
using ScriptPack.Model;

namespace ScriptPack.Domain;

/// <summary>
/// Representa um catálogo de produtos e seus scripts associados.
/// </summary>
public class CatalogNode : AbstractFileNode
{
  public CatalogNode()
  {
    this.Connections = new NodeList<ConnectionNode>();
    this.Products = new NodeList<ProductNode>();
  }

  /// <summary>
  /// O driver de acesso ao sistema de arquivos do catálogo.
  /// </summary>
  [JsonIgnore]
  public IDrive? Drive { get; set; }

  /// <summary>
  /// As bases de dados utilizadas pelo sistema.
  /// </summary>
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
