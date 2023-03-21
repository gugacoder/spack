using ScriptPack.Domain;
using ScriptPack.Helpers;
using ScriptPack.Model;

namespace SPack.Helpers;

public class NodeSelector
{
  /// <summary>
  /// Obtém ou define os pacotes a serem carregados.
  /// Cada pacote tem a forma:
  ///   PRODUTO[/VERSÃO[/MÓDULO[/PACOTE]]]
  /// Exemplo:
  ///   MyProduct/1.0.0/MyModule/MyPackage
  /// </summary>
  public List<string> SearchPackageCriteria { get; set; } = new();

  /// <summary>
  /// Obtém ou define os filtros de script a serem aplicados.
  /// Um filtro é um padrão de pesquisa de pastas e arquivos virtuais na
  /// árvode de nodos do catálogo.
  /// 
  /// Por exemplo, para selecionar todos os scripts da versão 1.0.0 disponível
  /// no catálogo o filtro poderia ser: **/1.0.0.
  /// </summary>
  public List<string> SearchScriptCriteria { get; set; } = new();

  /// <summary>
  /// Aplica os critérios de seleção de nodos.
  /// </summary>
  /// <param name="rootNode">Nodo raiz da pesquisa.</param>
  public INode[] SelectNodes(INode rootNode)
  {
    List<INode> selection = new();

    if (SearchPackageCriteria.Count == 0 && SearchScriptCriteria.Count == 0)
    {
      var packages = CreateDefaultSearchPackageCriteria(rootNode);
      SearchPackageCriteria.AddRange(packages);
    }

    // Selecionando pacotes de scripts.
    if (SearchPackageCriteria.Count > 0)
    {
      var scriptSearcher = new PackageSearcher(rootNode);
      foreach (var criterion in SearchPackageCriteria)
      {
        var package = scriptSearcher.SearchPackage(criterion);
        selection.Add(package);
      }
    }

    // Selecionando scripts por padrão de pesquisa no repositório.
    if (SearchScriptCriteria.Count > 0)
    {
      var treeNodeNavigator = new TreeNodeNavigator(rootNode);
      foreach (var criterion in SearchScriptCriteria)
      {
        var nodes = treeNodeNavigator.ListNodes(criterion);
        selection.AddRange(nodes);
      }
    }

    return selection.ToArray();
  }

  /// <summary>
  /// Constrói o critério de seleção padrão.
  /// Quando aplicado, o critério seleciona todos as versões mais recentes dos
  /// produtos habilitados em todos os catálogos do repositório ou da
  /// hierarquia a partir do nodo indicado.
  /// </summary>
  /// <param name="rootNode">
  /// Nodo raiz da pesquisa.
  /// Em geral um nodo de repositório ou catálogo.
  /// </param>
  /// <returns>
  /// Critério de seleção padrão.
  /// </returns>
  private string[] CreateDefaultSearchPackageCriteria(INode rootNode)
  {
    var treeNodeNavigator = new TreeNodeNavigator(rootNode);
    var packages =
        from product in treeNodeNavigator.ListNodes<ProductNode>("**")
        select $"{product.Name}/latest";
    return packages.ToArray();
  }
}
