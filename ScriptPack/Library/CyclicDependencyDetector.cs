// using ScriptPack.Domain;

// namespace ScriptPack.Library;

// public class CyclicDependencyDetector : IVisitor
// {
//   public void Visit(ScriptNode script)
//   {
//     var dependencies = new List<ScriptNode>();
//     CollectDependencyTree(script, dependencies);

//     foreach (var dependency in dependencies)
//     {
//       if (!dependency.Dependencies.Contains(script))
//         continue;

//       script.Faults.Add(FaultNode.EmitCircularDependency(script, dependency));
//     }
//   }

//   private void CollectDependencyTree(ScriptNode script, List<ScriptNode> tree)
//   {
//     var dependencies = script.Dependencies;
//     foreach (var dependency in dependencies)
//     {
//       if (tree.Contains(dependency))
//         continue;

//       tree.Add(dependency);

//       CollectDependencyTree(dependency, tree);
//     }
//   }
// }
