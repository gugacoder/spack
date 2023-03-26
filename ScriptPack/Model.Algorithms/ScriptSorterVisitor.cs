using ScriptPack.Domain;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Classe que representa um visitante que implementa a interface IVisitor e que
/// pode ser usado para classificar os scripts de um nodo de etapa usando um
/// classificador de script.
/// </summary>
internal class ScriptSorterVisitor : IVisitor
{
  private readonly IScriptSorter _sorter;

  /// <summary>
  /// Cria uma nova instância de ScriptSorterVisitor usando um novo
  /// classificador de script.
  /// </summary>
  public ScriptSorterVisitor()
  {
    _sorter = new ScriptSorter();
  }

  /// <summary>
  /// Cria uma nova instância de ScriptSorterVisitor usando o classificador de
  /// script especificado.
  /// </summary>
  /// <param name="sorter">O classificador de script a ser usado.</param>
  public ScriptSorterVisitor(IScriptSorter sorter)
  {
    _sorter = sorter;
  }

  /// <summary>
  /// Visita um nodo de etapa e classifica seus scripts usando o classificador
  /// de script especificado.
  /// </summary>
  /// <param name="step">O nodo de etapa a ser visitado.</param>
  public void Visit(StepNode step)
  {
    _sorter.SortScripts(step.Scripts);
  }
}
