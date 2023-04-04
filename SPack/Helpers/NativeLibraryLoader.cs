using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SPack.Helpers;

/// <summary>
/// Classe utilitária para carregar bibliotecas nativas.
/// </summary>
public class NativeLibraryLoader
{
  [DllImport("kernel32", SetLastError = true)]
  private static extern IntPtr LoadLibrary(string lpFileName);

  /// <summary>
  /// Carrega todas as bibliotecas nativas compatíveis com o sistema
  /// operacional.
  /// </summary>
  public static void LoadAllNativeLibraries()
  {
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return;
    }

    var resourcePrefix = "/native-libs/";
    var resourceSuffix = ".dll";

    var assembly = Assembly.GetExecutingAssembly();
    assembly.GetManifestResourceNames()
      .Where(name => name.StartsWith(resourcePrefix))
      .Where(name => name.EndsWith(resourceSuffix))
      .Select(name => name[resourcePrefix.Length..])
      .ToList()
      .ForEach(LoadNativeLibrary);
  }

  /// <summary>
  /// Carrega uma biblioteca nativa na biblioteca atual.
  /// </summary>
  /// <param name="libraryName">O nome da biblioteca nativa.</param>
  /// <exception cref="ArgumentNullException">
  /// Gerado se <paramref name="resourceName"/> ou
  /// <paramref name="libraryName"/> for nulo.
  /// </exception>
  /// <exception cref="FileNotFoundException">
  /// Gerado se o recurso incorporado não for encontrado ou não puder ser
  /// acessado.
  /// </exception>
  /// <exception cref="DllNotFoundException">
  /// Gerado se a biblioteca nativa não puder ser carregada.
  /// </exception>
  public static void LoadNativeLibrary(string libraryName)
  {
    string tempDirectory = Path.Combine(Path.GetTempPath(), nameof(SPack));
    string tempFilePath = Path.Combine(tempDirectory, libraryName);

    if (!Directory.Exists(tempDirectory))
    {
      Directory.CreateDirectory(tempDirectory);
    }

    var resourceName = $"/native-libs/{libraryName}";

    var assembly = Assembly.GetExecutingAssembly();
    using var stream = assembly.GetManifestResourceStream(resourceName)!;
    using var fileStream = new FileStream(tempFilePath, FileMode.Create);

    stream.CopyTo(fileStream);

    LoadLibrary(tempFilePath);
  }
}
