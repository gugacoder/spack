using System;
namespace SPack.Domain;

/// <summary>
/// Interface para visitantes na árvore de nodos de scripts.
/// </summary>
public interface IVisitor
{
  void Visit(Repository node) { }
  void Visit(Catalog node) { }
  void Visit(Product node) { }
  void Visit(Module node) { }
  void Visit(Package node) { }
  void Visit(Script node) { }
  void Visit(Pipeline node) { }
  void Visit(Stage node) { }
  void Visit(Step node) { }
  void Visit(Connection node) { }
  void Visit(ConnectionFactory node) { }
  void Visit(Fault node) { }
}
