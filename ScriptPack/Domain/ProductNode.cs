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
    this.Modules = new();
  }

  /// <summary>
  /// O caminho virtual do nodo dentro da árvore de nodos.
  /// </summary>
  [JsonIgnore]
  public override string Path => $"{Parent?.Path}/{Name}/{Version}";

  /// <summary>
  /// Obtém ou define a versão do produto segundo o padrão semântico.
  /// Referência: https://semver.org/
  /// </summary>
  public string Version { get; set; } = "";

  /// <summary>
  /// Obtém ou define os módulos do produto.
  /// </summary>
  [JsonIgnore]
  public NodeList<ModuleNode> Modules
  {
    get => Get<NodeList<ModuleNode>>();
    set => Set(value);
  }
}