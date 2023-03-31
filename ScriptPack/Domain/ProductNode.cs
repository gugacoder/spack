using Newtonsoft.Json;

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
  /// Obtém ou define a precedência do produto em relação aos demais.
  /// Deve ser um número entre -9999 e 9999.
  /// Precedência além destes valores é de uso exclusivo do sistema.
  /// </summary>
  public int Precedence { get; set; }

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