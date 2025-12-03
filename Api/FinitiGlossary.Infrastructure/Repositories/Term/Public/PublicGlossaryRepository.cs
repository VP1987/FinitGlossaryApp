using FinitiGlossary.Application.DTOs.Term.Public;
using FinitiGlossary.Application.Interfaces.Repositories.Term.Public;
using FinitiGlossary.Domain.Entities.Terms.Status;
using FinitiGlossary.Infrastructure.DAL;
using Microsoft.EntityFrameworkCore;

namespace FinitiGlossary.Infrastructure.Repositories.Term.Public
{
    public class PublicGlossaryRepository : IPublicGlossaryRepository
    {
        private readonly AppDbContext _db;

        public PublicGlossaryRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PublicTermListResult> GetAllAsync(PublicTermQuery request)
        {
            var latestVersionsQuery =
                from t in _db.GlossaryTerms
                where t.Status == TermStatus.Published
                group t by t.StableId into g
                select new
                {
                    StableId = g.Key,
                    LatestCreatedAt = g.Max(x => x.CreatedAt)
                };

            var query =
                from t in _db.GlossaryTerms
                join lv in latestVersionsQuery
                    on new { t.StableId, t.CreatedAt }
                    equals new { lv.StableId, CreatedAt = lv.LatestCreatedAt }
                where t.Status == TermStatus.Published
                select t;

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string q = request.Search.Trim().ToLower();
                query = query.Where(t =>
                    t.Term.ToLower().Contains(q) ||
                    t.Definition.ToLower().Contains(q));
            }

            query = request.Sort switch
            {
                "dateAsc" => query.OrderBy(t => t.CreatedAt),
                "dateDesc" => query.OrderByDescending(t => t.CreatedAt),
                "az" => query.OrderBy(t => t.Term),
                "za" => query.OrderByDescending(t => t.Term),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip(request.Offset)
                .Take(request.Limit)
                .Select(t => new GlossaryPublicTermDTO(
                    t.Id,
                    t.StableId,
                    t.Term,
                    t.Definition,
                    t.Version,
                    (int)t.Status,
                    t.CreatedAt,
                    t.CreatedById
                ))
                .ToListAsync();

            var meta = new PublicTermListMeta(
                Offset: request.Offset,
                Limit: request.Limit,
                Total: totalCount,
                HasMore: request.Offset + request.Limit < totalCount,
                Sort: request.Sort,
                Search: request.Search
            );

            return new PublicTermListResult(meta, items);
        }

        public async Task<PublicGlossaryDetail?> GetTermByIdAsync(int id)
        {
            var result = await _db.GlossaryTerms
                .Where(t => t.Id == id && t.Status == TermStatus.Published)
                .Select(t => new PublicGlossaryDetail(
                    t.Id,
                    t.Term,
                    t.Definition,
                    t.CreatedAt,
                    _db.Users
                        .Where(u => u.Id == t.CreatedById)
                        .Select(u => u.Username)
                        .FirstOrDefault() ?? "Unknown"
                ))
                .FirstOrDefaultAsync();

            return result;
        }
    }
}
