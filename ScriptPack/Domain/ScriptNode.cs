using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Representação de um script de comandos.
/// Em geral representa um script de banco de dados.
/// </summary>
public class ScriptNode : AbstractFileNode
{
  /// <summary>
  /// Obtém ou define a tag de categorização do script.
  /// A tag precede o nome do script segundo um dos padrões:
  /// <list type="bullet">
  /// <item>TAG ESQUEMA.OBJETO.sql</item>
  /// <item>TAG-ESQUEMA.OBJETO.sql</item>
  /// <item>TAG:ESQUEMA.OBJETO.sql</item>
  /// <item>TAG.ESQUEMA.OBJETO.sql</item>
  /// </list>
  /// </summary>
  /// <example>
  /// -pre dbo.sp_listar_usuarios.sql
  /// -pre-dbo.sp_listar_usuarios.sql
  /// -pre:dbo.sp_listar_usuarios.sql
  /// -pre.dbo.sp_listar_usuarios.sql
  /// </example>
  public string Tag { get; set; } = "";

  /// <summary>
  /// O caminho virtual do nodo dentro da árvore de nodos.
  /// </summary>
  [JsonIgnore]
  public override string Path
  {
    get
    {
      var fileName = System.IO.Path.GetFileName(FilePath);
      return Parent?.Path?.EndsWith("/") == true
       ? $"{Parent?.Path}{fileName}"
       : $"{Parent?.Path}/{fileName}";
    }
  }

  /// <summary>
  /// Scripts dos quais este script depende.
  /// </summary>
  [JsonIgnore]
  public List<ScriptNode> Dependencies { get; set; } = new();
}

