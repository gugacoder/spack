using System.Text.Json;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;

namespace ScriptPack.Algorithms;

//
//  NT-01 (Nota Técnica #1)
//
//  O algoritmo abaixo é responsável por carregar os catálogos de scripts na
//  sequência catálogo -> produto -> versão -> módulo -> pacote -> script.
//
//  Em geral, existem arquivos de definição de todos estes nodos exceto para
//  definição do produto. Para fins de facilitade de uso, o arquivo do produto,
//  chamado de "product.json", é o mesmo arquivo de definição de versão.
//
//  Os arquivos gerais são:
//
//  -   Catálogo: "catalog.json" (obrigatório)
//  -   Versão: "product.json" (opcional)
//  -   Módulo: "module.json" (opcional)
//  -   Pacote: "package.json" (opcional)
//
//  Exemplo:
//      /catalogo.json
//      /MeuProduto/tags/1.0.0/product.json
//      /MeuProduto/tags/1.0.0/MeuModulo/module.json
//      /MeuProduto/tags/1.0.0/MeuModulo/MeuPacote/package.json
//      /MeuProduto/trunk/product.json
//      /MeuProduto/trunk/MeuModulo/module.json
//      /MeuProduto/trunk/MeuModulo/MeuPacote/package.json
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
  /// Porém, não é necessário definir explicitamente arquivos JSON para cada
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
    //
    // Leia a NT-01 no início deste arquivo para entender esta parte.
    //
    // Note que não estamos carregando os produtos. Os arquivos de produtos,
    // chamados de "product.json", ocorrem várias vezes na estrtura de
    // arquivos, uma vez para cada versão do produto. No mesmo arquivo é
    // definido o nome do produto e o número de versão.
    //
    // O que fazemos é carregar os arquivos de produtos diretamente nas
    // instancias de versão e posteriormente criamos as instâncias de
    // produto pelo agrupamento destas versões, assim termos apenas um arquivo
    // de produto por produto e não um arquivo de produto por versão.
    // 
    var catalogs = await ReadNodesAsync<CatalogNode>(drive);
    var products = new List<ProductNode>();
    var versions = await ReadNodesAsync<VersionNode>(drive);
    var modules = await ReadNodesAsync<ModuleNode>(drive);
    var packages = await ReadNodesAsync<PackageNode>(drive);
    var scripts = await ReadNodesAsync<ScriptNode>(drive);

    foreach (var script in scripts)
    {
      await AdoptScriptIntoPackage(drive, script,
          packages, modules, versions, products, catalogs);
    }

    //
    // Leia a NT-01 no início deste arquivo para entender esta parte.
    //
    // Depois de carregar produtos com base em versões podemos modificar os
    // nomes das versões para que reflitam a versão do produto.
    //
    versions.ForEach(v => v.Name = v.Version);

    // Obtendo apenas catálogos com pelo menos um script definido.
    catalogs = catalogs.Where(c => c.Descendants<ScriptNode>().Any()).ToList();
    catalogs.ForEach(c => c.Drive = drive);
    return catalogs;
  }

  /// <summary>
  /// Adota um script em um pacote existente ou cria um novo pacote com base
  /// em um módulo existente ou cria um novo módulo com base em um produto
  /// existente ou cria um novo produto com base em um catálogo existente.
  /// </summary>
  /// <remarks>
  /// Durante a execução deste método, a estrtura de árvore de um script é
  /// organizada e associada. O resultado final da execução deste método é
  /// uma árvore de scripts completa, do produto aos scripts.
  /// </remarks>
  private async Task AdoptScriptIntoPackage(IDrive drive, ScriptNode script,
      List<PackageNode> packages, List<ModuleNode> modules,
      List<VersionNode> versions, List<ProductNode> products,
      List<CatalogNode> catalogs)
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

    var version = module?.Parent as VersionNode ?? versions
        .Where(n => n.FilePath != null)
        .OrderByDescending(n => n.FilePath!.Length)
        .FirstOrDefault(n =>
            module?.FilePath?.StartsWith(n.FileFolder!) == true
            || package?.FilePath?.StartsWith(n.FileFolder!) == true
            || script?.FilePath?.StartsWith(n.FileFolder!) == true);

    var product = version?.Parent as ProductNode ?? products
        .Where(n => n.FilePath != null)
        .OrderByDescending(n => n.FilePath!.Length)
        .FirstOrDefault(n =>
            version?.FilePath?.StartsWith(n.FileFolder!) == true
            || module?.FilePath?.StartsWith(n.FileFolder!) == true
            || package?.FilePath?.StartsWith(n.FileFolder!) == true
            || script?.FilePath?.StartsWith(n.FileFolder!) == true);

    var catalog = product?.Parent as CatalogNode ?? catalogs
        .Where(n => n.FilePath != null)
        .OrderByDescending(n => n.FilePath!.Length)
        .FirstOrDefault(n =>
            product?.FilePath?.StartsWith(n.FileFolder!) == true
            || version?.FilePath?.StartsWith(n.FileFolder!) == true
            || module?.FilePath?.StartsWith(n.FileFolder!) == true
            || package?.FilePath?.StartsWith(n.FileFolder!) == true
            || script?.FilePath?.StartsWith(n.FileFolder!) == true);

    if (version == null)
    {
      version = await ReadNodeFromFileAsync<VersionNode>(drive,
          catalog!.FilePath!);
      versions.Add(version);
    }

    if (product == null)
    {
      //
      // Leia a NT-01 no início deste arquivo para entender esta parte.
      //
      // Note que o arquivo de produto é lido a partir do arquivo de versão.
      //
      product = await ReadNodeFromFileAsync<ProductNode>(drive,
          version!.FilePath!);
      products.Add(product);
    }

    if (module == null)
    {
      module = await ReadNodeFromFileAsync<ModuleNode>(drive,
          version!.FilePath!);
      modules.Add(module);
    }

    if (package == null)
    {
      package = await ReadNodeFromFileAsync<PackageNode>(drive,
          module!.FilePath!);
      packages.Add(package);
    }

    //
    // Acrescentando relações de parentesco.
    //
    if (product.Parent == null) catalog!.Products.Add(product);
    if (version.Parent == null) product!.Versions.Add(version);
    if (module.Parent == null) version.Modules.Add(module);
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
    string[] filePaths;

    if (typeof(T) == typeof(ScriptNode))
    {
      filePaths = drive.GetFiles("/", "*.sql", SearchOption.AllDirectories);

      foreach (var filePath in filePaths)
      {
        var node = await ReadScriptFromFileAsync(drive, filePath);
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

    var fileName = $"{typeName.ToLower()}.json";

    filePaths = drive.GetFiles("/", fileName, SearchOption.AllDirectories);
    foreach (var filePath in filePaths)
    {
      var node = await ReadNodeFromFileAsync<T>(drive, filePath);
      nodes.Add(node);
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
