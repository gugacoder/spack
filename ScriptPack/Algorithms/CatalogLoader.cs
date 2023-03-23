using System.Xml.Linq;
using System.Text.Json;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using System.Collections;
using static System.IO.SearchOption;

namespace ScriptPack.Algorithms;

public class CatalogLoader
{
  private PathPatternInterpreter _pathPatternInterpreter = new();

  /// <summary>
  /// Carrega um array de nodos de catálogo e todos os seus subnodos a partir do
  /// <see cref="IDrive"/> fornecido.
  /// </summary>
  /// <param name="drive">
  /// O objeto <see cref="IDrive"/> que representa o driver a ser usado para
  /// carregar os nodos de catálogo.
  /// </param>
  /// <returns>
  /// Um array de objetos <see cref="CatalogNode"/>, representando os nodos de
  /// catálogo e seus subnodos.
  /// </returns>
  public async Task<CatalogNode[]> LoadCatalogsAsync(IDrive drive)
  {
    //
    // A lista de caminhos contém os arquivos de descrição da estrutura do
    // catálogo e os caminhos de pastas de scripts. Pastas são terminadas em
    // barra (/) para diferenciação.
    // Exemplo:
    //    /pasta/catalog.json
    //    /pasta/catalog/package.json
    //    /pasta/catalog/scripts/
    //

    List<string> paths = GetStructuralPaths(drive);

    //
    // A lista de pastas de scripts aninhadas contém pastas de scripts que não
    // representam um novo pacote de scripts mas apenas uma suborganização dos
    // scripts de um pacote.
    // Por exemplo, considere um pacote Xyz contendo uma série de scripts
    // separados em subpastas por conveniência:
    // 
    //    /pasta/package.json
    //    /pasta/sc-001.sql
    //    /pasta/sc-002.sql
    //    /pasta/create-tables/sc-003-tb_one.sql
    //    /pasta/create-tables/sc-004-tb_two.sql
    //    /pasta/create-tables/sc-005-tb_three.sql
    //    /pasta/updates/sc-006-update_1.sql
    //    /pasta/updates/sc-007-update_2.sql
    // 
    // As pastas de scripts aninhadas não modificam a estrutura do catálogo mas
    // apenas a forma como os scripts são organizados.
    // 
    // Estamos destacando estas pastas e as removendo da lista de caminhos.
    // Mais tarede no algoritmo nodos a reintroduziremos da estrutura para que
    // seus scripts sejam carregados.
    //

    List<(string Parent, string Folder)> nestedScriptFolders =
        ExtractNestedScriptFolders(paths);

    paths.RemoveAll(p => nestedScriptFolders.Any(f => f.Folder == p));

    // Lista de listas representando a estrutura do pacote.
    // Cada lista na lista externa representa uma pasta no pacote e contém os
    // caminhos de arquivo dos arquivos nessa pasta e suas pastas ancestrais.
    // O caminho começa pelo catálogo e termina na própria pasta de scripts.
    // Note que apenas os arquivos vistos na estrutura ancestral da pasta são
    // destacados. Esta lista pode estar incompleta já que a organização de
    // pastas do ScriptPack é bem flexível.
    // De uma forma geral a lista de caminhos terá uma aparência como esta:
    //    /catalog.json
    //    /catalog/product.json
    //    /catalog/product/module.json
    //    /catalog/product/module/package.json
    //    /catalog/product/module/scripts/

    List<List<string>> packagePathStructures =
        OrganizePackagePathStructures(paths);

    // A estrutura obtida acima pode esta incompleta já que a organização de
    // pastas do ScriptPack é bem flexível.
    // O algoritmo abaixo valida esta estrutura e acrescenta nela os arquivos
    // de configuração faltantes, sendo eles:
    // -  catalog.json
    // -  product.json
    // -  module.json
    // -  package.json
    // 
    // O resultado final é uma lista organizada hierarquicamente contendo
    // todos os arquivos de configuração necessários e os seus respectivos
    // tipos para que possam ser finalmente construídos.
    // Exemplo:
    //    ( CatalogNode, "/catalog.json" )
    //    ( ProductNode, "/catalog/product.json" )
    //    ( ModuleNode, "/catalog/product/module.json" )
    //    ( PackageNode, "/catalog/product/module/package.json" )
    //    ( ScriptNode, "/catalog/product/module/scripts/" )
    // Note que módulo pode ocorrer várias vezes criando uma estrtura de odos
    // aninhados.

    List<List<(Type Type, string Path)>> packagePathHierarchies =
        CreatePackagePathHierarchies(packagePathStructures);

    // Agora, com a estrutura hierarquica completa, nodos podemos devolver
    // para a lista as pastas de scripts aninhados, para que possam ser
    // carregados.

    foreach (var (parent, folder) in nestedScriptFolders)
    {
      var hierarchy = packagePathHierarchies.FirstOrDefault(
          hierarchy => hierarchy.Any(e => e.Path == parent))!;

      // Note que as pastas de scripts aninhas estão sendo incluídas depois da
      // pasta de scripts raiz.
      hierarchy.Add((typeof(ScriptNode), folder));
    }

    // foreach (var hierarchy in packagePathHierarchies)
    // {
    //   var folder = hierarchy.Last().Path;
    //   Console.WriteLine(folder);
    //   foreach (var (type, path) in hierarchy)
    //   {
    //     Console.WriteLine($"  {type.Name} -> {path}");
    //   }
    // }

    // As hierarquias de pacote agora contém informação sobre todos os nodos
    // necessários, na ordem de precedência correta, isto é:
    //    Catalog -> Product -> Version -> Module -> Package -> Script
    // Sendo que módulos podem ocorrer várias vezes, criando uma estrutura de
    // módulos e submódulos.
    // E contém ainda pastas de scripts aninhadas, quando aplicável.
    //
    // Agora podemos finalmente instanciar os nodos da hieraquia do pacote.

    List<CatalogNode> catalogs = await CreateCatalogNodeTreeAsync(
        drive, packagePathHierarchies);

    // foreach (var catalog in catalogs)
    // {
    //   foreach (var node in catalog.DescendantsAndSelf())
    //   {
    //     Console.Write(new string(' ', node.Ancestors().Count() * 2));
    //     Console.WriteLine(node);
    //   }
    // }

    return catalogs.ToArray();
  }

  /// <summary>
  /// Obtém a lista de todos os caminhos de interesse, incluindo arquivos com as
  /// extensões .json e .sql e os arquivos legados com extensão .info.
  /// </summary>
  /// <remarks>
  /// A estrutura do ScriptPack inclui os seguintes arquivos:
  /// - catalog.json
  /// - product.json
  /// - module.json
  /// - package.json
  /// </remarks>
  /// <param name="drive">
  /// O objeto IDrive que representa o driver a ser analisado.
  /// </param>
  /// <returns>
  /// A lista de caminhos de interesse encontrados.
  /// Caminhos de pastas terminam com uma barra.
  /// Exemplo:
  ///     /pasta/catalog.json
  ///     /pasta/catalog/package.json
  ///     /pasta/catalog/scripts/
  /// </returns>
  private List<string> GetStructuralPaths(IDrive drive)
  {
    var paths = new List<string>();

    // Nova estrutura
    var novosArquivos = new List<string> {
        "catalog.json",
        "product.json",
        "module.json",
        "package.json"
    };
    foreach (var arquivo in novosArquivos)
    {
      paths.AddRange(drive.GetFiles("/", arquivo, AllDirectories));
    }

    // Pastas de scripts
    paths.AddRange(drive
        .GetDirectories("/", "*", AllDirectories)
        .SelectMany(pasta => drive.GetFiles(pasta, "*.sql", AllDirectories))
        .Select(arquivo => $"{Path.GetDirectoryName(arquivo)}/")
        .Distinct());

    return paths;
  }

  /// <summary>
  /// Extrai pastas aninhadas de uma lista de caminhos de arquivos.
  /// </summary>
  /// <param name="paths">Lista de caminhos de arquivos.</param>
  /// <remarks>
  /// As pastas aninhadas são pastas criadas para organizar scripts, mas
  /// ainda fazem parte do mesmo pacote. Para a análise realizada por este
  /// método, apenas a pasta raiz de cada pacote é relevante.
  /// 
  /// Por exemplo, considere a seguinte lista de caminhos:
  ///      /MyPackage/pakage.json
  ///      /MyPackage/sc-001.sql
  ///      /MyPackage/SubPasta/sc-002.sql
  /// 
  /// Neste caso, o método considera a pasta "SubPasta" como uma pasta aninhada
  /// e inclui apenas a pasta raiz "MyPackage" na lista de pastas aninhadas.
  /// </remarks>
  /// <returns>Uma lista de tuplas representando pastas aninhadas.</returns> 
  private List<(string Parent, string Folder)> ExtractNestedScriptFolders(
      List<string> paths)
  {
    List<(string Parent, string Folder)> nestedScriptFolders = new();

    var folders = paths.Where(path => path.EndsWith("/"));
    var files = paths.Where(path => !path.EndsWith("/"));

    // Loop através de cada caminho para determinar se ele é aninhado ou não
    foreach (var targetFolder in folders)
    {
      // Encontre a pasta pai em potencial da pasta alvo
      var potentialParentFolder = (
          from folder in folders.Except(new[] { targetFolder })
          where targetFolder.StartsWith(folder)
          orderby folder.Length descending
          select folder
      ).FirstOrDefault();

      // Se não houver pasta pai em potencial, vá para a próxima iteração
      if (potentialParentFolder is null)
      {
        continue;
      }

      // Encontre a pasta de configuração pai da pasta alvo
      var configParentFolder = (
          from file in files
          let folder = $"{Path.GetDirectoryName(file)}/"
          where targetFolder.StartsWith(folder)
          orderby folder.Length descending
          select folder
      ).FirstOrDefault();

      // Determine se a pasta alvo é aninhada ou não
      var isNested = configParentFolder is null
          || configParentFolder.Length <= potentialParentFolder.Length;

      // Se a pasta alvo for aninhada, adicione-a à lista nestedScriptFolders
      if (isNested)
      {
        nestedScriptFolders.Add(new(potentialParentFolder, targetFolder));
      }
    }

    return nestedScriptFolders;
  }

  /// <summary>
  /// Extrai a estrutura de um pacote a partir de uma lista de caminhos de
  /// arquivos virtuais.
  /// </summary>
  /// <param name="paths">
  /// Uma lista de caminhos de arquivos virtuais para extrair a estrutura do
  /// pacote.
  /// </param>
  /// <remarks>
  /// Lista de listas representando a estrutura do pacote.
  /// Cada lista na lista externa representa uma pasta no pacote e contém os
  /// caminhos de arquivo dos arquivos nessa pasta e suas pastas ancestrais.
  /// O caminho começa pelo catálogo e termina na própria pasta de scripts.
  /// Note que apenas os arquivos vistos na estrutura ancestral da pasta são
  /// destacados. Esta lista pode estar incompleta já que a organização de
  /// pastas do ScriptPack é bem flexível.
  /// De uma forma geral a lista de caminhos terá uma aparência como esta:
  ///    /catalog.json
  ///    /catalog/product.json
  ///    /catalog/product/module.json
  ///    /catalog/product/module/package.json
  ///    /catalog/product/module/scripts/
  /// </remarks>
  /// <returns>
  /// Uma lista de listas representando a estrutura do pacote. Cada lista na
  /// lista externa representa uma pasta no pacote e contém os caminhos de
  /// arquivo dos arquivos nessa pasta e suas pastas ancestrais.
  /// </returns>
  private List<List<string>> OrganizePackagePathStructures(List<string> paths)
  {
    // Lista de listas representando a estrutura do pacote.
    // Cada lista na lista externa representa uma pasta no pacote e contém os
    // caminhos de arquivo dos arquivos nessa pasta e suas pastas ancestrais.
    // O caminho começa pelo catálogo e termina na própria pasta de scripts.
    // Note que apenas os arquivos vistos na estrutura ancestral da pasta são
    // destacados. Esta lista pode estar incompleta já que a organização de
    // pastas do ScriptPack é bem flexível.
    // De uma forma geral a lista de caminhos terá uma aparência como esta:
    //    /catalog.json
    //    /catalog/product.json
    //    /catalog/product/module.json
    //    /catalog/product/module/package.json
    //    /catalog/product/module/scripts/
    var packageStructures = new List<List<string>>();

    var folders = paths.Where(path => path.EndsWith("/"));
    var files = paths.Where(path => !path.EndsWith("/"));

    foreach (var folderToCheck in folders.OrderBy(x => x))
    {
      var ancestorFiles = (
          from possibleParentFile in files
          let fileFolder = $"{Path.GetDirectoryName(possibleParentFile)}/"
          where folderToCheck.StartsWith(fileFolder)
          orderby fileFolder.Length descending
          select possibleParentFile
      ).ToList();

      var seenFiles = new HashSet<string>();
      ancestorFiles.RemoveAll(fileToCheck =>
      {
        var currentFileName = Path.GetFileName(fileToCheck);
        if (currentFileName == "module.json")
        {
          return false;
        }
        if (seenFiles.Contains(currentFileName))
        {
          return true;
        }
        seenFiles.Add(currentFileName);
        return false;
      });

      // Adicionando a própria pasta no final da lista.
      ancestorFiles.Add(folderToCheck);
      packageStructures.Add(ancestorFiles);
    }

    return packageStructures;
  }

  /// <summary>
  /// Cria a hierarquia de pacotes a partir das estruturas de caminho de pacote.
  /// </summary>
  /// <param name="packagePathStructures">
  /// Lista de estruturas de caminho de pacote a serem processadas.
  /// A lista de caminhos terá uma aparência como esta:
  ///    /catalog.json
  ///    /catalog/product.json
  ///    /catalog/product/module.json
  ///    /catalog/product/module/package.json
  ///    /catalog/product/module/scripts/
  /// </param>
  /// <remarks>
  /// Algumas partes da estrutura de nodos são obrigatórias, por exemplo:
  ///
  /// - Catálogo: necessário para agrupar os pacotes lidos de um mesmo Drive e
  /// contém informações sobre as conexões de banco de dados.
  ///
  /// - Produto e Versão: necessário para identificar pacotes de scripts.
  /// O algoritmo de migração persiste na base de dados esta informação de
  /// produto e versão para identificar os pacotes que já foram migrados.
  ///
  /// - Pacote: contém as regras de migração dos scripts, como conexão destino e
  /// proridade de pacotes.
  ///
  /// Quando não há arquivo disponível no Drive para descrevê-las, elas são
  /// criadas com valores padrão.
  /// </remarks>
  /// <returns>
  /// Uma lista de hierarquias de pacotes, onde cada hierarquia é uma lista de
  /// tuplas (Type, Path) representando a hierarquia de tipos de pacote
  /// encontrada.
  /// </returns>
  private List<List<(Type Type, string Path)>> CreatePackagePathHierarchies(
      List<List<string>> packagePathStructures)
  {
    var packageHierarchyStructures =
        new List<List<(Type Type, string Path)>>();

    foreach (var packagePathStructure in packagePathStructures)
    {
      var hierarchy = new List<(Type Type, string Path)>();

      var queue = new Queue<string>(
          from file in packagePathStructure
          let folder = $"{Path.GetDirectoryName(file)}/"
          let hierarchicalOrder = Path.GetFileName(file) switch
          {
            "catalog.json" => 0,
            "product.json" => 1,
            "module.json" => 2,
            "package.json" => 3,
            _ => 4
          }
          orderby folder.Length, hierarchicalOrder
          select file
      );

      EnsurePath<CatalogNode>(queue, hierarchy, "catalog.json");
      EnsurePath<ProductNode>(queue, hierarchy, "product.json");
      EnsurePath<VersionNode>(queue, hierarchy, "product.json");

      while (queue.Peek().EndsWith("module.json"))
      {
        hierarchy.Add(new(typeof(ModuleNode), queue.Dequeue()));
      }

      EnsurePath<PackageNode>(queue, hierarchy, "package.json");

      // O último item restante na fila é a pasta de scripts.
      if (queue.Count != 1)
      {
        var path = queue.Last();
        throw new ArgumentException("A pasta de scripts não contém uma " +
            $"estrutura hierárquica válida: {path}");
      }

      // Validando a estrutura legada do ScriptPack.
      //
      // A versão corrente do ScriptPack utiliza arquivos JSON para sua
      // configuração, mas a versão antiga se utilizava de arquivos
      // properties.
      //
      // Porém, para que uma estrutura legada seja considerada válida é
      // necessário que a pasta de scripts contenha um arquivo de configuração
      // chamado module.info.

      var lastPath = hierarchy.Last().Path;
      if (!lastPath.EndsWith(".json") && !lastPath.EndsWith("/module.info"))
        continue;

      var targetFolder = queue.Dequeue();
      hierarchy.Add(new(typeof(ScriptNode), targetFolder));

      packageHierarchyStructures.Add(hierarchy);
    }

    return packageHierarchyStructures;
  }

  /// <summary>
  /// Garante que o caminho esteja presente na hierarquia.
  /// </summary>
  /// <typeparam name="T">Tipo de nodo da hierarquia.</typeparam>
  /// <param name="queue">Fila de caminhos a serem verificados.</param>
  /// <param name="hierarchy">
  /// Lista de tuplas (Tipo, Caminho) que representam a hierarquia.</param>
  /// <param name="filenames">
  /// Lista de nomes de arquivos a serem verificados na última camada do
  /// caminho.
  /// </param>
  /// <remarks>
  /// Verifica se a fila contém um arquivo com o nome presente em "filenames" na
  /// última camada do caminho.
  /// Se o arquivo existir, adiciona o tipo da hierarquia na lista e remove o
  /// caminho da fila.
  /// Caso contrário, adiciona o caminho na lista e verifica se a última camada
  /// do caminho termina com '/'.
  /// Se não terminar, adiciona o tipo da hierarquia na lista e retorna.
  /// Caso contrário, verifica se a hierarquia já possui um nodo anterior e
  /// adiciona o tipo da hierarquia com o caminho do último nodo na lista.
  /// Se não tiver, adiciona o tipo da hierarquia com o caminho do arquivo
  /// "package.json".
  /// </remarks>
  private void EnsurePath<T>(Queue<string> queue,
      List<(Type Type, string Path)> hierarchy, string filename)
          where T : INode
  {
    var type = typeof(T);

    if (queue.Peek().EndsWith(filename))
    {
      hierarchy.Add(new(type, queue.Dequeue()));
      return;
    }

    var path = queue.FirstOrDefault()!;

    if (!path.EndsWith("/"))
    {
      hierarchy.Add(new(type, path));
      return;
    }

    if (hierarchy.Count > 0)
    {
      var referencePath = hierarchy.Last();
      hierarchy.Add(new(type, referencePath.Path));
      return;
    }

    hierarchy.Add(new(type, $"{path}package.json"));
  }

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
  private async Task<List<CatalogNode>> CreateCatalogNodeTreeAsync(
      IDrive drive,
      List<List<(Type Type, string Path)>> packagePathHierarchies)
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
    // correspondem a arquivos de configuração do tipo `catalog.json`.
    // Isto que dizer que o repositório contém pacotes não estruturado
    // corretamente a partir de um catálogo definido.
    // 
    // Para uma melhor organização da árvore vamos mesclar estes catálogos
    // dentro de um apenas e utilizá-lo como raiz de todos os pacotes.

    var unidentifiedCatalogs = catalogs
        .Where(c => c.FilePath?.EndsWith("/catalog.json") != true)
        .ToList();
    if (unidentifiedCatalogs.Count > 0)
    {
      var products = unidentifiedCatalogs
          .SelectMany(c => c.Products)
          .ToList();

      var unifiedCatalog = new CatalogNode
      {
        Name = drive.Name,
        FilePath = "/catalog.json",
        Drive = drive,
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

    if (parent != null)
    {
      AddToParent(parent, node);
    }

    if (node is VersionNode version && string.IsNullOrEmpty(version.Version))
    {
      version.Version = VersionNode.UnidentifiedVersion;
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
  /// usado no carregamento do nodo, como package.json, module.json, etc.
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
    string? folder = null;
    string? name = null;

    // Pacotes recebem o nome "Package-INDICE", sendo INDICE o índice do
    // pacote na lista de pacotes do catálogo.
    if (node is PackageNode)
    {
      if (filePath.EndsWith("/package.json"))
      {
        folder = Path.GetDirectoryName(filePath);
        name = Path.GetFileNameWithoutExtension(folder)!;
        return "Package";
      }

      int index = node.Parent!.Children()
          .Select((item, index) => new { item, index })
          .FirstOrDefault(x => x.item == node)?.index ?? -1;

      return (index == 0) ? "Package" : $"Package-{index}";
    }

    if (node is ModuleNode)
    {
      if (filePath.EndsWith("/module.json"))
      {
        folder = Path.GetDirectoryName(filePath);
        name = Path.GetFileNameWithoutExtension(folder)!;
        return name;
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
      if (filePath.EndsWith("/product.json"))
      {
        folder = Path.GetDirectoryName(filePath);
        name = Path.GetFileNameWithoutExtension(folder)!;
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

    folder = Path.GetDirectoryName(filePath);
    name = Path.GetFileNameWithoutExtension(folder)!;
    return name;
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
    // Carregando o arquivo JSON...
    using var reader = await drive.OpenFileAsync(filePath);
    var @object = await JsonSerializer.DeserializeAsync(reader, type,
        JsonOptions.CamelCase);
    var node = (IFileNode)@object!;
    node.Parent = parent;
    node.FilePath = Path.GetDirectoryName(filePath)!;
    return node;
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
