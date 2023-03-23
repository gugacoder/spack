using System.Runtime.Intrinsics.X86;
using System.IO.Pipes;
using ScriptPack.Domain;
using ScriptPack.Algorithms;
using System.Reflection;
using ScriptPack.FileSystem;

namespace ScriptPack.Model;

/// <summary>
/// Construtor de pipelines de execução de scripts.
/// </summary>
public class PipelineBuilder
{
  private readonly List<INode> _nodes = new();
  private readonly ScriptSorterVisitor _scriptSorterVisitor = new();
  private readonly ConnectionSelector _connectionSelector = new();

  private readonly Dictionary<string, IScriptSorter> sorters = new()
  {
    { Orders.Auto, new ScriptSorterByDependency() },
    { Orders.Alpha, new ScriptSorterByName() }
  };
  private bool _addBuiltInScripts;

  /// <summary>
  /// Adiciona os scripts internos fornecidos pelo aplicativo.
  /// </summary>
  /// <remarks>
  /// Este método é usado para incluir scripts predefinidos que acompanham o
  /// aplicativo. Os scripts acrescentam objetos de automação do ScriptPack
  /// para scripts de migração de base de dados.
  /// </remarks>
  public void AddBuiltInScripts()
  {
    _addBuiltInScripts = true;
  }

  /// <summary>
  /// Adiciona um único script à coleção de scripts.
  /// </summary>
  /// <param name="script">O script a ser adicionado.</param>
  public void AddScript(ScriptNode script)
  {
    this._nodes.Add(script);
  }

  /// <summary>
  /// Adiciona uma coleção de scripts à coleção de scripts.
  /// </summary>
  /// <param name="scripts">A coleção de scripts a ser adicionada.</param>
  public void AddScripts(IEnumerable<ScriptNode> scripts)
  {
    this._nodes.AddRange(scripts);
  }

  /// <summary>
  /// Adiciona os scripts habilitados de um nodo à coleção de scripts.
  /// Se o nodo representar um script, o script será adicionado diretamente.
  /// Se o nodo representar um produto, módulo, pacote, etc, todos os scripts
  /// em sua estrutura habilitados serão adicionados.
  /// </summary>
  /// <param name="node">O nodo a ser adicionado.</param>
  public void AddScriptsFromNode(INode node)
  {
    this._nodes.Add(node);
  }

  /// <summary>
  /// Adiciona os scripts habilitados de vários nodos à coleção de scripts.
  /// Se um nodo representar um script, o script será adicionado diretamente.
  /// Se um nodo representar um produto, módulo, pacote, etc, todos os scripts
  /// em sua estrutura habilitados serão adicionados.
  /// </summary>
  /// <param name="nodes">Os nodos a serem adicionados.</param>
  public void AddScriptsFromNodes(IEnumerable<INode> nodes)
  {
    this._nodes.AddRange(nodes);
  }

  /// <summary>
  /// Constrói os pipelines de execução.
  /// </summary>
  /// <returns>
  /// Lista de pipelines de execução.
  /// </returns>
  public async Task<List<PipelineNode>> BuildPipelinesAsync()
  {
    List<INode> nodes = new(this._nodes);

    if (_addBuiltInScripts)
    {
      var assembly = Assembly.GetExecutingAssembly();
      var drive = new EmbeddedDrive(assembly);

      var catalogLoader = new CatalogLoader();
      var catalogs = await catalogLoader.LoadCatalogsAsync(drive);

      nodes.AddRange(catalogs);
    }

    // Agrupando por versão produto e base de dados.
    var selection = (
      // Selecionando nodos habiliados...
      from node in this._nodes
      where node.AncestorsAndSelf<IFileNode>().All(n => n.Enabled)
      // Selecionando scripts habilitados...
      from script in node.DescendantsAndSelf<ScriptNode>()
      where script.Enabled
      // Relacionando os pacotes...
      let package = script.Ancestor<PackageNode>()!
      from connection in _connectionSelector.SelectConnections(package)
        // Relacionando as versões dos produtos...
      let version = script.Ancestor<VersionNode>()
      group script
        by (version, connection)
        into g
      select (g.Key.version, g.Key.connection, scripts: g.Distinct().ToList())
    ).ToList();

    // Criando os pipelines para cada produto.
    var pipelines = new List<PipelineNode>();
    foreach (var (version, connection, scripts) in selection)
    {
      var pipeline = CreatePipeline(version, connection, scripts);
      pipelines.Add(pipeline);
    }

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
      stage.Name = $"Estágio-{i + 1}";
    }

    var product = version.Ancestor<ProductNode>()!;

    var pipeline = new PipelineNode();
    pipeline.Name = $"{product.Name}-{version.Version}";
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
    stage.Accept(_scriptSorterVisitor);

    // Verificando se há falhas no estágio.
    var hasFault = stage.Descendants().SelectMany(node => node.Faults).Any();
    if (hasFault)
    {
      stage.Faults.Add(new() { Message = "Existem falhas neste estágio." });
    }

    return stage;
  }
}
