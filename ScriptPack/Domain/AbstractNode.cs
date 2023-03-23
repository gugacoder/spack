using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Humanizer;
using ScriptPack.Helpers;

namespace ScriptPack.Domain;

/// <summary>
/// Classe abstrata que serve como implementação base para nodos em uma árvore
/// de nodos.
/// </summary>
public abstract class AbstractNode : INode
{
  private Dictionary<string, object> children = new();
  private string? _title;

  /// <summary>
  /// Construtor padrão que inicializa a lista de erros de compilação do
  /// script.
  /// </summary>
  public AbstractNode()
  {
    this.Faults = new();
  }

  /// <summary>
  /// Representa o pai do nodo.
  /// </summary>
  public virtual INode? Parent { get; set; }

  /// <summary>
  /// O nome do nodo.
  /// </summary>
  public virtual string Name { get; set; } = "";

  /// <summary>
  /// Obtém ou define o título do nodo do arquivo.
  /// </summary>
  public virtual string? Title
  {
    get => _title ?? Name.Titleize();
    set => _title = value;
  }

  /// <summary>
  /// A descrição do nodo.
  /// </summary>
  public virtual string? Description { get; set; }

  /// <summary>
  /// Indica se o conteúdo da seção está habilitado.
  /// Se estiver desabilitado, o conteúdo da seção não será executado.
  /// </summary>
  public virtual bool Enabled { get; set; } = true;

  /// <summary>
  /// O caminho virtual do nodo dentro da árvore de nodos.
  /// </summary>
  [JsonIgnore]
  public virtual string Path => VirtualPath.CreateNodePath(this);

  /// <summary>
  /// Lista de erros de compilação do script.
  /// </summary>
  [JsonIgnore]
  public virtual List<Fault> Faults { get; set; } = new();

  /// <summary>
  /// Retorna uma lista de todos os filhos do nodo, incluindo os filhos dos
  /// filhos.
  /// </summary>
  /// <returns>Uma coleção contendo todos os filhos do nodo.</returns>
  public virtual IEnumerable<INode> Children()
  {
    foreach (var item in children.Values.OfType<INode>()) yield return item;
    foreach (var list in children.Values.OfType<IEnumerable>())
    {
      foreach (var item in list.OfType<INode>()) yield return item;
    }
  }

  /// <summary>
  /// Aceita um visitante na árvore de nodos e repassa o visitante para seus
  /// filhos.
  /// </summary>
  /// <param name="visitor">O visitante a ser aceito.</param>
  public virtual void Accept(IVisitor visitor)
  {
    MethodInfo? method;

    method = typeof(IVisitor).GetMethod("Visit", new[] { GetType() });
    method?.Invoke(visitor, new object[] { this });

    foreach (var child in Children())
    {
      method = child.GetType().GetMethod("Accept",
          new[] { typeof(IVisitor) });
      method?.Invoke(child, new object[] { visitor });
    }
  }

  /// <summary>
  /// Aceita um visitante na árvore de nodos e repassa o visitante para seus
  ///  filhos, de forma assíncrona.
  /// </summary>
  /// <param name="visitor">O visitante a ser aceito.</param>
  public virtual async Task AcceptAsync(IAsyncVisitor visitor)
  {
    MethodInfo? method;
    Task? task;

    method = typeof(IAsyncVisitor).GetMethod("VisitAsync",
        new[] { GetType() });
    task = method?.Invoke(visitor, new object[] { this }) as Task;
    if (task != null) await task;

    foreach (var child in Children())
    {
      method = child.GetType().GetMethod("AcceptAsync",
          new[] { typeof(IAsyncVisitor) });
      task = method?.Invoke(child, new object[] { visitor }) as Task;
      if (task != null) await task;
    }
  }

  public override string ToString()
  {
    return $"{Path} ({base.ToString()})";
  }

  #region Implementação de automação da composição de árvore de nodos

  /// <summary>
  /// Obtém o valor de uma propriedade do nodo com o nome especificado.
  /// </summary>
  /// <typeparam name="T">O tipo de valor da propriedade.</typeparam>
  /// <param name="name">O nome da propriedade.</param>
  /// <returns>O valor da propriedade.</returns>
  protected virtual T Get<T>([CallerMemberName] string? name = null)
  {
    if (name is null) throw new ArgumentNullException(nameof(name));
    children.TryGetValue(name, out var value);
    return (T)value!;
  }

  /// <summary>
  /// Define o valor da propriedade com o nome especificado.
  /// </summary>
  /// <typeparam name="T">O tipo de valor da propriedade.</typeparam>
  /// <param name="value">O novo valor da propriedade.</param>
  /// <param name="name">O nome da propriedade.</param>
  protected virtual void Set<T>(T? value,
      [CallerMemberName] string? name = null)
  {
    if (name is null) throw new ArgumentNullException(nameof(name));

    // Libera o nodo definindo seu pai como nulo
    var current = Get<T>(name);
    current?.GetType().GetProperty("Parent")?.SetValue(current, null);

    if (value is null)
    {
      children.Remove(name);
      return;
    }

    children[name] = value;

    // Adota o nodo definido-se como o pai dele.
    var parentProperty = value.GetType().GetProperty("Parent")
        ?? throw new InvalidOperationException(
            $"A propriedade Parent não foi encontrada no tipo: "
                + (value.GetType().Name ?? "null"));

    parentProperty.SetValue(value, this);
  }

  #endregion
}