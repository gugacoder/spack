using Newtonsoft.Json;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;

namespace ScriptPack.Model.Algorithms;

//
//  NT-01 (Nota Técnica #1)
//
//  O algoritmo abaixo é responsável por carregar os catálogos de scripts na
//  sequência catálogo -> produto -> versão -> módulo -> pacote -> script.
//
//  Em geral, existem arquivos de definição de todos estes nodos exceto para
//  definição do produto. Para fins de facilitade de uso, o arquivo do produto,
//  chamado de "-product.jsonc", é o mesmo arquivo de definição de versão.
//
//  Os arquivos gerais são:
//
//  -   Catálogo: "-catalog.jsonc" (obrigatório)
//  -   Versão: "-product.jsonc" (opcional)
//  -   Módulo: "-module.jsonc" (opcional)
//  -   Pacote: "-package.jsonc" (opcional)
//
//  Exemplo:
//      /catalogo.jsonc
//      /MeuProduto/tags/1.0.0/-product.jsonc
//      /MeuProduto/tags/1.0.0/MeuModulo/-module.jsonc
//      /MeuProduto/tags/1.0.0/MeuModulo/MeuPacote/-package.jsonc
//      /MeuProduto/trunk/-product.jsonc
//      /MeuProduto/trunk/MeuModulo/-module.jsonc
//      /MeuProduto/trunk/MeuModulo/MeuPacote/-package.jsonc
//
//  Para resolver o problema de falta de definição de produto, o algoritmo
//  abaixo carrega as versões a partir do arquivo e não carrega o arquivo de
//  produto a priori.
//
//  Na etapa de finalização da estrtura de catálogo, quando as relações de
//  parentesco de nodos é criada, o algoritmo agrupa as versões e carrega um
//  produto para cada grupo com base no arquivo da versão mais recente.
//  

/// <summary>
/// Utilitário para carregamento de nodos de um catálogo a partir de arquivo
/// lidos de um drive.
/// </summary>
internal class PackageLoader : IPackageLoader
{
  private readonly PathPatternInterpreter _pathPatternInterpreter = new();

  /// <summary>
  /// Cria uma nova instância do carregador de nodos de catálogo.
  /// </summary>
  /// <param name="drive">
  /// Drive de onde os arquivos serão lidos.
  /// </param>
  public PackageLoader(IDrive drive)
  {
    this.Drive = drive;
  }

  /// <summary>
  /// Drive para carregamento dos arquivos.
  /// </summary>
  public IDrive Drive { get; }

  /// <summary>
  /// Carrega todos os nodos de um determinado tipo a partir de um drive.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser carregado.
  /// </typeparam>
  /// <returns>
  /// Lista de nodos carregados.
  /// </returns>
  public async Task<List<T>> ReadNodesAsync<T>() where T : IFileNode, new()
  {
    var nodes = new List<T>();
    string[] filePaths;

    if (typeof(T) == typeof(ScriptNode))
    {
      filePaths = Drive.GetFiles("/", "*.sql", SearchOption.AllDirectories);

      foreach (var filePath in filePaths)
      {
        var node = await ReadScriptFromFileAsync(filePath);
        nodes.Add((T)(object)node);
      }
      return nodes;
    }

    //
    // Leia a NT-01 no início deste arquivo para entender esta parte.
    //
    // Note que o arquivo para leitura da versão é o mesmo arquivo do produto.
    // Isso acontece porque não temos um arquivo separado para definição do
    // produto e definição de suas versões. Na verdade temos apenas os arquivos
    // definindo suas versões.
    //
    var typeName = typeof(T) == typeof(VersionNode)
        ? nameof(ProductNode)
        : typeof(T).Name;

    if (typeName.EndsWith("Node"))
    {
      typeName = typeName.Substring(0, typeName.Length - "Node".Length);
    }

    var fileName = $"-{typeName.ToLower()}.jsonc";

    filePaths = Drive.GetFiles("/", fileName, SearchOption.AllDirectories);
    foreach (var filePath in filePaths)
    {
      var node = await ReadNodeFromFileAsync<T>(filePath);
      nodes.Add(node);
    }

    return nodes;
  }

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
  public async Task<T> ReadNodeFromFileAsync<T>(string filePath)
    where T : IFileNode, new()
  {
    T node = new();
    try
    {
      var json = await Drive.ReadAllTextAsync(filePath);
      node = JsonConvert.DeserializeObject<T>(json, JsonOptions.CamelCase)!;
    }
    catch (Exception ex)
    {
      node = new T();
      node.Faults.Add(Fault.EmitException(ex));
    }

    node.FilePath = filePath;
    if (string.IsNullOrEmpty(node.Name))
    {
      var name = Path.GetFileName(Path.GetDirectoryName(filePath))!;
      node.Name = string.IsNullOrEmpty(name) ? Drive.Name : name;
    }

    if (node is CatalogNode catalog)
    {
      catalog.Description ??= "Catálogo de scripts.";
    }

    if (node is VersionNode product)
    {
      var versionTag = _pathPatternInterpreter.ExtractVersionTag(filePath);
      if (!string.IsNullOrEmpty(versionTag))
      {
        // Concatenando a tag na versão do produto na forma VERSAO-TAG
        product.Version = $"{product.Version}-{versionTag}";
      }
    }

    return node;
  }

  /// <summary>
  /// Carrega os scripts do pacote.
  /// </summary>
  /// <param name="parent">
  /// Nodo pai dos nodos a serem carregados.
  /// </param>
  /// <returns>
  /// Lista de nodos carregados.
  /// </returns>
  public Task<ScriptNode> ReadScriptFromFileAsync(string filePath)
  {
    var (name, tag) = _pathPatternInterpreter.ExtractObjectNameAndTag(filePath);
    var node = new ScriptNode
    {
      FilePath = filePath,
      Name = name,
      Tag = tag
    };
    return Task.FromResult(node);
  }

}
