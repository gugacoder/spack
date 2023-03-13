using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Classe abstrata que representa um nodo que possui um arquivo associado.
/// </summary>
public abstract class AbstractFileNode : AbstractNode, IFileNode
{
  /// <summary>
  /// A descrição do nodo.
  /// </summary>
  public virtual string? Description { get; set; }

  /// <summary>
  /// O caminho relativo do arquivo referente.
  /// </summary>
  [JsonIgnore]
  public virtual string? FilePath { get; set; }

  /// <summary>
  /// O caminho relativo do arquivo referente.
  /// </summary>
  [JsonIgnore]
  public virtual string? FileFolder
      => FilePath != null ? System.IO.Path.GetDirectoryName(FilePath) : null;
}
