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

      // Calcula as larguras de coluna baseadas no comprimento máximo dos dados em cada coluna
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

  private static void PrintHeaderRow(DataTable table, int[] columnWidths)
  {
    for (int i = 0; i < table.Columns.Count; i++)
    {
      // Adiciona uma largura de coluna extra para espaçamento
      Console.Write(table.Columns[i].ColumnName.PadRight(columnWidths[i] + 2));
    }
    Console.WriteLine();
  }

  private static void PrintDataRow(DataRow row, DataTable table, int[] columnWidths)
  {
    for (int i = 0; i < table.Columns.Count; i++)
    {
      string value = FormatValue(row[i], table.Columns[i].DataType);
      Console.Write(value.PadRight(columnWidths[i] + 2));
    }
    Console.WriteLine();
  }
}
