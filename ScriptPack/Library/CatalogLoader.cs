// using System.Text.Json;
// using ScriptPack.Domain;

// namespace ScriptPack.Library;

// /// <summary>
// /// Utilitário para carregamento de catálogo a partir de uma pasta de scripts,
// /// um arquivo compactado ou qualquer instância de <see cref="IDrive"/>.
// /// </summary>
// public class CatalogLoader
// {
//   /// <summary>
//   /// Carrega os catálogos disponíveis no drive.
//   /// 
//   /// Módulos e catálogos são opcionais e são construídos pelo algoritmo abaixo
//   /// com base no conteúdo do arquivo de produto.
//   /// Isto significa que o arquivo de produto é obrigatório e pode conter
//   /// propriedades de catálogo e módulo.
//   /// 
//   /// </summary>
//   /// <param name="drive">
//   /// Drive a ser carregado.
//   /// </param>
//   /// <returns>
//   /// Lista de catálogos carregados.
//   /// </returns>
//   public async Task<List<CatalogNode>> ReadCatalogAsync(IDrive drive)
//   {
//     var catalogs = await ReadNodesAsync<CatalogNode>(drive);
//     var products = await ReadNodesAsync<ProductNode>(drive);
//     var modules = await ReadNodesAsync<ModuleNode>(drive);
//     var packages = await ReadNodesAsync<PackageNode>(drive);
//     var scripts = await ReadNodesAsync<ScriptNode>(drive);

//     // Adotando os scripts aos respectivos pacotes
//     foreach (var package in packages)
//     {
//       var prefix = Path.GetDirectoryName(package.FilePath)!;
//       var matchingScripts = scripts.Where(s => s.FilePath!.StartsWith(prefix)).ToArray();
//       package.Scripts.AddRange(matchingScripts);
//       scripts.RemoveAll(script => matchingScripts.Contains(script));
//     }

//     // TODO: O que fazer com os scripts órfãos?

//     // Adotando os pacotes aos módulos
//     foreach (var module in modules)
//     {
//       var prefix = Path.GetDirectoryName(module.FilePath)!;
//       var matchingPackages = packages.Where(p => p.FilePath!.StartsWith(prefix)).ToArray();
//       module.Packages.AddRange(matchingPackages);
//       packages.RemoveAll(p => matchingPackages.Contains(p));
//     }

//     // Adotando os pacotes órfãos de módulos aos respectivos produtos
//     foreach (var product in products)
//     {
//       var prefix = Path.GetDirectoryName(product.FilePath)!;
//       var matchingPackages = packages.Where(p => p.FilePath!.StartsWith(prefix)).ToArray();

//       // Criando um módulo baseado no produto...
//       var module = await ReadNodeFromFileAsync<ModuleNode>(drive, product.FilePath!);
//       module.Packages.AddRange(matchingPackages);

//       product.Modules.Add(module);
//       packages.RemoveAll(p => matchingPackages.Contains(p));
//     }

//     // Adotando os módulos aos produtos
//     foreach (var product in products)
//     {
//       var prefix = Path.GetDirectoryName(product.FilePath)!;
//       var matchingModules = modules.Where(m => m.FilePath!.StartsWith(prefix)).ToArray();
//       product.Modules.AddRange(matchingModules);
//       modules.RemoveAll(m => matchingModules.Contains(m));
//     }

//     // Adotando os módulos órfãos de produto a um produto novo baseado no mesmo arquivo do módulo
//     while (modules.Count > 0)
//     {
//       var module = modules.First();

//       // Criando um produto baseado no módulo...
//       var product = await ReadNodeFromFileAsync<ProductNode>(drive, module.FilePath!);
//       product.Modules.Add(module);

//       products.Add(product);
//       modules.Remove(module);
//     }

//     // Adotando os produtos aos respectivos catálogos
//     foreach (var catalog in catalogs)
//     {
//       var prefix = Path.GetDirectoryName(catalog.FilePath)!;
//       var matchingProducts = products.Where(p => p.FilePath!.StartsWith(prefix)).ToArray();
//       catalog.Products.AddRange(matchingProducts);
//       products.RemoveAll(p => matchingProducts.Contains(p));
//     }

//     // Criando catálogo para produtos órfãos
//     if (products.Count > 0)
//     {
//       var catalog = new CatalogNode
//       {
//         Name = "Outros Produtos",
//         FilePath = "/catalog.json"
//       };
//       catalog.Products.AddRange(products);
//       products.Clear();
//       catalogs.Add(catalog);
//     }

//     ValidatePackages(catalogs.SelectMany(catalog => catalog.GetDescendants<PackageNode>()));

//     return catalogs;
//   }

//   private void ValidatePackages(IEnumerable<PackageNode> packages)
//   {
//     // Adicionado uma falha do tipo "NoConnectionSpecified" para cada pacote com sua lista de conexões vazia
//     foreach (var package in packages.Where(p => p.Connections.Count == 0))
//     {
//       package.Faults.Add(new FaultNode
//       {
//         Hint = FaultNode.Hints.NoConnectionSpecified,
//         Message = "Nenhuma conexão foi especificada para este pacote."
//       });
//     }
//   }

//   /// <summary>
//   /// Carrega todos os nodos de um determinado tipo a partir de um drive.
//   /// </summary>
//   /// <param name="drive">
//   /// Drive a ser carregado.
//   /// </param>
//   /// <typeparam name="T">
//   /// Tipo de nodo a ser carregado.
//   /// </typeparam>
//   /// <returns>
//   /// Lista de nodos carregados.
//   /// </returns>
//   private async Task<List<T>> ReadNodesAsync<T>(IDrive drive)
//     where T : IFileNode, new()
//   {
//     var nodes = new List<T>();

//     if (typeof(T) == typeof(ScriptNode))
//     {
//       var filePaths = drive.GetFiles("/", "*.sql", SearchOption.AllDirectories);

//       foreach (var filePath in filePaths)
//       {
//         var node = await ReadScriptFromFileAsync(drive, filePath);
//         nodes.Add((T)(object)node);
//       }
//     }
//     else
//     {
//       var filename = $"{typeof(T).Name.ToLower()}.json";
//       var filePaths = drive.GetFiles("/", filename, SearchOption.AllDirectories);

//       foreach (var filePath in filePaths)
//       {
//         var node = await ReadNodeFromFileAsync<T>(drive, filePath);
//         nodes.Add(node);
//       }
//     }

//     return nodes;
//   }

//   /// <summary>
//   /// Carrega o nodo do tipo especificado a partir do arquivo.
//   /// </summary>
//   /// <param name="drive">
//   /// Drive a ser carregado.
//   /// </param>
//   /// <param name="filePath">
//   /// Caminho do arquivo a ser carregado.
//   /// </param>
//   /// <typeparam name="T">
//   /// Tipo de nodo a ser carregado.
//   /// </typeparam>
//   /// <returns>
//   /// Nodo carregado.
//   /// </returns>
//   private async Task<T> ReadNodeFromFileAsync<T>(IDrive drive, string filePath)
//     where T : IFileNode, new()
//   {
//     T node = new();
//     try
//     {
//       var json = await drive.ReadAllTextAsync(filePath);
//       node = JsonSerializer.Deserialize<T>(json, Json.SPackOptions)!;
//     }
//     catch (Exception ex)
//     {
//       node = new T();
//       node.Faults.Add(new(ex));
//     }

//     node.FilePath = filePath;
//     if (string.IsNullOrEmpty(node.Name))
//     {
//       var name = Path.GetFileName(Path.GetDirectoryName(filePath))!;
//       node.Name = string.IsNullOrEmpty(name) ? drive.Name : name;
//     }

//     if (node is CatalogNode catalog)
//     {
//       catalog.Description ??= "Catálogo de scripts.";
//     }

//     if (node is ProductNode product)
//     {
//       var versionTag = FilePathParser.ParseVersionTag(filePath);
//       if (!string.IsNullOrEmpty(versionTag))
//       {
//         // Concatenando a tag na versão do produto na forma VERSAO-TAG
//         product.Version = $"{product.Version}-{versionTag}";
//       }
//     }

//     return node;
//   }

//   /// <summary>
//   /// Carrega os scripts do pacote.
//   /// </summary>
//   /// <param name="drive">
//   /// Drive a ser carregado.
//   /// </param>
//   /// <param name="parent">
//   /// Nodo pai dos nodos a serem carregados.
//   /// </param>
//   /// <returns>
//   /// Lista de nodos carregados.
//   /// </returns>
//   private Task<ScriptNode> ReadScriptFromFileAsync(IDrive drive, string filePath)
//   {
//     var (name, tag) = FilePathParser.ParseScriptTag(filePath);
//     var node = new ScriptNode
//     {
//       FilePath = filePath,
//       Name = name,
//       Tag = tag
//     };
//     return Task.FromResult(node);
//   }

// }
