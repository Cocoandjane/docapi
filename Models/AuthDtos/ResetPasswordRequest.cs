using System;
using System.ComponentModel.DataAnnotations;

namespace DocShareAPI.Models.AuthDtos
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        public string ComfirmPassword { get; set; } = string.Empty;

    }
}

