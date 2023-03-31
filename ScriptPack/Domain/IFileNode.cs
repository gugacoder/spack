using Newtonsoft.Json;

/// <summary>
/// Representa um nodo em uma hierarquia de arquivos que pode ser processado por
/// um mecanismo de script.
/// </summary>
namespace ScriptPack.Domain;

public interface IFileNode : INode
{
  /// <summary>
  /// Obtém ou define o caminho relativo do arquivo referente.
  /// </summary>
  [JsonIgnore]
  string? FilePath { get; set; }

  /// <summary>
  /// Pasta onde se encontra o arquivo referente. 
  /// </summary>
  [JsonIgnore]
  string? FileFolder { get; }
}