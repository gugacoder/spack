// using ScriptPack.Domain;

// namespace ScriptPack.Library;

// public class NodeLocator
// {
//   private RepositoryNode repository;

//   public NodeLocator(RepositoryNode repository)
//   {
//     this.repository = repository;
//   }

//   public IFileNode LocateNode(string path)
//   {
//     var node = repository
//       .Descendants<IFileNode>()
//       .FirstOrDefault(n => n.Path == path);

//     if (node == null)
//       throw new Exception($"Node not found: {path}");

//     return node;
//   }
// }
