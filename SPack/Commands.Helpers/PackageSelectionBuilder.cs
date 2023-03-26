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
  private bool _addValidators;

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
  /// Adiciona validador de dependência cíclica e outros validadores comuns
  /// disponíveis.
  /// </summary>
  public void AddValidators()
  {
    _addValidators = true;
  }

  /// <summary>
  /// Constrói a seleção de nodos.
  /// </summary>
  /// <returns>
  /// Lista de nodos selecionados.
  /// </returns>
  public async Task<List<INode>> BuildPackageSelectionAsync()
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
    var catalogPath = options.Catalog.Value;
    var builtInOn = options.BuiltIn.On;
    var encoding = options.Encode.On ? options.Encode.Value : null;

    // Tratando opções.
    if (!string.IsNullOrEmpty(mainCriteria))
    {
      searchCriteria.Add(mainCriteria);
    }
    if (searchCriteria.Count == 0 && packageSearchCriteria.Count == 0)
    {
      searchCriteria.Add("**");
    }

    // Criando um Drive para exploração dos catálogos.
    var drive = Drive.GetDrive(catalogPath);

    // Carregando os catálogos em um repositório.
    var repositoryBuilder = new RepositoryBuilder();
    repositoryBuilder.AddDrive(drive);
    repositoryBuilder.AddDefaultEncoding(encoding: Drive.DefaultEncoding);
    if (_addValidators)
    {
      repositoryBuilder.AddDependencyDetector();
      repositoryBuilder.AddCircularDependencyDetector();
    }
    if (builtInOn)
    {
      repositoryBuilder.AddBuiltInCatalog();
    }
    var repository = await repositoryBuilder.BuildRepositoryAsync();

    // Criando um navegador para exploração dos catálogos.
    var catalogs = repository.Descendants<CatalogNode>().ToArray();
    INode selectedNode = catalogs.Length == 1 ? catalogs[0] : repository;
    var repositoryNavigator = new TreeNodeNavigator(selectedNode);

    // Pesquisando os itens dos catálogos.
    List<INode> nodes = new();

    foreach (var criteria in searchCriteria)
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
