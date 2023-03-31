using Newtonsoft.Json;

namespace ScriptPack.Domain;

/// <summary>
/// Bloco de execução de scripts dentro de um pipeline.
/// </summary>
public class StageNode : AbstractNode, IPipeNode
{
  public StageNode()
  {
    this.Steps = new();
  }

  /// <summary>
  /// Sequência de passos a serem executados.
  /// </summary>
  [JsonIgnore]
  public NodeList<StepNode> Steps
  {
    get => Get<NodeList<StepNode>>();
    set => Set(value);
  }
}