using System.Text.Json;
using SPack.Domain;
using SPack.Library;
using SPack.Model;

Option help = new();
Option encode = new();
Option decode = new();
Option migrate = new();

for (var i = 0; i < args.Length; i++)
{
  var arg = args[i];
  try
  {
    if (arg == "help" || arg == "-h" || arg == "-?" || arg == "/?") help = new Option(true);
    if (arg == "encode") encode = new Option(true, args[++i]);
    if (arg == "decode") decode = new Option(true, args[++i]);
    if (arg == "migrate") migrate = new Option(true, args[++i]);
    else
    {
      Console.Error.WriteLine($"Argumento desconhecido: {arg}");
      return;
    }
  }
  catch (IndexOutOfRangeException)
  {
    Console.Error.WriteLine($"Valor do argumento não informado: {arg}");
    return;
  }
  catch (Exception ex)
  {
    Console.Error.WriteLine($"Falha processando o argumento: {arg}");
    Console.Error.WriteLine("Causa:");
    Console.Error.WriteLine(ex.GetStackMessage());
    return;
  }
}

if (encode.On)
{
  var text = encode.Value!;
  var encoded = Crypto.Encrypt(text);
  Console.WriteLine(encoded);
}

if (decode.On)
{
  var text = decode.Value!;
  var decoded = Crypto.Decrypt(text);
  Console.WriteLine(decoded);
}

// if (migrate.On)
{
  var drive = Drive.Get("SampleProject");
  var repositoryBuilder = new RepositoryBuilder();
  repositoryBuilder.AddDrive(drive);
  repositoryBuilder.AddDependencyDetector();
  repositoryBuilder.AddCyclicDependencyDetector();
  var repository = await repositoryBuilder.BuildRepositoryAsync();

  var pipelineBuilder = new PipelineBuilder();
  pipelineBuilder.AddScripts(repository);

  var pipelines = pipelineBuilder.BuildPipeline();
  foreach (var pipeline in pipelines)
  {
    Console.WriteLine($"{pipeline}");
    var stages = pipeline.GetDescendants<Stage>();
    foreach (var stage in stages)
    {
      Console.WriteLine($". {stage}");
      var steps = stage.GetDescendants<Step>();
      foreach (var step in steps)
      {
        Console.WriteLine($". . {step}");
        var scripts = step.GetDescendants<Script>();
        foreach (var script in scripts)
        {
          Console.WriteLine($". . . {script}");
        }
      }
    }
  }

  // var migrator = new Migrator(drive);
  // migrator.Migrate(catalog, migrate.Value);
}

if (help.On)
{
  Console.WriteLine("NAME");
  Console.WriteLine("    spack - utility for migrating database using the ScriptPack algorithm.");
  Console.WriteLine("");
  Console.WriteLine("SYNOPSIS");
  Console.WriteLine("    spack [OPTION]... [TEXT]");
  Console.WriteLine("");
  Console.WriteLine("DESCRIPTION");
  Console.WriteLine("    The spack utility is used to migrate database using the ScriptPack algorithm.");
  Console.WriteLine("");
  Console.WriteLine("OPTIONS");
  Console.WriteLine("    -h, -?, --help    display this help and exit");
  Console.WriteLine("    --encode TEXT     encrypt the given TEXT using the Crypto module");
  Console.WriteLine("    --decode TEXT     decrypt the given TEXT using the Crypto module");
  Console.WriteLine("    --migrate PATH    migrate the catalog located at the given PATH using the Migrator module");
  Console.WriteLine("");
  Console.WriteLine("EXAMPLES");
  Console.WriteLine("    spack --encode 'secret message'");
  Console.WriteLine("    spack --decode 'encrypted message'");
  Console.WriteLine("    spack --migrate /path/to/catalog.json");
  Console.WriteLine("");
}

record Option(bool On = false, string? Value = null);