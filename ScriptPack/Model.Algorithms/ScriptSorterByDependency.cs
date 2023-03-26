using ScriptPack.Domain;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Classe responsável por ordenar scripts por dependência.
/// </summary>
internal class ScriptSorterByDependency : IScriptSorter
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
    scripts.Sort(CompareScripts);
  }

  /// <summary>
  /// Compara dois scripts por suas dependências.
  /// </summary>
  /// <param name="a">
  /// O primeiro script a ser comparado.
  /// </param>
  /// <param name="b">
  /// O segundo script a ser comparado.
  /// </param>
  /// <returns>
  /// Um inteiro que indica a ordem dos scripts.
  /// 0: Os scripts são iguais.
  /// 1: O script a é maior que o script b.
  /// -1: O script a é menor que o script b.
  /// </returns>
  /// <exception cref="FaultException">
  /// É lançada uma exceção em caso de dependência cíclica.
  /// </exception>
  public static int CompareScripts(ScriptNode a, ScriptNode b)
  {
    if (a.Dependencies.Contains(b) && b.Dependencies.Contains(a))
      throw new FaultException(Fault.EmitCircularDependency(a, b));

    if (a.Dependencies.Contains(b)) return 1;
    if (b.Dependencies.Contains(a)) return -1;
    return 0;
  }
}