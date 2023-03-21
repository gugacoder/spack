using ScriptPack.Domain;
using ScriptPack.Model;
using SPack.Helpers;

namespace SPack.Commands;

/// <summary>
/// Comando de execução de scripts de migração de bases de dados.
/// </summary>
public class ValidateCommand : ICommand
{
  /// <summary>
  /// Obtém ou define um valor booleano que indica se a execução deve ser
  /// verbosa ou não.
  /// </summary>
  public bool Verbose { get; set; } = false;

  /// <summary>
  /// Obtém ou define o caminho da pasta ou arquivo do catálogo.
  /// </summary>
  public string? CatalogPath { get; set; }

  /// <summary>
  /// Obtém ou define os pacotes a serem carregados.
  /// Cada pacote tem a forma:
  ///   PRODUTO[/VERSÃO[/MÓDULO[/PACOTE]]]
  /// Exemplo:
  ///   MyProduct/1.0.0/MyModule/MyPackage
  /// </summary>
  public List<string> SearchPackageCriteria { get; set; } = new();

  /// <summary>
  /// Obtém ou define os filtros de script a serem aplicados.
  /// Um filtro é um padrão de pesquisa de pastas e arquivos virtuais na
  /// árvode de nodos do catálogo.
  /// 
  /// Por exemplo, para selecionar todos os scripts da versão 1.0.0 disponível
  /// no catálogo o filtro poderia ser: **/1.0.0.
  /// </summary>
  public List<string> SearchScriptCriteria { get; set; } = new();

  /// <summary>
  /// Executa o comando de migração de dados.
  /// </summary>
  public async Task RunAsync()
  {
    //
    // Abrindo o catálogo.
    //
    var repositoryOpener = new RepositoryCreator { DetectDependencies = true };
    var repositoryNavigator =
        await repositoryOpener.CreateRepositoryNavigatorAsync(CatalogPath);

    var rootNode = repositoryNavigator.RootNode;

    //
    // Selecionando nodos.
    //
    var selectedNodes = new INode[] { rootNode };

    if (SearchPackageCriteria.Count > 0 || SearchScriptCriteria.Count > 0)
    {
      var nodeSelector = new NodeSelector();
      nodeSelector.SearchPackageCriteria = SearchPackageCriteria;
      nodeSelector.SearchScriptCriteria = SearchScriptCriteria;

      selectedNodes = nodeSelector.SelectNodes(rootNode);
    }

    if (selectedNodes.Length == 0)
      throw new ArgumentException("Nenhum script selecionado.");

    //
    // Detectando e reportando falhas.
    //
    var faultReporter = new FaultReporter { Verbose = Verbose };
    var faultReport = faultReporter.CreateFaultReport(selectedNodes);
    if (faultReport.Length > 0)
    {
      Environment.ExitCode = 1;
      faultReporter.PrintFaultReport(faultReport);
      return;
    }

    Console.WriteLine("OK! Nenhuma falha detectada.");
  }
}
