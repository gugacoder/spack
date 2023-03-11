using System.Text;

namespace ScriptPack.Domain;

/// <summary>
/// Batch é um bloco de instruções de um script SQL.
/// Para scripts do SQLServer, representa os blocos lógicos separados pelo
/// comando "GO". Para os demais bancos de dados, representa um script completo.
/// </summary>
/// Em scripts do SQL Server, um Batch representa um bloco de código separado
/// pelo comando "GO". O comando "GO" é utilizado para separar logicamente os
/// blocos de código em instruções SQL executáveis pelo SGDB.
///
/// É possível utilizar o comando "GO" seguido de um número para indicar a
/// repetição do bloco de código pelo número de vezes indicado.
/// 
/// Por exemplo, o código SQL abaixo exibe a mensagem "Olá, mundo!" três vezes:
///
/// <code>
/// SELECT 'Olá, mundo!' AS mensagem
/// GO 3
/// </code>
///
/// Este comando "GO" com o número de repetições só é reconhecido pelo SQL
/// Server e não é suportado em outros SGDBs.
/// </remarks>
public class Batch
{
  /// <summary>
  /// O índice do bloco no script, começando em zero.
  /// </summary>
  public int Index { get; set; }

  /// <summary>
  /// O texto SQL do bloco.
  /// </summary>
  public StringBuilder Buffer { get; set; } = new StringBuilder();

  /// <summary>
  /// O número de repetições do bloco, começando em zero. 
  /// Zero significa que o bloco deve ser executado apenas uma vez.
  /// </summary>
  public int Repetition { get; set; }

  /// <summary>
  /// Concatena o conteúdo do buffer em um único texto e substitui o conteúdo
  /// atual do buffer pelo novo texto.
  /// </summary>
  /// <remarks>
  /// O buffer é utilizado para armazenar as partes do bloco de script à medida
  /// que o script é lido linha a linha. A cada invocação de ToString() o
  /// conteúdo do buffer é concatenado em uma única string.
  /// O método FlatBuffer() é utilizado para concatenar o conteúdo do buffer
  /// em uma única string e substituir o conteúdo atual do buffer pelo novo
  /// texto. Dessa forma, reduzindo o custo de invocação do método ToString().
  /// </remarks>
  public void FlatBuffer()
  {
    var text = this.Buffer.ToString();
    this.Buffer.Clear();
    this.Buffer.Append(text);
  }
}
