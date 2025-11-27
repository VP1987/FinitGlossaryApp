using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.DTOs.Response;
using FinitiGlossary.Application.DTOs.Term.Admin;
using System.Security.Claims;

namespace FinitiGlossary.Application.Interfaces.Term.Admin
{
    public interface IAdminGlossaryService
    {
        Task<AdminTermListResult> GetAdminTermListAsync(ClaimsPrincipal user, AdminTermQuery query);
        Task<PublishResult> PublishTermAsync(int id, ClaimsPrincipal user);
        Task<CreateResult> CreateTermAsync(CreateGlossaryRequest request, ClaimsPrincipal user);
        Task<UpdateResult> UpdateTermAsync(int id, UpdateGlossaryRequest request, ClaimsPrincipal user);
        Task<ArchiveResult> ArchiveTermAsync(int id, ClaimsPrincipal user);
        Task<RestoreResult> RestoreTermVersionAsync(Guid stableId, int version, ClaimsPrincipal user);
        Task<HistoryResult> GetTermHistoryAsync(Guid stableId, ClaimsPrincipal user);
        Task<DeleteResult> DeleteTermAsync(int id, ClaimsPrincipal user);
    }
}