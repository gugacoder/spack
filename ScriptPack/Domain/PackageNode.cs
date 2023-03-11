using System.Collections;
using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Representa um pacote de scripts.
/// </summary>
public class PackageNode : AbstractFileNode
{
  /// <summary>
  /// Cria uma nova instância da classe PackageNode.
  /// </summary>
  public PackageNode()
  {
    this.Scripts = new();
  }

  /// <summary>
  /// Define a ordem de execução dos scripts de migração de base de dados.
  /// </summary>
  public string Order { get; set; } = Orders.Auto;

  /// <summary>
  /// Obtém ou define a precedência do pacote em relação aos demais.
  /// </summary>
  public int Precedence { get; set; }

  /// <summary>
  /// Obtém ou define os nomes de configurações de conexões que devem ser
  /// utilizadas para execução do pacote.
  /// </summary>
  [JsonIgnore]
  public List<string> Connections { get; set; } = new();

  /// <summary>
  /// Obtém ou define os scripts do pacote.
  /// </summary>
  [JsonIgnore]
  public NodeList<ScriptNode> Scripts
  {
    get => Get<NodeList<ScriptNode>>();
    set => Set(value);
  }
}