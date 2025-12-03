namespace FinitiGlossary.Application.Interfaces.Term.Public
{
    using FinitiGlossary.Application.DTOs.Term.Public;

    public interface IPublicGlossaryService
    {
        Task<PublicTermListResult> GetAllAsync(PublicTermQuery query);
        Task<PublicGlossaryDetail?> GetTermByIdAsync(int id);
    }
}
