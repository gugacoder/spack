using Newtonsoft.Json;
using ScriptPack.Helpers;

namespace ScriptPack.Domain;

/// <summary>
/// Representa uma versão de um produto.
/// </summary>
public class VersionNode : AbstractFileNode
{
  /// <summary>
  /// Versão padrão para versões não identificadas.
  /// </summary>
  public const string UnidentifiedVersion = "no-version";

  /// <summary>
  /// Cria uma nova instância da classe VersionNode.
  /// </summary>
  public VersionNode()
  {
    this.Modules = new();
    this.Packages = new();
  }

  /// <summary>
  /// Obtém ou define a versão do produto segundo o padrão semântico.
  /// Referência: https://semver.org/
  /// </summary>
  public string Version { get; set; } = string.Empty;

  /// <summary>
  /// Obtém ou define os módulos da versão.
  /// </summary>
  [JsonIgnore]
  public NodeList<ModuleNode> Modules
  {
    get => Get<NodeList<ModuleNode>>();
    set => Set(value);
  }

  /// <summary>
  /// Obtém ou define os pacotes da versão.
  /// </summary>
  [JsonIgnore]
  public NodeList<PackageNode> Packages
  {
    get => Get<NodeList<PackageNode>>();
    set => Set(value);
  }
}