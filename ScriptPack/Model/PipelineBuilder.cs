using System.Buffers;
using System.Runtime.Intrinsics.X86;
using System.IO.Pipes;
using ScriptPack.Domain;
using ScriptPack.Model.Algorithms;
using System.Reflection;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;

namespace ScriptPack.Model;

/// <summary>
/// Construtor de pipelines de execução de scripts.
/// </summary>
public class PipelineBuilder
{
  private readonly List<INode> _nodes = new();
  private readonly ScriptSorterVisitor _scriptSorterVisitor = new();
  private readonly Dictionary<string, ConnectionNode> _connections = new();

  /// <summary>
  /// Adiciona os scripts habilitados de um nodo à coleção de scripts.
  /// Se o nodo representar um script, o script será adicionado diretamente.
  /// Se o nodo representar um produto, módulo, pacote, etc, todos os scripts
  /// em sua estrutura habilitados serão adicionados.
  /// </summary>
  /// <param name="node">O nodo a ser adicionado.</param>
  public void AddScripts(INode node)
  {
    this._nodes.Add(node);
  }

  /// <summary>
  /// Adiciona um template de conexão.
  /// Serão construídos pipelines para cada conexão que satisfizer o template.
  /// </summary>
  public void AddConnection(ConnectionNode connection)
  {
    this._connections[connection.Name.ToLower()] = connection;
  }

  public ConnectionNode AddConnection(string name, string provider,
      string connectionString)
  {
    var connection = new ConnectionNode
    {
      Name = name,
      Provider = provider,
      Factory = new(connectionString)
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
    // A implementação atual agrupa os scripts em pipelines, stages e steps.
    // Pipelines
    // -  Agrupamento por produto.
    // Stages
    // -  Agrupamento por conexão.
    // Steps
    // -  Agrupamento por precedência de pacote.
    // Scripts
    // -  Ordenados por nome ou dependência segundo a configuração do pacote.
    var sortedScripts = SortScripts();

    // Criando os pipelines para cada produto e conexão.
    var pipelines = new List<PipelineNode>();

    foreach (var (targetConnection, scripts) in sortedScripts)
    {
      var connections = SelectConnections(targetConnection);
      if (connections.Length == 0)
      {
        // Não estamos construindo pipelines para a conexão alvo deste conjunto
        // de scripts.
        continue;
      }

      foreach (var connection in connections)
      {
        var pipeline = CreatePipeline(connection, scripts);
        pipelines.Add(pipeline);
      }
    }

    // Agrupando os pipelines por conexão e garantindo que os pipelines para a
    // primeira conexão vista sejam executados primeiro.
    pipelines = (
        from entry in pipelines.Select((pipeline, index) => (pipeline, index))
        group entry by entry.pipeline.Connection into g
        let order = g.Select(e => e.index).Min()
        orderby order
        from item in g
        select item.pipeline
    ).Select(x => x).ToList();

    return pipelines;
  }

  /// <summary>
  /// Ordena e agrupa os scripts por ordem de execução.
  /// </summary>
  /// <remarks>
  /// 1. Primeiramente os produtos são ordenados e aguprados por suas ordens de
  ///    precedência.
  /// 2. Em seguida, pacotes são ordenados e agrupados por suas ordens de
  ///    precedência.
  /// 3. Como cada pacote pode referenciar uma conexão diferente, os grupos de
  ///    pacotes são novamente agrupados internamente por conexão.
  /// 4. A ordem utilizada para a ordenação dos grupos de pacotes é a ordem
  ///    natural de detecção das conexões entre os pacotes, isto é, a conexão
  ///    vista primeiro é a primeira a ser executada.
  /// 5. Por fim, os scripts são ordenados segundo a regra de ordenação de seus
  ///    respectivos pacotes.
  /// 6. Scripts ordenados alfabeticamente são executados primeiro.
  /// 7. Scripts ordenados por relação de dependência são executados em seguida
  ///    segundo a ordem de dependência.
  /// </remarks>
  /// <returns>
  /// Lista de tuplas contendo a conexão e os scripts a serem executados.
  /// </returns>
  private (string ConnectionName, ScriptNode[] Scripts)[] SortScripts()
  {
    var allScripts = (
        from node in _nodes
        from script in node.DescendantsAndSelf<ScriptNode>()
        select script
    ).Distinct().ToArray();

    // 1. Primeiramente os produtos são ordenados e aguprados por suas ordens de
    //    precedência.
    var byProducts = (
        from script in allScripts
        let product = script.Ancestor<ProductNode>()!
        group script by product.Precedence into g
        orderby g.Key
        select (major: g.Key, scripts: g.ToArray())
    ).ToArray();

    // 2. Em seguida, pacotes e módulos são ordenados e agrupados por suas
    //    ordens de precedência.
    var byPackages = byProducts.SelectMany(item =>
        from script in item.scripts
        let package = script.Ancestor<PackageNode>()!
        group script by package.Precedence into g
        select (item.major, minor: g.Key, scripts: g.ToArray())
    ).ToArray();

    // 3. Como cada pacote pode referenciar uma conexão diferente, os grupos de
    //    pacotes são novamente agrupados internamente por conexão.
    // 4. A ordem utilizada para a ordenação dos grupos de pacotes é a ordem
    //    natural de detecção das conexões entre os pacotes, isto é, a conexão
    //    vista primeiro é a primeira a ser executada.
    var byConnections = byPackages.SelectMany(item =>
        from script in item.scripts
        let package = script.Ancestor<PackageNode>()!
        from connection in package.TargetConnections.DefaultIfEmpty()
        group script by connection into g
        select (item.major, item.minor, connection: g.Key, scripts: g.ToList())
    ).ToArray();

    // 6. Scripts ordenados alfabeticamente são executados primeiro.
    // 7. Scripts ordenados por relação de dependência são executados em seguida
    //    segundo a ordem de dependência.
    var sorter = new ScriptSorter();
    foreach (var (_, _, _, scripts) in byConnections)
    {
      sorter.SortScripts(scripts);
    }

    var sortedScripts = (
        from item in byConnections
        orderby item.major, item.minor
        select (item.connection, item.scripts.ToArray())
    ).ToArray();
    return sortedScripts;
  }

  /// <summary>
  /// Algoritmo de seleção de uma conexão para no critério de pesquisa.
  /// </summary>
  /// <param name="searchCriteria">
  /// Critério de pesquisa. Pode ser o nome da conexão ou o nome da conexão
  /// seguido do nome do provedor, na forma:
  ///     NOME_DA_CONEXAO;Provider=NOME_DO_PROVEDOR
  /// Exemplos:
  ///     * (o mesmo que *;*)
  ///     *;Provider=SQLServer
  ///     *;Provider=PostgrSQL
  /// </param>
  /// <returns>
  /// A conexão selecionada.
  /// </returns>
  private ConnectionNode[] SelectConnections(string? searchCriteria)
  {
    // Se não temos um critério de pesquisa, selecionamos a conexão padrão.
    // É esperado que exista uma única conexão padrão.
    // Se não houver ou houver mais de uma temos dados inconsistentes no
    // repositório e lançameremos exceção de acordo.
    if (string.IsNullOrEmpty(searchCriteria))
    {
      var defaultConnections = (
          from cn in _connections.Values
          where cn.IsDefault
          select cn
      ).ToArray();

      return defaultConnections;
    }

    //
    // O nome da conexão pode conter o caracter curinga '*' para indicar
    // qualquer conexão. Neste caso o pacote é executado em todas as conexões
    // disponíveis.
    // O nome pode ainda ser seguido pelo provedor, na forma:
    //    <nome da conexão>;Provider=<nome do provedor>
    // Neste caso o pacote é executado apenas nas conexões que utilizam o
    // provedor indicado.
    // Exemplos:
    //    * (o mesmo que *;*)
    //    *;Provider=SQLServer
    //    *;Provider=PostgrSQL
    //
    var tokens = searchCriteria.Split(';');
    var connectionPattern = !string.IsNullOrEmpty(tokens[0]) ? tokens[0] : "*";
    var providerPattern = tokens.Skip(1).LastOrDefault() ?? "*";

    var selection = _connections.Values.AsEnumerable();

    if (providerPattern != "*")
    {
      var providerName = providerPattern.Split('=').Last();
      selection = selection.Where(
          cn => Providers.AreEqual(cn.Provider, providerName));
    }

    if (connectionPattern != "*")
    {
      selection = selection.Where(
          cn => cn.Name.ToLower() == connectionPattern.ToLower());
    }

    return selection.ToArray();
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
  private PipelineNode CreatePipeline(ConnectionNode connection,
      ScriptNode[] scripts)
  {
    // Criando os estágios de execução para cada produto e agrupado pela
    // precedência do módulo e do pacote.
    // Os estágios serão executados em sequência.
    var selection = (
      from script in scripts
      let module = script.Ancestor<ModuleNode>()
      let package = script.Ancestor<PackageNode>()!
      let major = module?.Precedence ?? 0
      let minor = package.Precedence
      group script by (major, minor) into g
      orderby g.Key.major, g.Key.minor
      select g.ToList()
    ).ToList();

    // Criando estágios para cada agrupamento de scripts.
    var stages = selection.Select(CreateStage).ToList();

    // Nomeando os estágios na forma "Estágio 1", "Estágio 2", etc.
    for (int i = 0; i < stages.Count; i++)
    {
      var stage = stages[i];
      stage.Name = $"Estágio-{i + 1}";
    }

    var product = scripts[0].Ancestor<ProductNode>()!;
    var version = scripts[0].Ancestor<VersionNode>()!;

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
