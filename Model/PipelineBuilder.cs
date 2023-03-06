using System.Runtime.Intrinsics.X86;
using System.IO.Pipes;
using SPack.Domain;
using SPack.Model.Algorithms;

namespace SPack.Model;

/// <summary>
/// Construtor de pipelines de execução de scripts.
/// </summary>
public class PipelineBuilder
{
  private readonly List<INode> nodes = new();
  private readonly CyclicDependencyDetector cyclicDependencyDetector = new();
  private readonly ScriptSorting scriptSorting = new();

  /// <summary>
  /// Adiciona um script para construção do pipeline.
  /// Apenas scipts habilitados serão adicionados.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser adicionado.
  /// Se o nodo representar um script, ele será adicionado diretamente.
  /// Se o nodo representar um produto, módulo, pacote, etc, todos os scripts em
  /// sua estrutura serão adicionados.
  /// </param>
  public void AddScripts(INode node)
  {
    this.nodes.Add(node);
  }

  /// <summary>
  /// Constrói os pipelines de execução.
  /// </summary>
  /// <returns>
  /// Lista de pipelines de execução.
  /// </returns>
  public List<Pipeline> BuildPipeline()
  {
    // Selecionado os scripts que estão habilitados agrupados por produto.
    var selection = (
      from node in this.nodes
      where node.GetAncestorsAndSelf<IFileNode>().All(n => n.Enabled)
      from script in node.GetDescendantsAndSelf<Script>()
      where script.Enabled
      // group by product
      group script by script.GetProduct() into g
      select (product: g.Key, scripts: g.ToList())
    ).ToList();

    // Criando os pipelines para cada produto.
    var pipelines = selection
      .Select(s => CreatePipeline(s.product, s.scripts))
      .ToList();

    return pipelines;
  }

  private Pipeline CreatePipeline(Product product, List<Script> scripts)
  {
    // Criando os estágios de execução para cada produto e agrupado pela
    // precedência do módulo e do pacote.
    // Os estágios serão executados em sequência.
    var selection = (
      from script in scripts
      let module = script.GetModule()
      let package = script.GetPackage()
      group script by new { major = module.Precedence, minor = package.Precedence } into g
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

    var pipeline = new Pipeline();
    pipeline.Name = $"{product.Name} - {product.Version}";
    pipeline.Stages.AddRange(stages);

    return pipeline;
  }

  private Stage CreateStage(List<Script> scripts)
  {
    var stepTags = new[] {
      "-pretran",
      "-pre",
      "",
      "-pos",
      "-postran"
    };
    var scriptTags = scripts.Select(script => script.Tag).Distinct().ToArray();

    var steps = stepTags
      .Where(tag => scriptTags.Contains(tag))
      .Select(tag =>
        new Step()
        {
          Tag = tag,
          Transactional = !tag.Contains("tran"),
          Scripts = scripts.Where(script => script.Tag == tag).ToList()
        })
      .ToList();

    // Nomeando os passos...
    for (int i = 0; i < steps.Count; i++)
    {
      var step = steps[i];
      step.Name = step.Tag switch
      {
        "-pretran" => "Pré-Transação",
        "-postran" => "Pós-Transação",
        "-pre" => "Pré",
        "-pos" => "Pós",
        _ => "Principal"
      };
    }

    var stage = new Stage { Steps = steps };

    // Sequenciando os scripts.
    stage.Accept(scriptSorting);

    // Verificando se há dependências cíclicas no estágio.
    stage.Accept(cyclicDependencyDetector);

    // Verificando se há falhas no estágio.
    if (stage.GetDescendants<Fault>().Any())
    {
      stage.Faults.Add(new Fault("Existem falhas neste estágio."));
    }

    return stage;
  }
}
