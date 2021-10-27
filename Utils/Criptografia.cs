using System;
using System.Security.Cryptography;

namespace ComunidadPractica
{
    public class Criptografia
    {
        public static string crearSalt(int tamanno)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[tamanno];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }

        public static string hashearPassword(string password, string salt)
        {
            HashAlgorithm algoritmo = new SHA256CryptoServiceProvider();
            byte[] bytesLimpios   = System.Text.Encoding.UTF8.GetBytes(password + salt);
            byte[] bytesHasheados = algoritmo.ComputeHash(bytesLimpios);
            return Convert.ToBase64String(bytesHasheados);
        }
    }
}