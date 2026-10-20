using System.Security.Cryptography;

namespace ReservAr.Helpers;

public static class HashHelper
{
    private const int Iteraciones = 600_000;
    private const int TamanoSalt = 16;
    private const int TamanoHash = 32;


    public static string Hashear(string password)
    {
        
        var salt = RandomNumberGenerator.GetBytes(TamanoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
        return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string password, string hashGuardado){

        var partes = hashGuardado.Split('.');
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones))
        return false;

        try
        {
            var salt = Convert.FromBase64String(partes[1]);
            var hashEsperado = Convert.FromBase64String(partes[2]);
            var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(password, salt, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);
            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }catch (FormatException)
        {
            return false;
        }


    }



}