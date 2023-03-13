using System.Transactions;
using System.Text.RegularExpressions;
using ScriptPack.Domain;
using ScriptPack.Model;
using ScriptPack.Helpers;
using ScriptPack.FileSystem;

namespace ScriptPack.Algorithms;

/// <summary>
/// Ferramenta de extração das dependências de um script pela análise de seu
/// texto e de anotações especiais nos seus comentários.
/// </summary>
/// <remarks>
/// O algoritmo funciona detectando nomes nas instruções SQL que correspondem ao
/// padrão esquema.objeto, que correspondem a objetos presentes no catálogo e os
/// extrai como dependências.
/// 
/// Além disso, ele extrai as seguintes anotações especiais dos comentários:
/// 
/// <list type="bullet">
/// <item><description>
/// @requisito-para: indica que um objeto é necessário para algo.
/// </description></item>
/// <item><description>
/// @depende-de: indica que um objeto depende de outro objeto.
/// </description></item>
/// <item><description>
/// @depende-apenas-de: indica que um objeto depende apenas de outro
/// objeto e não de nenhum outro objeto.
/// </description></item>
/// </list>
/// 
/// Veja abaixo um exemplo de como essas anotações podem ser usadas:
/// 
/// <code>
///   @requisito-para: dbo.TBempresa, do.TBvenda
///   @depende-de: dbo.TBgrupo, dbo.TBproduto
///   @depende-apenas-de: venda.TBcupom, venda.TBitem_cupom
/// </code>
/// 
/// O sinal de dois-pontos (:) é opcional.
/// </remarks>
public class DependencyDetector
{
  /// <summary>
  /// Lê o script do fluxo de leitura e extrai do texto os objetos dos quais ele
  /// depende.
  /// </summary>
  /// <param name="targetScript">
  /// O objeto ScriptNode que representa o script SQL.
  /// </param>
  /// <param name="allAvailableScripts">
  /// Todos os scripts disponíveis para vinculação de dependências.
  /// </param>
  public async Task DetectDependenciesAsync(ScriptNode targetScript,
      IEnumerable<ScriptNode> allAvailableScripts)
  {
    var ignoreStatements = false;

    var dependenciesFromStatements = new List<ScriptNode>();
    var dependenciesFromAnnotations = new List<ScriptNode>();
    var dependents = new List<ScriptNode>();

    string? line;

    using var scriptStream = targetScript.OpenScriptFile();
    using var scriptReader = new StreamReader(scriptStream);

    while ((line = await scriptReader.ReadLineAsync()) != null)
    {
      var tokens = line.Split("--");
      var statements = tokens.First().Trim();
      var comments = string.Join("--", tokens.Skip(1)).Trim();

      if (comments.Contains("@depende-apenas-de"))
      {
        ignoreStatements = true;
        dependenciesFromStatements.Clear();
      }

      if (comments.Contains("@"))
      {
        CollectAnnotations(comments,
            dependenciesFromAnnotations, dependents, allAvailableScripts);
      }
      if (comments.Contains(".") && !ignoreStatements)
      {
        CollectStatements(statements,
            dependenciesFromStatements, allAvailableScripts);
      }
    }

    targetScript.Dependencies = targetScript.Dependencies
        .Concat(dependenciesFromStatements)
        .Concat(dependenciesFromAnnotations)
        .Distinct()
        .Except(new[] { targetScript })
        .ToList();

    dependents.ForEach(d => d.Dependencies.Add(targetScript));
  }

  /// <summary>
  /// Detecta as dependências de um script a partir de anotações especiais nos
  /// seus comentários.
  /// </summary>
  /// <remarks>
  /// As anotações especiais que podem ser utilizadas são:
  /// <list type="bullet">
  /// <item><description>
  /// @requisito-para: indica que um objeto é necessário para algo.
  /// </description></item>
  /// <item><description>
  /// @depende-de: indica que um objeto depende de outro objeto.
  /// </description></item>
  /// <item><description>
  /// @depende-apenas-de: indica que um objeto depende apenas de outro objeto
  /// e não de nenhum outro objeto.
  /// /// </description></item>
  /// </list>
  ///
  /// As dependências encontradas são armazenadas nas listas de ScriptNode
  /// passadas como parâmetro.
  /// </remarks>
  /// <param name="text">O texto do script.</param>
  /// <param name="dependencies">Lista de dependências encontradas.</param>
  /// <param name="dependents">Lista de dependentes encontrados.</param>
  private void CollectAnnotations(string text,
      List<ScriptNode> dependencies,
      List<ScriptNode> dependents,
      IEnumerable<ScriptNode> allAvailableScripts)
  {
    var dependencyPattern = new Regex(@"@depende-(de|apenas-de):? (.+)");
    var dependetPattern = new Regex(@"@requisito-para:? (.+)");

    var dependencyMatch = dependencyPattern.Match(text);
    var dependetMatch = dependetPattern.Match(text);

    if (dependencyMatch.Success)
    {
      var candidates = dependencyMatch.Groups[2].Value
          .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
          .Select(word => word.Trim());

      var scripts = allAvailableScripts
          .Where(s => candidates.Any(d => s.Name.Like(d)))
          .Except(dependencies);
      dependencies.AddRange(scripts);
    }

    if (dependetMatch.Success)
    {
      var candidates = dependetMatch.Groups[2].Value
          .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
          .Select(word => word.Trim());

      var scripts = allAvailableScripts
          .Where(s => candidates.Any(d => s.Name.Like(d)))
          .Except(dependents);
      dependents.AddRange(scripts);
    }
  }

  /// <summary>
  /// Detecta as dependências de um script a partir de anotações especiais nos
  /// comentários.
  /// </summary>
  /// <param name="text">O texto do script a ser analisado.</param>
  /// <param name="dependencies">
  //  Lista de dependências a ser preenchida.</param>
  /// <param name="dependents">Lista de dependentes a ser preenchida.</param>
  private void CollectStatements(string text,
      List<ScriptNode> dependencies,
      IEnumerable<ScriptNode> allAvailableScripts)
  {
    var delimiters = " ,;()[]{}\t\r\n".ToArray();
    var candidates = text
        .Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
        .Where(word => word.Contains('.'))
        .SelectMany(word => CollectObjectNameCandidates(word))
        .ToList();

    var scripts = allAvailableScripts
        .Where(s => candidates.Any(d => s.Name.Like(d)))
        .Except(dependencies);
    dependencies.AddRange(scripts);
  }

  /// <summary>
  /// Coleta todas as palavras de uma palavra longa separada por pontos
  /// Exemplo: banco.esquema.tabela.campo deve retornar banco.esquema,
  /// `esquema.tabela` e `tabela.campo`.
  /// </summary>
  /// <param name="text">
  /// A palavra longa da qual extrair as palavras.
  /// Exemplo: banco.esquema.tabela.campo
  /// </param>
  /// <returns>
  /// Uma lista de palavras extraídas da palavra longa.
  /// Exemplo: banco.esquema, esquema.tabela e tabela.campo
  /// </returns>
  private IEnumerable<string> CollectObjectNameCandidates(string text)
  {
    var tokens = text.Split('.');
    for (var i = 0; i < tokens.Length - 1; i++)
    {
      var objectName = tokens[i] + "." + tokens[i + 1];
      yield return objectName;
    }
  }
}
