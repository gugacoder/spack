using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;
using SPack.Prompting;

namespace SPack.Commands.Helpers;

/// <summary>
/// Utilitário para carregamento do repositorio de catálogos de scripts
/// baseado nos argumentos de linha de comando.
/// </summary>
public class RepositoryUtilityBuilder
{
  private CommandLineOptions _options = null!;
  private bool _addValidators;

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
  /// Adiciona validador de dependência cíclica e outros validadores comuns
  /// disponíveis.
  /// </summary>
  public void AddValidators()
  {
    _addValidators = true;
  }

  /// <summary>
  /// Constrói o repositório com os catálogos carregados.
  /// </summary>
  /// <returns>
  /// Repositório com os catálogos carregados.
  /// </returns>
  public async Task<RepositoryNode> BuildRepositoryAsync()
  {
    var repositoryBuilder = new RepositoryBuilder();

    repositoryBuilder.AddDefaultEncoding(_options.Encoding.On
        ? Encodings.GetEncoding(_options.Encoding.Value)
        : Encodings.Iso88591);

    if (!_options.NoCatalog.On)
    {
      var filePath = _options.Catalog.On ? _options.Catalog.Value : ".";
      var drive = DriveFactory.CreateDrive(filePath, _options.Password.Value);
      repositoryBuilder.AddDrive(drive);
    }

    if (_options.BuiltIn.On)
    {
      repositoryBuilder.AddBuiltInCatalog();
    }

    if (_addValidators)
    {
      repositoryBuilder.AddDependencyDetector();
      repositoryBuilder.AddCircularDependencyDetector();
    }

    var repository = await repositoryBuilder.BuildRepositoryAsync();
    return repository;
  }

}
