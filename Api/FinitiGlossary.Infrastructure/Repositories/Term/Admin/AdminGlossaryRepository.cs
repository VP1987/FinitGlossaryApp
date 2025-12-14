using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.DTOs.Term.Admin;
using FinitiGlossary.Application.Interfaces.Repositories.Term.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;
using FinitiGlossary.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;

namespace FinitiGlossary.Infrastructure.Repositories.Term.Admin
{
    public class AdminGlossaryRepository : IAdminGlossaryRepository
    {
        private readonly AppDbContext _db;

        public AdminGlossaryRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<(List<AdminTermRow> Items, int Total)> GetAdminTermsPageAsync(
    int userId,
    string role,
    string tab,
    string? search,
    string sort,
    int offset,
    int limit)
        {
            bool isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            IQueryable<AdminTermRow> query =
                _db.GlossaryTerms.Select(t => new AdminTermRow
                {
                    Id = t.Id,
                    StableId = t.StableId,
                    Term = t.Term,
                    Definition = t.Definition,
                    Status = (int)t.Status,
                    CreatedOrArchivedAt = t.CreatedAt,
                    CreatedById = t.CreatedById
                });

            if (!isAdmin)
                query = query.Where(x => x.CreatedById == userId);

            if (!string.IsNullOrWhiteSpace(tab))
            {
                var status = tab.ToLower() switch
                {
                    "draft" => 0,
                    "published" => 1,
                    "archived" => 2,
                    _ => (int?)null
                };

                if (status != null)
                    query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    x.Term.Contains(search) ||
                    x.Definition.Contains(search));

            query = sort switch
            {
                "dateAsc" => query.OrderBy(x => x.CreatedOrArchivedAt),
                "az" => query.OrderBy(x => x.Term),
                "za" => query.OrderByDescending(x => x.Term),
                _ => query.OrderByDescending(x => x.CreatedOrArchivedAt)
            };

            var total = await query.CountAsync();

            var page = await query
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return (page, total);
        }

        public Task<List<GlossaryTerm>> GetActiveTermsForAdminViewAsync(GetTermsAdminRequest request)
        {
            IQueryable<GlossaryTerm> query = _db.GlossaryTerms.AsQueryable();

            bool isAdmin = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && request.UserId != null)
            {
                query = query.Where(t => t.CreatedById == request.UserId.Value);
            }

            return query.ToListAsync();
        }

        public Task<List<ArchivedGlossaryTerm>> GetArchivedTermsForAdminViewAsync(GetTermsAdminRequest request)
        {
            IQueryable<ArchivedGlossaryTerm> query = _db.ArchivedGlossaryTerms.AsQueryable();

            bool isAdmin = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && request.UserId != null)
            {
                query = query.Where(t => t.CreatedById == request.UserId.Value);
            }

            return query.ToListAsync();
        }

        public Task<GlossaryTerm?> GetActiveByIdAsync(int id)
        {
            return _db.GlossaryTerms.FirstOrDefaultAsync(t => t.Id == id);
        }

        public Task<GlossaryTerm?> GetActiveByStableIdAsync(Guid stableId)
        {
            return _db.GlossaryTerms.FirstOrDefaultAsync(t => t.StableId == stableId);
        }

        public Task<List<ArchivedGlossaryTerm>> GetArchivedByStableIdAsync(Guid stableId)
        {
            return _db.ArchivedGlossaryTerms
                .Where(a => a.StableId == stableId)
                .OrderByDescending(a => a.Version)
                .ToListAsync();
        }

        public Task<ArchivedGlossaryTerm?> GetArchivedVersionAsync(Guid stableId, int version)
        {
            return _db.ArchivedGlossaryTerms
                .FirstOrDefaultAsync(a => a.StableId == stableId && a.Version == version);
        }

        public async Task<int> GetLatestVersionAsync(Guid stableId)
        {
            var latestArchived = await _db.ArchivedGlossaryTerms
                .Where(x => x.StableId == stableId)
                .MaxAsync(x => (int?)x.Version) ?? 0;

            var latestActive = await _db.GlossaryTerms
                .Where(x => x.StableId == stableId)
                .MaxAsync(x => (int?)x.Version) ?? 0;

            return Math.Max(latestArchived, latestActive);
        }

        public void AddActiveTerm(GlossaryTerm term)
        {
            _db.GlossaryTerms.Add(term);
        }

        public void AddDraftTerm(GlossaryTerm term)
        {
            _db.GlossaryTerms.Add(term);
        }

        public void RemoveActiveTerm(GlossaryTerm term)
        {
            _db.GlossaryTerms.Remove(term);
        }

        public void AddArchivedTerm(ArchivedGlossaryTerm term)
        {
            _db.ArchivedGlossaryTerms.Add(term);
        }
        public Task<List<User>> GetAllUsersAsync()
        {
            return _db.Users.ToListAsync();
        }

        public void UpdateArchivedTerm(ArchivedGlossaryTerm term)
        {
            _db.ArchivedGlossaryTerms.Update(term);
        }

        public async Task<bool> CreateAsync(GlossaryTerm term)
        {
            _db.GlossaryTerms.Add(term);
            return await SaveChangesAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
