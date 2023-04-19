namespace DocShareAPI.Models.DocDtos
{
    public class DocumentWithInvitationId
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public int InvitationId { get; set; }
    }
}
