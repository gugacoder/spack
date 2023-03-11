// using System.Text.RegularExpressions;
// using ScriptPack.Domain;

// namespace ScriptPack.Library;

// /// <summary>
// /// Quebra um script SQL em blocos.
// /// </summary>
// public class SqlBatcher
// {

//   /// <summary>
//   /// Quebra o script em blocos.
//   ///
//   /// O script pode conter várias seções separadas por "GO".
//   /// O GO pode ocorrer sozinho em uma linha, pode ser seguido de um número
//   /// que indica o número de reptições do bloco anterior, ou pode ser seguido
//   /// de um comentário inciado com "--" ou "/*".
//   ///
//   /// Exemplos:
//   ///   GO
//   ///   GO 3
//   ///   GO -- comentário
//   ///   GO /* comentário
//   ///   GO 3 -- comentário
//   ///   GO 3 /* comentário
//   ///
//   /// </summary>
//   /// <param name="script">
//   /// O conteúdo SQL do script a ser quebrado.
//   /// </param>
//   /// <returns>
//   /// Os blocos obtidos do script.
//   /// </returns>
//   public Batch[] BreakInBatches(string script)
//   {
//     var batches = new List<Batch>();

//     var lines = script.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
//     foreach (string line in lines)
//     {
//       var match = Regex.Match(line, @"^\s*GO\s*(\d+)?\s*(--|/*)?", RegexOptions.IgnoreCase);
//       if (match.Success)
//       {
//         int repetition = 0;
//         int.TryParse(match.Groups[1].Value, out repetition);
//         int index = batches.Count;
//         batches.Add(new() { Index = index, Repetition = repetition });
//         continue;
//       }

//       if (batches.Count == 0)
//       {
//         batches.Add(new() { Index = 0, Buffer = new(line), Repetition = 0 });
//         continue;
//       }

//       batches[batches.Count - 1].Buffer.AppendLine(line);
//     }

//     batches.ForEach(b => b.FlatBuffer());

//     return batches.ToArray();
//   }
// }
