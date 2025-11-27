using FinitiGlossary.Application.DTOs.Term.Admin;
using FinitiGlossary.Application.Interfaces.Agregator;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Aggregators.AdminHistory
{
    public class GlossaryAdminViewHistoryAggregator : IGlossaryAdminViewHistoryAggregator
    {
        public List<GlossaryAdminTermDTO> AggregateHistoryView(
            List<GlossaryTerm> activeTerms,
            List<ArchivedGlossaryTerm> archivedTerms,
            List<User> allUsers)
        {
            var userNames = allUsers.ToDictionary(u => u.Id, u => u.Username);

            string ResolveUser(int? id) =>
    id.HasValue && userNames.TryGetValue(id.Value, out var name)
        ? name
        : "Unknown";


            bool AreIdentical(GlossaryTerm? active, ArchivedGlossaryTerm archived)
            {
                if (active == null) return false;

                return active.Term.Trim() == archived.Term.Trim() &&
                       active.Definition.Trim() == archived.Definition.Trim();
            }

            var result = new List<GlossaryAdminTermDTO>();

            GlossaryTerm? active = activeTerms.FirstOrDefault();

            if (active != null)
            {
                result.Add(new GlossaryAdminTermDTO
                {
                    Id = active.Id,
                    StableId = active.StableId,
                    Term = active.Term,
                    Definition = active.Definition,
                    Version = active.Version,
                    Status = 1,
                    CreatedOrArchivedAt = active.CreatedAt,
                    CreatedByName = ResolveUser(active.CreatedById),
                    ArchivedByName = null,
                    RestoredAt = null,
                    RestoredByName = null,
                    HasHistory = archivedTerms.Count > 0,
                    CanRestore = false
                });
            }

            foreach (var a in archivedTerms)
            {
                bool identical = AreIdentical(active, a);

                result.Add(new GlossaryAdminTermDTO
                {
                    Id = a.Id,
                    StableId = a.StableId,
                    Term = a.Term,
                    Definition = a.Definition,
                    Version = a.Version,
                    Status = 2,
                    CreatedOrArchivedAt = a.ArchivedAt,
                    CreatedByName = ResolveUser(a.CreatedById),
                    ArchivedByName = ResolveUser(a.ArchivedById),
                    RestoredAt = a.RestoredAt,
                    RestoredByName = ResolveUser(a.RestoredById),
                    HasHistory = true,
                    CanRestore = !identical
                });
            }

            return result
                .OrderByDescending(x => x.Version)
                .ToList();
        }
    }
}
