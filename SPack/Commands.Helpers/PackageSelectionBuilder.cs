using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;
using ScriptPack.Model.Algorithms;
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

    // Definindo o encoding para leitura dos arquivos.
    var encoding = options.Encode.On ? options.Encode.Value : null;

    // Criando os critérios de filtragem dos pacotes.
    var packageIds = options.Package.Items
        .SelectMany(x => x.Split(','))
        .Select(x => x.Trim())
        .ToArray();

    // Criando critério de pesquisa de caminhos.
    var searchCriteria = options.Search.Items.ToList();

    // Acrescentando critério de pesquisa de list ou show caso estejam ativos.
    if (options.List.On && !string.IsNullOrEmpty(options.List.Value))
    {
      searchCriteria.Add(options.List.Value);
    }
    if (options.Show.On && !string.IsNullOrEmpty(options.Show.Value))
    {
      searchCriteria.Add(options.Show.Value);
    }

    // Acrescentando critérios de pesquisa extras.
    _extraSearchCriteria.ForEach(c => searchCriteria.Add(c));

    // Acrescentando critério de pesquisa padrão.
    if (searchCriteria.Count == 0)
    {
      searchCriteria.Add("**");
    }

    //
    // Realizando a seleção de pacotes
    //
    // Primeiro vamos pesquisar os nodos aplicando os critérios de pesquisa.
    // Depois vamos extrair dos nodos os pacotes pela aplicação do filtro.
    //

    // Criando um navegador para exploração dos catálogos.
    var catalogs = _repository.Descendants<CatalogNode>().ToArray();
    INode selectedNode = catalogs.Length == 1 ? catalogs[0] : _repository;
    var repositoryNavigator = new TreeNodeNavigator(selectedNode);

    // Pesquisando os itens dos catálogos.
    List<INode> nodesFound = (
        from criteria in searchCriteria
        from node in repositoryNavigator.ListNodes(criteria)
        select node
    ).ToList();

    // Filtrando os pacotes.
    if (packageIds.Length > 0)
    {
      // O filtro tem a forma:
      //    [<produto>/][<modulo>:...]<pacote>[@<versão do produto>]
      // Exemplos:
      //    Api                 - pacote Api
      //    Api@1.0.0           - pacote Api na versão 1.0.0
      //    Module1:Api@1.0.0   - pacote Api do módulo Module1 na versão 1.0.0
      //    MySystem/Api@1.0.0  - pacote Api produto MySystem na versão 1.0.0
      // Como as partes produto, módulo e versão são opcionais, o parsing
      // do filtro pode resultar em múltiplos pacotes.
      var packageIdHandler = new PackageIdHandler();
      nodesFound = (
          from id in packageIds
          from node in packageIdHandler.ParsePackageId(id, nodesFound)
          select node
      ).Cast<INode>().ToList();
    }

    return nodesFound
        .Distinct()
        .OrderBy(x => x.Path, new PathComparer())
        .ToList();
  }
}
