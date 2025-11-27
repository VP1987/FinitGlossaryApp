using FinitiGlossary.Application.DTOs.Term.Admin;
using FinitiGlossary.Domain.Entities.Terms;
using FinitiGlossary.Domain.Entities.Users;

namespace FinitiGlossary.Application.Interfaces.Agregator
{
    public interface IGlossaryAdminViewAggregator
    {
        List<GlossaryAdminTermDTO> AggregateAdminView(
            List<GlossaryTerm> active,
            List<ArchivedGlossaryTerm> archived,
            List<User> users,
            int? currentUserId,
            bool isAdmin);
    }
}
