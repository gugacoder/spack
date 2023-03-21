using SPack.Commands;

await RunAsync(args);

/// <summary>
/// Executa um comando com base nos argumentos passados na linha de comando.
/// </summary>
/// <param name="args">Os argumentos passados para o programa.</param>
async Task RunAsync(string[] args)
{
  var options = new
  {
    help = new Switch(Long: true, 'h'),
    list = new Option(DefaultValue: "**"),
    show = new Option(),
    migrate = new Switch(),
    pipeline = new Switch(),
    validate = new Switch(),
    encode = new Option(),
    catalog = new Option(Long: true, 'c'),
    package = new OptionList(Long: true, 'p'),
    script = new OptionList(Long: true, 's'),
    database = new OptionList(Long: true, 'd'),
    verbose = new Switch(Long: true, 'v')
  };

  try
  {
    ParseArgs(args, options);

    var catalog = options.catalog.On ? options.catalog.Value : null;

    ICommand command = options switch
    {
      { help: { On: true } }
          => new HelpCommand(),

      { encode: { On: true } }
          => new EncodeCommand { Token = options.encode.Value },

      { list: { On: true } }
          => new ListCommand
          {
            CatalogPath = catalog,
            SearchPattern = options.list.Value
          },

      { show: { On: true } }
          => new ShowCommand
          {
            CatalogPath = catalog,
            SearchPattern = options.show.Value,
            ConnectionMaps = options.database.Items
          },

      { validate: { On: true } }
          => new ValidateCommand
          {
            CatalogPath = catalog,
            SearchPackageCriteria = options.package.Items,
            SearchScriptCriteria = options.script.Items
          },

      { pipeline: { On: true } }
          => new PipelineCommand
          {
            CatalogPath = catalog,
            SearchPackageCriteria = options.package.Items,
            SearchScriptCriteria = options.script.Items
          },

      { migrate: { On: true } }
          => new MigrateCommand
          {
            CatalogPath = catalog,
            SearchPackageCriteria = options.package.Items,
            SearchScriptCriteria = options.script.Items,
            ConnectionMaps = options.database.Items
          },

      _ => throw new ArgumentException(
          "USO INCORRETO! Nenhuma ação indicada. " +
          "Use --help para mais detalhes.")
    };

    command.Verbose = options.verbose.On;

    await command.RunAsync();

  }
  catch (Exception ex)
  {
    Environment.ExitCode = 1;
    Console.Error.WriteLine(ex.Message);
    if (options.verbose.On)
    {
      Exception? cause = ex;
      do
      {
        Console.Error.WriteLine("---");
        Console.Error.WriteLine(cause.StackTrace);
        cause = cause.InnerException;
      } while (cause != null);
    }
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
      var argValue = (args.Length > (i + 1) && !args[i + 1].StartsWith("-"))
          ? args[++i]
          : argument.DefaultValue;

      if (argValue == null)
        throw new IndexOutOfRangeException(
            $"USO INCORRETO! Valor do argumento não informado: {arg}");

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
        optList.Items.AddRange(items);
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
  string? DefaultValue { get; set; }
}

record Switch(
    bool Long = false, char? Short = null, bool On = false
    ) : IArgument
{
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public bool On { get; set; } = On;
  string? IArgument.DefaultValue { get; set; }
}


record Option(
    bool Long = false, char? Short = null, bool On = false,
    string Value = "", string? DefaultValue = null
    ) : IArgument
{
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public bool On { get; set; } = On;
  public string? DefaultValue { get; set; } = DefaultValue;
};

record OptionList(
    bool Long = false, char? Short = null, bool On = false,
    List<string> Items = null!, string? DefaultValue = null
    ) : IArgument
{
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public bool On { get; set; } = On;
  public string? DefaultValue { get; set; } = DefaultValue;
  public List<string> Items { get; set; } = Items ?? new List<string>();
};
