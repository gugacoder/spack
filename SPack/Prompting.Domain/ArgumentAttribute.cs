using Humanizer;

namespace SPack.Prompting.Domain;

/// <summary>
/// Representa um atributo usado para definir opções de linha de comando
/// seguindo o padrão POSIX.
/// </summary>
/// <remarks>
/// Use este atributo em propriedades para especificar como elas devem ser
/// mapeadas para opções de linha de comando.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ArgumentAttribute : Attribute
{
  /// <summary>
  /// Inicializa uma nova instância da classe <see cref="ArgumentAttribute"/>.
  /// </summary>
  public ArgumentAttribute()
  {
  }

  /// <summary>
  /// Inicializa uma nova instância da classe <see cref="ArgumentAttribute"/>.
  /// </summary>
  /// <param name="long">
  /// Um valor indicando se a opção de linha de comando deve ser tratada como
  /// uma opção longa (por exemplo, --opcao).
  /// </param>
  public ArgumentAttribute(bool @long)
  {
    this.Long = @long;
  }

  /// <summary>
  /// Inicializa uma nova instância da classe <see cref="ArgumentAttribute"/>.
  /// </summary>
  /// <param name="long">
  /// Um valor indicando se a opção de linha de comando deve ser tratada como
  /// uma opção longa (por exemplo, --opcao).
  /// </param>
  /// <param name="short">
  /// O caractere usado como uma opção curta (por exemplo, -o).
  /// </param>
  public ArgumentAttribute(bool @long, char @short)
  {
    this.Long = @long;
    this.Short = @short;
  }

  /// <summary>
  /// Obtém ou define um valor indicando se a opção de linha de comando deve ser
  /// tratada como uma opção longa (por exemplo, --opcao).
  /// </summary>
  public bool Long { get; set; }

  /// <summary>
  /// Obtém ou define o caractere usado como uma opção curta (por exemplo, -o).
  /// </summary>
  /// <value>
  /// O caractere representando a opção curta ou null se não houver opção curta.
  /// </value>
  public char? Short { get; set; }

  /// <summary>
  /// Valor padrão do argumento.
  /// </summary>
  public string? DefaultValue { get; set; }

  /// <summary>
  /// Verifica se a opção fornecida corresponde à definição.
  /// </summary>
  /// <param name="targetOption">
  /// A opção que está sendo verificada.
  /// </param>
  /// <param name="valueToCheck">
  /// O valor obtido da linha de comando que está sendo verificado conta a
  /// definição da opção.
  /// </param>
  /// <returns>
  /// Retorna verdadeiro se a opção fornecida corresponder à definição, caso
  /// contrário, retorna falso.
  /// </returns>
  public bool IsMatch(IArgument targetOption, string valueToCheck)
  {
    if (valueToCheck.StartsWith("--"))
    {
      return valueToCheck[2..] == targetOption.Name.Kebaberize();
    }

    if (valueToCheck.StartsWith("-"))
    {
      return valueToCheck[1..] == Short?.ToString();
    }

    return valueToCheck == targetOption.Name.Kebaberize();
  }
}