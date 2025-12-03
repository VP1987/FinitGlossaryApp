using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.DTOs.Response;
using FinitiGlossary.Application.DTOs.Term.Admin;
using FinitiGlossary.Application.Interfaces.Agregator;
using FinitiGlossary.Application.Interfaces.Repositories.Term.Admin;
using FinitiGlossary.Application.Interfaces.Repositories.UserIRepo;
using FinitiGlossary.Application.Interfaces.Term.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Terms.Status;
using FinitiGlossary.Domain.Entities.Users;
using System.Security.Claims;

namespace FinitiGlossary.Application.Services.Term.Admin
{
    public class AdminGlossaryService : IAdminGlossaryService
    {
        private readonly IAdminGlossaryRepository _repo;
        private readonly IGlossaryAdminViewAggregator _viewAgregator;
        private readonly IGlossaryAdminViewHistoryAggregator _viewHistoryAggregator;
        private readonly IUserRepository _userRepo;

        public AdminGlossaryService(
            IAdminGlossaryRepository repo,
            IGlossaryAdminViewAggregator viewAgregator,
    IGlossaryAdminViewHistoryAggregator viewHistoryAggregator,
            IUserRepository userRepo)
        {
            _repo = repo;
            _viewAgregator = viewAgregator;
            _viewHistoryAggregator = viewHistoryAggregator;
            _userRepo = userRepo;
        }

        public async Task<AdminTermListResult> GetAdminTermListAsync(ClaimsPrincipal user, AdminTermQuery query)
        {
            var (dbUser, role, userId) = await ValidateUserAsync(user);
            bool isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

            var active = await _repo.GetActiveTermsForAdminViewAsync(new GetTermsAdminRequest(userId, role));
            var archived = await _repo.GetArchivedTermsForAdminViewAsync(new GetTermsAdminRequest(userId, role));
            var users = await _repo.GetAllUsersAsync();

            var aggregated = _viewAgregator.AggregateAdminView(active, archived, users, userId, isAdmin);

            var statusFilter = MapTabToStatus(query.Tab);
            if (statusFilter != null)
            {
                aggregated = aggregated.Where(x => x.Status == (int)statusFilter.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLower();
                aggregated = aggregated
                    .Where(x => x.Term.ToLower().Contains(s) || x.Definition.ToLower().Contains(s))
                    .ToList();
            }

            aggregated = query.Sort switch
            {
                "dateAsc" => aggregated.OrderBy(x => x.CreatedOrArchivedAt).ToList(),
                "dateDesc" => aggregated.OrderByDescending(x => x.CreatedOrArchivedAt).ToList(),
                "az" => aggregated.OrderBy(x => x.Term).ToList(),
                "za" => aggregated.OrderByDescending(x => x.Term).ToList(),
                _ => aggregated.OrderByDescending(x => x.CreatedOrArchivedAt).ToList(),
            };

            var total = aggregated.Count;
            var page = aggregated.Skip(query.Offset).Take(query.Limit).ToList();

            return new AdminTermListResult(
                new AdminTermListMeta(query.Offset, query.Limit, total, query.Offset + query.Limit < total, query.Sort, query.Search, query.Tab),
                page
            );
        }

        public async Task<ArchiveResult> ArchiveTermAsync(int id, ClaimsPrincipal user)
        {
            var (dbUser, role, userId) = await ValidateUserAsync(user);

            var termToArchive = await _repo.GetActiveByIdAsync(id);
            if (termToArchive == null)
            {
                throw new InvalidOperationException($"Term with ID {id} not found.");
            }

            var latestVersion = await _repo.GetLatestVersionAsync(termToArchive.StableId);

            var archived = new ArchivedGlossaryTerm
            {
                OriginalTermId = termToArchive.Id,
                StableId = termToArchive.StableId,
                Term = termToArchive.Term,
                Definition = termToArchive.Definition,
                ArchivedAt = DateTime.UtcNow,
                ArchivedById = userId,
                CreatedById = termToArchive.CreatedById,
                ChangeSummary = "Manual archive",
                Version = latestVersion + 1
            };

            _repo.AddArchivedTerm(archived);
            _repo.RemoveActiveTerm(termToArchive);

            if (!await _repo.SaveChangesAsync())
            {
                throw new InvalidOperationException("Failed to save changes.");
            }

            return new ArchiveResult("Term archived successfully.");
        }

        public async Task<RestoreResult> RestoreTermVersionAsync(Guid stableId, int version, ClaimsPrincipal user)
        {
            var (dbUser, role, userId) = await ValidateUserAsync(user);

            var archivedVersion = await _repo.GetArchivedVersionAsync(stableId, version);
            if (archivedVersion == null)
            {
                throw new InvalidOperationException("Requested version not found.");
            }

            var existingActive = await _repo.GetActiveByStableIdAsync(stableId);

            if (existingActive != null)
            {
                bool identical =
                    existingActive.Term.Trim() == archivedVersion.Term.Trim() &&
                    existingActive.Definition.Trim() == archivedVersion.Definition.Trim();

                if (identical)
                {
                    return new RestoreResult(false, "Identical version already active.", stableId);
                }

                var nextVersion = await _repo.GetLatestVersionAsync(stableId) + 1;

                var autoArchived = new ArchivedGlossaryTerm
                {
                    OriginalTermId = existingActive.Id,
                    StableId = existingActive.StableId,
                    Term = existingActive.Term,
                    Definition = existingActive.Definition,
                    ArchivedAt = DateTime.UtcNow,
                    ArchivedById = userId,
                    CreatedById = existingActive.CreatedById,
                    ChangeSummary = "Auto-archived before restore",
                    Version = nextVersion
                };

                _repo.AddArchivedTerm(autoArchived);
                _repo.RemoveActiveTerm(existingActive);
            }

            var restored = new GlossaryTerm
            {
                StableId = archivedVersion.StableId,
                Term = archivedVersion.Term,
                Definition = archivedVersion.Definition,
                CreatedAt = DateTime.UtcNow,
                CreatedById = archivedVersion.CreatedById ?? 0,
                Status = TermStatus.Published
            };

            archivedVersion.RestoredAt = DateTime.UtcNow;
            archivedVersion.RestoredById = userId;

            _repo.AddActiveTerm(restored);
            _repo.UpdateArchivedTerm(archivedVersion);

            if (!await _repo.SaveChangesAsync())
            {
                throw new InvalidOperationException("Failed to save changes.");
            }

            return new RestoreResult(true, $"Version {version} restored.", stableId);
        }

        public async Task<HistoryResult> GetTermHistoryAsync(Guid stableId, ClaimsPrincipal user)
        {
            await ValidateUserAsync(user);

            var active = await _repo.GetActiveByStableIdAsync(stableId);
            var archived = await _repo.GetArchivedByStableIdAsync(stableId);
            var allUsers = await _userRepo.GetAllUsersAsync();

            if (active == null && archived.Count == 0)
            {
                throw new InvalidOperationException("No history found.");
            }

            var activeList = active != null
                ? new List<GlossaryTerm> { active }
                : new List<GlossaryTerm>();

            var versions = _viewHistoryAggregator.AggregateHistoryView(
                activeList,
                archived,
                allUsers
            );

            return new HistoryResult(
                stableId,
                versions
            );
        }

        public async Task<CreateResult> CreateTermAsync(CreateGlossaryRequest request, ClaimsPrincipal user)
        {
            var (dbUser, role, userId) = await ValidateUserAsync(user);

            var draft = new GlossaryTerm
            {
                StableId = Guid.NewGuid(),
                Term = request.Term.Trim(),
                Definition = request.Definition.Trim(),
                Version = 1,
                Status = TermStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            if (!await _repo.CreateAsync(draft))
            {
                throw new InvalidOperationException("Failed to save changes.");
            }

            return new CreateResult("Draft created successfully.", draft);
        }

        public async Task<UpdateResult> UpdateTermAsync(int id, UpdateGlossaryRequest request, ClaimsPrincipal user)
        {
            var (dbUser, role, userId) = await ValidateUserAsync(user);

            var active = await _repo.GetActiveByIdAsync(id);
            if (active == null)
            {
                throw new InvalidOperationException("Term not found.");
            }

            var archivedVersions = await _repo.GetArchivedByStableIdAsync(active.StableId);

            bool identicalExists = archivedVersions.Any(a =>
                a.Term.Trim() == active.Term.Trim() &&
                a.Definition.Trim() == active.Definition.Trim()
            );

            if (!identicalExists)
            {
                var latestVersion = await _repo.GetLatestVersionAsync(active.StableId);

                var archived = new ArchivedGlossaryTerm
                {
                    OriginalTermId = active.Id,
                    StableId = active.StableId,
                    Term = active.Term,
                    Definition = active.Definition,
                    ArchivedAt = DateTime.UtcNow,
                    ArchivedById = userId,
                    CreatedById = active.CreatedById,
                    ChangeSummary = "Updated",
                    Version = latestVersion + 1
                };

                _repo.AddArchivedTerm(archived);
            }

            var updated = new GlossaryTerm
            {
                StableId = active.StableId,
                Term = request.Term,
                Definition = request.Definition,
                CreatedAt = DateTime.UtcNow,
                CreatedById = active.CreatedById,
                Status = TermStatus.Published
            };

            _repo.RemoveActiveTerm(active);
            _repo.AddActiveTerm(updated);

            if (!await _repo.SaveChangesAsync())
            {
                throw new InvalidOperationException("Failed to save changes.");
            }

            return new UpdateResult("Term updated successfully.");
        }

        public async Task<PublishResult> PublishTermAsync(int id, ClaimsPrincipal user)
        {
            await ValidateUserAsync(user);

            var term = await _repo.GetActiveByIdAsync(id);

            if (term == null)
            {
                throw new InvalidOperationException("Term not found.");
            }

            term.Status = TermStatus.Published;

            if (!await _repo.SaveChangesAsync())
            {
                throw new InvalidOperationException("Failed to save changes.");
            }

            return new PublishResult("Term published.");
        }

        public async Task<DeleteResult> DeleteTermAsync(int id, ClaimsPrincipal user)
        {
            await ValidateUserAsync(user);

            var term = await _repo.GetActiveByIdAsync(id);

            if (term == null)
            {
                throw new InvalidOperationException("Term not found.");
            }

            _repo.RemoveActiveTerm(term);

            if (!await _repo.SaveChangesAsync())
            {
                throw new InvalidOperationException("Failed to save changes.");
            }

            return new DeleteResult("Deleted.");
        }

        private TermStatus? MapTabToStatus(string tab)
        {
            return tab?.ToLower() switch
            {
                "draft" => TermStatus.Draft,
                "published" => TermStatus.Published,
                "archived" => TermStatus.Archived,
                _ => null
            };
        }

        private async Task<(User DbUser, string Role, int UserId)> ValidateUserAsync(ClaimsPrincipal user)
        {
            var userIdStr = user.FindFirst("id")?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user.");
            }

            var dbUser = await _userRepo.GetUserByIdAsync(userId);
            if (dbUser == null)
            {
                throw new UnauthorizedAccessException("Invalid user.");
            }

            var role = user.FindFirstValue(ClaimTypes.Role);

            return (dbUser, role, userId);
        }
    }
}
