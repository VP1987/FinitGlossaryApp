namespace FinitiGlossary.Application.DTOs.Term.Public;

public record PublicGlossaryDetail(
    int Id,
    string Term,
    string Definition,
    DateTime CreatedAt,
    string CreatedBy
);

