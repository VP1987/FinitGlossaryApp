namespace FinitiGlossary.Application.DTOs.Term.Public;
public record PublicTermQuery(
      int Offset = 0,
      int Limit = 50,
      string Sort = "dateDesc",
      string? Search = null
  );
