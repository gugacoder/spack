namespace SPack.Prompting;

public record OptionList(
    bool Long = false, char? Short = null, bool On = false,
    List<string> Items = null!, string? DefaultValue = null
    ) : IArgument
{
  public bool Long { get; set; } = Long;
  public char? Short { get; set; } = Short;
  public bool On { get; set; } = On;
  public string? DefaultValue { get; set; } = DefaultValue;
  public List<string> Items { get; set; } = Items ?? new List<string>();
};
