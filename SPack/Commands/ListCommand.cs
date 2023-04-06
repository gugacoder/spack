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
using SPack.Commands.Printers;

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

    // Removendo o critério de pesquisa da opção caso seja uma dessas reservadas
    // pelo comando list.
    if (outputType != "path")
    {
      options.List.Value = "";
    }

    var repositoryUtilityBuilder = new RepositoryUtilityBuilder();
    repositoryUtilityBuilder.AddOptions(options);
    if (!options.IgnoneDependencies.On)
    {
      repositoryUtilityBuilder.AddValidators();
    }

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
    var nodePrinter = new NodePrinter();

    if (outputType == "connection") nodePrinter.AddTemplate<ConnectionNode>();
    if (outputType == "product") nodePrinter.AddTemplate<VersionNode>();
    if (outputType == "package") nodePrinter.AddTemplate<PackageNode>();

    nodePrinter
        .AddNodes(nodes)
        .SetVerbose(options.Verbose.On)
        .Print();
  }
}