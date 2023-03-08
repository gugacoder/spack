using SPack.Domain;

namespace SPack.Model.Algorithms;

public class NodeLocator
{
  private Repository repository;

  public NodeLocator(Repository repository)
  {
    this.repository = repository;
  }

  public IFileNode LocateNode(string path)
  {
    var node = repository
      .GetDescendants<IFileNode>()
      .FirstOrDefault(n => n.Path == path);

    if (node == null)
      throw new Exception($"Node not found: {path}");

    return node;
  }
}
