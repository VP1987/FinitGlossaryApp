using FinitiGlossary.Application.DTOs.Term.Public;
using FinitiGlossary.Application.Interfaces.Term.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinitiGlossary.Api.Controllers.Public
{
    [ApiController]
    [Route("api/public/glossary")]
    public class PublicGlossaryController : ControllerBase
    {
        private readonly IPublicGlossaryService _service;

        public PublicGlossaryController(IPublicGlossaryService service)
        {
            _service = service;
        }

        [HttpGet("get-terms-list")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] PublicTermQuery query)
        {
            try
            {
                var result = await _service.GetAllAsync(query);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected server error occurred while retrieving terms." });
            }
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTermById(int id)
        {
            try
            {
                var result = await _service.GetTermByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new { message = $"Term with ID {id} not found." });
                }

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = $"An unexpected server error occurred while retrieving term ID {id}." });
            }
        }
    }
}
