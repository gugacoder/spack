namespace SPack.Prompting;

public record Option(
    bool Long = false, char? Short = null, bool On = false,
    string Value = "", string? DefaultValue = null
    ) : IArgument
{
  public bool On { get; set; } = On;
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public string? DefaultValue { get; set; } = DefaultValue;
};
