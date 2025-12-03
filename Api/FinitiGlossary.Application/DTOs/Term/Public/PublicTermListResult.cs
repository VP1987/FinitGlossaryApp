namespace FinitiGlossary.Application.DTOs.Term.Public;

public record PublicTermListResult
    (
    PublicTermListMeta Meta,
    List<GlossaryPublicTermDTO> Data
    );
