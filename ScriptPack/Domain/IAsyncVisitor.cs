namespace ScriptPack.Domain;

/// <summary>
/// Interface para visitantes na árvore de nodos de scripts.
/// </summary>
public interface IAsyncVisitor
{
  Task VisitAsync(RepositoryNode node) => Task.CompletedTask;
  Task VisitAsync(CatalogNode node) => Task.CompletedTask;
  Task VisitAsync(ProductNode node) => Task.CompletedTask;
  Task VisitAsync(ModuleNode node) => Task.CompletedTask;
  Task VisitAsync(PackageNode node) => Task.CompletedTask;
  Task VisitAsync(ScriptNode node) => Task.CompletedTask;
  Task VisitAsync(PipelineNode node) => Task.CompletedTask;
  Task VisitAsync(StageNode node) => Task.CompletedTask;
  Task VisitAsync(StepNode step) => Task.CompletedTask;
  Task VisitAsync(ConnectionNode node) => Task.CompletedTask;
  Task VisitAsync(ConnectionStringFactoryNode node) => Task.CompletedTask;
  Task VisitAsync(Fault node) => Task.CompletedTask;
}
