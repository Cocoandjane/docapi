using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocShareAPI.Models
{
    public class Invitation
    {
        [Key]
        public int Id { get; set; }

        public string SenderEmail { get; set; } = string.Empty;

        public string RecipientEmail { get; set; } = string.Empty;

        public string? InvitationToken { get; set; }
        
        public DateTimeOffset? InvitationAcceptedAt { get; set; }

        public DateTimeOffset? InvitationTokenExpires { get; set; }


        [ForeignKey("DocumentId")]
        public int DocumentId { get; set; }
        public Document? Document { get; set; }
    }
}