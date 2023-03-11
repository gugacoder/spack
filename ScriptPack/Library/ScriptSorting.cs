// using ScriptPack.Domain;

// namespace ScriptPack.Library;

// public class ScriptSorting : IVisitor
// {
//   public void Visit(StepNode step)
//   {
//     var scripts = step.Scripts;

//     int i = 0;
//     while (i < scripts.Count)
//     {
//       var script = scripts[i];
//       var index = scripts.IndexOf(script);

//       var dependants = scripts.Where(s => s.Dependencies.Contains(script));

//       var newIndex = index;
//       foreach (var dependant in dependants)
//       {
//         var dependantIndex = scripts.IndexOf(dependant);
//         if (dependantIndex < newIndex)
//         {
//           newIndex = dependantIndex;
//         }
//       }

//       if (newIndex != index)
//       {
//         scripts.RemoveAt(index);
//         scripts.Insert(newIndex, script);
//         continue;
//       }

//       i++;
//     }

//   }
// }
