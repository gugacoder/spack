using System.Xml.Linq;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using System.Collections;
using Newtonsoft.Json;
using static System.IO.SearchOption;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Utilitário para instanciação dos nodos na hierarquia de um pacote.
/// </summary>
internal class CatalogLoaderNodeFactory
{
  private PathPatternInterpreter _pathPatternInterpreter = new();

  /// <summary>
  /// Cria a árvore de nodos do catálogo a partir das hierarquias de pacotes
  /// fornecidas.
  /// </summary>
  /// <param name="drive">
  /// O objeto IDrive que representa o Drive que contém os arquivos de
  /// configuração.
  /// </param>
  /// <param name="packagePathHierarchies">
  /// A lista de hierarquias de pacotes a serem processadas.
  /// </param>
  /// <returns>A lista de nodos de catálogo criados.</returns>
  public async Task<List<CatalogNode>> CreateNodeTreeAsync(IDrive drive,
      (Type Type, string Path)[][] packagePathHierarchies)
  {
    var nodeCache = new Dictionary<(Type, string), IFileNode>();

    foreach (var hierarchy in packagePathHierarchies)
    {
      var configFiles = hierarchy.Where(e => e.Type != typeof(ScriptNode));
      var scriptFolders = hierarchy.Where(e => e.Type == typeof(ScriptNode));

      IFileNode currentNode = null!;

      foreach (var (type, path) in configFiles)
      {
        // Verifica se o nodo já foi carregado a partir do cache.
        if (nodeCache.TryGetValue((type, path), out var cachedNode))
        {
          currentNode = cachedNode;
          continue;
        }

        currentNode = await LoadOrCreateNodeAsync(drive, currentNode, type,
            path);

        if (currentNode is CatalogNode catalogNode)
        {
          catalogNode.Drive = drive;
        }

        // Adiciona o ndo ao cache.
        nodeCache[(type, path)] = currentNode;
      }

      PackageNode packageNode = (PackageNode)currentNode;
      foreach (var (type, path) in scriptFolders)
      {
        var scripts = LoadScriptNodes(drive, path, packageNode);
      }
    }

    // Destacando os catálogos que contêm produtos.
    var catalogs = nodeCache.Values
        .OfType<CatalogNode>()
        .Where(c => c.Products.Count > 0)
        .ToList();

    // Depois de carregado, a árvore de nodos pode conter catálogos que não
    // correspondem a arquivos de configuração do tipo `-catalog.jsonc`.
    // Isto que dizer que o repositório contém pacotes não estruturado
    // corretamente a partir de um catálogo definido.
    // 
    // Para uma melhor organização da árvore vamos mesclar estes catálogos
    // dentro de um apenas e utilizá-lo como raiz de todos os pacotes.

    var unidentifiedCatalogs = catalogs
        .Where(c => c.FilePath?.EndsWith("/-catalog.jsonc") != true)
        .ToList();
    if (unidentifiedCatalogs.Count > 1)
    {
      var products = unidentifiedCatalogs
          .SelectMany(c => c.Products)
          .ToList();

      var connections = (
          from catalog in unidentifiedCatalogs
          from connection in catalog.Connections
          group connection by connection.Name into g
          select g.First()
      ).ToArray();

      var unifiedCatalog = new CatalogNode
      {
        Name = drive.Name,
        Title = unidentifiedCatalogs
            .Select(c => c.Title)
            .FirstOrDefault(t => !string.IsNullOrEmpty(t)),
        Description = unidentifiedCatalogs
            .Select(c => c.Description)
            .FirstOrDefault(d => !string.IsNullOrEmpty(d)),
        Connections = new(connections),
        Drive = drive,
        FilePath = "/-catalog.jsonc",
        Products = new(products)
      };

      catalogs.RemoveAll(c => unidentifiedCatalogs.Contains(c));
      catalogs.Add(unifiedCatalog);
    }

    return catalogs;
  }

  /// <summary>
  /// Carrega ou cria um nodo de arquivo assíncrono.
  /// </summary>
  /// <param name="drive">O drive a ser usado para carregar o arquivo.</param>
  /// <param name="parent">O nodo pai do nodo a ser carregado ou criado.</param>
  /// <param name="type">O tipo do nodo a ser carregado ou criado.</param>
  /// <param name="filePath">O caminho do arquivo do nodo.</param>
  /// <param name="parent">
  /// O nodo pai do nodo a ser carregado ou criado.
  /// </param>
  /// <returns>O nodo carregado ou criado.</returns>
  private async Task<IFileNode> LoadOrCreateNodeAsync(IDrive drive,
      INode parent, Type type, string filePath)
  {
    IFileNode? node = null;

    if (drive.FileExists(filePath))
    {
      node = await ReadConfigFileAsync(drive, parent, type, filePath);
    }

    node ??= (IFileNode)Activator.CreateInstance(type)!;
    node.FilePath = filePath;

    if (parent is not null)
    {
      AddToParent(parent, node);
    }

    if (node is VersionNode version)
    {
      if (string.IsNullOrEmpty(version.Version))
      {
        version.Version = VersionNode.UnidentifiedVersion;
      }
      version.Name = version.Version;
    }

    if (string.IsNullOrEmpty(node.Name))
    {
      node.Name = NameNode(node, filePath);
    }

    return node;
  }

  /// <summary>
  /// Algoritmo de nomeação automática de nodos.
  /// </summary>
  /// <remarks>
  /// Em geral, o nome do nodo é definido pelo próprio arquivo de configuração
  /// usado no carregamento do nodo, como -package.jsonc, -module.jsonc, etc.
  /// Quando o arquivo de configuração não existe, o nome do nodo é definido
  /// de forma automática para obter a melhor definição possível.
  /// </remarks>
  /// <param name="node">
  /// O nodo que está sendo nomeado.
  /// </param>
  /// <param name="filePath">
  /// O caminho do arquivo de configuração do nodo.
  /// </param>
  /// <returns>
  /// O nome do nodo.
  /// </returns>
  private string NameNode(IFileNode node, string filePath)
  {
    // Pacotes recebem o nome "Package-INDICE", sendo INDICE o índice do
    // pacote na lista de pacotes do catálogo.
    if (node is PackageNode)
    {
      if (filePath.EndsWith("/-package.jsonc"))
      {
        return Path.GetFileName(Path.GetDirectoryName(filePath))!;
      }

      int index = node.Parent!.Children()
          .Select((item, index) => new { item, index })
          .FirstOrDefault(x => x.item == node)?.index ?? -1;

      return (index == 0) ? "Package" : $"Package-{index}";
    }

    if (node is ModuleNode)
    {
      if (filePath.EndsWith("/-module.jsonc"))
      {
        return Path.GetFileName(Path.GetDirectoryName(filePath))!;
      }

      int index = node.Parent!.Children()
          .Select((item, index) => new { item, index })
          .FirstOrDefault(x => x.item == node)?.index ?? -1;

      return (index == 0) ? "Module" : $"Module-{index}";
    }

    if (node is VersionNode version)
    {
      return !string.IsNullOrWhiteSpace(version.Version)
          ? version.Version
          : VersionNode.UnidentifiedVersion;
    }

    if (node is ProductNode)
    {
      if (filePath.EndsWith("/-product.jsonc"))
      {
        var name = Path.GetFileName(Path.GetDirectoryName(filePath))!;
        if (name != "trunk" && name != "branches" && name != "tags")
        {
          return name;
        }
      }

      var catalog = node.Ancestors<CatalogNode>().FirstOrDefault();
      if (!string.IsNullOrWhiteSpace(catalog?.Name))
      {
        return catalog.Name;
      }

      int index = node.Parent!.Children()
          .Select((item, index) => new { item, index })
          .FirstOrDefault(x => x.item == node)?.index ?? -1;

      return (index == 0) ? "Product" : $"Product-{index}";
    }

    return Path.GetFileName(Path.GetDirectoryName(filePath))!;
  }

  /// <summary>
  /// Carrega o nodo do arquivo de configuração JSON.
  /// </summary>
  /// <param name="drive">
  /// O objeto IDrive que representa o Drive que contém os arquivos de
  /// configuração.
  /// </param>
  /// <param name="parent">
  /// O nodo pai do nodo a ser carregado ou criado.
  /// </param>
  /// <param name="type">
  /// O tipo do nodo a ser carregado ou criado.
  /// </param>
  /// <param name="filePath">
  /// O caminho do arquivo do nodo.
  /// </param>
  /// <returns>
  /// O nodo carregado ou criado.
  /// </returns>
  private async Task<IFileNode?> ReadConfigFileAsync(IDrive drive, INode parent,
      Type type, string filePath)
  {
    try
    {
      // Carregando o arquivo JSON...
      var jsonString = await drive.ReadAllTextAsync(filePath);
      var @object = JsonConvert.DeserializeObject(jsonString, type,
          JsonOptions.CamelCase);
      var node = (IFileNode)@object!;
      node.Parent = parent;
      node.FilePath = Path.GetDirectoryName(filePath)!;
      return node;
    }
    catch (Exception ex)
    {
      throw new Exception($"Erro ao carregar o arquivo {filePath}", ex);
    }
  }

  /// <summary>
  /// Carrega os nodos de script a partir da unidade de armazenamento
  /// especificada.
  /// </summary>
  /// <param name="drive">A unidade de armazenamento a ser consultada.</param>
  /// <param name="scriptFolder">A pasta onde os scripts se encontram.</param>
  /// <returns>
  /// Uma lista de ScriptNode representando os scripts encontrados na unidade de
  /// armazenamento.
  /// </returns>
  private List<ScriptNode> LoadScriptNodes(IDrive drive, string scriptFolder,
      PackageNode parentNode)
  {
    var scripts = new List<ScriptNode>();

    var filePaths = drive.GetFiles(scriptFolder, "*.sql", TopDirectoryOnly);
    foreach (var filePath in filePaths)
    {
      var (name, tag) = _pathPatternInterpreter.ExtractObjectNameAndTag(
          filePath);

      var script = new ScriptNode
      {
        FilePath = filePath,
        Name = name,
        Tag = tag
      };

      parentNode.Scripts.Add(script);

      scripts.Add(script);
    }

    return scripts;
  }

  /// <summary>
  /// Adicionando o nodo à lista de nodos do nodo pai.
  /// </summary>
  /// <param name="parentNode">Nodo pai.</param>
  /// <param name="childNode">Nodo filho.</param>
  /// <remarks>
  /// A propriedade lista no nodo pai tem o nome do tipo do nodo filho
  /// no plural.
  /// Por exemplo, se o nodo filho for um "ProductNode", a propriedade
  /// lista no nodo pai será "Products".
  /// </remarks>
  private void AddToParent(INode parentNode, IFileNode childNode)
  {
    var type = childNode.GetType();
    var propertyName = $"{type.Name[..^4]}s";
    var property = parentNode.GetType().GetProperty(propertyName)!;
    var children = (IList)property.GetValue(parentNode)!;
    children.Add(childNode);
  }
}
