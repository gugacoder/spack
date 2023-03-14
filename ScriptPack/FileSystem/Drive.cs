using System.IO.Compression;
using ScriptPack.Domain;

namespace ScriptPack.FileSystem;

public static class Drive
{
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
  public static IDrive GetDrive(string path)
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
