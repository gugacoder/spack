using System.Data;
using System.Text.Json;
using ScriptPack.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;

namespace SPack.Commands;

/// <summary>
/// Comando de exibição do conteúdo de um arquivo do catálogo.
/// </summary>
public class ShowCommand : ICommand
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
  /// Obtém ou define os mapas de configuração de conexão.
  /// Cada entrada no mapa tem a forma:
  ///    [nome]:[connection string]
  /// Exemplo:
  ///    myapp:Server=127.0.0.1;Database=MyDB;User Id=MyUser;Password=MyPass;
  /// </summary>
  public List<string>? ConnectionMaps { get; set; } = new();

  /// <summary>
  /// Executa o comando para mostrar o conteúdo dos itens de um catálogo.
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
    // Navegando pelos nodos.
    //
    var nodes = repositoryNavigator.ListNodes(this.SearchPattern);
    foreach (var node in nodes)
    {
      Console.WriteLine(new string('-', 80));
      Console.WriteLine($"#{node.Path}");
      try
      {
        if (node is ScriptNode script && script.FilePath != null)
        {
          var catalog = script.AncestorsAndSelf<CatalogNode>().FirstOrDefault();
          if (catalog?.Drive is IDrive drive)
          {
            var text = await drive.ReadAllTextAsync(script.FilePath);
            await Console.Out.WriteLineAsync(text);
            continue;
          }
        }

        // TODO: System.Text.Json.JsonSerializer ainda não é capaz de serializar
        // derivados de ObservableCollection<T> e List<T> sem hackings.
        // Estamos deixando a serialização incompleta por enquanto.
        // Se necessário for, recomendo a utilização da biblioteca
        // Newtonsoft.Json.
        var json = JsonSerializer.Serialize(node, JsonOptions.IndentedCamelCase);
        await Console.Out.WriteLineAsync(json);
      }
      catch (Exception ex)
      {
        await Console.Out.WriteLineAsync($"Error: {ex.Message}");
        if (Verbose)
        {
          await Console.Out.WriteLineAsync(ex.StackTrace);
        }
      }
      await Console.Out.WriteLineAsync();
    }
    await Console.Out.WriteLineAsync($"Total: {nodes.Length}");
  }
}
