using ScriptPack.Domain;

namespace ScriptPack.Algorithms;

/// <summary>
/// Classe responsável por ordenar scripts por dependência.
/// </summary>
public class ScriptSorterByDependency : IScriptSorter
{
  /// <summary>
  /// Ordena a lista de scripts com base em suas dependências.
  /// </summary>
  /// <param name="scripts">
  /// A lista de ScriptNode a ser ordenada.
  /// </param>
  /// <exception cref="FaultException">
  /// É lançada uma exceção em caso de dependência cíclica.
  /// </exception>
  public void SortScripts(List<ScriptNode> scripts)
  {
    scripts.Sort((a, b) =>
    {
      if (a.Dependencies.Contains(b) && b.Dependencies.Contains(a))
        throw new FaultException(Fault.EmitCircularDependency(a, b));

      if (a.Dependencies.Contains(b)) return 1;
      if (b.Dependencies.Contains(a)) return -1;
      return 0;
    });
  }
}