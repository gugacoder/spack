using Humanizer;

namespace SPack.Prompting;

/// <summary>
/// Definição de um argumento de linha de comando.
/// </summary>
public class ArgumentDesign
{
  /// <summary>
  /// Nome do argumento.
  /// Para argumentos longos tem a forma '--nome'.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Obtém ou define o caractere usado como uma opção curta (por exemplo, -o).
  /// </summary>
  /// <value>
  /// O caractere representando a opção curta ou null se não houver opção curta.
  /// </value>
  public char? Short { get; set; }

  /// <summary>
  /// Instância da opção representada pelo argumento
  /// </summary>
  public IArgument Option { get; set; } = null!;

  /// <summary>
  /// Verifica se a opção fornecida corresponde à definição.
  /// </summary>
  /// <param name="option">A opção da linha de comando como uma string.</param>
  /// <returns>
  /// Retorna verdadeiro se a opção fornecida corresponder à definição, caso
  /// contrário, retorna falso.
  /// </returns>
  public bool IsMatch(string option)
  {
    var isMatch = (option.StartsWith("-") && !option.StartsWith("--"))
        ? option[1..] == this.Short?.ToString()
        : option == Name;
    return isMatch;
  }

  /// <summary>
  /// Mapeia as propriedades de um objeto para uma lista de argumentos.
  /// </summary>
  /// <param name="options">O objeto que contém as propriedades.</param>
  /// <returns>
  /// Retorna uma lista de argumentos que representa as propriedades do objeto.
  /// </returns>
  public static ArgumentDesign[] ExtractArgumentDesigns(
      CommandLineOptions options)
  {
    var type = options.GetType();
    var properties = type.GetProperties();
    var args = new List<ArgumentDesign>();
    foreach (var property in properties)
    {
      var arg = CreateArgumentDesign(options, property.Name);
      args.Add(arg);
    }
    return args.ToArray();
  }

  /// <summary>
  /// Cria uma instância de <see cref="ArgumentAttribute"/> a partir de uma
  /// propriedade.
  /// </summary>
  /// <param name="type">O tipo que contém a propriedade.</param>
  /// <param name="propertyName">O nome da propriedade.</param>
  /// <returns>
  /// Retorna uma instância de <see cref="ArgumentAttribute"/> que representa a
  /// propriedade.
  /// </returns>
  private static ArgumentDesign CreateArgumentDesign(CommandLineOptions options,
      string propertyName)
  {
    var type = options.GetType();
    var property = type.GetProperty(propertyName)
        ?? throw new ArgumentException(
            $"A propriedade não existe: {propertyName}");

    var arg = property
        .GetCustomAttributes(typeof(ArgumentAttribute), false)
        .FirstOrDefault() as ArgumentAttribute
        ?? throw new ArgumentException(
            $"A propriedade não está mapeada com o atributo [Argument]: " +
            $"{propertyName}");

    var option = property.GetValue(options) as IArgument
        ?? throw new ArgumentException(
            $"A propriedade não foi inicializada: {propertyName}");

    var prefix = arg.Long ? "--" : "";

    var instance = new ArgumentDesign
    {
      Name = $"{prefix}{propertyName.Kebaberize()}",
      Short = arg.Short,
      Option = option
    };
    return instance;
  }
}
