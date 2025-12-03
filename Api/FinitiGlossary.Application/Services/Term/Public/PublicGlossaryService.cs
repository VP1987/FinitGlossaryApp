using FinitiGlossary.Application.DTOs.Term.Public;
using FinitiGlossary.Application.Interfaces.Repositories.Term.Public;
using FinitiGlossary.Application.Interfaces.Term.Public;

namespace FinitiGlossary.Application.Services.Term.Public
{
    public class PublicGlossaryService : IPublicGlossaryService
    {
        private readonly IPublicGlossaryRepository _repo;

        public PublicGlossaryService(IPublicGlossaryRepository repo)
        {
            _repo = repo;
        }

        public async Task<PublicTermListResult> GetAllAsync(PublicTermQuery query)
        {
            return await _repo.GetAllAsync(query);
        }

        public async Task<PublicGlossaryDetail?> GetTermByIdAsync(int id)
        {
            try
            {
                return await _repo.GetTermByIdAsync(id);
            }
            catch
            {
                throw new InvalidOperationException($"Failed to find term with id:{id}.");
            }
        }
    }
}
