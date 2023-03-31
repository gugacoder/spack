using Newtonsoft.Json;

namespace ScriptPack.Domain;

/// <summary>
/// Representa um módulo de produto.
/// </summary>
public class ModuleNode : AbstractFileNode
{
  /// <summary>
  /// Inicializa uma nova instância da classe <see cref="ModuleNode"/>.
  /// </summary>
  public ModuleNode()
  {
    this.Modules = new();
    this.Packages = new();
  }

  /// <summary>
  /// Obtém ou define a precedência do módulo em relação aos demais.
  /// Deve ser um número entre -9999 e 9999.
  /// Precedência além destes valores é de uso exclusivo do sistema.
  /// </summary>
  public int Precedence { get; set; }

  /// <summary>
  /// Obtém ou define os sub-módulos.
  /// </summary>
  [JsonIgnore]
  public NodeList<ModuleNode> Modules
  {
    get => Get<NodeList<ModuleNode>>();
    set => Set(value);
  }

  /// <summary>
  /// Obtém ou define os pacotes do módulo.
  /// </summary>
  [JsonIgnore]
  public NodeList<PackageNode> Packages
  {
    get => Get<NodeList<PackageNode>>();
    set => Set(value);
  }
}
