using System.Text.Json;
using ScriptPack.Algorithms;
using ScriptPack.Domain;
using ScriptPack.Helpers;
using ScriptPack.Model;
// using ScriptPack.Model.Algorithms;


var loader = new CatalogLoader();
var catalogs = await loader.ReadCatalogAsync(new FileDrive("Sandbox"));

var repository = new RepositoryNode();
repository.Catalogs.AddRange(catalogs);

var navigator = new NodeNavigator(repository);

navigator.ChangeInto("/SandboxApp");

var list = navigator.List("/SandboxApp/Sandbox/1.0.0-trunk/Sandbox/api");
foreach (var item in list)
{
  Console.WriteLine(item.Path);
}



// Switch help = new();
// Option open = new();
// Option list = new();
// Switch migrate = new();
// Option encode = new();
// Option decode = new();
// OptionList scripts = new();
// OptionList connections = new();

// if (args.Length == 0)
// {
//   Console.Error.WriteLine("USO INCORRETO! Nenhum argumento informado.");
//   return;
// }

// for (var i = 0; i < args.Length; i++)
// {
//   var arg = args[i];
//   try
//   {
//     if (arg == "help" || arg == "-h" || arg == "-?" || arg == "/?") { help = new Switch(true); continue; }
//     if (arg == "open") { open = new Option(true, args[++i]); continue; }
//     if (arg == "list") { list = new Option(true, args[++i]); continue; }
//     if (arg == "migrate") { migrate = new Switch(true); continue; }
//     if (arg == "encode") { encode = new Option(true, args[++i]); continue; }
//     if (arg == "decode") { decode = new Option(true, args[++i]); continue; }
//     if (arg == "--script" || arg == "-s") { (scripts = scripts.On ? scripts : new OptionList(true, new())).Values.Add(args[++i]); continue; }
//     if (arg == "--connection" || arg == "-d") { (connections = connections.On ? connections : new OptionList(true, new())).Values.Add(args[++i]); continue; }
//     else
//     {
//       Console.Error.WriteLine($"USO INCORRETO! Argumento desconhecido: {arg}");
//       return;
//     }
//   }
//   catch (IndexOutOfRangeException)
//   {
//     Console.Error.WriteLine($"USO INCORRETO! Valor do argumento não informado: {arg}");
//     return;
//   }
//   catch (Exception ex)
//   {
//     Console.Error.WriteLine($"Falha processando o argumento: {arg}");
//     Console.Error.WriteLine("Causa:");
//     Console.Error.WriteLine(ex.GetStackMessage());
//     return;
//   }
// }

// if (help.On)
// {
//   var assembly = System.Reflection.Assembly.GetExecutingAssembly();
//   using var stream = assembly.GetManifestResourceStream("ScriptPack.Assets.HELP.info");
//   if (stream == null)
//   {
//     Console.Error.WriteLine("FALHA! O arquivo de ajuda LEIAME.md não foi distribuído corretamente com a aplicação.");
//     return;
//   }
//   using var reader = new StreamReader(stream);
//   var content = reader.ReadToEnd();
//   Console.WriteLine(content);
//   return;
// }

// if (encode.On)
// {
//   var text = encode.Value!;
//   var encoded = Crypto.Encrypt(text);
//   Console.WriteLine(encoded);
//   return;
// }

// if (decode.On)
// {
//   var text = decode.Value!;
//   var decoded = Crypto.Decrypt(text);
//   Console.WriteLine(decoded);
//   return;
// }

// var source = open.On ? open.Value! : ".";
// var drive = Drive.Get(source);
// var repositoryBuilder = new RepositoryBuilder();
// repositoryBuilder.AddDrive(drive);
// repositoryBuilder.AddDependencyDetector();
// var repository = await repositoryBuilder.BuildRepositoryAsync();

// if (list.On)
// {
//   var objectName = list.Value!;
//   if (objectName == "directory")
//   {
//     var nodes = repository.Descendants<IFileNode>();
//     foreach (var node in nodes)
//     {
//       Console.WriteLine(node.Path);
//     }
//     return;
//   }
//   if (objectName == "connection")
//   {
//     var descendants = repository.Descendants<ConnectionNode>();
//     foreach (var connection in descendants)
//     {
//       var binding = connection.BoundTo == null ? "" : $", vinculado à base {connection.BoundTo}";
//       var suggestion = string.IsNullOrEmpty(connection.DefaultDatabaseName) ? "" : $", nome sugerido: {connection.DefaultDatabaseName}";
//       var description = string.IsNullOrEmpty(connection.Description) ? "" : $" - {connection.Description}";
//       Console.WriteLine($"{connection.Name} ({connection.Provider}{binding}{suggestion}){description}");
//     }
//     return;
//   }
//   if (objectName == "catalog")
//   {
//     var descendants = repository.Descendants<CatalogNode>();
//     foreach (var catalog in descendants)
//     {
//       var description = string.IsNullOrEmpty(catalog.Description) ? "" : $" - {catalog.Description}";
//       Console.WriteLine($"{catalog.Name} ({catalog.FilePath}){description}");
//     }
//     return;
//   }
//   if (objectName == "product")
//   {
//     var descendants = repository.Descendants<ProductNode>();
//     foreach (var product in descendants)
//     {
//       var description = string.IsNullOrEmpty(product.Description) ? "" : $" - {product.Description}";
//       Console.WriteLine($"{product.Path}{description}");
//     }
//     return;
//   }
//   if (objectName == "module")
//   {
//     var descendants = repository.Descendants<ModuleNode>();
//     foreach (var module in descendants)
//     {
//       var description = string.IsNullOrEmpty(module.Description) ? "" : $" - {module.Description}";
//       Console.WriteLine($"{module.Path}{description}");
//     }
//     return;
//   }
//   if (objectName == "package")
//   {
//     var descendants = repository.Descendants<PackageNode>();
//     foreach (var package in descendants)
//     {
//       var pckageConnections = string.Join(", ", package.Connections);
//       var description = string.IsNullOrEmpty(package.Description) ? "" : $" - {package.Description}";
//       Console.WriteLine($"{package.Path} ({pckageConnections}){description}");
//     }
//     return;
//   }
//   if (objectName == "script")
//   {
//     var descendants = repository.Descendants<ScriptNode>();
//     foreach (var script in descendants)
//     {
//       var tag = string.IsNullOrEmpty(script.Tag) ? "" : $" ({script.Tag})";
//       var description = string.IsNullOrEmpty(script.Description) ? "" : $" - {script.Description}";
//       Console.WriteLine($"{script.Path}{tag}{description}");
//     }
//     return;
//   }
//   Console.Error.WriteLine($"USO INCORRETO! Objeto desconhecido: {objectName}");
//   return;
// }

// if (migrate.On)
// {
//   if (!scripts.On || scripts.Values?.Any() != true)
//   {
//     Console.Error.WriteLine("USO INCORRETO! Nenhum script foi selecionado para migração.");
//     return;
//   }

//   var pipelineBuilder = new PipelineBuilder();
//   var connectionInstances = repository.Descendants<ConnectionNode>().ToArray();

//   foreach (var connection in connections.Values)
//   {
//     // A string de conexão informada no argumento de linha de comando tem a
//     // forma "connection:connectionString", por isso, é necessário separar
//     // o nome do banco de dados da string de conexão propriamente dita.

//     var tokens = connection.Split(":");
//     var connectionName = tokens[0].Trim();
//     var connectionString = string.Join(":", tokens.Skip(1)).Trim();

//     if (string.IsNullOrEmpty(connectionName) || string.IsNullOrEmpty(connectionString))
//     {
//       Console.Error.WriteLine($"USO INCORRETO! Conexão inválida: {connectionString}");
//       return;
//     }

//     // Atualizando a fábrica de conexão da base de dados com a string de conexão obtida.
//     var connectionInstance = connectionInstances.FirstOrDefault(d => d.Name == connectionName);
//     if (connectionInstance == null)
//     {
//       Console.Error.WriteLine($"USO INCORRETO! Base de dados desconhecida: {connectionName}");
//       return;
//     }

//     connectionInstance.ConnectionStringFactory = new ConnectionStringFactoryNode { ConnectionString = connectionString };
//   }

//   var nodeLocator = new NodeLocator(repository);
//   foreach (var script in scripts.Values)
//   {
//     var node = nodeLocator.LocateNode(script);
//     pipelineBuilder.AddScripts(node);
//   }

//   var pipelines = pipelineBuilder.BuildPipelines();

//   var migrantBuilder = new MigrantBuilder();
//   migrantBuilder.AddDrive(drive);
//   migrantBuilder.AddConnection(connectionInstances.ToArray());
//   migrantBuilder.AddPipeline(pipelines.ToArray());

//   var migrants = migrantBuilder.BuildMigrant();

//   foreach (var migrant in migrants)
//   {
//     migrant.OnMigrate += (sender, e) =>
//     {
//       Console.WriteLine($"Migrando {e.Script.Path} para {e.Pipeline.Connection.Name}...");
//     };
//     migrant.OnMigrateError += (sender, e) =>
//     {
//       Console.Error.WriteLine($"ERRO! {e.Script?.Path} não pode ser migrado para {e.Pipeline.Connection.Name}.");
//       Console.Error.WriteLine($"CAUSA: {e.Exception.Message}");
//       Console.Error.WriteLine(e.Exception.StackTrace);
//     };
//     migrant.OnMigrateSuccess += (sender, e) =>
//     {
//       Console.WriteLine($"SUCESSO! {e.Script.Path} foi migrado para {e.Pipeline.Connection.Name}.");
//     };

//     await migrant.MigrateAsync();
//   }
// }

// record Switch(bool On = false);
// record Option(bool On = false, string? Value = null);
// record OptionList(bool On = false, List<string> Values = null!);
