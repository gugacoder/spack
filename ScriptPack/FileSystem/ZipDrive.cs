using Ionic.Zip;
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

  /// <summary>
  /// Senha padrão utilizada para criptografar o arquivo .zip.
  /// </summary>
  // Password: SPack internal password!
  private const string InternalPassword = "enc:ECIYEx9PKhwNFQYBIh5ZABUcMAUWAhBO";

  private readonly Encoding _defaultEncoding;
  private readonly ZipFile _zip;

  /// <summary>
  /// Cria uma nova instância da classe ZipDrive a partir do caminho do arquivo
  /// .zip.
  /// </summary>
  /// <param name="filepath">Caminho completo do arquivo .zip.</param>
  /// <param name="password">
  /// Senha para descriptografar o arquivo .zip.
  /// </param>
  /// <param name="defaultEncoding">
  /// Codificação de caracteres utilizada para descriptografar o arquivo .zip.
  /// </param>
  /// <param name="mode">
  /// Modo de abertura do arquivo .zip.
  /// </param>
  public ZipDrive(string filePath, string? password = null,
      Mode mode = Mode.Read, Encoding? defaultEncoding = null)
  {
    this.FilePath = filePath;
    this.ReadOnly = mode == Mode.Read;
    this._defaultEncoding = defaultEncoding ?? Encodings.Iso88591;
    this._zip = OpenOrCreateZipFile(filePath, password, mode);
  }

  /// <summary>
  /// Abre ou cria um arquivo .zip.
  /// </summary>
  /// <param name="filepath">Caminho completo do arquivo .zip.</param>
  /// <param name="password">
  /// Senha para descriptografar o arquivo .zip.
  /// </param>
  /// <param name="writable">
  /// Indica se o arquivo .zip será aberto em modo somente leitura ou gravação.
  /// </param>
  /// <param name="mode">
  /// Modo de abertura do arquivo .zip.
  /// </param>
  /// <returns>
  /// Uma instância da classe ZipFile que representa o arquivo .zip.
  /// </returns>
  private static ZipFile OpenOrCreateZipFile(string filePath, string? password,
      Mode mode)
  {
    if (mode == Mode.Overwrite && File.Exists(filePath))
    {
      File.Delete(filePath);
    }

    // Se o arquivo existe o abrimos, testamos e retornamos.
    if (File.Exists(filePath))
    {
      return OpenZipFile(filePath, password);
    }

    // Se o arquivo não existe e não é para escrita falhamos.
    if (mode == Mode.Writable || mode == Mode.Overwrite)
    {
      return CreateZipFile(filePath, password);
    }

    throw new FileNotFoundException("Arquivo .zip não encontrado.", filePath);
  }

  /// <summary>
  /// Abre um arquivo .zip.
  /// </summary>
  /// <param name="filepath">Caminho completo do arquivo .zip.</param>
  /// <param name="password">
  /// Senha para descriptografar o arquivo .zip.
  /// </param>
  /// <remarks>
  /// O arquivo .zip pode ter nenhma senha, uma senha de usuário ou uma senha
  /// interna.
  /// </remarks>
  /// <returns>
  /// Uma instância da classe ZipFile que representa o arquivo .zip.
  /// </returns>
  private static ZipFile OpenZipFile(string filePath, string? password)
  {
    // Checando formas de abrir o arquivo.
    // O arquivo pode ter:
    // -  Nenhuma senha.
    // -  Uma senha de usuário especificada.
    // -  Uma senha interna.
    ZipFile? zip = null;
    try
    {
      zip = ZipFile.Read(filePath);
      zip.Password = string.IsNullOrEmpty(password)
          ? null : Crypto.Decrypt(password);
      zip.First();
      return zip;
    }
    catch
    {
      zip?.Dispose();
      try
      {
        zip = ZipFile.Read(filePath);
        zip.Password = Crypto.Decrypt(InternalPassword);
        zip.First();
        return zip;
      }
      catch (Exception ex)
      {
        zip?.Dispose();
        throw new FileLoadException(
            "Falha ao abrir o arquivo .zip. O arquivo pode não ser válido ou " +
            "a senha pode estar incorreta.", filePath, ex);
      }
    }
  }

  /// <summary>
  /// Cria um arquivo .zip.
  /// </summary>
  /// <param name="filepath">Caminho completo do arquivo .zip.</param>
  /// <param name="password">
  /// Senha para criptografar o arquivo .zip.
  /// </param>
  /// <returns>
  /// Uma instância da classe ZipFile que representa o arquivo .zip.
  /// </returns>
  private static ZipFile CreateZipFile(string filePath, string? password)
  {
    // Se o arqivo não existe e é para escrita o criamos.
    ZipFile? zip = null;
    try
    {
      zip = new ZipFile();
      zip.Password = !string.IsNullOrEmpty(password)
          ? Crypto.Decrypt(password)
          : Crypto.Decrypt(InternalPassword);
      zip.Save(filePath);
    }
    catch
    {
      zip?.Dispose();
      throw;
    }
    return zip;
  }

  /// <summary>
  /// Obtém o nome do arquivo .zip sem a extensão.
  /// </summary>
  public string Name => Path.GetFileNameWithoutExtension(FilePath);

  /// <summary>
  /// Obtém um valor booleano que indica se o arquivo .zip é somente leitura.
  /// </summary>
  public bool ReadOnly { get; }

  /// <summary>
  /// Obtém o caminho completo do arquivo .zip.
  /// </summary>
  public string FilePath { get; private set; }

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

    var entries = _zip.Entries
        .Where(e => e.FileName.StartsWith(path,
            StringComparison.InvariantCultureIgnoreCase))
        .Where(e => Path.GetFileName(e.FileName).Like(searchPattern));

    if (searchOption == SearchOption.AllDirectories)
    {
      entries = entries.Concat(
          entries.Where(e =>
              e.FileName.StartsWith(path, StringComparison.OrdinalIgnoreCase)
              && e.FileName.Count(f => f == '/') > path.Count(c => c == '/')));
    }

    var items = entries
        .Select(e => Path.GetDirectoryName(e.FileName)!)
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

    var entries = _zip.Entries
        .Where(e => e.FileName.StartsWith(path,
            StringComparison.OrdinalIgnoreCase))
        .Where(e => Path.GetFileName(e.FileName).Like(searchPattern));

    if (searchOption == SearchOption.AllDirectories)
    {
      entries = entries.Concat(
          entries.Where(e =>
              e.FileName.StartsWith(path, StringComparison.OrdinalIgnoreCase)
              && e.FileName.Count(f => f == '/') > path.Count(c => c == '/')));
    }

    var items = entries.Select(e => e.FileName).Distinct().ToArray();

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

    var exists = _zip.Entries
        .Where(e => e.FileName.StartsWith(path,
            StringComparison.OrdinalIgnoreCase))
        .Any();
    return exists;
  }

  /// <summary>
  /// Verifica se um arquivo existe no arquivo ZIP.
  /// </summary>
  public bool FileExists(string path)
  {
    path = GetPath(path);

    var entry = _zip.Entries.FirstOrDefault(e => e.FileName.Equals(path));
    return entry != null;
  }

  /// <summary>
  /// Exclui um diretório do arquivo ZIP.
  /// </summary>
  public void DeleteDirectory(string path)
  {
    path = GetPath(path);

    var entries = _zip.Entries
        .Where(e => e.FileName.StartsWith(path,
            StringComparison.OrdinalIgnoreCase))
        .ToArray();

    _zip.RemoveEntries(entries);
  }

  /// <summary>
  /// Exclui um arquivo do arquivo ZIP.
  /// </summary>
  public void DeleteFile(string path)
  {
    path = GetPath(path);

    var entry = _zip.Entries.FirstOrDefault(e => e.FileName.Equals(path));
    if (entry != null)
    {
      _zip.RemoveEntry(entry);
    }
  }

  /// <summary>
  /// Abre um arquivo do arquivo ZIP em modo de leitura.
  /// </summary>
  public Task<Stream> OpenFileAsync(string path)
  {
    path = GetPath(path);

    var entry = _zip.Entries.FirstOrDefault(e => e.FileName.Equals(path))
        ?? throw new FileNotFoundException();

    MemoryStream? buffer = null;
    try
    {
      buffer = new MemoryStream();
      entry.Extract(buffer);
      buffer.Position = 0;
      return Task.FromResult((Stream)buffer);
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

    var entry = _zip.Entries.FirstOrDefault(e => e.FileName.Equals(path))
        ?? throw new FileNotFoundException();

    encoding ??= entry.AlternateEncoding;

    var stream = await OpenFileAsync(path);
    return new StreamReader(stream, encoding);
  }

  /// <summary>
  /// Lê todo o conteúdo de um arquivo do arquivo ZIP de forma assíncrona.
  /// </summary>
  public async Task<string> ReadAllTextAsync(string path,
      Encoding? encoding = null)
  {
    path = GetPath(path);

    var entry = _zip.Entries.FirstOrDefault(e => e.FileName.Equals(path))
        ?? throw new FileNotFoundException();

    encoding ??= entry.AlternateEncoding;

    using var stream = await OpenFileAsync(path);
    using var reader = new StreamReader(stream, encoding);

    var content = await reader.ReadToEndAsync();
    return content;
  }

  /// <summary>
  /// Escreve um arquivo no arquivo ZIP de forma assíncrona.
  /// </summary>
  public Task WriteFileAsync(string path, Stream stream)
  {
    path = GetPath(path);

    _zip.AddEntry(path, stream);
    _zip.Save(this.FilePath);

    return Task.CompletedTask;
  }

  /// <summary>
  /// Escreve um arquivo no arquivo ZIP de forma assíncrona a partir de um
  /// leitor de texto.
  /// </summary>
  public async Task WriteFileAsync(string path, TextReader reader,
      Encoding? encoding = null)
  {
    encoding ??= _defaultEncoding;
    path = GetPath(path);

    // Bufferizando o conteúdo a ser compactado.
    using var stream = new MemoryStream();
    using var writer = new StreamWriter(stream);

    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
      await writer.WriteLineAsync(line);
    }
    stream.Position = 0;

    // Compactando o conteúdo.
    _zip.AddEntry(path, stream);
    _zip.Save(this.FilePath);
  }

  /// <summary>
  /// Escreve todo o texto em um arquivo do arquivo ZIP de forma assíncrona.
  /// </summary>
  public Task WriteAllTextAsync(string path, string text,
      Encoding? encoding = null)
  {
    encoding ??= Encodings.Iso88591;
    path = GetPath(path);

    // Bufferizando o conteúdo a ser compactado.
    using var stream = new MemoryStream();

    byte[] bytes = encoding.GetBytes(text);
    stream.Write(bytes, 0, bytes.Length);
    stream.Position = 0;

    // Compactando o conteúdo.
    _zip.AddEntry(path, stream);
    _zip.Save(this.FilePath);

    return Task.CompletedTask;
  }

  /// <summary>
  /// Verifica se o caminho especificado corresponde a um arquivo ZIP válido.
  /// </summary>
  /// <param name="filePath">Caminho para ser verificado.</param>
  /// <param name="password">Senha do arquivo ZIP.</param>
  /// <returns>
  /// True se o caminho corresponder a um arquivo ZIP válido,
  /// caso contrário, False.
  /// </returns>
  public static bool IsZipFile(string filePath, string? password)
  {
    try
    {
      if (File.Exists(filePath))
      {
        OpenZipFile(filePath, password);
        return true;
      }
    }
    catch { }
    return false;
  }
}

