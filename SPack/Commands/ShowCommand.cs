using System.Data;
using System.Text;
using Newtonsoft.Json;
using ScriptPack.Model.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;
using ScriptPack.Model;
using SPack.Helpers;
using SPack.Prompting;
using SPack.Commands.Helpers;

namespace SPack.Commands;

/// <summary>
/// Comando de exibição do conteúdo de um arquivo do catálogo.
/// </summary>
public class ShowCommand : ICommand
{
  /// <summary>
  /// Executa o comando para mostrar o conteúdo dos itens de um catálogo.
  /// </summary>
  public async Task RunAsync(CommandLineOptions options)
  {
    var repositoryUtilityBuilder = new RepositoryUtilityBuilder();
    repositoryUtilityBuilder.AddOptions(options);

    var repository = await repositoryUtilityBuilder.BuildRepositoryAsync();

    var nodeSelectorBuilder = new PackageSelectionBuilder();
    nodeSelectorBuilder.AddOptions(options);
    nodeSelectorBuilder.AddRepository(repository);

    var nodes = nodeSelectorBuilder.BuildPackageSelection();

    // Imprimindo o conteúdo dos nodos.
    foreach (var node in nodes)
    {
      if (options.Verbose.On)
      {
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"{node.Path}:");
        Console.WriteLine();
      }

      try
      {
        if (node is ScriptNode script && script.FilePath is not null)
        {
          var catalog = script.AncestorsAndSelf<CatalogNode>().FirstOrDefault();
          if (catalog?.Drive is IDrive driveCatalog)
          {
            var text = await driveCatalog.ReadAllTextAsync(script.FilePath);
            await Console.Out.WriteLineAsync(text);
            continue;
          }
        }

        // TODO: Newtonsoft.Json.JsonSerializer ainda não é capaz de serializar
        // derivados de ObservableCollection<T> e List<T> sem hackings.
        // Estamos deixando a serialização incompleta por enquanto.
        // Se necessário for, recomendo a utilização da biblioteca
        // Newtonsoft.Json.
        var json = JsonConvert.SerializeObject(node,
            JsonOptions.IndentedCamelCase);
        await Console.Out.WriteLineAsync(json);
      }
      catch (Exception ex)
      {
        await Console.Out.WriteLineAsync($"Error: {ex.Message}");
        if (options.Verbose.On)
        {
          await Console.Out.WriteLineAsync(ex.StackTrace);
        }
      }
      await Console.Out.WriteLineAsync();
    }
  }
}
