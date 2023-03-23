using System;
using System.Reflection;
using System.Text;
using DotNet.Globbing;

namespace ScriptPack.FileSystem;

/// <summary>
/// Drive para navegação em arquivos embarcados em um componente.
/// </summary>
public class EmbeddedDrive : IDrive
{
  private readonly Assembly _assembly;
  private readonly string[] _files;
  private readonly string[] _folders;

  public EmbeddedDrive(Assembly assembly)
  {
    _assembly = assembly;
    _files = _assembly
        .GetManifestResourceNames()
        .Where(m => m.EndsWith(".json") || m.EndsWith(".sql"))
        .ToArray();
    _folders = ExtractDirectoriesFromFilePaths(this._files);
  }

  private string[] ExtractDirectoriesFromFilePaths(string[] files)
  {
    HashSet<string> uniqueFolders = new HashSet<string>();

    foreach (var file in files)
    {
      var folder = Path.GetDirectoryName(file)!;
      while (!string.IsNullOrEmpty(folder) && folder != Path.GetPathRoot(folder))
      {
        uniqueFolders.Add(folder.EndsWith("/") ? folder : folder + "/");
        folder = Path.GetDirectoryName(folder)!;
      }
      uniqueFolders.Add(folder.EndsWith("/") ? folder : folder + "/");
    }

    return uniqueFolders.OrderBy(x => x).ToArray();
  }

  /// <summary>
  /// Nome de identificação do drive.
  /// </summary>
  public string Name => _assembly.GetType().Name;

  /// <summary>
  /// Determina se o drive é somente leitura.
  /// </summary>
  public bool ReadOnly => true;

  /// <summary>
  /// Enumera os arquivos de um diretório.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <param name="searchPattern">
  /// O filtro de pesquisa de nome com suporte ao caractere curinga *.
  /// </param>
  /// <param name="searchOption">
  /// Determina se a busca deve ser recursiva.
  /// </param>
  /// <returns>
  /// Arquivos enumerados.
  /// </returns>
  public string[] GetFiles(string path, string searchPattern,
      SearchOption searchOption)
  {
    var files = new List<string>();
    var glob = Glob.Parse(searchPattern);
    foreach (var file in _files)
    {
      var folder = Path.GetDirectoryName(file)!;

      if (searchOption == SearchOption.TopDirectoryOnly &&
          !path.Equals(folder))
      {
        continue;
      }
      else if (searchOption == SearchOption.AllDirectories &&
          !folder.StartsWith(path))
      {
        continue;
      }

      var filename = Path.GetFileName(file);
      if (glob.IsMatch(filename))
      {
        files.Add(file);
      }
    }
    return files.ToArray();
  }

  /// <summary>
  /// Enumera os diretórios de um diretório.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <param name="searchPattern">
  /// O filtro de pesquisa de nome com suporte ao caractere curinga *.
  /// </param>
  /// <param name="searchOption">
  /// Determina se a busca deve ser recursiva.
  /// </param>
  /// <returns>
  /// Diretórios enumerados.
  /// </returns>
  public string[] GetDirectories(string path, string searchPattern,
      SearchOption searchOption)
  {
    var folders = new List<string>();
    var glob = Glob.Parse(searchPattern);
    foreach (var folder in _folders)
    {
      if (searchOption == SearchOption.TopDirectoryOnly &&
          !path.Equals(folder))
      {
        continue;
      }
      else if (searchOption == SearchOption.AllDirectories &&
          !folder.StartsWith(path))
      {
        continue;
      }

      var foldername = Path.GetFileName(folder[..^1]);
      if (glob.IsMatch(foldername))
      {
        folders.Add(folder);
      }
    }
    return folders.ToArray();
  }

  /// <summary>
  /// Determina se um arquivo existe.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  public bool FileExists(string path)
  {
    return _files.Contains(path);
  }

  /// <summary>
  /// Determina se um diretório existe.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  public bool DirectoryExists(string path)
  {
    return _folders.Contains(path);
  }

  /// <summary>
  /// Abre um arquivo para leitura.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <returns>
  /// Stream para leitura do arquivo.
  /// </returns>
  public Task<Stream> OpenFileAsync(string path)
  {
    var stream = _assembly.GetManifestResourceStream(path);
    if (stream is null)
      throw new ArgumentException($"Arquivo não encontrado: {path}");

    return Task.FromResult(stream);
  }

  /// <summary>
  /// Abre um arquivo para leitura.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <returns>
  /// Stream para leitura do arquivo.
  /// </returns>
  public Task<TextReader> ReadFileAsync(string path, Encoding? encoding = null)
  {
    var stream = _assembly.GetManifestResourceStream(path);
    if (stream is null)
      throw new ArgumentException($"Arquivo não encontrado: {path}");

    var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
    return Task.FromResult((TextReader)reader);
  }

  /// <summary>
  /// Lê o conteúdo de um arquivo como texto.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <returns>
  /// Conteúdo do arquivo.
  /// </returns>
  public Task<string> ReadAllTextAsync(string path, Encoding? encoding = null)
  {
    var stream = _assembly.GetManifestResourceStream(path);
    if (stream is null)
      throw new ArgumentException($"Arquivo não encontrado: {path}");

    using (stream)
    {
      using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
      var text = reader.ReadToEndAsync();
      return text;
    }
  }

  #region Operações não suportadas

  /// <summary>
  /// Exclui um arquivo.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  public void DeleteFile(string path)
  {
    throw new NotSupportedException(
        "Edição de arquivos embutidos não é suportada.");
  }

  /// <summary>
  /// Exclui um diretório.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  public void DeleteDirectory(string path)
  {
    throw new NotSupportedException(
        "Edição de arquivos embutidos não é suportada.");
  }

  /// <summary>
  /// Escreve um arquivo.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <param name="stream">
  /// Stream com o conteúdo do arquivo.
  /// </param>
  public Task WriteFileAsync(string path, Stream stream)
  {
    throw new NotSupportedException(
        "Edição de arquivos embutidos não é suportada.");
  }

  /// <summary>
  /// Escreve um arquivo.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <param name="reader">
  /// Stream com o conteúdo do arquivo.
  /// </param>
  public Task WriteFileAsync(string path, TextReader reader,
      Encoding? encoding = null)
  {
    throw new NotSupportedException(
        "Edição de arquivos embutidos não é suportada.");
  }

  /// <summary>
  /// Escreve um arquivo.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <param name="text">
  /// Conteúdo do arquivo.
  /// </param>
  /// <param name="encoding">
  /// Codificação do texto.
  /// </param>
  public Task WriteAllTextAsync(string path, string text,
      Encoding? encoding = null)
  {
    throw new NotSupportedException(
        "Edição de arquivos embutidos não é suportada.");
  }

  #endregion
}
