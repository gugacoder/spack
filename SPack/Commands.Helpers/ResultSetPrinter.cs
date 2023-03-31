using System.Data;
using System.Data.Common;

namespace SPack.Commands.Helpers;

/// <summary>
/// Utilitário de impressão de conjuntos de resultados.
/// </summary>
public static class ResultSetPrinter
{
  /// <summary>
  /// Método assíncrono para imprimir um conjunto de resultados de um objeto
  /// DbDataReader em uma tabela bem formatada.
  /// </summary>
  /// <param name="reader">
  /// O objeto DbDataReader contendo os resultados a serem impressos.
  /// </param>
  public static void PrintResultSet(DbDataReader reader)
  {
    // Cria um DataSet e carrega os dados do objeto DbDataReader nele
    var dataSet = new DataSet();
    while (!reader.IsClosed)
    {
      DataTable dataTable = new DataTable();
      dataTable.Load(reader);
      dataSet.Tables.Add(dataTable);
    }

    // Itera sobre cada tabela no DataSet
    foreach (DataTable table in dataSet.Tables)
    {
      if (table.Rows.Count == 0)
      {
        continue;
      }

      // Calcula as larguras de coluna baseadas no comprimento máximo dos dados
      // em cada coluna
      int[] columnWidths = CalculateColumnWidths(table);
      var totalWidth = columnWidths.Sum() + ((columnWidths.Count() - 1) * 2);

      // Imprime os cabeçalhos das colunas
      PrintHeaderRow(table, columnWidths);

      Console.WriteLine(new string('-', totalWidth));

      // Imprime as linhas de dados
      foreach (DataRow row in table.Rows)
      {
        PrintDataRow(row, table, columnWidths);
      }

      Console.WriteLine(new string('-', totalWidth));
      Console.WriteLine();
    }
  }

  /// <summary>
  /// Calcula a largura máxima de cada coluna em um DataTable com base nos dados
  /// na tabela.
  /// </summary>
  /// <param name="table">
  /// O DataTable para o qual os tamanhos das colunas devem ser calculados.
  /// </param>
  /// <returns>
  /// Uma matriz de inteiros representando a largura máxima de cada coluna no
  /// DataTable.
  /// </returns>
  private static int[] CalculateColumnWidths(DataTable table)
  {
    int[] columnWidths = new int[table.Columns.Count];
    for (int i = 0; i < table.Columns.Count; i++)
    {
      columnWidths[i] = table.Columns[i].ColumnName.Length;
    }
    foreach (DataRow row in table.Rows)
    {
      for (int i = 0; i < table.Columns.Count; i++)
      {
        string value = FormatValue(row[i], table.Columns[i].DataType);
        columnWidths[i] = Math.Max(columnWidths[i], value.Length);
      }
    }
    return columnWidths;
  }

  /// <summary>
  /// Formata um valor com base em seu tipo de dados para uso no cálculo de
  /// larguras de coluna.
  /// </summary>
  /// <param name="value">O valor a ser formatado.</param>
  /// <param name="dataType">
  /// O tipo de dados da coluna que contém o valor.
  /// </param>
  /// <returns>Uma representação em string do valor formatado.</returns>
  private static string FormatValue(object value, Type dataType)
  {
    if (value == DBNull.Value || value == null)
    {
      return "NULL";
    }
    if (dataType == typeof(DateTime))
    {
      DateTime dateValue = (DateTime)value;
      if (dateValue.TimeOfDay.Ticks != 0)
      {
        return dateValue.ToString("dd/MM/yyyy HH:mm:ss");
      }
      return dateValue.ToString("dd/MM/yyyy");
    }
    return value.ToString()!;
  }

  /// <summary>
  /// Imprime uma linha de cabeçalho formatada para um DataTable.
  /// </summary>
  /// <param name="table">
  /// O DataTable para o qual a linha de cabeçalho deve ser impressa.
  /// </param>
  /// <param name="columnWidths">
  /// Um array de inteiros representando a largura máxima de cada coluna no
  /// DataTable.
  /// </param>
  private static void PrintHeaderRow(DataTable table, int[] columnWidths)
  {
    for (int i = 0; i < table.Columns.Count; i++)
    {
      // Adiciona uma largura de coluna extra para espaçamento
      Console.Write(table.Columns[i].ColumnName.PadRight(columnWidths[i] + 2));
    }
    Console.WriteLine();
  }

  /// <summary>
  /// Imprime uma linha de dados formatada para uma DataRow em um DataTable.
  /// </summary>
  /// <param name="row">
  /// A DataRow para a qual a linha de dados deve ser impressa.
  /// </param>
  /// <param name="table">O DataTable ao qual a linha de dados pertence.</param>
  /// <param name="columnWidths">
  /// Um array de inteiros representando a largura máxima de cada coluna no
  /// DataTable.
  /// </param>
  private static void PrintDataRow(DataRow row, DataTable table,
      int[] columnWidths)
  {
    for (int i = 0; i < table.Columns.Count; i++)
    {
      string value = FormatValue(row[i], table.Columns[i].DataType);
      Console.Write(value.PadRight(columnWidths[i] + 2));
    }
    Console.WriteLine();
  }
}
