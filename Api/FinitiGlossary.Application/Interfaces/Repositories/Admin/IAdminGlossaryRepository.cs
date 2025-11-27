using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Interfaces.Repositories.Admin
{
    public interface IAdminGlossaryRepository
    {
        Task<List<GlossaryTerm>> GetActiveTermsForAdminViewAsync(GetTermsAdminRequest request);
        Task<List<ArchivedGlossaryTerm>> GetArchivedTermsForAdminViewAsync(GetTermsAdminRequest request);
        Task<List<User>> GetAllUsersAsync();
        Task<GlossaryTerm?> GetActiveByIdAsync(int id);
        Task<GlossaryTerm?> GetActiveByStableIdAsync(Guid stableId);
        Task<List<ArchivedGlossaryTerm>> GetArchivedByStableIdAsync(Guid stableId);
        Task<ArchivedGlossaryTerm?> GetArchivedVersionAsync(Guid stableId, int version);
        Task<int> GetLatestVersionAsync(Guid stableId);


        void AddActiveTerm(GlossaryTerm term);
        void RemoveActiveTerm(GlossaryTerm term);
        void AddArchivedTerm(ArchivedGlossaryTerm term);
        void UpdateArchivedTerm(ArchivedGlossaryTerm term);

        Task<bool> SaveChangesAsync();
        Task<bool> CreateAsync(GlossaryTerm term);
    }
}