using System.Text.Json;
using SPack.Domain;
using SPack.Library;
using SPack.Model.Algorithms;
using SPack.Model.Algorithms;

namespace SPack.Model;

/// <summary>
/// Carregador de catálogos a partir de um drive navegador de arquivos.
/// </summary>
public class RepositoryBuilder
{
  private readonly List<IDrive> drives = new();
  private bool useDependencyDetector;
  private bool useCyclicDependencyDetector;

  public void AddDrive(IDrive drive)
  {
    this.drives.Add(drive);
  }

  public void AddDependencyDetector()
  {
    this.useDependencyDetector = true;
  }

  public void AddCyclicDependencyDetector()
  {
    this.AddDependencyDetector();
    this.useCyclicDependencyDetector = true;
  }


  /// <summary>
  /// Carrega os catálogos disponíveis no drive.
  /// </summary>
  /// <param name="drive">
  /// Drive a ser carregado.
  /// </param>
  /// <returns>
  /// Lista de catálogos carregados.
  /// </returns>
  public async Task<Repository> BuildRepositoryAsync()
  {
    var catalogLoader = new CatalogLoader();

    var repository = new Repository();
    foreach (var drive in drives)
    {
      var catalogs = await catalogLoader.ReadCatalogAsync(drive);

      if (useDependencyDetector)
      {
        var dependencyDetector = new AsyncDependencyDetector(drive);
        await Task.WhenAll(catalogs.Select(c => c.AcceptAsync(dependencyDetector)));
      }

      if (useCyclicDependencyDetector)
      {
        var cyclicDependencyDetector = new CyclicDependencyDetector();
        repository.Accept(cyclicDependencyDetector);
      }

      repository.Catalogs.AddRange(catalogs);
    }
    return repository;
  }
}
