namespace FinitiGlossary.Application.DTOs.Term.Admin;
public record UnifiedTermDTO(
    int Id,
    Guid StableId,
    string Term,
    string Definition,
    int Version,
    int Status,
    DateTime Timestamp,
    int? CreatedById,
    int? ArchivedById,
    DateTime? RestoredAt,
    int? RestoredById
);