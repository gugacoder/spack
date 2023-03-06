using System.Text.Json;

namespace SPack.Library;

public static class Json
{
  public static readonly JsonSerializerOptions SPackOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };
}
