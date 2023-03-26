using ScriptPack.Domain;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Implementa um visitante que detecta as dependências de um script.
/// </summary>
internal class DependencyDetectorVisitor : IAsyncVisitor
{
  private DependencyDetector _detector = new();

  /// <summary>
  /// Visita um nodo de script e detecta suas dependências.
  /// </summary>
  /// <param name="node">O nodo de script a ser visitado.</param>
  public async Task VisitAsync(ScriptNode node)
  {
    var catalog = node.Ancestor<CatalogNode>();
    var scripts = catalog?.Descendants<ScriptNode>()
        ?? throw new InvalidOperationException(
            "O nodo de script deve estar dentro de um nodo de catálogo");

    await _detector.DetectDependenciesAsync(node, scripts);
  }
}
