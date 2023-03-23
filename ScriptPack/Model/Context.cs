namespace ScriptPack.Model;

/// <summary>
/// Contexto de migração de scripts.
/// O contexto contém argumentos repassados para os scripts, parâmetros de
/// conexão com o banco de dados e informações sobre o ambiente de execução.
/// </summary>
public class Context
{
  /// <summary>
  /// Argumentos repassados para os scripts.
  /// Os argumentos podem ser capturados em scripts pelo uso das funções
  /// SQLs especiais `scriptpack.*_arg`, construídas automaticamente na base
  /// pelo próprio executor do ScriptPack.
  /// 
  /// Algumas das funções SQLs especiais são:
  /// - `scriptpack.INT_ARG`: Captura um argumento inteiro.
  /// - `scriptpack.STR_ARG`: Captura um argumento string.
  /// - `scriptpack.DATE_ARG`: Captura um argumento data.
  /// 
  /// A função recebe como primeiro parâmetro o nome do argumento e como
  /// segundo parâmetro o valor padrão do argumento.
  /// 
  /// Exemplo:
  /// 
  /// ```sql
  /// SELECT scriptpack.INT_ARG('arg1', 0) AS arg1,
  ///      , scriptpack.STR_ARG('arg2', 'default') AS arg2,
  ///      , scriptpack.DATE_ARG('arg3', '2021-01-01') AS arg3
  /// ```
  /// </summary>
  public Dictionary<string, string> Arguments { get; } = new();

  /// <summary>
  /// Strings de conexão com as bases de dados migradas pelo ScriptPack.
  /// A chave corresponde ao nome de uma conexão definida em um nodo do tipo
  /// <cref name="ScriptPack.Model.ConnectionNode" /> e o valor corresponde à
  /// string de conexão.
  /// </summary>
  public Dictionary<string, string> ConnectionStrings { get; } = new();
}
