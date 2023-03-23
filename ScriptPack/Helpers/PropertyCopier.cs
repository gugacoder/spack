namespace ScriptPack.Helpers;

/// <summary>
/// Classe que contém o método para copiar as propriedades de um objeto para
/// outro.
/// </summary>
public static class PropertyCopier
{
  /// <summary>
  /// Copia todas as propriedades do objeto "a" para o objeto "b".
  /// </summary>
  /// <param name="sourceObject">O objeto a ser copiado.</param>
  /// <param name="targetObject">
  /// O objeto no qual as propriedades serão copiadas.
  /// </param>
  public static void CopyProperties(object sourceObject, object targetObject)
  {
    var sourceProperties = sourceObject.GetType().GetProperties();
    foreach (var sourceProperty in sourceProperties)
    {
      var targetProperty = targetObject.GetType().GetProperty(
          sourceProperty.Name);

      if (targetProperty is null)
        continue;

      object? value = sourceProperty.GetValue(sourceObject);
      targetProperty.SetValue(targetObject, value);
    }
  }

  /// <summary>
  /// Cria uma nova instância do tipo "T" e copia as propriedades do objeto de
  /// origem para a nova instância.
  /// </summary>
  /// <typeparam name="T">O tipo da nova instância.</typeparam>
  /// <param name="sourceObject">O objeto de origem a ser copiado.</param>
  /// <returns>A nova instância com as propriedades copiadas.</returns>
  public static T CopyToNewInstance<T>(object sourceObject)
      where T : new()
  {
    T target = new T();
    CopyProperties(sourceObject, target);
    return target;
  }
}
