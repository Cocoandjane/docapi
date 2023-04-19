using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocShareAPI.Models
{
    public class Document
    {

        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    }
}

