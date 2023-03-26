using System.Data;
using System.Text;
using ScriptPack.Model.Algorithms;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Model;
using SPack.Helpers;
using ScriptPack.Helpers;
using SPack.Prompting;
using SPack.Commands.Helpers;

namespace SPack.Commands;

/// <summary>
/// Implementa a interface ICommand e representa um comando para listar os itens
/// de um catálogo.
/// </summary>
public class ListCommand : ICommand
{
  /// <summary>
  /// Executa o comando para listar os itens de um catálogo.
  /// </summary>
  public async Task RunAsync(CommandLineOptions options)
  {
    var nodeSelectorBuilder = new PackageSelectionBuilder();
    nodeSelectorBuilder.AddOptions(options);

    var nodes = await nodeSelectorBuilder.BuildPackageSelectionAsync();
    var paths = nodes.Select(x => x.Path).ToList();

    // Imprimindo os itens dos catálogos.
    foreach (var node in paths.OrderBy(x => x, new PathComparer()))
    {
      await Console.Out.WriteLineAsync(node);
    }
    await Console.Out.WriteLineAsync($"Total: {paths.Count}");
  }
}