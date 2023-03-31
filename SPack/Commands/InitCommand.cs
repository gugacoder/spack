using System.Text;
using ScriptPack.Model;
using SPack.Commands.Helpers;
using SPack.Helpers;
using SPack.Prompting;

namespace SPack.Commands;

/// <summary>
/// Comando de execução de scripts de migração de bases de dados.
/// </summary>
public class InitCommand : ICommand
{
  /// <summary>
  /// Executa o comando de migração de dados.
  /// </summary>
  public async Task RunAsync(CommandLineOptions options)
  {
    // O comando `init` realiza a migração da base com os scripts internos do
    // ScriptPack.
    // Por isso, vamos manipular as opções para que o comando `migrate` seja
    // executado com os scripts internos.
    options.Migrate.On = true;
    options.BuiltIn.On = true;

    var migrate = new MigrateCommand();
    await migrate.RunAsync(options);
  }

  /// <summary>
  /// Imprime um relatório de erros ocorridos nos pipelines e seus scripts.
  /// </summary>
  /// <param name="databaseMigrator">
  /// Objeto DatabaseMigrator para registro dos eventos.
  /// </param>
  /// <param name="verbose">
  /// Indica se a execução deve ser verbosa ou não.
  /// </param>
  private void RegisterListeners(DatabaseMigrator databaseMigrator, bool verbose)
  {
    if (verbose)
    {
      databaseMigrator.OnPipelineStart += (sender, args) =>
          Console.WriteLine($"[PIPELINE] {args.Phase.Name}");
      databaseMigrator.OnStageStart += (sender, args) =>
          Console.WriteLine($"[STAGE] {args.Phase.Name}");
      databaseMigrator.OnConnection += (sender, args) =>
          Console.WriteLine($"[DATEBASE] {args.Connection.Database}");
      databaseMigrator.OnConnectionMessage += (sender, args) =>
          Console.WriteLine($"{args.Message}");
      databaseMigrator.OnStepStart += (sender, args) =>
          Console.WriteLine($"[STEP] {args.Phase.Name}");
      databaseMigrator.OnError += (sender, args) =>
      {
        var cause = args.Exception;
        Console.Error.WriteLine($"[ERRO] {cause.Message}");
        while ((cause = cause.InnerException) != null)
        {
          Console.Error.WriteLine($"- {cause.Message}");
        }
        if (verbose)
        {
          Console.Error.WriteLine(args.Exception.StackTrace);
        }
        Console.Error.WriteLine();
      };
    }

    databaseMigrator.OnMigrate += (sender, args) =>
        Console.WriteLine(args.Script.Path);

    databaseMigrator.OnResultSet += (sender, args) =>
        ResultSetPrinter.PrintResultSet(args.Result);

  }
}
