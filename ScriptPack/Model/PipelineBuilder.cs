using System.Runtime.Intrinsics.X86;
using System.IO.Pipes;
using ScriptPack.Domain;
using ScriptPack.Algorithms;

namespace ScriptPack.Model;

/// <summary>
/// Construtor de pipelines de execução de scripts.
/// </summary>
public class PipelineBuilder
{
  private readonly List<INode> nodes = new();
  private readonly ScriptSorterVisitor scriptSorterVisitor = new();
  private readonly ConnectionSelector connectionSelector = new();

  private readonly Dictionary<string, IScriptSorter> sorters = new()
  {
    { Orders.Auto, new ScriptSorterByDependency() },
    { Orders.Alpha, new ScriptSorterByName() }
  };

  /// <summary>
  /// Adiciona um único script à coleção de scripts.
  /// </summary>
  /// <param name="script">O script a ser adicionado.</param>
  public void AddScript(ScriptNode script)
  {
    this.nodes.Add(script);
  }

  /// <summary>
  /// Adiciona uma coleção de scripts à coleção de scripts.
  /// </summary>
  /// <param name="scripts">A coleção de scripts a ser adicionada.</param>
  public void AddScripts(IEnumerable<ScriptNode> scripts)
  {
    this.nodes.AddRange(scripts);
  }

  /// <summary>
  /// Adiciona os scripts habilitados de um nó à coleção de scripts.
  /// Se o nó representar um script, o script será adicionado diretamente.
  /// Se o nó representar um produto, módulo, pacote, etc, todos os scripts em sua estrutura habilitados serão adicionados.
  /// </summary>
  /// <param name="node">O nó a ser adicionado.</param>
  public void AddScriptsFromNode(INode node)
  {
    this.nodes.Add(node);
  }

  /// <summary>
  /// Adiciona os scripts habilitados de vários nós à coleção de scripts.
  /// Se um nó representar um script, o script será adicionado diretamente.
  /// Se um nó representar um produto, módulo, pacote, etc, todos os scripts em sua estrutura habilitados serão adicionados.
  /// </summary>
  /// <param name="nodes">Os nós a serem adicionados.</param>
  public void AddScriptsFromNodes(IEnumerable<INode> nodes)
  {
    this.nodes.AddRange(nodes);
  }

  /// <summary>
  /// Constrói os pipelines de execução.
  /// </summary>
  /// <returns>
  /// Lista de pipelines de execução.
  /// </returns>
  public List<PipelineNode> BuildPipelines()
  {
    // Selecionado os scripts que estão habilitados agrupados por produto.
    var selection = (

      // Selecionando apenas os nodos com ancestors habilitados.
      from node in this.nodes
      where node.AncestorsAndSelf<IFileNode>().All(n => n.Enabled)

      // Selecionando apenas os scripts habilitados.
      from script in node.DescendantsAndSelf<ScriptNode>()
      where script.Enabled

      let package = script.Ancestor<PackageNode>()!
      from connection in connectionSelector.SelectConnections(package)

        // Agrupando por produto e conexão.
      group script
        by (version: script.Ancestor<VersionNode>(), connection)
        into g

      select (g.Key, scripts: g.ToList())
    ).ToList();

    // Criando os pipelines para cada produto.
    var pipelines = selection
      .Select(s =>
          CreatePipeline(s.Key.version, s.Key.connection, s.scripts))
      .ToList();

    return pipelines;
  }

  /// <summary>
  /// Constrói o pipeline e seus estágios e passos para execução dos scripts
  /// indicados na ordem correta
  /// </summary>
  /// <param name="version">
  /// A versão do produto que será executado.
  /// </param>
  /// <param name="connection">
  /// A conexão que será utilizada para executar os scripts.
  /// </param>
  /// <param name="scripts">
  /// Lista de scripts a serem executados.
  /// </param>
  /// <returns>
  /// O pipeline de execução.
  /// </returns>
  private PipelineNode CreatePipeline(VersionNode version,
      ConnectionNode connection, List<ScriptNode> scripts)
  {
    // Criando os estágios de execução para cada produto e agrupado pela
    // precedência do módulo e do pacote.
    // Os estágios serão executados em sequência.
    var selection = (
      from script in scripts
      let module = script.Ancestor<ModuleNode>()
      let package = script.Ancestor<PackageNode>()
      group script
        by new { major = module.Precedence, minor = package.Precedence }
        into g
      select g.ToList()
    ).ToList();

    // Criando estágios para cada agrupamento de scripts.
    var stages = selection
      .Select(scripts => CreateStage(scripts))
      .ToList();

    // Nomeando os estágios na forma "Estágio 1", "Estágio 2", etc.
    for (int i = 0; i < stages.Count; i++)
    {
      var stage = stages[i];
      stage.Name = $"Estágio {i + 1}";
    }

    var product = version.Ancestor<ProductNode>()!;

    var pipeline = new PipelineNode();
    pipeline.Name = $"{product.Name} - {version.Version}";
    pipeline.Connection = connection;
    pipeline.Stages.AddRange(stages);

    return pipeline;
  }

  /// <summary>
  /// Constrói o estágio de execução para os scripts indicados.
  /// </summary>
  /// <param name="scripts">
  /// Lista de scripts a serem executados.
  /// </param>
  /// <returns>
  /// O estágio de execução.
  /// </returns>
  private StageNode CreateStage(List<ScriptNode> scripts)
  {
    var scriptTags = scripts.Select(script => script.Tag).Distinct().ToArray();

    var steps = Steps.AllSteps
      .Where(tag => scriptTags.Contains(tag))
      .Select(tag =>
        new StepNode()
        {
          Tag = tag,
          Transactional = Steps.IsTransactional(tag),
          Scripts = scripts.Where(script => script.Tag == tag).ToList()
        })
      .ToList();

    // Nomeando os passos...
    for (int i = 0; i < steps.Count; i++)
    {
      var step = steps[i];
      step.Name = Steps.NameStep(step.Tag);
    }

    var stage = new StageNode();
    stage.Steps.AddRange(steps);

    // Sequenciando os scripts.
    stage.Accept(scriptSorterVisitor);

    // Verificando se há falhas no estágio.
    var hasFault = stage.Descendants().SelectMany(node => node.Faults).Any();
    if (hasFault)
    {
      stage.Faults.Add(new() { Message = "Existem falhas neste estágio." });
    }

    return stage;
  }
}
