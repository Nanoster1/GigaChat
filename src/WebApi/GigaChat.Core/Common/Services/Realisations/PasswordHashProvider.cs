﻿using System.Security.Cryptography;

using GigaChat.Core.Common.Services.Interfaces;
using GigaChat.Core.Common.Entities.Users;

namespace GigaChat.Core.Common.Services.Realisations;

public class PasswordHashProvider : IPasswordHashProvider
{
    private const int HashSize = 50;
    private const int Iterations = 256;

    public HashedPassword GetHash(string password, byte[] salt)
    {
        var hash = new byte[HashSize];
        Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA512,
            destination: hash);

        return new HashedPassword(Convert.ToBase64String(hash), salt);
    }

    public HashedPassword GetHash(string password)
    {
        var hash = new byte[HashSize];
        var salt = GenerateRandomSalt();
        Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA512,
            destination: hash);
        return new HashedPassword(Convert.ToBase64String(hash), salt);
    }

    private byte[] GenerateRandomSalt()
    {
        using var rng = RandomNumberGenerator.Create();

        var randomBytes = new byte[1];
        rng.GetBytes(randomBytes);
        var saltSize = randomBytes[0];

        var salt = new byte[saltSize];
        rng.GetBytes(salt);

        return salt;
    }
}