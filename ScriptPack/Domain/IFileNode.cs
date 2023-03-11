/// <summary>
/// Representa um nodo em uma hierarquia de arquivos que pode ser processado por
/// um mecanismo de script.
/// </summary>
namespace ScriptPack.Domain;

public interface IFileNode : INode
{
  /// <summary>
  /// Obtém ou define a descrição do nodo do arquivo.
  /// </summary>
  string? Description { get; set; }

  /// <summary>
  /// Obtém ou define um valor que indica se o nodo do arquivo está habilitado.
  /// Se estiver desabilitado, o conteúdo do nodo não será executado.
  /// </summary>
  bool Enabled { get; set; }

  /// <summary>
  /// Obtém ou define o caminho relativo do arquivo referente.
  /// </summary>
  string? FilePath { get; set; }

  /// <summary>
  /// Pasta onde se encontra o arquivo referente. 
  /// </summary>
  string? FileFolder { get; }
}