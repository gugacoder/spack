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
    // O comando list suporta casos especiais de pesquisa:
    // -  product[s]: Lista todas as versões de todos os produtos.
    // -  package[s]: Lista todos os pacotes do catálogo.
    var outputType = options.List.Value switch
    {
      "connections" => "connection",
      "connection" => "connection",
      "products" => "product",
      "product" => "product",
      "packages" => "package",
      "package" => "package",
      _ => "path"
    };

    var repositoryUtilityBuilder = new RepositoryUtilityBuilder();
    repositoryUtilityBuilder.AddOptions(options);
    repositoryUtilityBuilder.AddValidators();

    var repository = await repositoryUtilityBuilder.BuildRepositoryAsync();

    var nodeSelectorBuilder = new PackageSelectionBuilder();
    nodeSelectorBuilder.AddOptions(options);
    nodeSelectorBuilder.AddRepository(repository);

    if (outputType == "connection")
    {
      nodeSelectorBuilder.AddSearchCriteria("**/*.connection");
    }
    else if (outputType == "product")
    {
      nodeSelectorBuilder.AddSearchCriteria("**/-version.jsonc");
    }
    else if (outputType == "package")
    {
      nodeSelectorBuilder.AddSearchCriteria("**/-package.jsonc");
    }

    var nodes = nodeSelectorBuilder.BuildPackageSelection();

    // Detectando e reportando falhas.
    var faultReportBuilder = new FaultReportBuilder();
    faultReportBuilder.AddOptions(options);
    faultReportBuilder.AddNodes(repository);

    var faultReport = faultReportBuilder.BuildFaultReport();
    if (faultReport.Length > 0)
    {
      var printer = new FaultReportPrinter();
      printer.AddOptions(options);
      printer.AddFaultReport(faultReport);
      printer.PrintFaultReport();
    }

    // Imprimindo os itens dos catálogos.
    if (outputType == "connection")
    {
      PrintConnections(nodes);
      return;
    }
    if (outputType == "product")
    {
      PrintProducts(nodes);
      return;
    }
    if (outputType == "package")
    {
      PrintPackages(nodes);
      return;
    }
    PrintPaths(nodes);
  }

  /// <summary>
  /// Lista informação sobre as conexões pré-configuradas.
  /// </summary>
  /// <param name="nodes">
  /// Lista de nodos selecionados.
  /// </param>
  private void PrintConnections(List<INode> nodes)
  {
    var items = (
        from connection in nodes.OfType<ConnectionNode>()
        orderby connection.Name
        select (connection.Name, connection.Provider)
    ).ToList();

    // Calcula o tamanho de cada coluna
    int nameColumnWidth = items.Max(x => x.Name.Length);
    int providerColumnWidth = items.Max(x => x.Provider.Length);

    // Imprime as propriedades
    foreach (var item in items)
    {
      Console.WriteLine(
          $"{item.Name.PadRight(nameColumnWidth)}  " +
          $"{item.Provider.PadRight(providerColumnWidth)}"
          );
    }

    Console.Out.WriteLine($"Total: {items.Count}");
  }

  /// <summary>
  /// Lista os nomes dos produtos e suas versões dentre os nodos indicados.
  /// </summary>
  /// <param name="nodes">
  /// Lista de nodos selecionados.
  /// </param>
  private void PrintProducts(List<INode> nodes)
  {
    var items = (
        from version in nodes.OfType<VersionNode>()
        let product = version.Ancestor<ProductNode>()!
        let name = $"{product.Name}/{version.Name}"
        orderby name
        select name
    ).ToList();
    foreach (var item in items)
    {
      Console.Out.WriteLine(item);
    }
    Console.Out.WriteLine($"Total: {items.Count}");
  }

  /// <summary>
  /// Lista os nomes dos pacotes entre os nodos indicados.
  /// </summary>
  /// <param name="nodes">
  /// Lista de nodos selecionados.
  /// </param>
  private void PrintPackages(List<INode> nodes)
  {
    var items = (
        from package in nodes.OfType<PackageNode>()
        let modules = package.Ancestors<ModuleNode>()
        let version = package.Ancestor<VersionNode>()!
        let product = package.Ancestor<ProductNode>()!
        let moduleNames = string.Join("", modules.Select(x => $"/{x.Name}"))
        let name = $"{product.Name}/{version.Name}{moduleNames}/{package.Name}"
        orderby name
        select name
    ).ToList();
    foreach (var item in items)
    {
      Console.Out.WriteLine(item);
    }
    Console.Out.WriteLine($"Total: {items.Count}");
  }

  /// <summary>
  /// Imprime todos os caminhos dos nodos indicados.
  /// </summary>
  /// <param name="nodes">
  /// Lista de nodos selecionados.
  /// </param>
  private void PrintPaths(List<INode> nodes)
  {
    var paths = nodes
        .Except(nodes.OfType<ConnectionStringFactoryNode>())
        .Select(x => x.Path)
        .OrderBy(x => x, new PathComparer())
        .ToList();
    foreach (var node in paths)
    {
      Console.Out.WriteLine(node);
    }
    Console.Out.WriteLine($"Total: {paths.Count}");
  }
}