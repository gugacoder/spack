using System.IO.Compression;
using System.Text;
using ScriptPack.Domain;

namespace ScriptPack.FileSystem;

/// <summary>
/// Utilitário para fabricação de drives para navegação em arquivos.
/// </summary>
public static class DriveFactory
{
  /// <summary>
  /// Obtém uma instância de IDrive baseado no caminho especificado.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório ou arquivo para o qual será criado um IDrive.
  /// </param>
  /// <param name="password">
  /// Senha para descriptografar o arquivo ZIP, se for o caso.
  /// </param>
  /// <returns>
  /// Instância de IDrive correspondente ao caminho especificado.
  /// </returns>
  /// <exception cref="NotSupportedException">
  /// Se o tipo de drive correspondente ao caminho especificado não é suportado.
  /// </exception>
  public static IDrive CreateDrive(string path, string? password)
  {
    path = Path.GetFullPath(path);

    if (Directory.Exists(path))
    {
      return new FileDrive(path);
    }

    if (ZipDrive.IsZipFile(path, password))
    {
      return new ZipDrive(path, password);
    }

    // Se for um dos arquivos JSON suportados podemos criar um Drive para
    // navegação em seu diretório.
    if (Path.GetExtension(path) == ".jsonc")
    {
      var folder = Path.GetDirectoryName(path)!;
      return new FileDrive(folder);
    }

    throw new NotSupportedException($"Drive not supported: {path}");
  }
}
