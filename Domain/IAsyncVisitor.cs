namespace SPack.Domain;

/// <summary>
/// Interface para visitantes na árvore de nodos de scripts.
/// </summary>
public interface IAsyncVisitor
{
  Task VisitAsync(Repository node) => Task.CompletedTask;
  Task VisitAsync(Catalog node) => Task.CompletedTask;
  Task VisitAsync(Product node) => Task.CompletedTask;
  Task VisitAsync(Module node) => Task.CompletedTask;
  Task VisitAsync(Package node) => Task.CompletedTask;
  Task VisitAsync(Script node) => Task.CompletedTask;
  Task VisitAsync(Pipeline node) => Task.CompletedTask;
  Task VisitAsync(Stage node) => Task.CompletedTask;
  Task VisitAsync(Step step) => Task.CompletedTask;
  Task VisitAsync(Connection node) => Task.CompletedTask;
  Task VisitAsync(ConnectionStringFactory node) => Task.CompletedTask;
  Task VisitAsync(Fault node) => Task.CompletedTask;
}
