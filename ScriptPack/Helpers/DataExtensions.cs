using System.Data.Common;

namespace ScriptPack.Helpers;

/// <summary>
/// Extensões para objetos de dados.
/// </summary>
public static class DataExtensions
{
  /// <summary>
  /// Adiciona um parâmetro com o nome e valor especificados ao comando.
  /// </summary>
  /// <param name="cmd">Comando ao qual o parâmetro será adicionado.</param>
  /// <param name="parameterName">Nome do parâmetro.</param>
  /// <param name="value">Valor do parâmetro.</param>
  /// <returns>O parâmetro adicionado.</returns>
  public static DbParameter AddParameterWithValue(this DbCommand cmd,
      string parameterName, object value)
  {
    var parameter = cmd.CreateParameter();
    parameter.ParameterName = parameterName;
    parameter.Value = value ?? DBNull.Value;
    cmd.Parameters.Add(parameter);
    return parameter;
  }

  /// <summary>
  /// Executa o comando e retorna o valor da primeira coluna da primeira linha
  /// do resultado convertido para o tipo especificado.
  /// </summary>
  /// <typeparam name="T">Tipo para o qual o valor será convertido.</typeparam>
  /// <param name="command">Comando a ser executado.</param>
  /// <returns>
  /// O valor da primeira coluna da primeira linha do resultado convertido para
  /// o tipo especificado.
  /// </returns>
  public static async Task<T?> ExecuteScalarAsync<T>(this DbCommand command)
  {
    var result = await command.ExecuteScalarAsync();
    if (result is null || result == DBNull.Value)
    {
      return default;
    }
    return (T)Convert.ChangeType(result, typeof(T));
  }
}
