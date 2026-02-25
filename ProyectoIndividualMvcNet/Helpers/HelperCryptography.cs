using System.Security.Cryptography;
using System.Text;

namespace ProyectoIndividualMvcNet.Helpers
{
    public class HelperCryptography
    {
        public static byte[] EncryptPassword(string password, string salt)
        {
            string contenido = password + salt;
            SHA512 managed = SHA512.Create();
            byte[] salida = Encoding.UTF8.GetBytes(contenido);

            // Bucle de 15 iteraciones para aumentar la seguridad del hash
            for (int i = 1; i <= 15; i++)
            {
                salida = managed.ComputeHash(salida);
            }

            managed.Clear();
            return salida;
        }
    }
}