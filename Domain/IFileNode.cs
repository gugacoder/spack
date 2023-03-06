namespace SPack.Domain;

public interface IFileNode : IMetaNode
{
  /// <summary>
  /// Nome do nó.
  /// </summary>
  string Name { get; set; }

  /// <summary>
  /// Caminho virtual do nodo dentro da árvore de nodos.
  /// </summary>
  string Path { get; }

  /// <summary>
  /// Nome do nó.
  /// </summary>
  string? Description { get; set; }

  /// <summary>
  /// Caminho relativo do arquivo referente.
  /// </summary>
  string? FilePath { get; set; }

  /// <summary>
  /// Indica se a seção está habilitada.
  /// Se estiver desabilitada, o conteúdo da seção não será executado.
  /// </summary>
  bool Enabled { get; set; }
}
