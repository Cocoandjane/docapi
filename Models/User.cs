using System;
using System.ComponentModel.DataAnnotations;

namespace DocShareAPI.Models
{
    public class User
    {
        [Key]
        public int? Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public byte[] PasswordHash { get; set; } = new byte[32];

        public byte[] PasswordSalt { get; set; } = new byte[32];

        public string? VerificationToken { get; set; }

        public int MyProperty { get; set; }

        public DateTimeOffset? VerifiedAt { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTimeOffset? ResetTokenExpires { get; set; }
    }
}

