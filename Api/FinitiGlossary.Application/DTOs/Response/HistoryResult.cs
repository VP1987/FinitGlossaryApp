using FinitiGlossary.Application.DTOs.Term.Admin;

namespace FinitiGlossary.Application.DTOs.Response;

public record HistoryResult(
       Guid StableId,
       List<GlossaryAdminTermDTO> Versions
   );
