namespace SPack.Domain;

public static class NodeExtesions
{
  /// <summary>
  /// Detecta o repositório na árvore de parentesco.
  /// </summary>
  public static Repository? GetRepository(this INode node)
  {
    if (node is Repository repository) return repository;
    if (node.Parent == null) return null;
    return node.Parent.GetRepository();
  }

  /// <summary>
  /// Detecta o catálogo na árvore de parentesco.
  /// </summary>
  public static Catalog? GetCatalog(this INode node)
  {
    if (node is Catalog catalog) return catalog;
    if (node.Parent == null) return null;
    return node.Parent.GetCatalog();
  }

  /// <summary>
  /// Detecta o produto na árvore de parentesco.
  /// </summary>
  public static Product? GetProduct(this INode node)
  {
    if (node is Product product) return product;
    if (node.Parent == null) return null;
    return node.Parent.GetProduct();
  }

  /// <summary>
  /// Detecta o módulo na árvore de parentesco.
  /// </summary>
  public static Module? GetModule(this INode node)
  {
    if (node is Module module) return module;
    if (node.Parent == null) return null;
    return node.Parent.GetModule();
  }

  /// <summary>
  /// Detecta o pacote na árvore de parentesco.
  /// </summary>
  public static Package? GetPackage(this INode node)
  {
    if (node is Package package) return package;
    if (node.Parent == null) return null;
    return node.Parent.GetPackage();
  }

  /// <summary>
  /// Detecta o script na árvore de parentesco.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <typeparam name="T">
  /// Tipo de nodo a ser verificado.
  /// </typeparam>
  /// <returns>
  /// O nodo script ou null.
  /// </returns>
  public static IEnumerable<INode> GetAncestors(this INode node)
  {
    if (node.Parent == null) yield break;
    yield return node.Parent;
    foreach (var ancestor in node.Parent.GetAncestors())
    {
      yield return ancestor;
    }
  }

  /// <summary>
  /// Detecta o script na árvore de parentesco.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <typeparam name="T">
  /// Tipo de nodo a ser verificado.
  /// </typeparam>
  /// <returns>
  /// O nodo script ou null.
  /// </returns>
  public static IEnumerable<INode> GetAncestorsAndSelf(this INode node)
  {
    yield return node;
    if (node.Parent == null) yield break;
    yield return node.Parent;
    foreach (var ancestor in node.Parent.GetAncestors())
    {
      yield return ancestor;
    }
  }

  /// <summary>
  /// Detecta o script na árvore de parentesco.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <typeparam name="T">
  /// Tipo de nodo a ser verificado.
  /// </typeparam>
  /// <returns>
  /// O nodo script ou null.
  /// </returns>
  public static IEnumerable<T> GetAncestors<T>(this INode node)
    where T : INode
  {
    if (node.Parent == null) yield break;
    if (node.Parent is T t) yield return t;
    foreach (var ancestor in node.Parent.GetAncestors<T>())
    {
      yield return ancestor;
    }
  }

  /// <summary>
  /// Detecta o script na árvore de parentesco.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <typeparam name="T">
  /// Tipo de nodo a ser verificado.
  /// </typeparam>
  /// <returns>
  /// O nodo script ou null.
  /// </returns>
  public static IEnumerable<T> GetAncestorsAndSelf<T>(this INode node)
    where T : INode
  {
    if (node is T t1) yield return t1;
    if (node.Parent == null) yield break;
    if (node.Parent is T t2) yield return t2;
    foreach (var ancestor in node.Parent.GetAncestors<T>())
    {
      yield return ancestor;
    }
  }

  /// <summary>
  /// Enumera os descendentes do nodo.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser enumerado.
  /// </typeparam>
  public static IEnumerable<INode> GetDescendants(this INode node)
  {
    foreach (var child in node.GetChildren())
    {
      yield return child;
      foreach (var descendant in child.GetDescendants())
      {
        yield return descendant;
      }
    }
  }

  /// <summary>
  /// Enumera os descendentes do nodo.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser enumerado.
  /// </typeparam>
  public static IEnumerable<INode> GetDescendantsAndSelf(this INode node)
  {
    yield return node;
    foreach (var descendant in node.GetDescendants())
    {
      yield return descendant;
    }
  }

  /// <summary>
  /// Enumera os descendentes do nodo.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser enumerado.
  /// </typeparam>
  public static IEnumerable<T> GetDescendants<T>(this INode node)
    where T : INode
  {
    foreach (var child in node.GetChildren())
    {
      if (child is T t) yield return t;
      foreach (var descendant in child.GetDescendants<T>())
      {
        yield return descendant;
      }
    }
  }

  /// <summary>
  /// Enumera os descendentes do nodo.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser enumerado.
  /// </typeparam>
  public static IEnumerable<T> GetDescendantsAndSelf<T>(this INode node)
    where T : INode
  {
    if (node is T t1) yield return t1;
    foreach (var descendant in node.GetDescendants<T>())
    {
      yield return descendant;
    }
  }
}
