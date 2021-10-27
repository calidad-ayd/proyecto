using System;
namespace ComunidadPractica
{
    public class Tokenizador
    {
        public static string TokenizarHilera(string hilera)
        {
            byte[] bytes;
            try
            {
                bytes = System.Text.Encoding.UTF8.GetBytes(hilera);
            }
            catch (Exception)
            {
                return "";
            }

            return Convert.ToBase64String(bytes);
        }

        public static string DetokenizarHilera(string hilera)
		{
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(hilera);
            }
            catch (Exception)
            {
                return "";
            }
           
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}