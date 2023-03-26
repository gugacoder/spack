using System.IO.Compression;
using ScriptPack.Domain;

namespace ScriptPack.FileSystem;

public static class DriveExtensions
{
  /// <summary>
  /// Obtém um leitor de texto para o arquivo especificado.
  /// </summary>
  /// <param name="drive">
  /// Instância de IDrive.
  /// </param>
  /// <param name="script">
  /// Instância de ScriptNode.
  /// </param>
  /// <returns>
  /// O leitor de texto para o arquivo especificado.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// O script não possui um arquivo associado.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// O script não possui um catálogo associado.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// O catálogo não possui um drive associado.
  /// </exception>
  /// <exception cref="FileNotFoundException">
  /// O arquivo não existe.
  /// </exception>
  public static async Task<TextReader> ReadScriptFileAsync(
      this ScriptNode script)
  {
    var filePath = script.FilePath
        ?? throw new InvalidOperationException(
            $"O script não possui um arquivo associado: {script.Name}");

    var catalog = script.Ancestor<CatalogNode>()
        ?? throw new InvalidOperationException(
            $"O script não possui um catálogo associado: {script.Name}");

    var drive = script.Ancestor<CatalogNode>()?.Drive
        ?? throw new InvalidOperationException(
            $"O catálogo não possui um drive associado: {catalog.Name}");

    if (!drive.FileExists(script.FilePath))
      throw new FileNotFoundException(
          $"O arquivo não existe: {script.FilePath}");

    var encoding = catalog.Encoding ?? Drive.DefaultEncoding;

    return await drive.ReadFileAsync(script.FilePath, encoding);
  }
}
