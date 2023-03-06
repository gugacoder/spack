using SPack.Domain;

namespace SPack.Model.Algorithms;

public class CyclicDependencyDetector : IVisitor
{
  public void Visit(Script script)
  {
    var dependencies = new List<Script>();
    CollectDependencyTree(script, dependencies);

    foreach (var dependency in dependencies)
    {
      if (!dependency.Dependencies.Contains(script))
        continue;

      script.Faults.Add(new Fault
      {
        Hint = Fault.Hints.CyclicDependency,
        Message = $"O script {script.Name} tem dependência cíclica com {dependency.Name}"
      });
    }
  }

  private void CollectDependencyTree(Script script, List<Script> tree)
  {
    var dependencies = script.Dependencies;
    foreach (var dependency in dependencies)
    {
      if (tree.Contains(dependency))
        continue;

      tree.Add(dependency);

      CollectDependencyTree(dependency, tree);
    }
  }
}
