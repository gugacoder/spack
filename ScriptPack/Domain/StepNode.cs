namespace ScriptPack.Domain;

/// <summary>
/// Bloco de execução de scripts dentro de um pipeline.
/// </summary>
public class StepNode : AbstractNode, IPipeNode
{
  public StepNode()
  {
    this.Scripts = new();
  }

  /// <summary>
  /// Rótulo de filtragem de scripts incluídos na fila.
  /// </summary>
  public string Tag { get; set; } = "";

  /// <summary>
  /// Precedência de execução do estágio em relação aos demais que compartilham o mesmo rótulo.
  /// </summary>
  public int Precedence { get; set; }

  /// <summary>
  /// Determina se a execução dos scripts devem ser feita em uma transação quando disponível.
  /// </summary>
  public bool Transactional { get; set; } = true;

  /// <summary>
  /// Sequência de scripts a serem executados.
  /// </summary>
  /// <remarks>
  /// Cada script é definido pela classe ScriptNode e é adicionado à lista de scripts
  /// através do método Add() da classe List.
  /// </remarks>
  public List<ScriptNode> Scripts { get; set; } = new();
}
