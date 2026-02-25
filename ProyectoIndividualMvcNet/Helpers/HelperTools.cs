using System;

namespace ProyectoIndividualMvcNet.Helpers
{
    public class HelperTools
    {
        // Genera una cadena aleatoria de 50 caracteres
        public static string GenerateSalt()
        {
            Random random = new Random();
            string salt = "";
            for (int i = 1; i <= 50; i++)
            {
                // Rango de caracteres ASCII visibles
                int num = random.Next(1, 255);
                char letra = Convert.ToChar(num);
                salt += letra;
            }
            return salt;
        }

        // Compara dos arrays de bytes (el del login vs el de la base de datos)
        public static bool CompareArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Equals(b[i]) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}