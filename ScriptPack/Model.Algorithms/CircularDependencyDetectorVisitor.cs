using ScriptPack.Domain;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Implementação da interface IVisitor para detectar dependências circulares em
/// um ScriptNode.
/// </summary>
internal class CircularDependencyDetectorVisitor : IVisitor
{
  private readonly CircularDependencyDetector _detector = new();

  /// <summary>
  /// Executa a detecção de dependências circulares no ScriptNode fornecido.
  /// </summary>
  /// <param name="node">O ScriptNode a ser visitado.</param>
  public void Visit(ScriptNode node)
  {
    _detector.DetectCircularDependencies(node);
  }
}