using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ScriptPack.Helpers;

namespace ScriptPack.Domain;

/// <summary>
/// Representa uma lista de nodo com um nodo pai opcional.
/// </summary>
/// <typeparam name="T">O tipo dos nodo na lista. Deve implementar a interface
/// INode.</typeparam>
public class NodeList<T> : ObservableCollection<T> where T : INode
{
  private INode? parent;

  /// <summary>
  /// Cria uma nova instância da classe NodeList.
  /// </summary>
  public NodeList()
  {
    CollectionChanged += OnCollectionChanged!;
  }

  /// <summary>
  /// Obtém ou define o nodo pai da lista.
  /// </summary>
  public INode? Parent
  {
    get => parent;
    set { parent = value; this.ForEach(item => SetParent(item, value)); }
  }

  /// <summary>
  /// Adota um nodo na lista.
  /// O método permite uma generalização da interface INode mas requer que a
  /// instância seja do tipo correto definido pelo tipo <see cref="T"/>.
  /// </summary>
  public void AddNode(INode node)
  {
    this.Add((T)node);
  }

  /// <summary>
  /// Adiciona uma coleção de itens à lista.
  /// </summary>
  /// <param name="items">A coleção de itens a serem adicionados.</param>
  public void AddRange(IEnumerable<T> items)
  {
    foreach (var item in items) this.Add(item);
  }

  /// <summary>
  /// Chamado quando a coleção é alterada.
  /// </summary>
  /// <param name="sender">O objeto que gerou o evento.</param>
  /// <param name="e">Os argumentos do evento.</param>
  private void OnCollectionChanged(object sender,
    NotifyCollectionChangedEventArgs e)
  {
    // Define o nodo pai como nulo para todos os itens removidos da lista.
    e.OldItems?.OfType<T>().ForEach(item => SetParent(item, null));
    // Define o nodo pai para todos todos os itens adicionados na lista.
    e.NewItems?.OfType<T>().ForEach(item => SetParent(item, Parent));
  }

  /// <summary>
  /// Define o nodo pai de um item da lista.
  /// </summary>
  /// <param name="item">O item a ser atualizado.</param>
  /// <param name="parent">O nodo pai a ser definido. Pode ser nulo.</param>
  private static void SetParent(T item, INode? parent)
  {
    // Define o nodo pai dinamicamente.
    item?.GetType().GetProperty("Parent")?.SetValue(item, parent);
  }
}
