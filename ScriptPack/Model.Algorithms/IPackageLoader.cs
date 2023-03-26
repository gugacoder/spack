using ScriptPack.Domain;
using ScriptPack.FileSystem;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Utilitário para carregamento de nodos de um catálogo a partir de um Drive.
/// </summary>
public interface IPackageLoader
{
  /// <summary>
  /// Drive para carregamento dos arquivos.
  /// </summary>
  IDrive Drive { get; }

  /// <summary>
  /// Carrega todos os nodos de um determinado tipo a partir de um drive.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser carregado.
  /// </typeparam>
  /// <returns>
  /// Lista de nodos carregados.
  /// </returns>
  Task<List<T>> ReadNodesAsync<T>() where T : IFileNode, new();

  /// <summary>
  /// Carrega o nodo do tipo especificado a partir do arquivo.
  /// </summary>
  /// <param name="filePath">
  /// Caminho do arquivo a ser carregado.
  /// </param>
  /// <typeparam name="T">
  /// Tipo de nodo a ser carregado.
  /// </typeparam>
  /// <returns>
  /// Nodo carregado.
  /// </returns>
  Task<T> ReadNodeFromFileAsync<T>(string filePath) where T : IFileNode, new();

  /// <summary>
  /// Carrega os scripts do pacote.
  /// </summary>
  /// <param name="parent">
  /// Nodo pai dos nodos a serem carregados.
  /// </param>
  /// <returns>
  /// Lista de nodos carregados.
  /// </returns>
  Task<ScriptNode> ReadScriptFromFileAsync(string filePath);
}
