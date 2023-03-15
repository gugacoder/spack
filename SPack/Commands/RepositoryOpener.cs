using ScriptPack.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Model;

namespace SPack.Commands;

/// <summary>
/// Utilitário para abertura de repositórios de catálogos de scripts para
/// processamento.
/// </summary>
public class RepositoryOpener
{
  /// <summary>
  /// Carrega o repositório com os catálogos lidos do caminho.
  /// </summary>
  /// <returns>Caminho da pasta ou do arquivo do catálogo.</returns>
  /// <returns>O repositório carregado.</returns>
  public async Task<RepositoryNode> OpenRepositoryAsync(string? catalogPath)
  {
    var repositoryBuilder = CreateRepositoryBuilder(catalogPath);
    var repository = await repositoryBuilder.BuildRepositoryAsync();
    return repository;
  }

  /// <summary>
  /// Carrega o repositório com os catálogos lidos do caminho e cria um
  /// <see cref="TreeNodeNavigator"/> para navegação.
  /// </summary>
  /// <returns>Caminho da pasta ou do arquivo do catálogo.</returns>
  public async Task<TreeNodeNavigator> OpenRepositoryNavigatorAsync(
      string? catalogPath)
  {
    var repository = await OpenRepositoryAsync(catalogPath);

    INode rootNode = repository.Descendants<CatalogNode>().Count() == 1
        ? repository.Descendants<CatalogNode>().First()
        : repository;

    var repositoryNavigator = new TreeNodeNavigator(rootNode);
    return repositoryNavigator;
  }

  /// <summary>
  /// Cria uma instância de <see cref="RepositoryBuilder"/> preparado para
  /// carregar os catálogos.
  /// </summary>
  /// <param name="catalogPath">
  /// Caminho da pasta ou do arquivo do catálogo.
  /// </param>
  /// <returns>A instância do <see cref="RepositoryBuilder"/>.</returns>
  public RepositoryBuilder CreateRepositoryBuilder(string? catalogPath)
  {
    if (string.IsNullOrEmpty(catalogPath)) catalogPath = ".";

    var drive = Drive.GetDrive(catalogPath);

    var repositoryBuilder = new RepositoryBuilder();
    repositoryBuilder.AddDrive(drive);

    return repositoryBuilder;
  }
}