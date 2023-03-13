using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Representa uma sequência dos scripts selecionados para execução.
/// </summary>
public class PipelineNode : AbstractNode
{
  /// <summary>
  /// Cria uma nova instância da classe PipelineNode.
  /// </summary>
  public PipelineNode()
  {
    this.Stages = new();
  }

  /// <summary>
  /// Obtém ou define a conexão utilizada na execução dos scripts do pipeline.
  /// </summary>
  [JsonIgnore]
  public ConnectionNode Connection { get; set; } = null!;

  /// <summary>
  /// Obtém ou define os blocos de scripts executados em sequência dentro do
  /// pipeline.
  /// </summary>
  [JsonIgnore]
  public NodeList<StageNode> Stages
  {
    get => Get<NodeList<StageNode>>();
    set => Set(value);
  }
}