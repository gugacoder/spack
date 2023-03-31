using System.Text;

namespace ScriptPack.FileSystem;

public static class Encodings
{
  /// <summary>
  /// Codificação ISO-8859-1.
  /// </summary>
  public static readonly Encoding Iso88591 = Encoding.GetEncoding("ISO-8859-1");

  /// <summary>
  /// Obtém o encoding indicado ou o encoding padrão caso seja indicado nulo.
  /// </summary>
  /// <param name="encoding">
  /// Nome do encoding a ser obtido.
  /// </param>
  /// <returns>
  /// Instância de Encoding correspondente ao nome especificado ou o encoding
  /// padrão caso seja indicado nulo.
  /// </returns>
  public static Encoding GetEncoding(string? encoding)
  {
    if (string.IsNullOrWhiteSpace(encoding)) return Iso88591;
    return Encoding.GetEncoding(encoding);
  }
}
