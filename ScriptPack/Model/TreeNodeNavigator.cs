using System.Text.RegularExpressions;
using DotNet.Globbing;
using ScriptPack.Domain;
using ScriptPack.Helpers;

namespace ScriptPack.Model;

/// <summary>
/// Utilitário para navegação entre nodos de uma estrutura hierárquica baseada
/// em <see cref="INode"/>.
/// </summary>
public class TreeNodeNavigator
{
  /// <summary>
  /// Cria um novo navegador de nodo de árvore com o nodo raiz especificado.
  /// </summary>
  /// <param name="rootNode">
  /// O nodo raiz a partir do qual a navegação deve começar.
  /// </param>
  public TreeNodeNavigator(INode rootNode)
  {
    this.RootNode = rootNode;
  }
  /// <summary>
  /// O nodo raiz da estrutura hierárquica a partir da qual é permitida a
  /// navegação.
  /// Os caminhos dos nodos são relativos a este nodo.
  /// </summary>
  public INode RootNode { get; set; }

  /// <summary>
  /// Obtém o caminho absoluto correspondente a um caminho relativo em relação
  /// ao nodo raiz.
  /// </summary>
  /// <param name="path">O caminho relativo.</param>
  /// <returns>O caminho absoluto correspondente.</returns>
  public string GetAbsolutePath(string path)
  {
    return AppendPrefix(path);
  }

  /// <summary>
  /// Retorna uma lista de caminhos de arquivos que correspondem ao padrão de
  /// busca especificado.
  /// </summary>
  /// <param name="searchPattern">
  /// O padrão de busca a ser aplicado para filtrar a lista de caminhos
  /// disponíveis.
  /// </param>
  /// <remarks>
  /// Esta função lista os caminhos que correspondem ao padrão de busca]
  /// especificado em <paramref name="searchPattern"/>.
  ///
  /// <para>
  /// O parâmetro <paramref name="path"/> segue a convenção de caminho do
  /// sistema de arquivos UNIX, onde:
  /// </para>
  /// <list type="bullet">
  /// <item><description>
  /// os diretórios são separados por <c>/</c>
  /// </description></item>
  /// <item><description>
  /// o caminho absoluto começa com <c>/</c>
  /// </description></item>
  /// </list>
  ///
  /// <para>
  /// O parâmetro <paramref name="searchPattern"/> segue a convenção de padrão
  /// de busca do sistema de arquivos UNIX, onde:
  /// </para>
  /// <list type="bullet">
  /// <item><description>
  /// o caractere <c></c> corresponde a qualquer conjunto de caracteres, exceto
  /// <c>/</c>
  /// </description></item>
  /// <item><description>
  /// os caracteres <c>**/</c> correspondem a qualquer número de diretórios
  /// aninhados
  /// </description></item>
  /// <item><description>
  /// o padrão pode incluir múltiplas extensões separadas por vírgula
  /// </description></item>
  /// </list>
  ///
  /// <para>
  /// Exemplos de uso do parâmetro <paramref name="searchPattern"/>:
  /// </para>
  /// <list type="bullet">
  /// <item><description><c>/caminho/para/dir</c></description></item>
  /// <item><description><c>/caminho/para/dir/</c></description></item>
  /// <item><description><c>/caminho/para/dir/.ext</c></description></item>
  /// <item><description>
  /// <c>/caminho/para/dir/.ext1,.ext2</c>
  /// </description></item>
  /// <item><description>
  /// <c>/**/qualquer/path/to/dir/.ext1,.ext2,.ext3</c>
  /// </description></item>
  /// <item><description>
  /// <c>/caminho/para/dir/**/qualquer/path/to/dir/.ext1,.ext2,*.ext3</c>
  /// </description></item>
  /// </list>
  ///
  /// <para>
  /// A função retornará uma lista de strings correspondentes aos caminhos que
  /// satisfazem o padrão de busca.
  /// </para>
  ///
  /// </remarks>
  /// <param name="searchPattern"></param>
  /// <returns>
  /// Uma matriz de strings contendo os caminhos dos arquivos correspondentes ao
  /// padrão de busca especificado.
  /// </returns>
  public string[] List(string searchPattern)
  {
    Glob glob = Glob.Parse(AppendPrefix(searchPattern));
    var paths = RootNode
        .DescendantsAndSelf()
        .Select(node => node.Path);
    var matchingPaths = paths
        .Where(glob.IsMatch)
        .Select(path => RemovePrefix(path))
        .ToArray();
    return matchingPaths;
  }

  /// <summary>
  /// Obtém o nodo que corresponde ao padrão de pesquisa especificado.
  /// </summary>
  /// <param name="searchPattern">O padrão de pesquisa a ser usado.</param>
  /// <returns>
  /// O nodo correspondente ao padrão de pesquisa ou null se nenhum nodo for
  /// encontrado.
  /// </returns>
  public INode? GetNode(string searchPattern)
  {
    Glob glob = Glob.Parse(AppendPrefix(searchPattern));
    var node = RootNode
        .DescendantsAndSelf()
        .SingleOrDefault(node => glob.IsMatch(node.Path));
    return node;
  }

  /// <summary>
  /// Obtém o nodo do tipo especificado que corresponde ao padrão de pesquisa
  /// especificado.
  /// </summary>
  /// <typeparam name="T">O tipo de nodo a ser procurado.</typeparam>
  /// <param name="searchPattern">O padrão de pesquisa a ser usado.</param>
  /// <returns>
  /// O nodo do tipo especificado correspondente ao padrão de pesquisa ou null
  /// se nenhum nodo for encontrado.
  /// </returns>
  public T? GetNode<T>(string searchPattern)
  where T : INode
  {
    Glob glob = Glob.Parse(AppendPrefix(searchPattern));
    var node = RootNode
        .DescendantsAndSelf<T>()
        .SingleOrDefault(node => glob.IsMatch(node.Path));
    return node;
  }

  /// <summary>
  /// Lista todos os nodos que correspondem ao padrão de pesquisa especificado.
  /// </summary>
  /// <param name="searchPattern">O padrão de pesquisa a ser usado.</param>
  /// <returns>
  /// Um array de todos os nodos correspondentes ao padrão de pesquisa.
  /// </returns>
  public INode[] ListNodes(string searchPattern)
  {
    Glob glob = Glob.Parse(AppendPrefix(searchPattern));
    var nodes = RootNode
        .DescendantsAndSelf()
        .Where(node => glob.IsMatch(node.Path));
    return nodes.ToArray();
  }

  /// <summary>
  /// Lista todos os nodos do tipo especificado que correspondem ao padrão de
  /// pesquisa especificado.
  /// </summary>
  /// <typeparam name="T">O tipo de nodo a ser procurado.</typeparam>
  /// <param name="searchPattern">O padrão de pesquisa a ser usado.</param>
  /// <returns>
  /// Um array de todos os nodos do tipo especificado correspondentes ao padrão
  /// de pesquisa.
  /// </returns>
  public T[] ListNodes<T>(string searchPattern)
  where T : INode
  {
    Glob glob = Glob.Parse(AppendPrefix(searchPattern));
    var nodes = RootNode
        .DescendantsAndSelf<T>()
        .Where(node => glob.IsMatch(node.Path));
    return nodes.ToArray();
  }

  /// <summary>
  /// Este método recebe uma string 'text' e retorna uma nova string que contém
  /// o texto com uma barra ('/') adicionada ao final, caso essa barra ainda não
  /// exista.
  /// </summary>
  /// <param name="text">A string de texto.</param>
  /// <returns>A nova string de texto com a barra adicionada.</returns>
  private string AppendPrefix(string text)
  {
    return text;
    // var prefix = Path.GetDirectoryName(RootNode.Path)![..^1];
    // var prefixedText = text.StartsWith("/")
    //     ? $"{prefix}{text[1..]}"
    //     : $"{prefix}{text}";
    // return prefixedText;
  }

  /// <summary>
  /// Este método recebe uma string 'text' e retorna uma nova string que contém
  /// o texto sem a barra ('/') no início, caso essa barra exista.
  /// </summary>
  /// <param name="prefixedText">A string de texto.</param>
  /// <returns>A nova string de texto sem a barra no início.</returns>
  private string RemovePrefix(string prefixedText)
  {
    return prefixedText;
    // var prefix = Path.GetDirectoryName(RootNode.Path)![..^1];
    // var text = prefixedText[(prefix.Length)..];
    // return text;
  }
}
