using System.Text;

namespace ScriptPack.FileSystem;

/// <summary>
/// Classe que implementa a interface IDrive para abstrair uma estrutura de
/// arquivos e pastas.
/// </summary>
public class FileDrive : IDrive
{
  /// <summary>
  /// Cria uma nova instância de FileDrive com o diretório especificado.
  /// </summary>
  /// <param name="directory">
  /// O diretório que será abstraído por esta instância.
  /// </param>
  public FileDrive(string directory)
  {
    this.Directory = directory.Replace('\\', '/');
  }

  /// <summary>
  /// Obtém o nome de identificação do drive.
  /// </summary>
  public string Name => Path.GetFileName(Directory);

  /// <summary>
  /// Obtém um valor que indica se o drive é somente leitura.
  /// </summary>
  public bool ReadOnly => false;

  /// <summary>
  /// Obtém o diretório que é abstraído por esta instância.
  /// </summary>
  public string Directory { get; }

  /// <summary>
  /// Obtém o caminho completo para um arquivo ou diretório dentro do diretório
  /// abstraído por esta instância.
  /// </summary>
  /// <param name="path">
  /// O caminho para o arquivo ou diretório desejado.
  /// </param>
  /// <returns>
  /// O caminho completo para o arquivo ou diretório desejado.
  /// </returns>
  private string GetPath(string path)
  {
    path = path.Replace('\\', '/');
    if (path.StartsWith('/')) path = path[1..];
    path = Path.Combine(Directory, path);
    return path;
  }

  /// <summary>
  /// Converte um array de caminhos absolutos para um array de caminhos
  /// relativos ao diretório abstraído por esta instância.
  /// </summary>
  /// <param name="paths">O array de caminhos absolutos.</param>
  /// <returns>O array de caminhos relativos.</returns>
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

  /// <summary>
  /// Remove caminhos ocultos (que começam com um ponto) de um array de
  /// caminhos.
  /// </summary>
  /// <param name="paths">O array de caminhos a ser filtrado.</param>
  /// <returns>O array de caminhos sem os caminhos ocultos.</returns>
  private string[] RemoveHiddenPaths(string[] paths)
  {
    return paths.Where(p => !p.Split('/').Any(s => s.StartsWith('.')))
        .ToArray();
  }

  /// <summary>
  /// Obtém os nomes dos diretórios que correspondem ao padrão de busca
  /// especificado, no caminho especificado e opções de busca.
  /// </summary>
  /// <param name="path">
  /// O caminho do diretório a ser pesquisado. O caminho deve ser relativo ao
  /// diretório raiz do drive.
  /// </param>
  /// <param name="searchPattern">
  /// O filtro de pesquisa de nome com suporte ao caractere curinga *.
  /// </param>
  /// <param name="searchOption">
  /// Especifica se a pesquisa deve incluir somente o diretório especificado ou
  /// deve ser recursiva em todos os subdiretórios.
  /// </param>
  /// <returns>
  /// Uma matriz de strings contendo os nomes dos diretórios correspondentes ao
  /// padrão de busca especificado.
  /// </returns>
  public string[] GetDirectories(string path, string searchPattern,
      SearchOption searchOption)
  {
    path = GetPath(path);
    var items = System.IO.Directory.GetDirectories(path, searchPattern,
        searchOption);
    return RemoveHiddenPaths(MakeRelative(items));
  }

  /// <summary>
  /// Obtém os nomes dos arquivos que correspondem ao padrão de busca
  /// especificado, no caminho especificado e opções de busca.
  /// </summary>
  /// <param name="path">
  /// O caminho do diretório a ser pesquisado. O caminho deve ser relativo ao
  /// diretório raiz do drive.
  /// </param>
  /// <param name="searchPattern">
  /// O filtro de pesquisa de nome com suporte ao caractere curinga *.
  /// </param>
  /// <param name="searchOption">
  /// Especifica se a pesquisa deve incluir somente o diretório especificado ou
  /// deve ser recursiva em todos os subdiretórios.
  /// </param>
  /// <returns>
  /// Uma matriz de strings contendo os nomes dos arquivos correspondentes ao
  /// padrão de busca especificado.
  /// </returns>
  public string[] GetFiles(string path, string searchPattern,
      SearchOption searchOption)
  {
    path = GetPath(path);
    var items = System.IO.Directory.GetFiles(path, searchPattern, searchOption);
    return RemoveHiddenPaths(MakeRelative(items));
  }

  /// <summary>
  /// Verifica se um diretório existe no caminho especificado.
  /// </summary>
  /// <param name="path">
  /// O caminho do diretório a ser verificado. O caminho deve ser relativo ao
  /// diretório raiz do drive.
  /// </param>
  /// <returns>True se o diretório existir; caso contrário, false.</returns>
  public bool DirectoryExists(string path)
  {
    path = GetPath(path);
    return System.IO.Directory.Exists(path);
  }

  /// <summary>
  /// Verifica se um arquivo existe no caminho especificado.
  /// </summary>
  /// <param name="path">
  /// O caminho do arquivo a ser verificado. O caminho deve ser relativo ao
  /// diretório raiz do drive.
  /// </param>
  /// <returns>True se o arquivo existir; caso contrário, false.</returns>
  public bool FileExists(string path)
  {
    path = GetPath(path);
    return File.Exists(path);
  }

  /// <summary>
  /// Exclui o diretório especificado no caminho relativo ao diretório raiz do
  /// drive.
  /// </summary>
  /// <param name="path">Caminho do diretório a ser excluído.</param>
  /// <remarks>
  /// A exclusão do diretório é realizada de forma recursiva, ou seja, todos os
  /// arquivos e subdiretórios dentro do diretório especificado também serão
  /// excluídos.
  /// </remarks>
  public void DeleteDirectory(string path)
  {
    path = GetPath(path);
    System.IO.Directory.Delete(path, true);
  }

  /// <summary>
  /// Exclui o arquivo especificado no caminho relativo ao diretório raiz do
  /// drive.
  /// </summary>
  /// <param name="path">Caminho do arquivo a ser excluído.</param>
  /// <remarks>
  /// Caso o arquivo não exista, nenhuma ação será tomada.
  /// </remarks>
  public void DeleteFile(string path)
  {
    path = GetPath(path);
    File.Delete(path);
  }

  /// <summary>
  /// Abre um arquivo especificado no caminho relativo ao diretório raiz do
  /// drive para leitura.
  /// </summary>
  /// <param name="path">Caminho do arquivo a ser aberto.</param>
  /// <returns>Stream para leitura do arquivo.</returns>
  /// <remarks>
  /// O método retorna um stream que pode ser utilizado para ler os dados do
  /// arquivo.
  /// </remarks>
  public Stream OpenFile(string path)
  {
    path = GetPath(path);
    return File.OpenRead(path);
  }

  /// <summary>
  /// Abre um arquivo especificado no caminho relativo ao diretório raiz do
  /// drive para leitura.
  /// </summary>
  /// <param name="path">Caminho do arquivo a ser aberto.</param>
  /// <param name="encoding">
  /// Codificação a ser utilizada para ler o arquivo. O padrão é UTF-8.
  /// </param>
  /// <returns>TextReader para leitura do arquivo.</returns>
  /// <remarks>
  /// O método retorna um TextReader que pode ser utilizado para ler os dados do
  /// arquivo.
  /// </remarks>
  public TextReader ReadFile(string path, Encoding? encoding = null)
  {
    path = GetPath(path);
    encoding ??= Encoding.UTF8;
    return new StreamReader(path, encoding);
  }

  /// <summary>
  /// Lê todo o conteúdo de um arquivo especificado no caminho relativo ao
  /// diretório raiz do drive como uma string assincronamente.
  /// </summary>
  /// <param name="path">Caminho do arquivo a ser lido.</param>
  /// <param name="encoding">
  /// Codificação a ser utilizada para ler o arquivo. O padrão é UTF-8.
  /// </param>
  /// <returns>
  /// Uma tarefa que representa a operação de leitura do arquivo. O resultado é
  /// o conteúdo do arquivo em formato de string.
  /// </returns>
  /// <remarks>
  /// O método lê todo o conteúdo do arquivo especificado como uma string
  /// utilizando a codificação especificada.
  /// </remarks>
  public Task<string> ReadAllTextAsync(string path, Encoding? encoding = null)
  {
    path = GetPath(path);
    encoding ??= Encoding.UTF8;
    return File.ReadAllTextAsync(path, encoding);
  }

  /// <summary>
  /// Escreve os dados de um stream em um arquivo especificado no caminho
  /// relativo ao diretório raiz do drive assincronamente.
  /// </summary>
  /// <param name="path">
  /// Caminho do arquivo a ser escrito.
  /// </param>
  /// <param name="stream">
  /// Stream contendo os dados a serem escritos no arquivo.
  /// </param>
  /// <returns>
  /// Uma tarefa que representa a operação de escrita do arquivo.
  /// </returns>
  /// <remarks>
  /// O método cria ou sobrescreve o arquivo especificado e escreve os dados
  /// contidos no stream no arquivo.
  /// </remarks>
  public async Task WriteFileAsync(string path, Stream stream)
  {
    path = GetPath(path);
    using var output = File.OpenWrite(path);
    await stream.CopyToAsync(output);
    await output.FlushAsync();
  }

  /// <summary>
  /// Escreve os dados de um TextReader em um arquivo especificado no caminho
  /// relativo ao diretório raiz do drive assincronamente.
  /// </summary>
  /// <param name="path">Caminho do arquivo a ser escrito.
  /// </param>
  /// <param name="reader">
  /// TextReader contendo os dados a serem escritos no arquivo.
  /// </param>
  /// <param name="encoding">
  /// Codificação dos caracteres contidos no TextReader.
  /// </param>
  /// <returns>
  /// Uma tarefa que representa a operação de escrita do arquivo.
  /// </returns>
  /// <remarks>
  /// O método cria ou sobrescreve o arquivo especificado e escreve os dados
  /// contidos no TextReader no arquivo.
  /// </remarks>
  public Task WriteFileAsync(string path, TextReader reader,
      Encoding? encoding = null)
  {
    path = GetPath(path);
    encoding ??= Encoding.UTF8;
    return File.WriteAllTextAsync(path, reader.ReadToEnd(), encoding);
  }

  /// <summary>
  /// Escreve o texto especificado em um arquivo especificado no caminho
  /// relativo ao diretório raiz do drive assincronamente.
  /// </summary>
  /// <param name="path">Caminho do arquivo a ser escrito.</param>
  /// <param name="text">Texto a ser escrito no arquivo.</param>
  /// <param name="encoding">
  /// Codificação dos caracteres contidos no texto.
  /// </param>
  /// <returns>
  /// Uma tarefa que representa a operação de escrita do arquivo.
  /// </returns>
  /// <remarks>
  /// O método cria ou sobrescreve o arquivo especificado e escreve o texto no
  /// arquivo.
  /// </remarks>
  public Task WriteAllTextAsync(string path, string text,
      Encoding? encoding = null)
  {
    path = GetPath(path);
    encoding ??= Encoding.UTF8;
    return File.WriteAllTextAsync(path, text, encoding);
  }
}
