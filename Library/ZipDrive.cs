using System.IO.Compression;
using System.Text;

namespace SPack.Library;

public class ZipDrive : IDrive
{
  public ZipDrive(string filepath)
  {
    this.ZipFile = filepath;
    if (!File.Exists(this.ZipFile))
    {
      using var zip = System.IO.Compression.ZipFile.Open(this.ZipFile, ZipArchiveMode.Create);
    }
  }

  /// <summary>
  /// Nome de identificação do drive.
  /// </summary>
  public string Name => Path.GetFileNameWithoutExtension(ZipFile);

  /// <summary>
  /// Determina se o drive é somente leitura.
  /// </summary>
  public bool ReadOnly => true;

  public string ZipFile { get; private set; }

  private string GetPath(string path)
  {
    if (path.StartsWith('/')) path = path[1..];
    return path;
  }

  private string[] MakeRelative(string[] items)
  {
    for (var i = 0; i < items.Length; i++)
    {
      items[i] = "/" + items[i];
    }
    return items;
  }

  public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
  {
    path = GetPath(path);

    var items = new List<string>();

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Read);

    var entries = archive.Entries
      .Where(e => e.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase))
      .Where(e => Path.GetFileName(e.FullName).Like(searchPattern));

    if (searchOption == SearchOption.AllDirectories)
    {
      entries = entries.Concat(
        entries.Where(e =>
          e.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase)
            && e.FullName.Count(f => f == '/') > path.Count(c => c == '/')));
    }

    items.AddRange(
      entries
        .Select(e => Path.GetDirectoryName(e.FullName)!)
        .Distinct()
        .Where(d => d != path)
      );

    return MakeRelative(items.ToArray());
  }

  public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
  {
    path = GetPath(path);

    var items = new List<string>();

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Read);

    var entries = archive.Entries
      .Where(e => e.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase))
      .Where(e => Path.GetFileName(e.FullName).Like(searchPattern));

    if (searchOption == SearchOption.AllDirectories)
    {
      entries = entries.Concat(
        entries.Where(e =>
          e.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase)
            && e.FullName.Count(f => f == '/') > path.Count(c => c == '/')));
    }

    items.AddRange(entries.Select(e => e.FullName));

    return MakeRelative(items.ToArray());
  }

  public bool DirectoryExists(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Read);
    var entries = archive.Entries.Where(e => e.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase));
    return entries.Any();
  }

  public bool FileExists(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Read);
    var entry = archive.GetEntry(path);
    return entry != null;
  }

  public void DeleteDirectory(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Update);
    var entries = archive.Entries.Where(e => e.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase));
    foreach (var entry in entries)
    {
      entry.Delete();
    }
  }

  public void DeleteFile(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Update);
    var entry = archive.GetEntry(path);
    if (entry != null)
    {
      entry.Delete();
    }
  }

  public Stream OpenFile(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Read);
    var entry = archive.GetEntry(path);

    return entry?.Open() ?? throw new FileNotFoundException();
  }

  public TextReader ReadFile(string path, Encoding? encoding = null)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Read);
    var entry = archive.GetEntry(path);

    return new StreamReader(
      entry?.Open() ?? throw new FileNotFoundException()
        , encoding ?? Encoding.UTF8);
  }

  public Task<string> ReadAllTextAsync(string path, Encoding? encoding = null)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Read);
    var entry = archive.GetEntry(path);

    return new StreamReader(
      entry?.Open() ?? throw new FileNotFoundException()
        , encoding ?? Encoding.UTF8).ReadToEndAsync();
  }

  public Task WriteFileAsync(string path, Stream stream)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Update);
    var entry = archive.CreateEntry(path);
    using var entryStream = entry.Open();
    return stream.CopyToAsync(entryStream);
  }

  public async Task WriteFileAsync(string path, TextReader reader, Encoding? encoding = null)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Update);
    var entry = archive.CreateEntry(path);
    using var entryStream = entry.Open();
    using var writer = new StreamWriter(entryStream, encoding ?? Encoding.UTF8);

    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
      await writer.WriteLineAsync(line);
    }
  }

  public Task WriteAllTextAsync(string path, string text, Encoding? encoding = null)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile, ZipArchiveMode.Update);
    var entry = archive.CreateEntry(path);
    using var entryStream = entry.Open();
    using var writer = new StreamWriter(entryStream, encoding ?? Encoding.UTF8);
    return writer.WriteAsync(text);
  }
}
