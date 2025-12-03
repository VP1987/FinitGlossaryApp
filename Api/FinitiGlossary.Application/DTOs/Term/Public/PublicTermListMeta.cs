namespace FinitiGlossary.Application.DTOs.Term.Public;

public record PublicTermListMeta(
       int Offset,
       int Limit,
       int Total,
       bool HasMore,
       string? Sort,
       string? Search
   );
