using DataHandlerLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Services
{
    public class AESEncryptDecryptServices
    {
        private string _encryptionKey { get; set; } = string.Empty;

        /// <summary>
        /// Generates a cryptographically secure encryption key based on device ID and salt
        /// </summary>
        /// <returns>Base64 encoded encryption key</returns>
        public string GenerateEncryptionKey(EncryptionSettings encryptionSettings)
        {
            try
            {
                // Combine device ID and salt
                string keyMaterial = $"{encryptionSettings.GetSalt()}{encryptionSettings.GetPepper()}";

                // Use SHA256 to create a consistent 256-bit key
                using (var sha256 = SHA256.Create())
                {
                    byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyMaterial));
                    _encryptionKey = Convert.ToBase64String(keyBytes);
                    return _encryptionKey;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate encryption key: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Encrypts a plain text string using AES encryption
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <param name="key">Optional encryption key. If not provided, uses the generated key</param>
        /// <returns>Base64 encoded encrypted string</returns>
        public string Encrypt(string plainText, string Key)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(Key))
                throw new ArgumentException("Plain text or Key cannot be null or empty", nameof(plainText) + nameof(Key));

            try
            {
                // Use provided key or generate one if not available
                string encryptionKey = Key;
                byte[] keyBytes = Convert.FromBase64String(encryptionKey);

                using (var aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.GenerateIV(); // Generate a random IV for each encryption

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                        // Combine IV and encrypted data
                        byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
                        Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                        Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Encryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Decrypts an encrypted string using AES decryption
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted string</param>
        /// <param name="key">Optional encryption key. If not provided, uses the generated key</param>
        /// <returns>Decrypted plain text string</returns>
        public string Decrypt(string encryptedText, string Key)
        {
            if (string.IsNullOrEmpty(encryptedText))
                throw new ArgumentException("Encrypted text cannot be null or empty", nameof(encryptedText));

            try
            {
                // Use provided key or generate one if not available
                string encryptionKey = Key;
                byte[] keyBytes = Convert.FromBase64String(encryptionKey);
                byte[] encryptedData = Convert.FromBase64String(encryptedText);

                using (var aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // Extract IV from the beginning of the encrypted data
                    byte[] iv = new byte[aes.BlockSize / 8];
                    byte[] cipherText = new byte[encryptedData.Length - iv.Length];

                    Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                    Array.Copy(encryptedData, iv.Length, cipherText, 0, cipherText.Length);

                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Decryption failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generates a random AES encryption key
        /// </summary>
        /// <returns>Base64 encoded random encryption key</returns>
        public static string GenerateRandomKey()
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.GenerateKey();
                    return Convert.ToBase64String(aes.Key);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate random key: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates if a string is a valid Base64 encoded AES key
        /// </summary>
        /// <param name="key">The key to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            try
            {
                byte[] keyBytes = Convert.FromBase64String(key);
                return keyBytes.Length == 32; // AES-256 requires 32 bytes
            }
            catch
            {
                return false;
            }
        }
    }
}
