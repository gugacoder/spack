// using System.Runtime.Intrinsics.X86;
// using System.IO.Pipes;
// using ScriptPack.Domain;
// using ScriptPack.Model.Algorithms;

// namespace ScriptPack.Model;

// /// <summary>
// /// Construtor de pipelines de execução de scripts.
// /// </summary>
// public class PipelineBuilder
// {
//   private readonly List<INode> nodes = new();
//   private readonly CyclicDependencyDetector cyclicDependencyDetector = new();
//   private readonly ScriptSorting scriptSorting = new();

//   /// <summary>
//   /// Adiciona um script para construção do pipeline.
//   /// Apenas scipts habilitados serão adicionados.
//   /// </summary>
//   /// <param name="node">
//   /// Nodo a ser adicionado.
//   /// Se o nodo representar um script, ele será adicionado diretamente.
//   /// Se o nodo representar um produto, módulo, pacote, etc, todos os scripts em
//   /// sua estrutura serão adicionados.
//   /// </param>
//   public void AddScripts(INode node)
//   {
//     this.nodes.Add(node);
//   }

//   /// <summary>
//   /// Constrói os pipelines de execução.
//   /// </summary>
//   /// <returns>
//   /// Lista de pipelines de execução.
//   /// </returns>
//   public List<PipelineNode> BuildPipelines()
//   {
//     // Selecionado os scripts que estão habilitados agrupados por produto.
//     var selection = (

//       // Selecionando apenas os nodos com ancestors habilitados.
//       from node in this.nodes
//       where node.AncestorsAndSelf<IFileNode>().All(n => n.Enabled)

//       // Selecionando apenas os scripts habilitados.
//       from script in node.DescendantsAndSelf<ScriptNode>()
//       where script.Enabled

//       from connectionName in script.Ancestor<PackageNode>()!.Connections

//         // Agrupando por produto e conexão.
//       group script by (product: script.Ancestor<ProductNode>(), connectionName) into g

//       select (g.Key, scripts: g.ToList())
//     ).ToList();

//     // Criando os pipelines para cada produto.
//     var pipelines = selection
//       .Select(s => CreatePipeline(s.Key.product, s.Key.connectionName, s.scripts))
//       .ToList();

//     return pipelines;
//   }

//   private PipelineNode CreatePipeline(ProductNode product, string connectionName, List<ScriptNode> scripts)
//   {
//     var connections = product.Ancestor<CatalogNode>()!.Connections;
//     var connection = connections.FirstOrDefault(c => c.Name == connectionName)
//       ?? throw new Exception($"Conexão não encontrada: {connectionName}");

//     // Criando os estágios de execução para cada produto e agrupado pela
//     // precedência do módulo e do pacote.
//     // Os estágios serão executados em sequência.
//     var selection = (
//       from script in scripts
//       let module = script.Ancestor<ModuleNode>()
//       let package = script.Ancestor<PackageNode>()
//       group script by new { major = module.Precedence, minor = package.Precedence } into g
//       select g.ToList()
//     ).ToList();

//     // Criando estágios para cada agrupamento de scripts.
//     var stages = selection
//       .Select(scripts => CreateStage(scripts))
//       .ToList();

//     // Nomeando os estágios na forma "Estágio 1", "Estágio 2", etc.
//     for (int i = 0; i < stages.Count; i++)
//     {
//       var stage = stages[i];
//       stage.Name = $"Estágio {i + 1}";
//     }

//     var pipeline = new PipelineNode();
//     pipeline.Name = $"{product.Name} - {product.Version}";
//     pipeline.Connection = connection;
//     pipeline.Stages.AddRange(stages);

//     return pipeline;
//   }

//   private StageNode CreateStage(List<ScriptNode> scripts)
//   {
//     var scriptTags = scripts.Select(script => script.Tag).Distinct().ToArray();

//     var steps = Steps.AllSteps
//       .Where(tag => scriptTags.Contains(tag))
//       .Select(tag =>
//         new Step()
//         {
//           Tag = tag,
//           Transactional = Steps.IsTransactional(tag),
//           Scripts = scripts.Where(script => script.Tag == tag).ToList()
//         })
//       .ToList();

//     // Nomeando os passos...
//     for (int i = 0; i < steps.Count; i++)
//     {
//       var step = steps[i];
//       step.Name = Steps.NameStep(step.Tag);
//     }

//     var stage = new StageNode();
//     stage.Steps.AddRange(steps);

//     // Sequenciando os scripts.
//     stage.Accept(scriptSorting);

//     // Verificando se há dependências cíclicas no estágio.
//     stage.Accept(cyclicDependencyDetector);

//     // Verificando se há falhas no estágio.
//     if (stage.Descendants<FaultNode>().Any())
//     {
//       stage.Faults.Add(new() { Message = "Existem falhas neste estágio." });
//     }

//     return stage;
//   }
// }
