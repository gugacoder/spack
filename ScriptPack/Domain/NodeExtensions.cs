using ScriptPack.Model;

namespace ScriptPack.Domain;

/// <summary>
/// Classe que contém métodos de extensão para a interface <see cref="INode"/>.
/// </summary>
public static class NodeExtesions
{
  /// <summary>
  /// Obtém o nodo raiz da árvore de parentesco.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// O nodo raiz.
  /// </returns>
  public static INode Root(this INode node)
  {
    if (node.Parent is null) return node;
    return node.Parent.Root();
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
  public static T? Ancestor<T>(this INode node)
    where T : INode
  {
    return node.Ancestors<T>().FirstOrDefault();
  }

  /// <summary>
  /// Obtém uma sequência de nodos ancestrais do nodo atual.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// Uma sequência de nodos ancestrais.
  /// </returns>
  public static IEnumerable<INode> Ancestors(this INode node)
  {
    if (node.Parent is null) yield break;
    yield return node.Parent;
    foreach (var ancestor in node.Parent.Ancestors())
    {
      yield return ancestor;
    }
  }

  /// <summary>
  /// Obtém uma sequência de nodos ancestrais, incluindo o próprio nodo atual.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// Uma sequência de nodos ancestrais, incluindo o próprio nodo atual.
  /// </returns>
  public static IEnumerable<INode> AncestorsAndSelf(this INode node)
  {
    yield return node;
    if (node.Parent is null) yield break;
    yield return node.Parent;
    foreach (var ancestor in node.Parent.Ancestors())
    {
      yield return ancestor;
    }
  }

  /// <summary>
  /// Obtém uma sequência de nodos ancestrais do tipo especificado.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser verificado.
  /// </typeparam>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// Uma sequência de nodos ancestrais do tipo especificado.
  /// </returns>
  public static IEnumerable<T> Ancestors<T>(this INode node)
    where T : INode
  {
    if (node.Parent is null) yield break;
    if (node.Parent is T t) yield return t;
    foreach (var ancestor in node.Parent.Ancestors<T>())
    {
      yield return ancestor;
    }
  }

  /// <summary>
  /// Obtém uma sequência de nodos ancestrais do tipo especificado, incluindo o
  /// próprio nodo atual.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser verificado.
  /// </typeparam>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// Uma sequência de nodos ancestrais do tipo especificado, incluindo o
  /// próprio nodo atual.
  /// </returns>
  public static IEnumerable<T> AncestorsAndSelf<T>(this INode node)
    where T : INode
  {
    if (node is T t1) yield return t1;
    if (node.Parent is null) yield break;
    if (node.Parent is T t2) yield return t2;
    foreach (var ancestor in node.Parent.Ancestors<T>())
    {
      yield return ancestor;
    }
  }

  /// <summary>
  /// Obtém uma sequência de nodos descendentes do nodo atual.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// Uma sequência de nodos descendentes.
  /// </returns>
  public static IEnumerable<INode> Descendants(this INode node)
  {
    foreach (var child in node.Children())
    {
      yield return child;
      foreach (var descendant in child.Descendants())
      {
        yield return descendant;
      }
    }
  }

  /// <summary>
  /// Obtém uma sequência de nodos descendentes do nodo atual, incluindo o
  /// próprio nodo atual.
  /// </summary>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// Uma sequência de nodos descendentes, incluindo o próprio nodo atual.
  /// </returns>
  public static IEnumerable<INode> DescendantsAndSelf(this INode node)
  {
    yield return node;
    foreach (var descendant in node.Descendants())
    {
      yield return descendant;
    }
  }

  /// <summary>
  /// Obtém uma sequência de nodos descendentes do tipo especificado.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser verificado.
  /// </typeparam>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// Uma sequência de nodos descendentes do tipo especificado.
  /// </returns>
  public static IEnumerable<T> Descendants<T>(this INode node)
      where T : INode
  {
    foreach (var child in node.Children())
    {
      if (child is T t) yield return t;
      foreach (var descendant in child.Descendants<T>())
      {
        yield return descendant;
      }
    }
  }

  /// <summary>
  /// Obtém uma sequência de nodos descendentes do tipo especificado, incluindo
  /// o próprio nodo atual.
  /// </summary>
  /// <typeparam name="T">
  /// Tipo de nodo a ser verificado.
  /// </typeparam>
  /// <param name="node">
  /// Nodo a ser verificado.
  /// </param>
  /// <returns>
  /// Uma sequência de nodos descendentes do tipo especificado, incluindo o
  /// próprio nodo atual.
  /// </returns>
  public static IEnumerable<T> DescendantsAndSelf<T>(this INode node)
      where T : INode
  {
    if (node is T t1) yield return t1;
    foreach (var descendant in node.Descendants<T>())
    {
      yield return descendant;
    }
  }
}
