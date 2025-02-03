using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FundoonoteController : ControllerBase
    {
        private readonly IFundoonoteService _fundoonoteService;

        public FundoonoteController(IFundoonoteService fundoonoteService)
        {
            _fundoonoteService = fundoonoteService;
        }

        // POST: api/Fundoonote/create
        [HttpPost("create")]
        public IActionResult Create([FromBody] Fundoonote fundoonote)
        {
            if (fundoonote == null || string.IsNullOrEmpty(fundoonote.Title) || string.IsNullOrEmpty(fundoonote.Description))
            {
                return BadRequest(new { Message = "Title and description are required.", Success = false });
            }

            fundoonote.CreatedAt = DateTime.Now; // Set creation date
            _fundoonoteService.AddFundoonote(fundoonote);

            return Ok(new { Message = "Fundoonote created successfully.", Success = true, Data = fundoonote });
        }

        // GET: api/Fundoonote/{id}
        [HttpGet("{id}")]
        public IActionResult GetFundoonote(int id)
        {
            var fundoonote = _fundoonoteService.GetFundoonoteById(id);
            if (fundoonote == null)
            {
                return NotFound(new { Message = "Fundoonote not found.", Success = false });
            }

            return Ok(new { Message = "Fundoonote retrieved successfully.", Success = true, Data = fundoonote });
        }

        // GET: api/Fundoonote
        [HttpGet]
        public IActionResult GetAllFundoonotes()
        {
            var fundoonotes = _fundoonoteService.GetAllFundoonotes();
            return Ok(new { Message = "Fundoonotes retrieved successfully.", Success = true, Data = fundoonotes });
        }

        // PUT: api/Fundoonote/{id}
        [HttpPut("{id}")]
        public IActionResult EditFundoonote(int id, [FromBody] Fundoonote fundoonote)
        {
            if (fundoonote == null || id != fundoonote.Id)
            {
                return BadRequest(new { Message = "Invalid fundoonote data.", Success = false });
            }

            _fundoonoteService.EditFundoonote(fundoonote);
            return Ok(new { Message = "Fundoonote edited successfully.", Success = true });
        }

        // DELETE: api/Fundoonote/{id}
        [HttpDelete("{id}")]
        public IActionResult TrashFundoonote(int id)
        {
            var fundoonote = _fundoonoteService.GetFundoonoteById(id);
            if (fundoonote == null)
            {
                return NotFound(new { Message = "Fundoonote not found.", Success = false });
            }

            _fundoonoteService.TrashFundoonote(id);
            return Ok(new { Message = "Fundoonote moved to trash.", Success = true });
        }
    }
}
