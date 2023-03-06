using System.Collections;
using System.Text.Json.Serialization;

namespace SPack.Domain;

/// <summary>
/// Representação de um script de comandos.
/// Em geral representa um script de banco de dados.
/// </summary>
public class Script : IFileNode
{
  public Script()
  {
    this.Faults = new(this);
  }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  public Package? Parent { get; set; }

  /// <summary>
  /// Nodo pai.
  /// </summary>
  [JsonIgnore]
  INode? INode.Parent { get => Parent; set => Parent = (Package?)value; }

  /// <summary>
  /// Nome do script.
  /// </summary>
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Tag de categorização do script.
  /// A tag precede o nome do script segundo um dos padrões:
  /// -TAG ESQUEMA.OBJETO.sql
  /// -TAG-ESQUEMA.OBJETO.sql
  /// -TAG:ESQUEMA.OBJETO.sql
  /// -TAG.ESQUEMA.OBJETO.sql
  /// </summary>
  /// <example>
  /// -pre dbo.sp_listar_usuarios.sql
  /// -pre-dbo.sp_listar_usuarios.sql
  /// -pre:dbo.sp_listar_usuarios.sql
  /// -pre.dbo.sp_listar_usuarios.sql
  /// </example>
  public string Tag { get; set; } = string.Empty;

  /// <summary>
  /// Caminho virtual do nodo dentro da árvore de nodos.
  /// </summary>
  public string Path => $"{Parent?.Path}/{Name}";

  /// <summary>
  /// Nome do nó.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// Caminho relativo do arquivo referente.
  /// </summary>
  [JsonIgnore]
  public string? FilePath { get; set; } = string.Empty;

  /// <summary>
  /// Indica se a seção está habilitada.
  /// Se estiver desabilitada, o conteúdo da seção não será executado.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// Scripts dos quais este script depende.
  /// </summary>
  [JsonIgnore]
  public List<Script> Dependencies { get; set; } = new();

  /// <summary>
  /// Falhas ocorridas durante a execução do script.
  /// </summary>
  [JsonIgnore]
  public NodeList<Fault> Faults { get; set; }

  public IEnumerable<INode> GetChildren()
  {
    foreach (var item in Faults) yield return item;
  }

  public void Accept(IVisitor visitor)
  {
    visitor.Visit(this);
    Faults.ForEach(item => item.Accept(visitor));
  }

  public async Task AcceptAsync(IAsyncVisitor visitor)
  {
    await visitor.VisitAsync(this);
    await Task.WhenAll(Faults.Select(item => item.AcceptAsync(visitor)));
  }

  public override string ToString()
  {
    var name = $"{Tag} {Path}".Trim();
    return $"{base.ToString()} {name}".Trim();
  }
}

