using System.IO.Compression;
using System.Text.RegularExpressions;
namespace SPack.Library;

public static class Drive
{
  public static IDrive Get(string path)
  {
    if (IsZipFile(path))
      return new ZipDrive(path);

    if (IsDirectory(path))
      return new FileDrive(path);

    throw new NotSupportedException($"Drive not supported: {path}");
  }

  private static bool IsDirectory(string path)
  {
    return Directory.Exists(path);
  }

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
