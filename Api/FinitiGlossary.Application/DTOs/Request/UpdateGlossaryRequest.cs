using FinitiGlossary.Domain.Entities.Terms.Status;
namespace FinitiGlossary.Application.DTOs.Request;
public record UpdateGlossaryRequest(string? Term, string? Definition, TermStatus? Status);
