using ScriptPack.Domain;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Classe responsável por ordenar scripts por dependência.
/// </summary>
internal class ScriptSorter : IScriptSorter
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
  /// Compara dois scripts segundo a ordem de comparação definida em seus
  /// pacotes.
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
  /// <remarks>
  /// Se qualquer dos pacotes de a e b tiver a propriedade Order definida como
  /// Alpha, a comparação será feita pelo nome dos scripts. Caso contrário, a
  /// comparação será feita por dependência.
  /// </remarks>
  /// <exception cref="FaultException">
  /// É lançada uma exceção em caso de dependência cíclica.
  /// </exception>
  private int CompareScripts(ScriptNode a, ScriptNode b)
  {
    var byName =
        a.Ancestor<PackageNode>()?.Order == Orders.Alpha
        || b.Ancestor<PackageNode>()?.Order == Orders.Alpha;
    return byName
        ? ScriptSorterByName.CompareScripts(a, b)
        : ScriptSorterByDependency.CompareScripts(a, b);
  }
}