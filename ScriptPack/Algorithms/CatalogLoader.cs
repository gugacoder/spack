using System.Text.Json;
using ScriptPack.Domain;
using ScriptPack.Helpers;
using ScriptPack.Model;

namespace ScriptPack.Algorithms;

/// <summary>
/// Utilitário para carregamento de catálogo a partir de uma pasta de scripts,
/// um arquivo compactado ou qualquer instância de <see cref="IDrive"/>.
/// </summary>
public class CatalogLoader
{
  private readonly PathPatternInterpreter _pathPatternInterpreter = new();

  /// <summary>
  /// Carrega os catálogos disponíveis no drive.
  /// </summary>
  /// <remarks>
  /// O ScriptPack é organizado em produto -> módulo -> pacote -> script.
  /// 
  /// Esta é a estrutura obrigatória final do catálogo.
  /// Porém, não é ncessário definir explicitamente arquivos JSON para cada
  /// um destes componentes.
  /// 
  /// Este algoritmo se encarrega de detectar quais arquivos JSON estão
  /// faltando e criar os componentes faltantes com base em algum outro
  /// arquivo JSON existente.
  /// 
  /// A decisão de qual arquivo usar como base segue a seguinte definição:
  /// 
  /// -   Se o pacote não existe, um é criado com base no módulo.
  /// -   Se o módulo não existe, um é criado com base no produto.
  /// -   Se o produto não existe, um é criado com base no catálogo.
  ///</remarks>
  /// <param name="drive">
  /// Drive a ser carregado.
  /// </param>
  /// <returns>
  /// Lista de catálogos carregados.
  /// </returns>
  public async Task<List<CatalogNode>> ReadCatalogAsync(IDrive drive)
  {
    var catalogs = await ReadNodesAsync<CatalogNode>(drive);
    var products = await ReadNodesAsync<ProductNode>(drive);
    var modules = await ReadNodesAsync<ModuleNode>(drive);
    var packages = await ReadNodesAsync<PackageNode>(drive);
    var scripts = await ReadNodesAsync<ScriptNode>(drive);

    foreach (var script in scripts)
    {
      await AdoptScriptIntoPackage(drive, script,
          packages, modules, products, catalogs);
    }

    return catalogs;
  }

  private async Task AdoptScriptIntoPackage(IDrive drive, ScriptNode script,
      List<PackageNode> packages, List<ModuleNode> modules,
      List<ProductNode> products, List<CatalogNode> catalogs)
  {
    if (script?.FilePath == null)
      throw new InvalidOperationException(
          "É necessário que o script exista fisicamente para ser processado.");

    var package = script?.Parent as PackageNode ?? packages
        .Where(n => n.FilePath != null)
        .OrderByDescending(n => n.FilePath!.Length)
        .FirstOrDefault(n =>
            script?.FilePath?.StartsWith(n.FileFolder!) == true);

    // Se o pacote já está associado a um modulo então sua árvore já está
    // completa. Podemos prosseguir daqui.
    if (package?.Parent != null)
    {
      package.Scripts.Add(script!);
      return;
    }

    // Como o pacote não está associado a um módulo, precisamos criar a sua
    // árvore completa a partir do script.

    var module = package?.Parent as ModuleNode ?? modules
        .Where(n => n.FilePath != null)
        .OrderByDescending(n => n.FilePath!.Length)
        .FirstOrDefault(n =>
            package?.FilePath?.StartsWith(n.FileFolder!) == true
            || script?.FilePath?.StartsWith(n.FileFolder!) == true);

    var product = module?.Parent as ProductNode ?? products
        .Where(n => n.FilePath != null)
        .OrderByDescending(n => n.FilePath!.Length)
        .FirstOrDefault(n =>
            module?.FilePath?.StartsWith(n.FileFolder!) == true
            || package?.FilePath?.StartsWith(n.FileFolder!) == true
            || script?.FilePath?.StartsWith(n.FileFolder!) == true);

    var catalog = product?.Parent as CatalogNode ?? catalogs
        .Where(n => n.FilePath != null)
        .OrderByDescending(n => n.FilePath!.Length)
        .FirstOrDefault(n =>
            product?.FilePath?.StartsWith(n.FileFolder!) == true
            || module?.FilePath?.StartsWith(n.FileFolder!) == true
            || package?.FilePath?.StartsWith(n.FileFolder!) == true
            || script?.FilePath?.StartsWith(n.FileFolder!) == true);

    if (product == null)
    {
      product = await ReadNodeFromFileAsync<ProductNode>(drive,
          catalog!.FilePath!);
      products.Add(product);
    }

    if (module == null)
    {
      module = await ReadNodeFromFileAsync<ModuleNode>(drive,
          product!.FilePath!);
      modules.Add(module);
    }

    if (package == null)
    {
      package = await ReadNodeFromFileAsync<PackageNode>(drive,
          module!.FilePath!);
      packages.Add(package);
    }

    if (product.Parent == null) catalog!.Products.Add(product);
    if (module.Parent == null) product.Modules.Add(module);
    if (package.Parent == null) module.Packages.Add(package);
    if (script!.Parent == null) package.Scripts.Add(script!);
  }

  /// <summary>
  /// Carrega todos os nodos de um determinado tipo a partir de um drive.
  /// </summary>
  /// <param name="drive">
  /// Drive a ser carregado.
  /// </param>
  /// <typeparam name="T">
  /// Tipo de nodo a ser carregado.
  /// </typeparam>
  /// <returns>
  /// Lista de nodos carregados.
  /// </returns>
  private async Task<List<T>> ReadNodesAsync<T>(IDrive drive)
    where T : IFileNode, new()
  {
    var nodes = new List<T>();

    if (typeof(T) == typeof(ScriptNode))
    {
      var filePaths = drive.GetFiles("/", "*.sql", SearchOption.AllDirectories);

      foreach (var filePath in filePaths)
      {
        var node = await ReadScriptFromFileAsync(drive, filePath);
        nodes.Add((T)(object)node);
      }
    }
    else
    {
      var typename = typeof(T).Name;
      if (typename.EndsWith("Node"))
      {
        typename = typename.Substring(0, typename.Length - "Node".Length);
      }
      var filename = $"{typename.ToLower()}.json";
      var filePaths = drive.GetFiles("/", filename, SearchOption.AllDirectories);

      foreach (var filePath in filePaths)
      {
        var node = await ReadNodeFromFileAsync<T>(drive, filePath);
        nodes.Add(node);
      }
    }

    return nodes;
  }

  /// <summary>
  /// Carrega o nodo do tipo especificado a partir do arquivo.
  /// </summary>
  /// <param name="drive">
  /// Drive a ser carregado.
  /// </param>
  /// <param name="filePath">
  /// Caminho do arquivo a ser carregado.
  /// </param>
  /// <typeparam name="T">
  /// Tipo de nodo a ser carregado.
  /// </typeparam>
  /// <returns>
  /// Nodo carregado.
  /// </returns>
  private async Task<T> ReadNodeFromFileAsync<T>(IDrive drive, string filePath)
    where T : IFileNode, new()
  {
    T node = new();
    try
    {
      var json = await drive.ReadAllTextAsync(filePath);
      node = JsonSerializer.Deserialize<T>(json, JsonOptions.CamelCase)!;
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
      node.Name = string.IsNullOrEmpty(name) ? drive.Name : name;
    }

    if (node is CatalogNode catalog)
    {
      catalog.Description ??= "Catálogo de scripts.";
    }

    if (node is ProductNode product)
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
  /// <param name="drive">
  /// Drive a ser carregado.
  /// </param>
  /// <param name="parent">
  /// Nodo pai dos nodos a serem carregados.
  /// </param>
  /// <returns>
  /// Lista de nodos carregados.
  /// </returns>
  private Task<ScriptNode> ReadScriptFromFileAsync(IDrive drive,
      string filePath)
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
