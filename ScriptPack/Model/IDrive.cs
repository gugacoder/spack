using System.Text;

namespace ScriptPack.Model;

/// <summary>
/// Abstração de um navegador de arquivos para padronizar a navegação seja no
/// sistema de arquivos local, seja na estrutura do conteúdo de um arquivo
/// compactado, seja na estrutura de arquivos embarcados em um componente ou
/// qualquer outra forma de estocagem de arquivos.
/// </summary>
public interface IDrive
{
  /// <summary>
  /// Nome de identificação do drive.
  /// </summary>
  string Name { get; }

  /// <summary>
  /// Determina se o drive é somente leitura.
  /// </summary>
  bool ReadOnly { get; }

  /// <summary>
  /// Enumera os arquivos de um diretório.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <param name="searchPattern">
  /// Padrão de busca para os arquivos a serem enumerados.
  /// O padrão é "*.*".
  /// </param>
  /// <param name="searchOption">
  /// Determina se a busca deve ser recursiva.
  /// </param>
  /// <returns>
  /// Arquivos enumerados.
  /// </returns>
  string[] GetFiles(string path, string searchPattern,
      SearchOption searchOption);

  /// <summary>
  /// Enumera os diretórios de um diretório.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  /// <param name="searchPattern">
  /// Padrão de busca para os arquivos a serem enumerados.
  /// O padrão é "*.*".
  /// </param>
  /// <param name="searchOption">
  /// Determina se a busca deve ser recursiva.
  /// </param>
  /// <returns>
  /// Diretórios enumerados.
  /// </returns>
  string[] GetDirectories(string path, string searchPattern,
      SearchOption searchOption);

  /// <summary>
  /// Determina se um arquivo existe.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  bool FileExists(string path);

  /// <summary>
  /// Determina se um diretório existe.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  bool DirectoryExists(string path);

  /// <summary>
  /// Exclui um arquivo.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  void DeleteFile(string path);

  /// <summary>
  /// Exclui um diretório.
  /// </summary>
  /// <param name="path">
  /// Caminho do diretório a ser enumerado.
  /// O caminho deve ser relativo ao diretório raiz do drive.
  /// </param>
  void DeleteDirectory(string path);

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
  Stream OpenFile(string path);

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
  TextReader ReadFile(string path, Encoding? encoding = null);

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
  Task<string> ReadAllTextAsync(string path, Encoding? encoding = null);

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
  Task WriteFileAsync(string path, Stream stream);

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
  Task WriteFileAsync(string path, TextReader reader,
      Encoding? encoding = null);

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
  Task WriteAllTextAsync(string path, string text, Encoding? encoding = null);
}
