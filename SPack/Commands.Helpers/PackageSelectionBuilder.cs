using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;
using SPack.Prompting;

namespace SPack.Commands.Helpers;

/// <summary>
/// Utilitário para construção de seleção de nodos.
/// </summary>
public class PackageSelectionBuilder
{
  private CommandLineOptions _options = null!;
  private RepositoryNode _repository = null!;
  private readonly List<string> _extraSearchCriteria = new();

  /// <summary>
  /// Adiciona critérios de seleção a partir das opções de linha de comando.
  /// </summary>
  /// <param name="options">
  /// Opções de linha de comando.
  /// </param>
  public void AddOptions(CommandLineOptions options)
  {
    _options = options;
  }

  /// <summary>
  /// Adiciona um repositório de nodos.
  /// </summary>
  /// <param name="repository">
  /// Repositório de nodos.
  /// </param>
  public void AddRepository(RepositoryNode repository)
  {
    _repository = repository;
  }

  /// <summary>
  /// Adiciona um critério de pesquisa.
  /// </summary>
  /// <param name="criteria">
  /// Critério de pesquisa.
  /// </param>
  public void AddSearchCriteria(string criteria)
  {
    _extraSearchCriteria.Add(criteria);
  }

  /// <summary>
  /// Constrói a seleção de nodos.
  /// </summary>
  /// <param name="repository">
  /// Repositório com os catálogos carregados.
  /// </param>
  /// <returns>
  /// Lista de nodos selecionados.
  /// </returns>
  public List<INode> BuildPackageSelection()
  {
    var options = _options;

    // Coletando opcoes.
    var mainCriteria = options switch
    {
      { List: { On: true } } => options.List.Value,
      { Show: { On: true } } => options.Show.Value,
      _ => null
    };

    var searchCriteria = options.Search.Items;
    var packageSearchCriteria = options.Package.Items;
    var encoding = options.Encode.On ? options.Encode.Value : null;

    // Criando criterio de pesquisa
    var allSearchCriteria = searchCriteria.ToList();
    if (!string.IsNullOrEmpty(mainCriteria))
    {
      allSearchCriteria.Add(mainCriteria);
    }
    _extraSearchCriteria.ForEach(c => allSearchCriteria.Add(c));
    if (allSearchCriteria.Count == 0 && packageSearchCriteria.Count == 0)
    {
      allSearchCriteria.Add("**");
    }

    // Criando um navegador para exploração dos catálogos.
    var catalogs = _repository.Descendants<CatalogNode>().ToArray();
    INode selectedNode = catalogs.Length == 1 ? catalogs[0] : _repository;
    var repositoryNavigator = new TreeNodeNavigator(selectedNode);

    // Pesquisando os itens dos catálogos.
    List<INode> nodes = new();

    foreach (var criteria in allSearchCriteria)
    {
      nodes.AddRange(repositoryNavigator.ListNodes(criteria));
    }

    foreach (var criteria in packageSearchCriteria)
    {
      if (criteria.Contains("/latest"))
      {
        var productName = criteria.Split('/').First();
        var products = repositoryNavigator
            .ListNodes<ProductNode>($"**/{productName}/**");
        var versions = products
            .SelectMany(p => p.Descendants<VersionNode>())
            .Select(v => v.Version)
            .OrderBy(v => v, new VersionComparer())
            .ToArray();
        if (!versions.Contains("latest"))
        {
          var latestVersion = versions.LastOrDefault();
          if (latestVersion is not null)
          {
            var pattern = criteria.Replace("latest", latestVersion);
            nodes.AddRange(repositoryNavigator.ListNodes($"**/{pattern}/**"));
            continue;
          }
        }
      }

      nodes.AddRange(repositoryNavigator.ListNodes($"**/{criteria}/**"));
    }

    return nodes
        .Distinct()
        .OrderBy(x => x.Path, new PathComparer())
        .ToList();
  }
}
