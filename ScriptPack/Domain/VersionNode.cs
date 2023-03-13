using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Representa uma versão de um produto.
/// </summary>
public class VersionNode : AbstractFileNode
{
  /// <summary>
  /// Cria uma nova instância da classe VersionNode.
  /// </summary>
  public VersionNode()
  {
    this.Modules = new();
  }

  /// <summary>
  /// Obtém ou define a versão do produto segundo o padrão semântico.
  /// Referência: https://semver.org/
  /// </summary>
  public string Version { get; set; } = string.Empty;

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