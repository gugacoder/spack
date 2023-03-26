using System.Runtime.Intrinsics.X86;
using System.IO.Pipes;
using ScriptPack.Domain;
using ScriptPack.Model.Algorithms;
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
  private readonly Dictionary<string, ConnectionNode> _connections = new();

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
    this._nodes.Add(script);
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
  /// Adiciona um template de conexão.
  /// Serão construídos pipelines para cada conexão que satisfizer o template.
  /// </summary>
  public void AddConnection(ConnectionNode connection)
  {
    this._connections.Add(connection.Name.ToLower(), connection);
  }

  public ConnectionNode AddConnection(string name, string provider,
      string connectionString)
  {
    var connection = new ConnectionNode
    {
      Name = name,
      Provider = provider,
      ConnectionStringFactory = new(connectionString)
    };
    _connections.Add(name.ToLower(), connection);
    return connection;
  }

  /// <summary>
  /// Constrói os pipelines de execução.
  /// </summary>
  /// <returns>
  /// Lista de pipelines de execução.
  /// </returns>
  public List<PipelineNode> BuildPipelines()
  {
    // A implementação atual constrói um pipeline para cada par de produto e
    // conexão correspondente.

    // Selecionando os scripts e os agrupando por versão de produto.
    // Note que a conexão pode ser nula. Neste caso a conexão será associada à
    // conexão padrão se especificada.
    List<(VersionNode Product, string? TargetConnection, ScriptNode[] Scripts)>
        selection = new(
            from node in _nodes
            from script in node.DescendantsAndSelf<ScriptNode>()
            let package = script.Ancestor<PackageNode>()!
            from connection in package.TargetConnections.DefaultIfEmpty()
            let version = script.Ancestor<VersionNode>()!
            group script by (version, connection) into g
            select (g.Key.version, g.Key.connection, g.Distinct().ToArray())
        );

    // Criando os pipelines para cada produto e conexão.
    var pipelines = new List<PipelineNode>();
    foreach (var (version, targetConnection, scripts) in selection)
    {
      var connectionName = targetConnection;

      if (string.IsNullOrEmpty(connectionName))
      {
        var defaultConnections = (
            from cn in _connections.Values
            where cn.IsDefault
            select cn.Name
        ).ToArray();

        if (defaultConnections.Count() == 0)
        {
          var product = version.Ancestor<ProductNode>()!;
          throw new InvalidOperationException(
              "Não foi especificada nenhuma conexão padrão para o produto " +
              $"{product.Name} versão {version.Version}.");
        }
        if (defaultConnections.Count() > 1)
        {
          var product = version.Ancestor<ProductNode>()!;
          throw new InvalidOperationException(
              "Mais de uma conexão padrão foi especificada para o produto " +
              $"{product.Name} versão {version.Version}.");
        }

        connectionName = defaultConnections[0];
      }

      if (!_connections.ContainsKey(connectionName.ToLower()))
      {
        // Não estamos construindo pipelines para a conexão alvo deste conjunto
        // de scripts.
        continue;
      }

      var connection = _connections[connectionName.ToLower()];
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
      ConnectionNode connection, ScriptNode[] scripts)
  {
    // Criando os estágios de execução para cada produto e agrupado pela
    // precedência do módulo e do pacote.
    // Os estágios serão executados em sequência.
    var selection = (
      from script in scripts
      let module = script.Ancestor<ModuleNode>()
      let package = script.Ancestor<PackageNode>()!
      group script
        by new {
          major = module?.Precedence ?? 0,
          minor = package.Precedence
        }
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
