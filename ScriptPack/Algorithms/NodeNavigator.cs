using ScriptPack.Domain;

namespace ScriptPack.Algorithms;

public class NodeNavigator
{
  private readonly RepositoryNode repository;

  private string CurrentPath = "/";

  public NodeNavigator(RepositoryNode repository)
  {
    this.repository = repository;
  }

  public string GetFullPath(string path)
  {
    // Verifica se o caminho fornecido é absoluto
    if (path.StartsWith("/"))
      return path;

    // Separa o caminho fornecido em seus componentes
    var pathComponents = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

    // Separa o caminho atual em seus componentes
    var currentPathComponents = CurrentPath.Split('/',
        StringSplitOptions.RemoveEmptyEntries);

    // Itera sobre os componentes do caminho fornecido
    foreach (var component in pathComponents)
    {
      // Se o componente for "..", remove o último componente do caminho atual
      if (component == "..")
      {
        if (currentPathComponents.Length > 0)
        {
          currentPathComponents = currentPathComponents[..^1];
        }
      }
      // Se o componente for ".", não faz nada
      else if (component == ".")
      {
        continue;
      }
      // Caso contrário, adiciona o componente ao caminho atual
      else
      {
        currentPathComponents = currentPathComponents.Append(component)
            .ToArray();
      }
    }

    // Retorna o caminho completo
    return "/" + string.Join('/', currentPathComponents);
  }

  public void ChangeInto(string path)
  {
    CurrentPath = GetFullPath(path);
  }

  public void Create(string path)
  {
  }

  public void Remove(string path)
  {
  }

  public IEnumerable<INode> List(string path)
  {
    path = GetFullPath(path);
    foreach (var descendant in repository.DescendantsAndSelf())
    {
      if (descendant.Path.StartsWith(path))
      {
        yield return descendant;
      }
    }
  }

  public void Search(string path)
  {
  }

  public void SearchInContent(string path)
  {
  }

}
