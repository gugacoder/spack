using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SPack.Domain;

public class NodeList<T> : ObservableCollection<T>
  where T : INode
{
  public NodeList()
  {
    CollectionChanged += OnCollectionChanged!;
  }

  public NodeList(INode parent)
  {
    this.Parent = parent;
    CollectionChanged += OnCollectionChanged!;
  }

  public INode? Parent { get; set; }

  public void ForEach(Action<T> action)
  {
    foreach (var item in this) action(item);
  }

  public void AddRange(IEnumerable<T> items)
  {
    foreach (var item in items) this.Add(item);
  }

  private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.OldItems != null) foreach (var item in e.OldItems.OfType<T>()) item.Parent = null;
    if (e.NewItems != null) foreach (var item in e.NewItems.OfType<T>()) item.Parent = Parent;
  }
}