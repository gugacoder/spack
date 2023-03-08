namespace SPack.Library;

public static class LinqExtensions
{
  public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
  {
    foreach (var item in source)
    {
      action(item);
    }
    return source;
  }
}
