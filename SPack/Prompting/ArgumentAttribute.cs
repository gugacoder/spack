namespace SPack.Prompting;

/// <summary>
/// Representa um atributo usado para definir opções de linha de comando
/// seguindo o padrão POSIX.
/// </summary>
/// <remarks>
/// Use este atributo em propriedades para especificar como elas devem ser
/// mapeadas para opções de linha de comando.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
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
}