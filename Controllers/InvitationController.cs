using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocShareAPI.Data;
using DocShareAPI.Models;
using System.Security.Cryptography;
using MimeKit;
using MailKit.Security;
using MimeKit.Text;
using DocShareAPI.Models.DocDtos;

namespace DocShareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public InvitationController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Invitation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invitation>>> GetInvitations()
        {
            if (_context.Invitations == null)
            {
                return NotFound();
            }
            return await _context.Invitations.ToListAsync();
        }

        [HttpGet("documents/{recipientEmail}")]
        public async Task<IEnumerable<DocumentWithInvitationId>> GetDocumentsForRecipientEmail(string recipientEmail)
        {
            // Retrieve all invitations for the specified recipient email address
            var invitations = await _context.Invitations
                .Where(i => i.RecipientEmail == recipientEmail && i.InvitationAcceptedAt != null)
                .ToListAsync();

            // Get the document IDs associated with the invitations
            var documentIds = invitations.Select(i => i.DocumentId).Distinct();

            // Retrieve the documents associated with the document IDs
            var documents = await _context.Documents
                .Where(d => documentIds.Contains(d.Id))
                .ToListAsync();

            // Map the documents to the DocumentWithInvitationId model, which includes the invitation ID
            var documentsWithInvitationId = documents.Select(d => new DocumentWithInvitationId
            {
                Id = d.Id,
                Title = d.Title,
                Body = d.Body,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                InvitationId = invitations.First(i => i.DocumentId == d.Id).Id
            });

            return documentsWithInvitationId;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Invitation>> GetInvitation(int id)
        {
            if (_context.Invitations == null)
            {
                return NotFound();
            }
            var invitation = await _context.Invitations.FindAsync(id);

            if (invitation == null)
            {
                return NotFound();
            }

            return invitation;
        }

        // PUT: api/Invitation/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvitation(int id, Invitation invitation)
        {
            if (id != invitation.Id)
            {
                return BadRequest();
            }

            _context.Entry(invitation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvitationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Invitation
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Invitation>> PostInvitation(Invitation request)
        {
            if (_context.Invitations == null)
            {
                return Problem("Entity set 'DataContext.Invitations'  is null.");
            }
            var document = await _context.Documents.FindAsync(request.DocumentId);
            if (document == null)
            {
                return NotFound("Document not found.");
            }

            var invitation = new Invitation
            {
                SenderEmail = request.SenderEmail,
                RecipientEmail = request.RecipientEmail,
                InvitationToken = CreateRandomToken(),
                DocumentId = request.DocumentId,
                Document = document,
                InvitationAcceptedAt = null,
                InvitationTokenExpires = DateTimeOffset.UtcNow.AddHours(24),

            };
            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_configuration["EmailUsername"]);
            email.To.Add(MailboxAddress.Parse(invitation.RecipientEmail));
            email.Subject = "JaneWrite Document Invitation";
            email.Body = new TextPart("plain")
            {
                Text = $"You have been invited to view a document on JaneWrite. Please click the link below to view the document.{_configuration["APIUrl"]}/api/invitation/accept/{invitation.InvitationToken}"
            };
            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_configuration["EmailUsername"], _configuration["EmailPassword"]);
            smtp.Send(email);
            smtp.Disconnect(true);
            return CreatedAtAction("GetInvitation", new { id = invitation.Id }, invitation);
        }

        [HttpGet("accept/{token}")]

        public async Task<ActionResult<Invitation>> AcceptInvitation(string token)
        {
            if (_context.Invitations == null)
            {
                return NotFound();
            }
            var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.InvitationToken == token);
            if (invitation == null)
            {
                return NotFound();
            }
            if (invitation.InvitationTokenExpires < DateTimeOffset.UtcNow)
            {
                return Problem("Invitation token has expired.");
            }
            invitation.InvitationAcceptedAt = DateTimeOffset.UtcNow;
            _context.Entry(invitation).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Invitatin accepted", redirectUrl = _configuration["DocShareUrl"] + "/" + invitation.DocumentId });
        }


        // DELETE: api/Invitation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            if (_context.Invitations == null)
            {
                return NotFound(new { message = "Entity set 'DataContext.Invitations'  is null." });
            }   
            var invitation = await _context.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return NotFound(new { message = "Document not found" });
            }

            _context.Invitations.Remove(invitation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Invitation deleted successfully" });
        }

        private bool InvitationExists(int id)
        {
            return (_context.Invitations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
    }
}
