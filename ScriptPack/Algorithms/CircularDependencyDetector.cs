using ScriptPack.Domain;

namespace ScriptPack.Algorithms;

/// <summary>
/// Classe responsável por detectar dependências circulares entre scripts.
/// </summary>
public class CircularDependencyDetector
{
  /// <summary>
  /// Função que determina se um script está dentro do escopo de análise.
  /// </summary>
  public Func<ScriptNode, bool> IsInScopeFunction { get; set; } = _ => true;

  /// <summary>
  /// Método que detecta dependências circulares a partir de um script.
  /// </summary>
  /// <param name="script">
  /// ScriptNode que representa o script a ser analisado.
  /// </param>
  public void DetectCircularDependencies(ScriptNode script)
  {
    var dependencies = new List<ScriptNode>();
    CollectDependencyTree(script, dependencies);

    foreach (var dependency in dependencies)
    {
      if (!dependency.Dependencies.Contains(script))
        continue;

      script.Faults.Add(Fault.EmitCircularDependency(script, dependency));
    }
  }

  /// <summary>
  /// Método auxiliar que coleta a árvore de dependências de um script.
  /// </summary>
  /// <param name="script">
  /// ScriptNode que representa o script a ser analisado.
  /// </param>
  /// <param name="tree">
  /// Lista de ScriptNode que representa a árvore de dependências coletada.
  /// </param>
  private void CollectDependencyTree(ScriptNode script, List<ScriptNode> tree)
  {
    var dependencies = script.Dependencies.Where(IsInScopeFunction);
    foreach (var dependency in dependencies)
    {
      if (tree.Contains(dependency))
        continue;

      tree.Add(dependency);

      CollectDependencyTree(dependency, tree);
    }
  }
}
