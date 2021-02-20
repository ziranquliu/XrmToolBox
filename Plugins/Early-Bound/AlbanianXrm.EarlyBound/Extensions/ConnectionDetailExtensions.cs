﻿using McTools.Xrm.Connection;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AlbanianXrm.EarlyBound.Extensions
{
    static class ConnectionDetailExtensions
    {
        private const string PASS_PHRASE = "MsCrmTools";
        private const string SALT = "Tanguy 92*";
        private const string HASH_ALGORITHM = "SHA1";
        private const string IV = "ahC3@bCa2Didfc3d";
        private const int KEY_SIZE = 256;
        private const int ITERATIONS = 2;

        public static string GetConnectionStringWithoutPassword(this ConnectionDetail connection)
        {
            if (!connection.ClientSecretIsEmpty)
            {
                return $"AuthType=ClientSecret;url={connection.OriginalUrl};ClientId={connection.UserName};Secret=********;SkipDiscovery=true;";
            }
            return connection.GetConnectionString();
        }

        public static string GetConnectionStringWithPassword(this ConnectionDetail connection)
        {
            string password = "";

            if (!connection.ClientSecretIsEmpty)
            {
                var prop = connection.GetType().GetProperty("ClientSecretEncrypted", BindingFlags.Instance | BindingFlags.Public);
                if (prop != null && (string)prop.GetValue(connection) != null)
                {
                    password = Decrypt((string)prop.GetValue(connection));
                }
                return connection.GetConnectionStringWithoutPassword().Replace("********", password);
            }

            var field = connection.GetType().GetField("userPassword", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null && (string)field.GetValue(connection)!=null)
            {
                password = Decrypt((string)field.GetValue(connection));
            }

            if (string.IsNullOrEmpty(password))
            {
                // Lookup Old Public Property
                var prop = connection.GetType().GetProperty("UserPassword", BindingFlags.Instance | BindingFlags.Public);
                if (prop != null && (string)prop.GetValue(connection) != null)
                {
                    password = (string)prop.GetValue(connection);
                }
            }
            
            return connection.GetConnectionStringWithoutPassword().Replace("********", password);
        }

        private static string Decrypt(string cipherText)
        {
            using (var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC })
            {
                using (PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(
                            PASS_PHRASE,
                            Encoding.ASCII.GetBytes(SALT),
                            HASH_ALGORITHM,
                            ITERATIONS))
                {
                    var decryptor =
                        rijndaelManaged.CreateDecryptor(
                            passwordDeriveBytes.GetBytes(KEY_SIZE / 8),
                            Encoding.ASCII.GetBytes(IV));
                    var buffer = Convert.FromBase64String(cipherText);
                    using (var memoryStream = new MemoryStream(buffer))
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        var numArray = new byte[buffer.Length];
                        var count = cryptoStream.Read(numArray, 0, numArray.Length);
                        return Encoding.UTF8.GetString(numArray, 0, count);
                    }
                }
            }
        }
    }
}