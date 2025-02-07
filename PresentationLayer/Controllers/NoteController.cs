using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace PresentationLayer.Controllers
{
    [Route("api/note")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] NoteModel noteDto)
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

        [HttpGet("getnotes")]
        [Authorize]
        public IActionResult GetAllNotes()
        {
            try
            {
                int userId = GetUserIdFromToken();
                var notes = _noteService.GetAllNotes()?.Where(n => n.CreatedBy == userId) ?? new List<Note>();

                return Ok(new { Message = "Notes retrieved successfully.", Success = true, Data = notes });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }

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

        [HttpPut("update/{id}")]
        [Authorize]
        public IActionResult Update([FromRoute] int id, [FromBody] NoteModel noteDto)
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

        [HttpDelete("delete/{id}")]
        [Authorize]
        public IActionResult Delete([FromRoute] int id)
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

        private int GetUserIdFromToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                throw new UnauthorizedAccessException("Invalid or missing token.");
            }

            var token = authHeader.Replace("Bearer ", "");
            return _noteService.GetUserIdFromToken(token);
        }
    }
}
