using System.Data;
using ScriptPack.FileSystem;
using ScriptPack.Model;

namespace SPack.Commands;

/// <summary>
/// Implementa a interface ICommand e representa um comando para listar os itens
/// de um repositório.
/// </summary>
public class ListCommand : ICommand
{
  /// <summary>
  /// Obtém ou define o caminho do diretório do repositório.
  /// </summary>
  public string Catalog { get; set; } = "";

  /// <summary>
  /// Obtém ou define o padrão de pesquisa para listar os itens do repositório.
  /// </summary>
  public string SearchPattern { get; set; } = "";

  /// <summary>
  /// Executa o comando para listar os itens de um repositório.
  /// </summary>
  public async Task RunAsync()
  {
    var drive = Drive.GetDrive(this.Catalog);

    var repositoryBuilder = new RepositoryBuilder();
    repositoryBuilder.AddDrive(drive);
    var repository = await repositoryBuilder.BuildRepositoryAsync();

    var repositoryNavigator = new RepositoryNavigator(repository);
    var items = repositoryNavigator.List(this.SearchPattern);

    foreach (var item in items)
    {
      await Console.Out.WriteLineAsync(item);
    }
  }
}
