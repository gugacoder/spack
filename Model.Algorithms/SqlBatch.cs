using System.Text;

namespace SPack.Model.Algorithms;

/// <summary>
/// Um bloco de SQL.
/// </summary>
public class SqlBatch
{
  /// <summary>
  /// O índice do bloco no script iniciando em zero.
  /// </summary>
  public int Index { get; set; }

  /// <summary>
  /// O texto SQL do bloco.
  /// </summary>
  /// </value>
  public StringBuilder Buffer { get; set; } = new();

  /// <summary>
  /// O número de repetições do bloco inicinando em zero.
  /// Zero siginfica que o bloco não deve ser repetido, portanto, deve ser
  /// executado apenas uma vez.
  /// </summary>
  public int Repetition { get; set; }

  /// <summary>
  /// Concatena o conteúdo do buffer em um único texto e substitui o conteúdo
  /// atual do buffer pelo novo texto.
  /// </summary>
  public void FlatBuffer()
  {
    var text = this.Buffer.ToString();
    this.Buffer.Clear();
    this.Buffer.Append(text);
  }
}
