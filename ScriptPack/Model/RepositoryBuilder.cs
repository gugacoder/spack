using ScriptPack.Model.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using System.Text;

namespace ScriptPack.Model;

/// <summary>
/// Utilitário para carregamento de catálogos para dentro de um repositório.
/// </summary>
public class RepositoryBuilder
{
  private readonly Dictionary<IDrive, Encoding?> _drives = new();

  private DependencyDetectorVisitor? _dependencyDetectorVisitor;
  private CircularDependencyDetectorVisitor? _circularDependencyDetectorVisitor;
  private Encoding? _defaultEncoding;

  /// <summary>
  /// Adiciona um drive ao carregador de catálogos.
  /// </summary>
  /// <param name="drive">O drive a ser adicionado.</param>
  public void AddDrive(IDrive drive, Encoding? encoding = null)
  {
    _drives.Add(drive, encoding);
  }

  /// <summary>
  /// Adiciona os scripts internos fornecidos pelo aplicativo.
  /// </summary>
  /// <remarks>
  /// Este método é usado para incluir scripts predefinidos que acompanham o
  /// aplicativo. Os scripts acrescentam objetos de automação do ScriptPack
  /// para scripts de migração de base de dados.
  /// </remarks>
  public void AddBuiltInCatalog()
  {
    var drive = new ResourceDrive(typeof(INode).Assembly);
    _drives.Add(drive, Drive.DefaultEncoding);
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
  /// Adiciona um codificador de caracteres ao carregador de catálogos.
  /// </summary>
  public void AddDefaultEncoding(Encoding encoding)
  {
    _defaultEncoding = encoding;
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

    foreach (var (drive, driveEncoding) in _drives)
    {
      var encoding = driveEncoding ?? _defaultEncoding ?? Drive.DefaultEncoding;
      try
      {
        var catalogs = await catalogLoader.LoadCatalogsAsync(drive, encoding);

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
