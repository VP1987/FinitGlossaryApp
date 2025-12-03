namespace FinitiGlossary.Application.DTOs.Term.Public;

public record GlossaryPublicTermDTO(int Id,
    Guid StableId,
    string Term,
    string Definition,
    int Version,
    int Status,
    DateTime CreatedAt,
    int CreatedById);
