namespace FinitiGlossary.Application.DTOs.Term.Admin;
public record AdminTermQuery
{
    public int Offset { get; init; } = 0;
    public int Limit { get; init; } = 50;
    public string Sort { get; init; } = "dateDesc";
    public string? Search { get; init; } = null;
    public string Tab { get; init; } = "all";
}
