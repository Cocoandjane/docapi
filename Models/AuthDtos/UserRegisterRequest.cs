using System;
using System.ComponentModel.DataAnnotations;

namespace DocShareAPI.Models
{
	public class UserRegisterRequest
	{
		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters.")]
		public string Password { get; set;  } = string.Empty;

		[Required, Compare("Password")]

		public string ComfirmPassword { get; set; } = string.Empty;

    }
}

