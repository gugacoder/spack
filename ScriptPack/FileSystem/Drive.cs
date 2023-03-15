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
    path = Path.GetFullPath(path);

    if (Directory.Exists(path))
      return new FileDrive(path);

    if (IsZipFile(path))
      return new ZipDrive(path);

    // Se for um dos arquivos JSON suportados podemos criar um Drive para
    // navegação em seu diretório.
    var filename = Path.GetFileName(path);
    if (filename == "catalog.json"
        || filename == "product.json"
        || filename == "module.json"
        || filename == "package.json")
    {
      var folder = Path.GetDirectoryName(path)!;
      return new FileDrive(folder);
    }

    throw new NotSupportedException($"Drive not supported: {path}");
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
