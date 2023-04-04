using ScriptPack.Domain;
using ScriptPack.FileSystem;
using static System.IO.SearchOption;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Componente do <cref see="CatalogLoader"/> responsável por organizar a
/// estrutura de pastas e arquivos lida do <see cref="IDrive"/>.
/// </summary>
/// <remarks>
/// Embora a estrutura de pastas de um catálogo seja oranizada em uma hierarquia
/// de Catálogo, Produto, Módulo e Pacote, o ScriptPack não exige que a
/// estrutura de pastas siga esta hierarquia. Por exemplo, um pacote pode
/// conter uma pasta de scripts sem que haja um módulo ou um produto.
/// 
/// Esta classe é responsável por organizar a estrutura de pastas e arquivos
/// lida do <see cref="IDrive"/> de forma a criar uma estrutura hierárquica
/// coerente com a estrutura de pastas, criando os nodos faltantes com base
/// nos arquivos possíveis encontrados na estrutura de pastas.
/// </remarks>
internal class CatalogLoaderPathHandler
{
  /// <summary>
  /// Extrai as hierarquias de pacotes a partir do IDrive fornecido.
  /// </summary>
  /// <remarks>
  /// A lista resultada contém as hierarquias de pacotes encontradas no IDrive.
  /// Para cada pacote encontrado ou inferido é criado um array de tuplas
  /// contendo o tipo de nodo e o caminho completo para o arquivo que deve ser
  /// usado no carregamento do nodo, exceto para o nodo <cref see="ScriptNode"/>
  /// que é representado não por um arquivo mas por nomes de pastas de onde os
  /// scripts devem ser lidos.
  /// 
  /// É garantido que a sequência de caminhos para cada pacote contenha todas
  /// as partes esperadas na hierarquia de pacotes, ou seja:
  /// - Catálogo
  /// - Produto
  /// - Versão
  /// - Módulo (Pode ocorrer nenhuma ou várias vezes)
  /// - Pacote
  /// - Scripts (Pode ocorrer uma ou várias vezes)
  /// 
  /// Por exemplo, para um pacote qualquer, sem módulo e com uma única pasta de
  /// scripts a sua hierarquia obtida poderia ser algo como:
  /// 
  /// - (CatalogNode, "/-catalog.jsonc")
  /// - (ProductNode, "/Catálogo/trunk/-product.jsonc")
  /// - (VersionNode, "/Catálogo/trunk/-product.jsonc")
  /// - (PackageNode, "/Catálogo/trunk/Scripts/-package.jsonc")
  /// - (ScriptNode, "/Catálogo/trunk/Scripts/")
  /// 
  /// Ou, em outro exempĺo, para um pacote contendo módulo e submódulo e com 
  /// pastas de scripts aninhadas, a sua hierarquia obtida poderia ser algo
  /// como:
  /// 
  /// - (CatalogNode, "/-catalog.jsonc")
  /// - (ProductNode, "/Catálogo/trunk/-product.jsonc")
  /// - (VersionNode, "/Catálogo/trunk/-product.jsonc")
  /// - (ModuleNode, "/Catálogo/trunk/Site/-module.jsonc")
  /// - (ModuleNode, "/Catálogo/trunk/Site/Backend/-module.jsonc")
  /// - (PackageNode, "/Catálogo/trunk/Site/Backend/SqlServer/-package.jsonc")
  /// - (ScriptNode, "/Catálogo/trunk/Site/Backend/SqlServer/Cadastros/")
  /// - (ScriptNode, "/Catálogo/trunk/Site/Backend/SqlServer/Consultas/")
  /// </remarks>
  /// <param name="drive">
  /// O IDrive a ser utilizado para extrair as hierarquias de pacotes.
  /// </param>
  /// <returns>
  /// Uma matriz de tuplas que contém o tipo de cada nodo e o caminho completo
  /// para o arquivo ou pasta.
  /// </returns>
  public (Type Type, string Path)[][] ExtractPackageHierarchies(IDrive drive)
  {
    //
    // A lista de caminhos contém os arquivos de descrição da estrutura do
    // catálogo e os caminhos de pastas de scripts. Pastas são terminadas em
    // barra (/) para diferenciação.
    // Exemplo:
    //    /pasta/-catalog.jsonc
    //    /pasta/catalog/-package.jsonc
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
    //    /pasta/-package.jsonc
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
    //    /-catalog.jsonc
    //    /catalog/-product.jsonc
    //    /catalog/product/-module.jsonc
    //    /catalog/product/module/-package.jsonc
    //    /catalog/product/module/scripts/

    List<List<string>> packagePathStructures =
        OrganizePackagePathStructures(paths);

    // A estrutura obtida acima pode esta incompleta já que a organização de
    // pastas do ScriptPack é bem flexível.
    // O algoritmo abaixo valida esta estrutura e acrescenta nela os arquivos
    // de configuração faltantes, sendo eles:
    // -  -catalog.jsonc
    // -  -product.jsonc
    // -  -module.jsonc
    // -  -package.jsonc
    // 
    // O resultado final é uma lista organizada hierarquicamente contendo
    // todos os arquivos de configuração necessários e os seus respectivos
    // tipos para que possam ser finalmente construídos.
    // Exemplo:
    //    ( CatalogNode, "/-catalog.jsonc" )
    //    ( ProductNode, "/catalog/-product.jsonc" )
    //    ( ModuleNode, "/catalog/product/-module.jsonc" )
    //    ( PackageNode, "/catalog/product/module/-package.jsonc" )
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

    var hierarchies = packagePathHierarchies
        .Select(h => h.ToArray())
        .ToArray();

    return hierarchies;
  }

  /// <summary>
  /// Obtém a lista de todos os caminhos de interesse, incluindo arquivos com as
  /// extensões .jsonc e .sql e os arquivos legados com extensão .info.
  /// </summary>
  /// <remarks>
  /// A estrutura do ScriptPack inclui os seguintes arquivos:
  /// - -catalog.jsonc
  /// - -product.jsonc
  /// - -module.jsonc
  /// - -package.jsonc
  /// </remarks>
  /// <param name="drive">
  /// O objeto IDrive que representa o driver a ser analisado.
  /// </param>
  /// <returns>
  /// A lista de caminhos de interesse encontrados.
  /// Caminhos de pastas terminam com uma barra.
  /// Exemplo:
  ///     /pasta/-catalog.jsonc
  ///     /pasta/catalog/-package.jsonc
  ///     /pasta/catalog/scripts/
  /// </returns>
  private List<string> GetStructuralPaths(IDrive drive)
  {
    var paths = new List<string>();

    // Nova estrutura
    var novosArquivos = new List<string> {
        "-catalog.jsonc",
        "-product.jsonc",
        "-version.jsonc",
        "-module.jsonc",
        "-package.jsonc"
    };
    foreach (var arquivo in novosArquivos)
    {
      paths.AddRange(drive.GetFiles("/", arquivo, AllDirectories));
    }

    // Pastas de scripts
    var directories = new List<string>(new[] { "/" });
    directories.AddRange(drive.GetDirectories("/", "*", AllDirectories));

    paths.AddRange((
        from directory in directories
        from file in drive.GetFiles(directory, "*.sql", AllDirectories)
        let folder = Path.GetDirectoryName(file)!
        select folder.EndsWith("/") ? folder : $"{folder}/"
    ).Distinct());

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
  ///      /MyPackage/pakage.jsonc
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
  ///    /-catalog.jsonc
  ///    /catalog/-product.jsonc
  ///    /catalog/product/-module.jsonc
  ///    /catalog/product/module/-package.jsonc
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
    //    /-catalog.jsonc
    //    /catalog/-product.jsonc
    //    /catalog/product/-module.jsonc
    //    /catalog/product/module/-package.jsonc
    //    /catalog/product/module/scripts/
    var packageStructures = new List<List<string>>();

    var folders = paths.Where(path => path.EndsWith("/"));
    var files = paths.Where(path => !path.EndsWith("/"));

    foreach (var folderToCheck in folders.OrderBy(x => x))
    {
      var ancestorFiles = (
          from possibleParentFile in files
          let fileFolder =
              $"{Path.GetDirectoryName(possibleParentFile)}/"
                  .Replace("//", "/")
          where folderToCheck.StartsWith(fileFolder)
          orderby fileFolder.Length descending
          select possibleParentFile
      ).ToList();

      var seenFiles = new HashSet<string>();
      ancestorFiles.RemoveAll(fileToCheck =>
      {
        var currentFileName = Path.GetFileName(fileToCheck);
        if (currentFileName == "-module.jsonc")
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

      // Reverter a lista para que ela comece com o catálogo.
      ancestorFiles.Reverse();

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
  ///    /-catalog.jsonc
  ///    /catalog/-product.jsonc
  ///    /catalog/product/-module.jsonc
  ///    /catalog/product/module/-package.jsonc
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
            "-catalog.jsonc" => 0,
            "-product.jsonc" => 1,
            "-module.jsonc" => 2,
            "-package.jsonc" => 3,
            _ => 4
          }
          orderby folder.Length, hierarchicalOrder
          select file
      );

      EnsurePath<CatalogNode>(queue, hierarchy, "-catalog.jsonc");
      EnsurePath<ProductNode>(queue, hierarchy, "-product.jsonc");
      EnsurePath<VersionNode>(queue, hierarchy, "-version.jsonc",
          copyFromPrevious: true);

      while (queue.Peek().EndsWith("-module.jsonc"))
      {
        hierarchy.Add(new(typeof(ModuleNode), queue.Dequeue()));
      }

      EnsurePath<PackageNode>(queue, hierarchy, "-package.jsonc");

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
      if (!lastPath.EndsWith(".jsonc") && !lastPath.EndsWith("/module.info"))
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
  /// <param name="dequeue">
  /// Indica se o caminho deve ser removido da fila.
  /// </param>
  /// <param name="copyFromPrevious">
  /// Indica se o caminho deve ser copiado do último nodo da hierarquia em vez
  /// do próximo, caso não encontrado.
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
  /// "-package.jsonc".
  /// </remarks>
  private void EnsurePath<T>(Queue<string> queue,
      List<(Type Type, string Path)> hierarchy, string filename,
      bool copyFromPrevious = false)
          where T : INode
  {
    var type = typeof(T);

    if (queue.Peek().EndsWith(filename))
    {
      hierarchy.Add(new(type, queue.Dequeue()));
      return;
    }

    if (copyFromPrevious && hierarchy.Count > 0)
    {
      var referencePath = hierarchy.Last();
      hierarchy.Add(new(type, referencePath.Path));
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

    hierarchy.Add(new(type, $"{path}-package.jsonc"));
  }
}
