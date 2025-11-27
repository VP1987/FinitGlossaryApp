using FinitiGlossary.Domain.Entities.Terms;

namespace FinitiGlossary.Application.DTOs.Response;
public record ArchiveResult(string Message);
public record PublishResult(string Message);
public record DeleteResult(string Message);
public record UpdateResult(string Message);
public record CreateResult(string Message, GlossaryTerm Term);
public record RestoreResult(bool Restored, string Message, Guid StableId);
