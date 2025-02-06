using BusinessLogicLayer.Interfaces;
using BusinessLogicLayer.Services;
using DataAccessLayer.Entity;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PresentationLayer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly AuthService _authService;

        public NoteController(INoteService noteService, AuthService authService)
        {
            _noteService = noteService;
            _authService = authService;
        }

        // Extract userId from token
        private int GetUserIdFromToken()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userClaims = _authService.DecodeJwtToken(token);

            if (userClaims.TryGetValue("UserId", out string userIdString) && int.TryParse(userIdString, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Invalid token.");
        }

        // Create Note
        [HttpPost("create")]
        [Authorize]
        public IActionResult Create([FromBody] NoteDto noteDto)
        {
            if (noteDto == null || string.IsNullOrEmpty(noteDto.Title) || string.IsNullOrEmpty(noteDto.Description))
            {
                return BadRequest(new { Message = "Title and description are required.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();

                var note = new Note
                {
                    Title = noteDto.Title,
                    Description = noteDto.Description,
                    CreatedBy = userId
                };

                _noteService.AddNote(note, userId);

                return Ok(new { Message = "Note created successfully.", Success = true, Data = noteDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }

        // Get All Notes of Logged-in User
        [HttpGet]
        [Authorize]
        public IActionResult GetAllNotes()
        {
            try
            {
                int userId = GetUserIdFromToken();
                var notes = _noteService.GetAllNotes().Where(n => n.CreatedBy == userId);

                return Ok(new { Message = "Notes retrieved successfully.", Success = true, Data = notes });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }

        // Get Note by ID (only if it belongs to the logged-in user)
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetNote(int id)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var note = _noteService.GetNoteById(id);

                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                return Ok(new { Message = "Note retrieved successfully.", Success = true, Data = note });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }

        // Update Note (only if it belongs to the logged-in user)
        [HttpPut("update/{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] NoteDto noteDto)
        {
            if (noteDto == null || string.IsNullOrEmpty(noteDto.Title) || string.IsNullOrEmpty(noteDto.Description))
            {
                return BadRequest(new { Message = "Title and description are required.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                var note = _noteService.GetNoteById(id);

                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                note.Title = noteDto.Title;
                note.Description = noteDto.Description;
                _noteService.EditNote(note);

                return Ok(new { Message = "Note updated successfully.", Success = true, Data = noteDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }

        // Delete Note (only if it belongs to the logged-in user)
        [HttpDelete("delete/{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var note = _noteService.GetNoteById(id);

                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                _noteService.TrashNote(id);
                return Ok(new { Message = "Note deleted successfully.", Success = true });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }
    }
}
