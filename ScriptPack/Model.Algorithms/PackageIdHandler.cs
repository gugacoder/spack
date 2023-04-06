using ScriptPack.Domain;
using ScriptPack.Helpers;

namespace ScriptPack.Model.Algorithms;

/// <summary>
/// Utilitário para manipulação de identificadores de pacotes.
/// O identificador de pacote é uma alternativa para identificação de pacotes
/// a partir de strings.
/// O identificador tem a forma:
///   [produto/][modulo:]pacote[@versao]
/// Sendo:
///   produto - Opcional. Nome do produto seguido de uma barra.
///   modulo  - Opcional. Nomes dos módulos separados por dois pontos e seguidos
///             de dois ponto.
///   pacote  - Nome do pacote.
///   versao  - Opcional. Versão do pacote.
/// Vários identificador de pacotes podem ser indicados em uma única string,
/// separados por vírgula.
/// Exemplos:
///   Api                 - pacote Api
///   Api@1.0.0           - pacote Api na versão 1.0.0
///   Module1:Api@1.0.0   - pacote Api do módulo Module1 na versão 1.0.0
///   MySystem/Api@1.0.0  - pacote Api produto MySystem na versão 1.0.0
/// Exemplo de nome completo de um pacote com submódulos:
///   MySystem/Module1:SubModule2:Api@1.0.0
/// </summary>
public class PackageIdHandler
{
  /// <summary>
  /// Representação das partes de um identificador de pacote.
  /// </summary>
  /// <param name="Product">Opcional. Nome do produto.</param>
  /// <param name="Version">Opcional. Versão do pacote.</param>
  /// <param name="Modules">Opcional. Array de nomes de módulos.</param>
  /// <param name="Package">Nome do pacote.</param>
  public record IdComponents(
      string? Product,
      string? Version,
      string?[]? Modules,
      string? Package
  );

  /// <summary>
  /// Cria uma string de identificação para o pacote.
  /// </summary>
  /// <param name="package">
  /// Pacote para o qual será gerado o identificador.
  /// </param>
  /// <returns>
  /// String de identificação do pacote.
  /// </returns>
  public string CreatePackageId(PackageNode package)
  {
    var product = package.Ancestor<ProductNode>();
    var version = package.Ancestor<VersionNode>();
    var modules = package.Ancestors<ModuleNode>();
    return CreatePackageId(
        product?.Name,
        version?.Version,
        modules.Select(m => m.Name).ToArray(),
        package.Name
    );
  }

  /// <summary>
  /// Cria uma string de nome de pacote a partir de um array de nodos de
  /// pacote. Os identificadores de produto são gerados para cada coleção de
  /// componentes de identificação e concatenados por vírgula produzindo
  /// a string final.
  /// </summary>
  /// <param name="packages">Array de identificação dos pacotes.</param>
  /// <returns>String de identificação dos pacotes.</returns>
  /// <param name="product">Opcional. Nome do produto.</param>
  /// <param name="version">Opcional. Versão do pacote.</param>
  /// <param name="modules">Opcional. Array de nomes de módulos.</param>
  /// <param name="package">Opcional. Nome do pacote.</param>
  public string CreatePackageId(string? product, string? version,
      string?[]? modules, string? package)
  {
    modules = modules?.Select(x => x is null ? "*" : x).ToArray();
    return string.Concat(
        !string.IsNullOrEmpty(product) ? $"{product}/" : "",
        (modules?.Any() == true) ? $"{string.Join(":", modules)}:" : "",
        !string.IsNullOrEmpty(package) ? $"{package}" : "",
        !string.IsNullOrEmpty(version) ? $"@{version}" : ""
    );
  }

  /// <summary>
  /// Interpreta uma string de identificação de pacote e retorna o pacote
  /// correspondente caso encontrado na coleção de pacotes.
  /// </summary>
  /// <param name="packageId">String de identificação do pacote.</param>
  /// <param name="availableNodes">
  /// Nodos disponíveis para pesquisa do pacote.
  /// </param>
  /// <returns>
  /// Array contendo os pacotes que correspondem ao identificador.
  /// </returns>
  public PackageNode[] ParsePackageId(string packageId,
      IEnumerable<INode> availableNodes)
  {
    const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

    var (productName, versionName, moduleNames, packageName) =
        ParsePackageId(packageId);

    var selectLatestVersion = (versionName?.Equals("latest", IgnoreCase) == true);
    if (selectLatestVersion)
    {
      versionName = null;
    }

    var allPackageIdComponents = (
        from node in availableNodes
        from package in node.DescendantsAndSelf<PackageNode>()
        let product = package.Ancestor<ProductNode>()
        let version = package.Ancestor<VersionNode>()
        let module = package.Ancestors<ModuleNode>().DefaultIfEmpty()
        select new
        {
          Product = product.Name,
          Version = version.Version,
          Modules = module
                .Where(x => x is not null)
                .Select(x => x!.Name)
                .ToArray(),
          Package = package.Name,
          PackageInstance = package
        }
    ).Distinct().ToArray();

    var foundPackages = (
        from id in allPackageIdComponents
        where productName is null
            || string.Equals(id.Product, productName, IgnoreCase)
        where versionName is null
            || string.Equals(id.Version, versionName, IgnoreCase)
        where packageName is null
            || string.Equals(id.Package, packageName, IgnoreCase)
        where (moduleNames?.Any() != true)
            || AllTokensMatches(moduleNames, id.Modules)
        select id.PackageInstance
    ).Distinct().ToArray();

    if (selectLatestVersion)
    {
      var latestVersion = foundPackages
          .OrderByDescending(
              x => x.Ancestor<VersionNode>()?.Version, new VersionComparer())
          .FirstOrDefault();
      foundPackages = new[] { latestVersion };
    }

    return foundPackages.ToArray();
  }

  /// <summary>
  /// Verifica se todos os tokens de um array de strings são iguais a um
  /// outro array de strings.
  /// </summary>
  private bool AllTokensMatches(string?[] tokens1, string[] tokens2)
  {
    const StringComparison IgnoreCase = StringComparison.OrdinalIgnoreCase;

    if (tokens1.Length != tokens2.Length) return false;

    for (int i = 0; i < tokens1.Length; i++)
    {
      if (tokens1[i] is null) continue;
      if (!string.Equals(tokens1[i], tokens2[i], IgnoreCase))
      {
        return false;
      }
    }

    return true;
  }

  /// <summary>
  /// Cria um array de nodos de pacote a partir de uma string de nome de
  /// pacote.
  /// </summary>
  public IdComponents ParsePackageId(string packageId)
  {
    var parts = packageId.Split('@');
    var version = parts.Length > 1 ? parts[1] : null;

    parts = parts[0].Split('/');
    var product = parts.Length > 1 ? parts[0] : null;

    parts = parts[parts.Length - 1].Split(':');
    var package = parts[parts.Length - 1];

    string?[]? modules = parts.Length > 1
        ? parts.Take(parts.Length - 1).ToArray()
        : null;

    if (product == "" || product == "*") product = null;
    if (version == "" || version == "*") version = null;
    if (package == "" || package == "*") package = null;
    modules = modules?.Select(x => x == "" || x == "*" ? null : x).ToArray();

    return new(product, version, modules, package);
  }
}
