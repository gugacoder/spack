using SPack.Commands;

await RunAsync(args);

/// <summary>
/// Executa um comando com base nos argumentos passados na linha de comando.
/// </summary>
/// <param name="args">Os argumentos passados para o programa.</param>
async Task RunAsync(string[] args)
{
  try
  {
    var options = new
    {
      help = new Switch(Long: true, 'h'),
      encode = new Option(),
      open = new Option(),
      list = new Option(),
      migrate = new Switch(),
      scripts = new OptionList(Long: true, 's'),
      connections = new OptionList(Long: true, 'c')
    };

    ParseArgs(args, options);

    var catalog = options.open.On ? options.open.Value : "";

    ICommand command = options switch
    {
      { help: { On: true } }
          => new HelpCommand(),

      { encode: { On: true } }
          => new EncodeCommand { Token = options.encode.Value },

      { list: { On: true } }
          => new ListCommand
          {
            Catalog = catalog,
            SearchPattern = options.list.Value
          },

      { migrate: { On: true } }
          => new MigrateCommand
          {
            Catalog = options.open.Value,
            Scripts = options.scripts.Items,
            Connections = options.connections.Items
          },

      _ => throw new ArgumentException(
          "USO INCORRETO! Nenhuma ação indicada. " +
          "Use --help para mais detalhes.")
    };

    await command.RunAsync();

  }
  catch (Exception ex)
  {
    Console.Error.WriteLine(ex.Message);
  }
}

/// <summary>
/// Analisa os argumentos passados para o programa e os mapeia para as ações,
/// opções e parâmetros correspondentes.
/// </summary>
/// <param name="args">Os argumentos passados para o programa.</param>
/// <param name="options">
/// Um objeto com as opções, ações e argumentos esperados.
/// </param>
/// <remarks>
/// Os argumentos podem ser passados de duas formas:
/// 1. Com a opção completa, precedida de dois hífens, como "--help".
/// 2. Com a opção abreviada, precedida de um hífen, como "-h".
/// </remarks>
/// <exception cref="Exception">
/// Lançada quando um argumento desconhecido é passado.
/// </exception>
void ParseArgs(string[] args, object options)
{
  if (args.Length == 0)
    throw new ArgumentException("USO INCORRETO! Nenhum argumento informado.");

  for (var i = 0; i < args.Length; i++)
  {
    var arg = args[i];
    try
    {

      //
      // Determinando a instância do argumento declarado.
      //

      IArgument? argument = null;

      if (arg.StartsWith("--"))
      {
        var value = options
            .GetType()
            .GetProperty(arg.TrimStart('-'))?
            .GetValue(options)
                as IArgument;
        if (value?.Long == true) argument = value;
      }
      else if (arg.StartsWith("-"))
      {
        argument = options
            .GetType()
            .GetProperties()
            .Select(x => x.GetValue(options))
            .OfType<IArgument>()
            .SingleOrDefault(x => x.Short == arg[1]);
      }
      else
      {
        var value = options
            .GetType()
            .GetProperty(arg)?
            .GetValue(options)
                as IArgument;
        if (value?.Long == false) argument = value;
      }

      if (argument == null)
        throw new ArgumentException($"Argumento desconhecido: {arg}");

      //
      // Ativando o uso da opção.
      //

      argument.On = true;

      if (argument is Switch)
        continue;

      //
      // A opção existe um valor. Verificando se o valor foi informado.
      //

      if (args.Length <= i + 1)
        throw new IndexOutOfRangeException(
            $"USO INCORRETO! Valor do argumento não informado: {arg}");

      var argValue = args[++i];

      //
      // Realizando o parsing do argumento
      //

      if (argument is Option opt)
      {
        opt.GetType().GetProperty("Value")?.SetValue(opt, argValue);
        continue;
      }

      if (argument is OptionList optList)
      {
        var items = argValue
            .Split(',')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        optList.GetType().GetProperty("Items")?.SetValue(optList, items);
        continue;
      }
    }
    catch (ArgumentException) { throw; }
    catch (IndexOutOfRangeException) { throw; }
    catch (Exception ex)
    {
      throw new ArgumentException(
          $"USO INCORRETO! Valor do argumento não informado: {arg}", ex);
    }
  }
}


interface IArgument
{
  bool Long { get; set; }
  char? Short { get; set; }
  bool On { get; set; }
}

record Switch(
    bool Long = false, char? Short = null, bool On = false
    ) : IArgument
{
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public bool On { get; set; } = On;
}


record Option(
    bool Long = false, char? Short = null, bool On = false,
    string Value = ""
    ) : IArgument
{
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public bool On { get; set; } = On;
};

record OptionList(
    bool Long = false, char? Short = null, bool On = false,
    List<string> Items = null!
    ) : IArgument
{
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public bool On { get; set; } = On;
};
