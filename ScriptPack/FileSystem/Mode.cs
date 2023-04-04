namespace ScriptPack.FileSystem;

/// <summary>
/// Modos de abertura de arquivo zip.
/// </summary>
public enum Mode
{
  /// <summary>
  /// Abre o arquivo zip em modo somente leitura.
  /// </summary>
  Read,
  /// <summary>
  /// Abre o arquivo zip para leitura e gravação.
  /// </summary>
  Writable,
  /// <summary>
  /// Abre o arquivo zip para leitura e gravação, sobrescrevendo o arquivo
  /// existente.
  /// </summary>
  Overwrite
};