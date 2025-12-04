namespace FinitiGlossary.Application.DTOs.Request
{
    public record EmailMessageRequest(string To, string Subject, string Body);
}
