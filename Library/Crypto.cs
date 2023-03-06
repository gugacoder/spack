using System.Text;

namespace SPack.Library;

public static class Crypto
{
  private const string Prefix = "enc:spk:";
  private const string EncryptionKey = nameof(SPack.Library.Crypto);

  public static string Encrypt(string text)
  {
    if (text.StartsWith(Prefix))
      return text;

    var plainBytes = Encoding.UTF8.GetBytes(text);
    var keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);

    var encryptedBytes = new byte[plainBytes.Length];
    for (int i = 0; i < plainBytes.Length; i++)
    {
      encryptedBytes[i] = (byte)(plainBytes[i] ^ keyBytes[i % keyBytes.Length]);
    }

    return Prefix + Convert.ToBase64String(encryptedBytes);
  }

  public static string Decrypt(string text)
  {
    if (!text.StartsWith(Prefix))
      return text;

    var encryptedBytes = Convert.FromBase64String(text[Prefix.Length..]);
    var keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);

    var decryptedBytes = new byte[encryptedBytes.Length];
    for (int i = 0; i < encryptedBytes.Length; i++)
    {
      decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
    }

    return Encoding.UTF8.GetString(decryptedBytes);
  }
}

