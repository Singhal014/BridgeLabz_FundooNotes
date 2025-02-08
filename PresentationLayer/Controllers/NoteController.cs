using BusinessLayer.Interfaces;
using BusinessLogicLayer.Interfaces;
using ModelLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using RepoLayer.Entity;

namespace PresentationLayer.Controllers
{
    [Route("notes")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteBL _noteService;
        private readonly IUserBL _userService;

        public NoteController(INoteBL noteService, IUserBL userService)
        {
            _noteService = noteService;
            _userService = userService;
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
                var notes = _noteService.GetAllNotes()?
                    .Where(n => n.CreatedBy == userId)
                    .Select(n => new NoteResponse
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Description = n.Description,
                        Color = n.Color,
                        IsArchived = n.IsArchived,
                        Labels = n.Labels.Select(l => l.Name).ToList()
                    })
                    .ToList() ?? new List<NoteResponse>();

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

                var noteResponse = new NoteResponse
                {
                    Id = note.Id,
                    Title = note.Title,
                    Description = note.Description,
                    Color = note.Color,
                    IsArchived = note.IsArchived,
                    Labels = note.Labels.Select(l => l.Name).ToList()
                };

                return Ok(new { Message = "Note retrieved successfully.", Success = true, Data = noteResponse });
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

        // Label API

        [HttpPost("label")]
        [Authorize]
        public IActionResult AddLabel([FromBody] LabelModel labelDto)
        {
            if (labelDto == null || string.IsNullOrEmpty(labelDto.LabelName))
            {
                return BadRequest(new { Message = "Label name is required.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                var label = new Label
                {
                    Name = labelDto.LabelName,
                    CreatedBy = userId
                };

                _noteService.AddLabel(label);
                return Ok(new { Message = "Label added successfully.", Success = true, Data = labelDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }
        [HttpGet("labels")]
        [Authorize]
        public IActionResult GetAllLabels()
        {
            try
            {
                var labels = _noteService.GetAllLabels()
                    .Select(l => new
                    {
                        LabelId = l.Id,
                        Name = l.Name
                    })
                    .ToList();

                return Ok(new { Message = "Labels retrieved successfully.", Success = true, Data = labels });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving labels.", Success = false, Error = ex.Message });
            }
        }

        [HttpPost("{noteId}/label/{labelId}")]
        [Authorize]
        public IActionResult AddLabelToNote([FromRoute] int noteId, [FromRoute] int labelId)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var note = _noteService.GetNoteById(noteId);

                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                _noteService.AddLabelToNote(noteId, labelId);
                return Ok(new { Message = "Label added to note successfully.", Success = true });
            }
            catch
            {
                return StatusCode(500, new { Message = "Failed to add label to note.", Success = false });
            }
        }

        [HttpDelete("{noteId}/label/{labelId}")]
        [Authorize]
        public IActionResult RemoveLabelFromNote([FromRoute] int noteId, [FromRoute] int labelId)
        {
            try
            {
                int userId = GetUserIdFromToken();
                var note = _noteService.GetNoteById(noteId);

                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                _noteService.RemoveLabelFromNote(noteId, labelId);
                return Ok(new { Message = "Label removed from note successfully.", Success = true });
            }
            catch
            {
                return StatusCode(500, new { Message = "Failed to remove label from note.", Success = false });
            }
        }

      

        

        private int GetUserIdFromToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader)) return 0;

            var token = authHeader.Replace("Bearer ", "");
            return _userService.GetUserIdFromToken(token);
        }
    }
}
