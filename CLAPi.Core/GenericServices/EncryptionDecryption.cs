using CLAPi.Core.Settings;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace CLAPi.Core.GenericServices;

public static class EncryptionDecryption
{
    public static byte[] EncryptStringAes(string value)
    {
        if (value == null || value.Length <= 0)
            throw new ArgumentNullException(nameof(value));

        var Key = Convert.FromBase64String(ConstantValues.AESKey);
        var IV = StringToByteArray(ConstantValues.InitialisationVector);

        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                swEncrypt.Write(value);
            }
            encrypted = msEncrypt.ToArray();
        }
        return encrypted;
    }

    public static string DecryptStringAes(string cipherText)
    {
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException(cipherText);

        var Key = Convert.FromBase64String(ConstantValues.AESKey);
        var IV = StringToByteArray(ConstantValues.InitialisationVector);

        string DecryptedValue = null!;
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var cipherByte = Convert.FromBase64String(cipherText);
            using MemoryStream msDecrypt = new(cipherByte);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);
            DecryptedValue = srDecrypt.ReadToEnd();
        }
        return DecryptedValue;
    }

    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }
}