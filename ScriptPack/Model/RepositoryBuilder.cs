using ScriptPack.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;

namespace ScriptPack.Model;

/// <summary>
/// Utilitário para carregamento de catálogos para dentro de um repositório.
/// </summary>
public class RepositoryBuilder
{
  private readonly List<IDrive> _drives = new();

  private DependencyDetectorVisitor? _dependencyDetectorVisitor;
  private CircularDependencyDetectorVisitor? _circularDependencyDetectorVisitor;

  /// <summary>
  /// Adiciona um drive ao carregador de catálogos.
  /// </summary>
  /// <param name="drive">O drive a ser adicionado.</param>
  public void AddDrive(IDrive drive)
  {
    _drives.Add(drive);
  }

  /// <summary>
  /// Adiciona um detector de dependências ao carregador de catálogos.
  /// Quando utilizado, o carregador de catálogos preenche a propriedade
  /// <see cref="ScriptNode.Dependencies"/> de cada script com os objetos
  /// dos quais ele depende.
  /// </summary>
  public void AddDependencyDetector()
  {
    _dependencyDetectorVisitor ??= new();
  }

  /// <summary>
  /// Adiciona um detector de dependências circulares ao carregador de catálogos.
  /// Dependências circulares detectadas são adicionadas como falhas nos scripts
  /// relacionados.
  /// </summary>
  public void AddCircularDependencyDetector()
  {
    _circularDependencyDetectorVisitor ??= new();
  }

  /// <summary>
  /// Constrói o repositório de catálogos a partir dos drives adicionados.
  /// </summary>
  /// <returns>
  /// O repositório de catálogos carregado a partir dos drives.
  /// </returns>
  public async Task<RepositoryNode> BuildRepositoryAsync()
  {
    var repository = new RepositoryNode();
    var catalogLoader = new CatalogLoader();

    foreach (var drive in _drives)
    {
      try
      {
        var catalogs = await catalogLoader.ReadCatalogAsync(drive);

        if (_dependencyDetectorVisitor != null)
        {
          await repository.AcceptAsync(_dependencyDetectorVisitor);
        }
        if (_circularDependencyDetectorVisitor != null)
        {
          repository.Accept(_circularDependencyDetectorVisitor);
        }

        repository.Catalogs.AddRange(catalogs);
      }
      catch (Exception ex)
      {
        repository.Faults.Add(Fault.EmitException(ex,
            $"Falha ao carregar catálogo do drive {drive.Name}."));
      }
    }
    return repository;
  }
}
