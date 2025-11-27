namespace FinitiGlossary.Application.DTOs.Term.Admin;

public record AdminTermListMeta(
    int Offset,
    int Limit,
    int Total,
    bool HasMore,
    string Sort,
    string? Search,
    string? Tab
);
