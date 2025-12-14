namespace FinitiGlossary.Application.DTOs.Term.Admin
{
    public class AdminTermRow
    {
        public int Id { get; set; }
        public Guid StableId { get; set; }
        public string Term { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public int Status { get; set; }
        public DateTime CreatedOrArchivedAt { get; set; }
        public int CreatedById { get; set; }
        public string? CreatedByName { get; set; }
    }
}
