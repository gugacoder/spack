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
    scripts.Sort((a, b) => a.Name.CompareTo(b.Name));
  }
}
