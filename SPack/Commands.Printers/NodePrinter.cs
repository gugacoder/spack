using ScriptPack.Domain;
using ScriptPack.Model.Algorithms;

namespace SPack.Commands.Printers;

/// <summary>
/// Imprime os pacotes encontrados entre os nodos.
/// </summary>
public class NodePrinter : IPrinter
{
  private readonly List<INode> _nodes = new();
  private readonly List<Type> _templates = new();
  private bool _verbose = false;

  /// <summary>
  /// Adiciona um nodo à lista de nodos a serem impressos.
  /// </summary>
  public NodePrinter AddNodes(List<INode> nodes)
  {
    _nodes.AddRange(nodes);
    return this;
  }

  /// <summary>
  /// Adiciona um template de impressão.
  /// Serão impressos apenas os nodos que satisfazem o template no formato
  /// apropriado para o tipo.
  /// </summary>
  /// <returns>
  /// O próprio objeto para encadeamento de chamadas.
  /// </returns>
  public NodePrinter AddTemplate<T>() where T : INode
  {
    _templates.Add(typeof(T));
    return this;
  }

  /// <summary>
  /// Ativa a impressão de mais detalhes sobre os nodos.
  /// </summary>
  /// <param name="verbose">
  /// Indica se a impressão deve ser mais detalhada.
  /// </param>
  /// <returns>
  /// O próprio objeto para encadeamento de chamadas.
  /// </returns>
  public NodePrinter SetVerbose(bool verbose = true)
  {
    _verbose = true;
    return this;
  }

  /// <summary>
  /// Imprime os pacotes encontrados entre os nodos.
  /// </summary>
  public void Print()
  {
    foreach (var template in _templates)
    {
      PrintWithTemplate(template);
    }
  }

  /// <summary>
  /// Seleciona e imprime os nodos que satisfazem o template.
  /// </summary>
  /// <param name="template">
  /// O tipo do template a ser utilizado para a impressão.
  /// </param>
  private void PrintWithTemplate(Type template)
  {
    var selectedNodes = (
        // Extraindo informação da árvore de catálogo.
        from node in _nodes
        from candidate in node.DescendantsAndSelf()
        where candidate.GetType() == template
        select node
    ).Union(
        // Extraindo informação da árvode do pipeline.
        // Como a árvore do pipeline não contém os nodos da árvore de catálogo,
        // temos que descer até os nodos StepNode e obter deles a coleção de
        // scripts vinculados.
        from node in _nodes
        from step in node.DescendantsAndSelf<StepNode>()
        from script in step.Scripts
        from candidate in script.AncestorsAndSelf()
        where candidate.GetType() == template
        select candidate
    ).ToArray();

    // Imprimindo conexões.
    if (template == typeof(ConnectionNode))
    {
      PrintTable(
          from connection in selectedNodes.OfType<ConnectionNode>()
          orderby connection.Name
          select new
          {
            connection.Name,
            connection.Provider,
            IsDefault = connection.IsDefault ? "Sim" : "Não",
            BoundTo = connection.BoundTo ?? "",
          }
      );

      return;
    }

    // Imprimindo produtos.
    if (template == typeof(VersionNode))
    {
      PrintTable(
          from version in selectedNodes.OfType<VersionNode>()
          let product = version.Ancestor<ProductNode>()
          orderby product.Precedence, product.Name, version.Version
          select new
          {
            product.Name,
            version.Version,
            product.Precedence
          }
      );
      return;
    }

    // Imprimindo pacotes.
    if (template == typeof(PackageNode))
    {
      var packageIdHandler = new PackageIdHandler();
      if (_verbose)
      {
        PrintTable(
            from package in selectedNodes.OfType<PackageNode>()
            let product = package.Ancestor<ProductNode>()
            let version = package.Ancestor<VersionNode>()
            let modules = package.Ancestors<ModuleNode>()
            orderby product.Name, version.Version,
                    package.Precedence, package.Name
            select new
            {
              Product = product.Name,
              version.Version,
              package.Name,
              Module = string.Join("/", modules.Select(x => x.Name)),
              package.Order,
              package.Precedence,
              Connections = string.Join(",", package.TargetConnections),
              ID = packageIdHandler.CreatePackageId(package)
            }
        );
      } else {
        PrintTable(
            from package in selectedNodes.OfType<PackageNode>()
            let product = package.Ancestor<ProductNode>()
            let version = package.Ancestor<VersionNode>()
            let modules = package.Ancestors<ModuleNode>()
            orderby product.Name, version.Version,
                    package.Precedence, package.Name
            select new
            {
              ID = packageIdHandler.CreatePackageId(package),
              package.Order,
              package.Precedence,
              Connections = string.Join(",", package.TargetConnections),
            }
        );
      }
      return;
    }

    //
    // Imprime os caminhos dos nodos selecionados.
    //
    foreach (var node in selectedNodes.OrderBy(x => x.Path))
    {
      Console.Out.WriteLine(node.Path);
    }
    Console.Out.WriteLine($"Total: {selectedNodes.Count()}");
  }

  /// <summary>
  /// Imprime as propriedades em forma de tabela com cabeçalho, uma linha
  /// divisória e os campos com colunas de espaçamento otimizados.
  /// </summary>
  /// <param name="items">
  /// As linhas da tabela a serem impressas.
  /// </param>
  private void PrintTable(IEnumerable<object> items)
  {
    if (items.Any())
    {
      // Nomes para as colunas
      string[] columns = items.First()
          .GetType().GetProperties()
          .Select(p => p.Name)
          .ToArray();

      // Valores dos campos
      string[][] rows = items
          .Select(item => item
              .GetType().GetProperties()
              .Select(p => p.GetValue(item)?.ToString() ?? "")
              .ToArray()
          ).ToArray();

      var columnLengths = columns
          .Select((_, i)
              => new[] { columns }.Concat(rows).Max(row => row[i].Length))
          .ToArray();

      // Imprime os títulos
      Console.WriteLine(string.Join("  ", columns
          .Select((name, i) => $"{name.PadRight(columnLengths[i])}")));

      // Imprime uma linha de separação
      Console.WriteLine(string.Join("  ", columnLengths
          .Select(length => new string('-', length))));

      // Imprime as propriedades
      foreach (var row in rows)
      {
        Console.WriteLine(string.Join("  ", row.Select(
            (property, i) => property.ToString().PadRight(columnLengths[i]))));
      }

      // Imprime uma linha de separação
      Console.WriteLine(string.Join("  ", columnLengths
          .Select(length => new string('-', length))));
    }
    Console.Out.WriteLine($"Total: {items.Count()}");
  }
}
