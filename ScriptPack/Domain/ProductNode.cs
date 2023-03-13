using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Representa um produto modificável por scripts.
/// </summary>
public class ProductNode : AbstractFileNode
{
  /// <summary>
  /// Cria uma nova instância da classe ProductNode.
  /// </summary>
  public ProductNode()
  {
    this.Versions = new();
  }

  /// <summary>
  /// Obtém ou define os módulos do produto.
  /// </summary>
  [JsonIgnore]
  public NodeList<VersionNode> Versions
  {
    get => Get<NodeList<VersionNode>>();
    set => Set(value);
  }
}