using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.DTOs.Term.Admin;
using FinitiGlossary.Application.Interfaces.Term.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/glossary")]
[Authorize]
public class AdminGlossaryController : ControllerBase
{
    private readonly IAdminGlossaryService _service;

    public AdminGlossaryController(IAdminGlossaryService service)
    {
        _service = service;
    }

    [HttpGet("get-terms-list")]
    public async Task<IActionResult> GetTermList([FromQuery] AdminTermQuery query)
    {
        try
        {
            var result = await _service.GetAdminTermListAsync(User, query);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPost("create-term")]
    public async Task<IActionResult> CreateTerm(CreateGlossaryRequest request)
    {
        try
        {
            var result = await _service.CreateTermAsync(request, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPost("publish-term/{id:int}")]
    public async Task<IActionResult> PublishTerm(int id)
    {
        try
        {
            var result = await _service.PublishTermAsync(id, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPut("update-term/{id:int}")]
    public async Task<IActionResult> UpdateTerm(int id, UpdateGlossaryRequest request)
    {
        try
        {
            var result = await _service.UpdateTermAsync(id, request, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPost("archive-term/{id:int}")]
    public async Task<IActionResult> ArchiveTerm(int id)
    {
        try
        {
            var result = await _service.ArchiveTermAsync(id, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPost("restore-term/{stableId:guid}/{version:int}")]
    public async Task<IActionResult> RestoreTerm(Guid stableId, int version)
    {
        try
        {
            var result = await _service.RestoreTermVersionAsync(stableId, version, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpGet("get-term-history/{stableId:guid}")]
    public async Task<IActionResult> GetTermHistory(Guid stableId)
    {
        try
        {
            var result = await _service.GetTermHistoryAsync(stableId, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpDelete("delete-term/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteTermAsync(id, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }
}
