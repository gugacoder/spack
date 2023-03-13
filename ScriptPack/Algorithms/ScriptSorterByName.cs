using ScriptPack.Domain;

namespace ScriptPack.Algorithms;

/// <summary>
/// Classe que implementa a interface IScriptSorter e é responsável por ordenar
/// scripts pelo nome.
/// </summary>
public class ScriptSorterByName : IScriptSorter
{
  /// <summary>
  /// Método que ordena a lista de scripts pelo seu nome.
  /// </summary>
  /// <param name="scripts">Lista de ScriptNode a ser ordenada.</param>
  public void SortScripts(List<ScriptNode> scripts)
  {
    scripts.Sort(CompareScripts);
  }

  /// <summary>
  /// Compara dois scripts pelo seu nome.
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
  public static int CompareScripts(ScriptNode a, ScriptNode b)
  {
    return a.Name.CompareTo(b.Name);
  }
}
