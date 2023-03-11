using System;
namespace ScriptPack.Domain;

/// <summary>
/// Interface para visitantes na árvore de nodos de scripts.
/// </summary>
public interface IVisitor
{
  void Visit(RepositoryNode node) { }
  void Visit(CatalogNode node) { }
  void Visit(ProductNode node) { }
  void Visit(ModuleNode node) { }
  void Visit(PackageNode node) { }
  void Visit(ScriptNode node) { }
  void Visit(PipelineNode node) { }
  void Visit(StageNode node) { }
  void Visit(StepNode node) { }
  void Visit(ConnectionNode node) { }
  void Visit(ConnectionStringFactoryNode node) { }
  void Visit(Fault node) { }
}
