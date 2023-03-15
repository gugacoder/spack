using System.Data;
using ScriptPack.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Model;

namespace SPack.Commands;

/// <summary>
/// Implementa a interface ICommand e representa um comando para listar os itens
/// de um catálogo.
/// </summary>
public class ListCommand : ICommand
{
  /// <summary>
  /// Obtém ou define um valor booleano que indica se a execução deve ser
  /// verbosa ou não.
  /// </summary>
  public bool Verbose { get; set; } = false;

  /// <summary>
  /// Obtém ou define o caminho da pasta ou do arquivo do catálogo.
  /// </summary>
  public string? CatalogPath { get; set; }

  /// <summary>
  /// Obtém ou define o padrão de pesquisa para listar os itens do catálogo.
  /// </summary>
  public string SearchPattern { get; set; } = "";

  /// <summary>
  /// Executa o comando para listar os itens de um catálogo.
  /// </summary>
  public async Task RunAsync()
  {
    //
    // Abrindo o navegador de nodos.
    //
    var repositoryOpener = new RepositoryOpener();
    var repositoryNavigator =
        await repositoryOpener.OpenRepositoryNavigatorAsync(CatalogPath);

    //
    // Navegando e imprimindo o resultado da pesquisa.
    //
    var items = repositoryNavigator.List(this.SearchPattern);
    foreach (var item in items)
    {
      await Console.Out.WriteLineAsync(item);
    }
    await Console.Out.WriteLineAsync($"Total: {items.Length}");
  }
}
