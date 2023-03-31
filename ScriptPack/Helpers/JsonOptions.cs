using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ScriptPack.Helpers;

/// <summary>
/// Fornecer opções de serialização/desserialização de objetos JSON com a
/// biblioteca padrão Newtonsoft.Json.
/// </summary>
public static class JsonOptions
{
  /// <summary>
  /// Opções de serialização/desserialização de objetos JSON com formatação
  /// CamelCase e com diferenciação de maiúsculas e minúsculas desativada.
  /// </summary>
  public static readonly JsonSerializerSettings CamelCase = new()
  {
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    MissingMemberHandling = MissingMemberHandling.Ignore
  };

  /// <summary>
  /// Opções de serialização/desserialização de objetos JSON com formatação
  /// CamelCase, diferenciação de maiúsculas e minúsculas desativada e com
  /// indentação.
  /// </summary>
  public static readonly JsonSerializerSettings IndentedCamelCase = new()
  {
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    Formatting = Formatting.Indented,
    NullValueHandling = NullValueHandling.Ignore
  };
}