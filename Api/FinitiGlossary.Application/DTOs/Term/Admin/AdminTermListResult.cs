namespace FinitiGlossary.Application.DTOs.Term.Admin;

public record AdminTermListResult(
 AdminTermListMeta Meta,
 List<GlossaryAdminTermDTO> Data
);
