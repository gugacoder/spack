using System.Text.RegularExpressions;
using SPack.Domain;
using SPack.Library;

namespace SPack.Model.Algorithms;

public class AsyncDependencyDetector : IAsyncVisitor
{
  private readonly IDrive drive;

  public AsyncDependencyDetector(IDrive drive)
  {
    this.drive = drive;
  }

  /// <summary>
  /// Visita um Script e detecta os scripts dos quais ele dependen dentro do
  /// mesmo produto.
  /// </summary>
  /// <param name="script">
  /// O Script a ser visitado.
  /// </param>
  public async Task VisitAsync(Script script)
  {
    try
    {
      var scripts = script
        .GetProduct()?
        .GetDescendants<Script>()
        .Where(s => s.Tag.Equals(script.Tag));

      if (scripts?.Any() != true) return;

      var content = await drive.ReadAllTextAsync(script.FilePath!);
      var lines = content.Split('\n', '\r');

      var statements = lines
        .Select(line => line.Split("--").First());

      var comments = lines
        .Select(line => string.Join("--", line.Split("--").Skip(1)))
        .Where(line => !string.IsNullOrWhiteSpace(line));

      var requiredBy = comments
        .Where(line => line.Contains("@requisito-para"))
        .SelectMany(line => ParseAnnotation(line));

      var dependantOf = comments
        .Where(line => line.Contains("@depende-de"))
        .SelectMany(line => ParseAnnotation(line))
        .Concat(comments
            .Where(line => line.Contains("@depende-apenas-de"))
            .SelectMany(line => ParseAnnotation(line))
        )
        .SelectMany(line => line.Split(',').Select(word => word.Trim()));

      var ignoreBody = comments
        .Any(line => line.Contains("@depende-apenas-de"));

      if (requiredBy.Any())
      {
        // add this script as dependency of the scripts whose name is equals to the word
        var dependencies = scripts
          .Except(new[] { script })
          .Where(s => requiredBy.Contains(s.Name))
          .ToList();

        dependencies.ForEach(d => d.Dependencies.Add(script));
      }

      if (dependantOf.Any())
      {
        var dependencies = scripts
          .Except(new[] { script })
          .Where(s => dependantOf.Contains(s.Name))
          .ToList();

        script.Dependencies.AddRange(dependencies);
      }

      if (!ignoreBody)
      {
        var words = statements
          .SelectMany(line => line.Split(' ', ',', ';', '(', ')', '[', ']', '{', '}', '\t', '\r', '\n'))
          .Where(token => token.Contains('.'))
          .SelectMany(longWord => CollectWords(longWord))
          .ToList();

        // select the scripts whose name is equals to the word
        var dependencies = scripts
          .Except(new[] { script })
          .Where(s => words.Contains(s.Name))
          .ToList();

        script.Dependencies.AddRange(dependencies);
      }

    }
    catch (Exception ex)
    {
      script.Faults.Add(Fault.FromException(ex));
    }
  }

  /// <summary>
  /// Coleta todas as palavras de uma palavra longa separada por pontos
  /// Exemplo: banco.esquema.tabela.campo deve retornar banco.esquema, esquema.tabela e tabela.campo
  /// </summary>
  /// <param name="longWord">
  /// A palavra longa da qual extrair as palavras.
  /// Exemplo: banco.esquema.tabela.campo
  /// </param>
  /// <returns>
  /// Uma lista de palavras extraídas da palavra longa.
  /// Exemplo: banco.esquema, esquema.tabela e tabela.campo
  /// </returns>
  private IEnumerable<string> CollectWords(string longWord)
  {
    var tokens = longWord.Split('.');
    for (var i = 0; i < tokens.Length - 1; i++)
    {
      var word = tokens[i] + "." + tokens[i + 1];
      yield return word;
    }
  }

  public string[] ParseAnnotation(string line)
  {
    //  @requisito-para: esquema.tabela, esquema.outra_tabela
    //  @depende-de esquema.tabela, esquema.outra_tabela
    //  @depende-apenas-de:  esquema.tabela, esquema.outra_tabela

    var match = Regex.Match(line, @"@[\w-]+[\s:]+(.+)");
    if (!match.Success) return Array.Empty<string>();

    var content = match.Groups[1].Value;
    var values = content.Split(',').Select(v => v.Trim()).ToArray();

    return values;
  }
}
