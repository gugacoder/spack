namespace SPack.Prompting.Domain;

/// <summary>
/// Interface de argumento de linha de comando.
/// </summary>
public interface IArgument
{
  /// <summary>
  /// Obtém o atributo de design do argumento. 
  /// </summary>
  ArgumentAttribute Design { get; set; }

  /// <summary>
  /// Nome do argumento em PascalCase.
  /// </summary>
  string Name { get; set; }

  /// <summary>
  /// Determina se o argumento foi indicado na linha de comando ou não.
  /// </summary>
  bool On { get; set; }
}
