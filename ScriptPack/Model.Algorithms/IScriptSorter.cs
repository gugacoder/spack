using ScriptPack.Domain;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Interface responsável por definir o contrato para ordenação de scripts.
/// </summary>
public interface IScriptSorter
{
  /// <summary>
  /// Ordena uma lista de ScriptNode.
  /// </summary>
  /// <param name="scripts">Lista de ScriptNode a ser ordenada.</param>
  void SortScripts(List<ScriptNode> scripts);
}