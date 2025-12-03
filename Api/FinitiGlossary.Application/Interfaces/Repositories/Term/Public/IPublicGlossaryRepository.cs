using FinitiGlossary.Application.DTOs.Term.Public;

namespace FinitiGlossary.Application.Interfaces.Repositories.Term.Public;

public interface IPublicGlossaryRepository
{
    Task<PublicTermListResult> GetAllAsync(PublicTermQuery query);
    Task<PublicGlossaryDetail?> GetTermByIdAsync(int id);
}
