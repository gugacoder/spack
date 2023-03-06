using System.Data.SqlTypes;
using System.Text;

namespace SPack.Library;

public class FileDrive : IDrive
{
  public FileDrive(string directory)
  {
    this.Directory = directory.Replace('\\', '/');
  }

  /// <summary>
  /// Nome de identificação do drive.
  /// </summary>
  public string Name => Path.GetFileName(Directory);

  /// <summary>
  /// Determina se o drive é somente leitura.
  /// </summary>
  public bool ReadOnly => false;

  public string Directory { get; }

  private string GetPath(string path)
  {
    path = path.Replace('\\', '/');
    if (path.StartsWith('/')) path = path[1..];
    path = Path.Combine(Directory, path);
    return path;
  }

  private string[] MakeRelative(string[] paths)
  {
    for (var i = 0; i < paths.Length; i++)
    {
      var item = paths[i];
      item = item.Replace('\\', '/');
      item = item[(Directory.Length)..];
      paths[i] = item;
    }
    return paths;
  }

  private string[] RemoveHiddenPaths(string[] paths)
  {
    return paths.Where(p => !p.Split('/').Any(s => s.StartsWith('.'))).ToArray();
  }

  public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
  {
    path = GetPath(path);
    var items = System.IO.Directory.GetDirectories(path, searchPattern, searchOption);
    return RemoveHiddenPaths(MakeRelative(items));
  }

  public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
  {
    path = GetPath(path);
    var items = System.IO.Directory.GetFiles(path, searchPattern, searchOption);
    return RemoveHiddenPaths(MakeRelative(items));
  }

  public bool DirectoryExists(string path)
  {
    path = GetPath(path);
    return System.IO.Directory.Exists(path);
  }

  public bool FileExists(string path)
  {
    path = GetPath(path);
    return File.Exists(path);
  }

  public void DeleteDirectory(string path)
  {
    path = GetPath(path);
    System.IO.Directory.Delete(path, true);
  }

  public void DeleteFile(string path)
  {
    path = GetPath(path);
    File.Delete(path);
  }

  public Stream OpenFile(string path)
  {
    path = GetPath(path);
    return File.OpenRead(path);
  }

  public TextReader ReadFile(string path, Encoding? encoding = null)
  {
    path = GetPath(path);
    encoding ??= Encoding.UTF8;
    return new StreamReader(path, encoding);
  }

  public Task<string> ReadAllTextAsync(string path, Encoding? encoding = null)
  {
    path = GetPath(path);
    encoding ??= Encoding.UTF8;
    return File.ReadAllTextAsync(path, encoding);
  }

  public async Task WriteFileAsync(string path, Stream stream)
  {
    path = GetPath(path);
    using var output = File.OpenWrite(path);
    await stream.CopyToAsync(output);
    await output.FlushAsync();
  }

  public Task WriteFileAsync(string path, TextReader reader, Encoding? encoding = null)
  {
    path = GetPath(path);
    encoding ??= Encoding.UTF8;
    return File.WriteAllTextAsync(path, reader.ReadToEnd(), encoding);
  }

  public Task WriteAllTextAsync(string path, string text, Encoding? encoding = null)
  {
    path = GetPath(path);
    encoding ??= Encoding.UTF8;
    return File.WriteAllTextAsync(path, text, encoding);
  }
}
