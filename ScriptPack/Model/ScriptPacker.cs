using Newtonsoft.Json;
using ScriptPack.Domain;
using ScriptPack.FileSystem;
using ScriptPack.Helpers;

namespace ScriptPack.Model;

/// <summary>
/// Utilitário de empacotamento de scripts.
/// </summary>
public class ScriptPacker
{
  private string? _filePath;
  private string? _password;
  private readonly List<INode> _nodes = new();

  /// <summary>
  /// Caminho do arquivo ZIP de destino do pacote.
  /// Este será o arquivo de pacote gerado no formato ZIP com os scripts
  /// protegidos por senha.
  /// O arquivo ZIP será explorável mas o conteúdo dos arquivos empacotados
  /// nele somente poderão ser lidos se a senha for fornecida.
  /// </summary>
  public void AddTargetZipFile(string filePath)
  {
    _filePath = filePath;
  }

  /// <summary>
  /// Lista de drives para geração do pacote.
  /// </summary>
  /// <param name="password">
  /// Senha para criptografia do pacote.
  /// </param>
  public void AddPassword(string? password)
  {
    _password = password;
  }

  /// <summary>
  /// Adiciona os scripts do nodo indicado ao pacote a ser gerado.
  /// </summary>
  /// <param name="node">
  /// Nodo para leitura dos scripts.
  /// </param>
  public void AddScript(INode node)
  {
    _nodes.Add(node);
  }

  /// <summary>
  /// Gera o pacote de scripts.
  /// </summary>
  public async Task PackScriptsAsync()
  {
    if (string.IsNullOrEmpty(_filePath))
    {
      throw new InvalidOperationException(
          "Nenhum arquivo de destino para gravação do pacote indicado.");
    }

    var drive = new ZipDrive(_filePath, _password, Mode.Overwrite);

    var scripts = _nodes
        .SelectMany(n => n.DescendantsAndSelf<ScriptNode>())
        .Distinct()
        .ToArray();
    var configs = scripts
        .SelectMany(s => s.Ancestors<IFileNode>())
        .Distinct()
        .ToArray();

    foreach (var item in configs)
    {
      var json = JsonConvert.SerializeObject(item, JsonOptions.CamelCase);
      await drive.WriteAllTextAsync(item.Path, json);
    }

    foreach (var item in scripts)
    {
      using var reader = await item.ReadScriptFileAsync();
      var content = await reader.ReadToEndAsync();
      await drive.WriteAllTextAsync(item.Path, content);
    }
  }

}
