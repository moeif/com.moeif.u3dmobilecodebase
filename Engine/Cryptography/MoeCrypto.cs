using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public static class MoeCrypto
{
    static byte[] KEY = new byte[] { 0x51, 0x08, 0xd3, 0xff, 0x6b, 0x11, 0x9e, 0x1c, 0xbe, 0xbb, 0x15, 0x27, 0xc5, 0x6e, 0x0c, 0xec };
    static byte[] IV = new byte[] { 0xcb, 0x55, 0x4d, 0x26, 0x25, 0xf3, 0x1f, 0x80, 0x60, 0xf7, 0x7d, 0x56, 0x1e, 0xd3, 0x05, 0x40 };

    public static string EncryptToAesBase64(string srcText)
    {
        byte[] encrypted_bytes = EncryptStringToBytes_Aes(srcText, KEY, IV);
        return Convert.ToBase64String(encrypted_bytes);
    }

    public static string DecryptFromAesBase64(string base64)
    {
        byte[] encrypted_bytes = Convert.FromBase64String(base64);
        return DecryptStringFromBytes_Aes(encrypted_bytes, KEY, IV);
    }

    public static string DecryptFromBase64Bytes(byte[] encrypted_bytes)
    {
        return DecryptStringFromBytes_Aes(encrypted_bytes, KEY, IV);
    }


    static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");
        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }

}