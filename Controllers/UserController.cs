using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DocShareAPI.Data;
using DocShareAPI.Models;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Microsoft.Extensions.Configuration;
using DocShareAPI.Models.AuthDtos;


namespace DocShareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public UserController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest("User already exist");
            }

            CreatePasswordHash(request.Password,
                out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_configuration["EmailUsername"]);
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Verify your email address";
            email.Body = new TextPart("plain")

            {
                Text = $"Please verify your email address by clicking on the link below: {_configuration["APIUrl"]}/api/user/verify?token={user.VerificationToken}"
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration["EmailUsername"], _configuration["EmailPassword"]);
            smtp.Send(email);
            smtp.Disconnect(true);

            return Ok("User sucessfully created!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == request.Email);
            if (user == null)
            {
                return BadRequest("User not found");

            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("Not verified!");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password is incorrect.");
            }

            return Ok(new { message = $"Welcome back, {user.Email}!", userId = user.Id, email = user.Email });
            // return Ok($"Welcome back, {user.Email}!");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.VerificationToken == token);
            if (user == null)
            {
                return BadRequest("Invalid token.");

            }

            user.VerifiedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("User verified!");

        }
        [HttpGet("verify")]
        public async Task<IActionResult> GetVerify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.VerificationToken == token);
            if (user == null)
            {
                return BadRequest("Invalid token.");
            }

            user.VerifiedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            // return Ok("User verified!");
            // Redirect the user to the login page with a success message
           

            return Ok(new { message = "User verified!", redirectUrl = _configuration["RedirectUrl"]});
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
            if (user == null)
            {
                return BadRequest("User not found");

            }

            user.PasswordResetToken = CreateRandomToken();

            user.ResetTokenExpires = DateTimeOffset.UtcNow.AddDays(1);

            await _context.SaveChangesAsync();

            return Ok("You may now reset your password.");

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("Invalid Token.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Password successfully reset.");
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
    }
}

