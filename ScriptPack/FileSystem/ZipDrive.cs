using System.IO.Compression;
using System.Text;
using ScriptPack.Helpers;

namespace ScriptPack.FileSystem;

/// <summary>
/// Classe que implementa a interface IDrive para abstrair um arquivo .zip como
/// estrutura de arquivos e pastas.
/// </summary>
public class ZipDrive : IDrive
{
  /// <summary>
  /// Cria uma nova instância da classe ZipDrive a partir do caminho do arquivo
  /// .zip.
  /// </summary>
  /// <param name="filepath">Caminho completo do arquivo .zip.</param>
  public ZipDrive(string filepath)
  {
    this.ZipFile = filepath;
    if (!File.Exists(this.ZipFile))
    {
      using var zip = System.IO.Compression.ZipFile.Open(this.ZipFile,
          ZipArchiveMode.Create);
    }
  }

  /// <summary>
  /// Obtém o nome do arquivo .zip sem a extensão.
  /// </summary>
  public string Name => Path.GetFileNameWithoutExtension(ZipFile);

  /// <summary>
  /// Obtém um valor booleano que indica se o arquivo .zip é somente leitura.
  /// </summary>
  public bool ReadOnly => true;

  /// <summary>
  /// Obtém o caminho completo do arquivo .zip.
  /// </summary>
  public string ZipFile { get; private set; }

  /// <summary>
  /// Retorna o caminho completo de um arquivo ou pasta dentro do arquivo .zip.
  /// </summary>
  /// <param name="path">
  /// Caminho relativo da pasta ou arquivo dentro do arquivo .zip.
  /// </param>
  /// <returns>
  /// O caminho completo do arquivo ou pasta dentro do arquivo .zip.
  /// </returns>
  private string GetPath(string path)
  {
    if (path.StartsWith('/')) path = path[1..];
    return path;
  }

  /// <summary>
  /// Converte os caminhos absolutos em caminhos relativos ao arquivo .zip.
  /// </summary>
  /// <param name="items">Um array de caminhos absolutos.</param>
  /// <returns>Um array de caminhos relativos ao arquivo .zip.</returns>
  private string[] MakeRelative(string[] items)
  {
    for (var i = 0; i < items.Length; i++)
    {
      items[i] = "/" + items[i];
    }
    return items;
  }

  /// <summary>
  /// Obtém uma lista de subdiretórios de uma pasta dentro do arquivo .zip.
  /// </summary>
  /// <param name="path">
  /// O caminho relativo da pasta dentro do arquivo .zip.
  /// </param>
  /// <param name="searchPattern">
  /// O filtro de pesquisa de nome com suporte ao caractere curinga *.
  /// </param>
  /// <param name="searchOption">
  /// O tipo de busca a ser realizada.
  /// </param>
  /// <returns>
  /// Um array de caminhos relativos dos subdiretórios encontrados.
  /// </returns>
  public string[] GetDirectories(string path, string searchPattern,
      SearchOption searchOption)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Read);

    var entries = archive.Entries
      .Where(e => e.FullName.StartsWith(path,
          StringComparison.OrdinalIgnoreCase))
      .Where(e => Path.GetFileName(e.FullName).Like(searchPattern));

    if (searchOption == SearchOption.AllDirectories)
    {
      entries = entries.Concat(
        entries.Where(e =>
          e.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase)
            && e.FullName.Count(f => f == '/') > path.Count(c => c == '/')));
    }

    var items = entries
        .Select(e => Path.GetDirectoryName(e.FullName)!)
        .Distinct()
        .Where(d => d != path)
        .Distinct()
        .ToArray();

    return MakeRelative(items);
  }

  /// <summary>
  /// Obtém uma lista de arquivos de uma pasta dentro do arquivo .zip.
  /// </summary>
  /// <param name="path">
  /// O caminho relativo da pasta dentro do arquivo .zip.
  /// </param>
  /// <param name="searchPattern">
  /// O filtro de pesquisa de nome com suporte ao caractere curinga *.
  /// </param>
  /// <param name="searchOption">O tipo de busca a ser realizada.</param>
  /// <returns>
  /// Um array de caminhos relativos dos arquivos encontrados.
  /// </returns>
  public string[] GetFiles(string path, string searchPattern,
      SearchOption searchOption)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Read);

    var entries = archive.Entries
      .Where(e => e.FullName.StartsWith(path,
          StringComparison.OrdinalIgnoreCase))
      .Where(e => Path.GetFileName(e.FullName).Like(searchPattern));

    if (searchOption == SearchOption.AllDirectories)
    {
      entries = entries.Concat(
        entries.Where(e =>
          e.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase)
            && e.FullName.Count(f => f == '/') > path.Count(c => c == '/')));
    }

    var items = entries.Select(e => e.FullName).Distinct().ToArray();

    return MakeRelative(items);
  }

  /// <summary>
  /// Verifica se um diretório dentro do arquivo .zip existe.
  /// </summary>
  /// <param name="path">
  /// O caminho relativo do diretório dentro do arquivo .zip.
  /// </param>
  /// <returns>True se o diretório existe, false caso contrário.</returns>
  public bool DirectoryExists(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Read);
    var entries = archive.Entries.Where(e => e.FullName.StartsWith(path,
        StringComparison.OrdinalIgnoreCase));
    return entries.Any();
  }

  /// <summary>
  /// Verifica se um arquivo existe no arquivo ZIP.
  /// </summary>
  public bool FileExists(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Read);
    var entry = archive.GetEntry(path);
    return entry != null;
  }

  /// <summary>
  /// Exclui um diretório do arquivo ZIP.
  /// </summary>
  public void DeleteDirectory(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Update);
    var entries = archive.Entries.Where(e => e.FullName.StartsWith(path,
        StringComparison.OrdinalIgnoreCase));
    foreach (var entry in entries)
    {
      entry.Delete();
    }
  }

  /// <summary>
  /// Exclui um arquivo do arquivo ZIP.
  /// </summary>
  public void DeleteFile(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Update);
    var entry = archive.GetEntry(path);
    if (entry != null)
    {
      entry.Delete();
    }
  }

  /// <summary>
  /// Abre um arquivo do arquivo ZIP em modo de leitura.
  /// </summary>
  public async Task<Stream> OpenFileAsync(string path)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Read);
    var entry = archive.GetEntry(path);

    var stream = entry?.Open() ?? throw new FileNotFoundException();

    MemoryStream? buffer = null;
    try
    {
      buffer = new MemoryStream();

      await stream.CopyToAsync(buffer);

      buffer.Position = 0;
      return buffer;
    }
    catch
    {
      buffer?.Dispose();
      throw;
    }
  }

  /// <summary>
  /// Lê um arquivo do arquivo ZIP.
  /// </summary>
  public async Task<TextReader> ReadFileAsync(string path,
      Encoding? encoding = null)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Read);
    var entry = archive.GetEntry(path);

    var stream = await OpenFileAsync(path);
    var reader = new StreamReader(stream, encoding ?? Drive.DefaultEncoding);

    return reader;
  }

  /// <summary>
  /// Lê todo o conteúdo de um arquivo do arquivo ZIP de forma assíncrona.
  /// </summary>
  public async Task<string> ReadAllTextAsync(string path,
      Encoding? encoding = null)
  {
    path = GetPath(path);
    using var reader = await ReadFileAsync(path, encoding);
    var text = await reader.ReadToEndAsync();
    return text;
  }

  /// <summary>
  /// Escreve um arquivo no arquivo ZIP de forma assíncrona.
  /// </summary>
  public Task WriteFileAsync(string path, Stream stream)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Update);
    var entry = archive.CreateEntry(path);
    using var entryStream = entry.Open();
    return stream.CopyToAsync(entryStream);
  }

  /// <summary>
  /// Escreve um arquivo no arquivo ZIP de forma assíncrona a partir de um
  /// leitor de texto.
  /// </summary>
  public async Task WriteFileAsync(string path, TextReader reader,
      Encoding? encoding = null)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Update);
    var entry = archive.CreateEntry(path);
    using var entryStream = entry.Open();
    using var writer = new StreamWriter(entryStream,
        encoding ?? Drive.DefaultEncoding);

    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
      await writer.WriteLineAsync(line);
    }
  }

  /// <summary>
  /// Escreve todo o texto em um arquivo do arquivo ZIP de forma assíncrona.
  /// </summary>
  public Task WriteAllTextAsync(string path, string text,
      Encoding? encoding = null)
  {
    path = GetPath(path);

    using var archive = System.IO.Compression.ZipFile.Open(ZipFile,
        ZipArchiveMode.Update);
    var entry = archive.CreateEntry(path);
    using var entryStream = entry.Open();
    using var writer = new StreamWriter(entryStream,
        encoding ?? Drive.DefaultEncoding);
    return writer.WriteAsync(text);
  }
}
