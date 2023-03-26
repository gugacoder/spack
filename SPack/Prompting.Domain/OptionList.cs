using System.Runtime.CompilerServices;

namespace SPack.Prompting.Domain;

/// <summary>
/// Argumento de linha de comando com múltiplos valores definidos.
/// </summary>
public class OptionList : IArgument
{
  /// <summary>
  /// Inicializa uma nova instância da classe <see cref="OptionList"/>.
  /// </summary>
  /// <param name="name">
  /// Nome do argumento em PascalCase.
  /// Se não for informado, será obtido automaticamente a partir do nome da
  /// propriedade declarante.
  /// </param>
  public OptionList([CallerMemberName] string name = "")
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
  /// Valores coletados da linha de comando.
  /// </summary>
  public List<string> Items { get; set; } = new();
}
