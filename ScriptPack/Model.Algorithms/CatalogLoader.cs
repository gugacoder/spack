using ScriptPack.Domain;
using ScriptPack.FileSystem;
using System.Text;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Utilitário de carregamento do catálogo de scripts a partir de uma instância
/// de <cref="IDrive"/>.
/// </summary>
internal class CatalogLoader
{
  /// <summary>
  /// Carrega um array de nodos de catálogo e todos os seus subnodos a partir do
  /// <see cref="IDrive"/> fornecido.
  /// </summary>
  /// <param name="drive">
  /// O objeto <see cref="IDrive"/> que representa o driver a ser usado para
  /// carregar os nodos de catálogo.
  /// </param>
  /// <param name="encoding">
  /// O objeto <see cref="Encoding"/> que representa o codificador de caracteres
  /// a ser usado para carregar os nodos de catálogo.
  /// </param>
  /// <returns>
  /// Um array de objetos <see cref="CatalogNode"/>, representando os nodos de
  /// catálogo e seus subnodos.
  /// </returns>
  public async Task<CatalogNode[]> LoadCatalogsAsync(IDrive drive,
      Encoding? encoding = null)
  {
    // Hierarquia de pastas dos pacotes.
    //
    // É garantido que a sequência de caminhos para cada pacote contenha todas
    // as partes esperadas na hierarquia de pacotes, ou seja:
    // - Catálogo
    // - Produto
    // - Versão
    // - Módulo (Pode ocorrer nenhuma ou várias vezes)
    // - Pacote
    // - Scripts (Pode ocorrer uma ou várias vezes)
    // 
    // Por exemplo, para um pacote qualquer, sem módulo e com uma única pasta de
    // scripts a sua hierarquia obtida poderia ser algo como:
    // 
    // - (CatalogNode, "/-catalog.jsonc")
    // - (ProductNode, "/Catálogo/trunk/-product.jsonc")
    // - (VersionNode, "/Catálogo/trunk/-product.jsonc")
    // - (PackageNode, "/Catálogo/trunk/Scripts/-package.jsonc")
    // - (ScriptNode, "/Catálogo/trunk/Scripts/")
    // 
    // Ou, em outro exempĺo, para um pacote contendo módulo e submódulo e com 
    // pastas de scripts aninhadas, a sua hierarquia obtida poderia ser algo
    // como:
    // 
    // - (CatalogNode, "/-catalog.jsonc")
    // - (ProductNode, "/Catálogo/trunk/-product.jsonc")
    // - (VersionNode, "/Catálogo/trunk/-product.jsonc")
    // - (ModuleNode, "/Catálogo/trunk/Site/-module.jsonc")
    // - (ModuleNode, "/Catálogo/trunk/Site/Backend/-module.jsonc")
    // - (PackageNode, "/Catálogo/trunk/Site/Backend/SqlServer/-package.jsonc")
    // - (ScriptNode, "/Catálogo/trunk/Site/Backend/SqlServer/Cadastros/")
    // - (ScriptNode, "/Catálogo/trunk/Site/Backend/SqlServer/Consultas/")
    //
    (Type Type, string Path)[][] packagePathHierarchies;

    var pathHandler = new CatalogLoaderPathHandler();
    packagePathHierarchies = pathHandler.ExtractPackageHierarchies(drive);

    // O array packagePathHierarchies contém a estrtura de pastas de cada
    // pacote de scripts disponível no driver.
    //
    // É garantido que cada estrutura contenha todos arquivos e pastas
    // relevantes:
    // -  Catálogo
    // -  Produto
    // -  Versão
    // -  Módulo (Pode ocorrer NENHUMA ou VÁRIAS vezes)
    // -  Pacote
    // -  Scripts (Pode ocorrer UMA ou VÁRIAS vezes)
    // 
    // Basta percorrermos cada estrutura e instanciar os nodos correspondentes
    // e termos a árvore de nodos de cada pacote.

    var nodeFactory = new CatalogLoaderNodeFactory();

    List<CatalogNode> catalogs = await nodeFactory.CreateNodeTreeAsync(drive,
        packagePathHierarchies);

    return catalogs.ToArray();
  }
}