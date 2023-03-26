using System.Runtime.CompilerServices;

namespace SPack.Prompting.Domain;

/// <summary>
/// Argumento de linha de comando com valor definido.
/// </summary>
public class Option : IArgument
{
  /// <summary>
  /// Inicializa uma nova instância da classe <see cref="Option"/>.
  /// </summary>
  /// <param name="name">
  /// Nome do argumento em PascalCase.
  /// Se não for informado, será obtido automaticamente a partir do nome da
  /// propriedade declarante.
  /// </param>
  public Option([CallerMemberName] string name = "")
  {
    this.Design = null!;
    this.Name = name;
  }

  /// <summary>
  /// Obtém o atributo de design do argumento. 
  /// </summary>
  public ArgumentAttribute Design { get; set; }

  /// <summary>
  /// Nome do argumento em PascalCase.
  /// </summary>
  public string Name { get; set; }

  /// <summary>
  /// Determina se o argumento foi indicado na linha de comando ou não.
  /// </summary>
  public bool On { get; set; } = false;

  /// <summary>
  /// Valor indicado na linha de comando.
  /// </summary>
  public string Value { get; set; } = string.Empty;
}
