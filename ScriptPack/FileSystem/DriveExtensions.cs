using System.IO.Compression;
using ScriptPack.Domain;

namespace ScriptPack.FileSystem;

public static class DriveExtensions
{
  /// <summary>
  /// Obtém uma instância de Stream para o arquivo especificado.
  /// </summary>
  /// <param name="drive">
  /// Instância de IDrive.
  /// </param>
  /// <param name="script">
  /// Instância de ScriptNode.
  /// </param>
  /// <returns>
  /// Instância de Stream para o arquivo especificado.
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
  public static Stream OpenScriptFile(this ScriptNode script)
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

    return drive.OpenFile(script.FilePath);
  }

  /// <summary>
  /// Obtém uma instância de IDrive baseado no caminho especificado.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório ou arquivo para o qual será criado um IDrive.
  /// </param>
  /// <returns>
  /// Instância de IDrive correspondente ao caminho especificado.
  /// </returns>
  /// <exception cref="NotSupportedException">
  /// Se o tipo de drive correspondente ao caminho especificado não é suportado.
  /// </exception>
  public static IDrive Get(string path)
  {
    if (IsDirectory(path))
      return new FileDrive(path);

    if (IsZipFile(path))
      return new ZipDrive(path);

    throw new NotSupportedException($"Drive not supported: {path}");
  }

  /// <summary>
  /// Verifica se o caminho especificado corresponde a um diretório.
  /// </summary>
  /// <param name="path">Caminho para ser verificado.</param>
  /// <returns>
  /// True se o caminho corresponder a um diretório, caso contrário, False.
  /// </returns>
  private static bool IsDirectory(string path)
  {
    return Directory.Exists(path);
  }

  /// <summary>
  /// Verifica se o caminho especificado corresponde a um arquivo ZIP válido.
  /// </summary>
  /// <param name="path">Caminho para ser verificado.</param>
  /// <returns>
  /// True se o caminho corresponder a um arquivo ZIP válido,
  /// caso contrário, False.
  /// </returns>
  private static bool IsZipFile(string path)
  {
    if (!File.Exists(path)) return false;
    try
    {
      using var archive = ZipFile.Open(path, ZipArchiveMode.Read);
      return true;
    }
    catch (Exception)
    {
      return false;
    }
  }
}
