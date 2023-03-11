using System.Text.Json;

namespace ScriptPack.Helpers;

/// <summary>
/// Fornecer opções de serialização/desserialização de objetos JSON com a
/// biblioteca padrão System.Text.Json.
/// </summary>
public static class JsonOptions
{
  /// <summary>
  /// Opções de serialização/desserialização de objetos JSON com formatação
  /// CamelCase e com diferenciação de maiúsculas e minúsculas desativada.
  /// </summary>
  public static readonly JsonSerializerOptions CamelCase = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
  };
}