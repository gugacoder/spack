namespace SPack.Prompting;

public interface IArgument
{
  bool On { get; set; }
  string? DefaultValue { get; set; }
}
