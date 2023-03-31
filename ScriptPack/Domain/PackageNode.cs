using System.Collections;
using Newtonsoft.Json;

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
  public string Order { get; set; } = Orders.Dependency;

  /// <summary>
  /// Obtém ou define a precedência do pacote em relação aos demais.
  /// Deve ser um número entre -9999 e 9999.
  /// Precedência além destes valores é de uso exclusivo do sistema.
  /// </summary>
  public int Precedence { get; set; }

  /// <summary>
  /// Obtém ou define os nomes de configurações de conexões que devem ser
  /// utilizadas para execução do pacote.
  /// </summary>
  public List<string> TargetConnections { get; set; } = new();

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