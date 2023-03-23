namespace SPack.Prompting;

public record Switch(bool Long = false, char? Short = null, bool On = false)
    : IArgument
{
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public bool On { get; set; } = On;
  string? IArgument.DefaultValue { get; set; }
}
