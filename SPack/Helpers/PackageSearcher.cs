using ScriptPack.Domain;
using ScriptPack.Helpers;
using ScriptPack.Model;

namespace SPack.Helpers;

/// <summary>
/// Utilitário para encontrar no repositório as versões corretas de pacotes de
/// scripts conforme os critérios de busca informados.
/// </summary>
public class PackageSearcher
{
  private readonly INode _rootNode;

  /// <summary>
  /// Cria uma nova instância de <see cref="PackageSearcher"/> com o nodo raiz
  /// da árvore de nodos do nodo de navegação especificado.
  /// </summary>
  /// <param name="treeNodeNavigator">
  /// Nó de navegação da árvore de nodos.
  /// </param>
  public PackageSearcher(INode rootNode)
  {
    _rootNode = rootNode;
  }

  /// <summary>
  /// Procura um pacote com base nos critérios de pesquisa fornecidos.
  /// </summary>
  /// <param name="searchPackage">O pacote de pesquisa a ser procurado.</param>
  /// <returns>O nodo correspondente ao pacote de pesquisa.</returns>
  public INode SearchPackage(string searchPackage)
  {
    var parts = searchPackage.Split('/');
    var productName = parts[0];
    var versionName = parts.Length > 1 ? parts[1] : "latest";
    var moduleName = parts.Length > 2 ? parts[2] : null;
    var packageName = parts.Length > 3 ? parts[3] : null;

    //
    // SELECIONANDO O PRODUTO
    //

    var products = _rootNode
        .Descendants<ProductNode>()
        .Where(x => x.Name == productName)
        .ToArray();

    //
    // SELECIONADO A VERSÃO
    //

    if (versionName == "latest")
    {
      versionName = (
          from p in products
          from v in p.Descendants<VersionNode>()
          orderby v.Version, new VersionComparer() descending
          select v.Version
      ).FirstOrDefault()
          ?? throw new Exception(
              $"Nenhuma versão encontrada para o produto: {productName}");
    }

    var versions = products
        .SelectMany(x => x.Descendants<VersionNode>())
        .Where(x => x.Name == versionName)
        .ToArray();

    if (versions.Length == 0) throw new Exception(
        $"Nenhuma versão encontrada para o produto: {productName}");
    if (versions.Length > 1) throw new Exception(
        $"Mais de uma versão encontrada para o produto: {productName}");

    var version = versions[0];
    if (moduleName == null) return version;

    //
    // SELECIONANDO O MÓDULO
    //

    var modules = version
        .Descendants<ModuleNode>()
        .Where(x => x.Name == moduleName)
        .ToArray();

    if (modules.Length == 0) throw new Exception(
        $"Nenhum módulo encontrado para o produto: {productName}");
    if (modules.Length > 1) throw new Exception(
        $"Mais de um módulo encontrado para o produto: {productName}");

    var module = modules[0];
    if (packageName == null) return module;

    //
    // SELECIONANDO O PACOTE
    //

    var packages = module
        .Descendants<PackageNode>()
        .Where(x => x.Name == packageName)
        .ToArray();

    if (packages.Length == 0) throw new Exception(
        $"Nenhum pacote encontrado para o produto: {productName}");
    if (packages.Length > 1) throw new Exception(
        $"Mais de um pacote encontrado para o produto: {productName}");

    return packages[0];
  }
}
