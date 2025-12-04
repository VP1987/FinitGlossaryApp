using FinitiGlossary.Application.DTOs.Request;

namespace FinitiGlossary.Application.Interfaces.Auth.EMail
{
    public interface IEmailService
    {
        Task SendAsync(EmailMessageRequest request);
    }
}
