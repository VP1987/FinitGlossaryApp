using FinitiGlossary.Application.DTOs.Term.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Interfaces.Agregator
{
    public interface IGlossaryAdminViewHistoryAggregator
    {
        List<GlossaryAdminTermDTO> AggregateHistoryView(
            List<GlossaryTerm> active,
            List<ArchivedGlossaryTerm> archived,
            List<User> users);
    }
}
