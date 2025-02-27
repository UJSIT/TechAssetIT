using System;
using System.Security.Cryptography;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        // Generate a salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

        // Combine the password and the salt
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);

        // Combine the salt and hash
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);

        // Convert to base64
        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string savedHash)
    {
        // Extract the bytes
        byte[] hashBytes = Convert.FromBase64String(savedHash);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        // Hash the input password with the stored salt
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbkdf2.GetBytes(20);

        // Compare the hash with the stored hash
        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[i + 16] != hash[i])
                return false;
        }

        return true;
    }
}
