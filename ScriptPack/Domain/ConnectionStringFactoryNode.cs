using System.Text.Json.Serialization;

namespace ScriptPack.Domain;

/// <summary>
/// Classe responsável por definir uma string de conexão de uma base de dados,
/// seja diretamente ou por definição de consulta em uma base de dados
/// subsequente.
/// </summary>
public class ConnectionStringFactoryNode : AbstractNode
{
  public ConnectionStringFactoryNode()
  {
  }

  /// <summary>
  /// Inicializa uma nova instância da classe ConnectionStringFactoryNode.
  /// </summary>
  /// <param name="connectionString">
  /// A string de conexão com a base de dados.
  /// </param>
  public ConnectionStringFactoryNode(string connectionString)
  {
    this.ConnectionString = connectionString;
  }

  /// <summary>
  /// A string de conexão com a base de dados.
  /// </summary>
  /// <remarks>
  /// Se esta propriedade estiver preenchida, as propriedades
  /// <see cref="Connection"/> e <see cref="Query"/> serão ignoradas.
  /// </remarks>
  public string ConnectionString { get; set; } = "";

  /// <summary>
  /// O nome da configuração da base de dados a ser usada na consulta.
  /// </summary>
  /// <remarks>
  /// Esta propriedade é utilizada em conjunto com a propriedade
  /// <see cref="Query"/>.
  /// </remarks>
  public string Connection { get; set; } = "";

  /// <summary>
  /// A consulta SQL a ser usada para produzir a string de conexão.
  /// </summary>
  /// <remarks>
  /// Esta propriedade é utilizada em conjunto com a propriedade
  /// <see cref="Connection"/>.
  /// </remarks>
  public string Query { get; set; } = "";
}
