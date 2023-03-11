using System.Text.RegularExpressions;
using ScriptPack.Domain;

namespace ScriptPack.Algorithms;

/// <summary>
/// Representa um objeto usado para extrair blocos de um script SQL.
/// </summary>
public class BatchExtractor
{
  /// <summary>
  /// Regex para identificar o comando "GO" seguido de um número opcional.
  /// </summary>
  private static readonly Regex GoRegex = new(@"^\s*GO\s*(\d+)?\s*(--|/*)?",
      RegexOptions.IgnoreCase);

  /// <summary>
  /// Extrai blocos de comandos SQL de um script com base na separação lógica do
  /// comando "GO". Aplicável apenas para scripts do SQLServer. Para os demais
  /// SGDBs, retorna um único bloco com o script completo.
  /// </summary>
  /// <param name="scriptReader">
  /// Um objeto <c>StreamReader</c> para leitura do script SQL.
  /// </param>
  /// <returns>
  /// Um array de objetos <c>Batch</c> representando os blocos extraídos.
  /// </returns>
  public async Task<Batch[]> ExtactBatchesAsync(StreamReader scriptReader)
  {
    var batches = new List<Batch>();

    string? line;
    while ((line = await scriptReader.ReadLineAsync()) != null)
    {
      // Checa se a linha contém o comando "GO" seguido de um número opcional.
      var match = GoRegex.Match(line);
      if (match.Success)
      {
        // Obtém o número de repetições do bloco (se houver).
        // O número de repetições é um número que segue o comando "GO" e
        // indica quantas vezes o bloco deve ser executado.
        // Por exemplo, o comando "GO 3" indica que o bloco deve ser executado
        // três vezes.
        int repetition = 0;
        int.TryParse(match.Groups[1].Value, out repetition);

        int index = batches.Count;
        batches.Add(new() { Index = index, Repetition = repetition });
        continue;
      }

      // Se ainda não houver blocos na lista, cria um novo bloco.
      if (batches.Count == 0)
      {
        batches.Add(new() { Index = 0, Buffer = new(line), Repetition = 0 });
        continue;
      }

      // Adiciona a linha atual ao bloco mais recente da lista.
      batches[batches.Count - 1].Buffer.AppendLine(line);
    }

    // Concatena o conteúdo de cada bloco em uma única string.
    // Como o buffer contém as partes do bloco de script, a cada invocação de
    // ToString() o conteúdo do buffer é concatenado em uma única string.
    // O método FlatBuffer() faz essa concatenação e substitui o conteúdo do
    // buffer por ela, dessa forma, invocações subsequentes ao método
    // ToString() retornam o conteúdo do buffer já concatenado. 
    batches.ForEach(b => b.FlatBuffer());

    return batches.ToArray();
  }
}
