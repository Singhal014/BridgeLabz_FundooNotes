using BusinessLayer.Interfaces;
using BusinessLogicLayer.Interfaces;
using ModelLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using RepoLayer.Entity;
using Microsoft.Extensions.Logging;
using System;

namespace PresentationLayer.Controllers
{
    [Route("notes")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly INoteBL _noteService;
        private readonly IUserBL _userService;
        private readonly ILogger<NoteController> _logger;

        public NoteController(INoteBL noteService, IUserBL userService, ILogger<NoteController> logger)
        {
            _noteService = noteService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Create([FromBody] NoteModel noteDto)
        {
            _logger.LogInformation("Create method called.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in Create method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            if (noteDto == null || string.IsNullOrEmpty(noteDto.Title) || string.IsNullOrEmpty(noteDto.Description))
            {
                _logger.LogWarning("Invalid input in Create method.");
                return BadRequest(new { Message = "Title and description are required.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Creating note for user ID: {userId}.");

                var note = new Note
                {
                    Title = noteDto.Title,
                    Description = noteDto.Description,
                    CreatedBy = userId
                };

                _noteService.AddNote(note, userId);

                _logger.LogInformation("Note created successfully.");
                return Ok(new { Message = "Note created successfully.", Success = true, Data = noteDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in Create method.");
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Create method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpGet]
        public IActionResult GetAllNotes()
        {
            _logger.LogInformation("GetAllNotes method called.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in GetAllNotes method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Retrieving all notes for user ID: {userId}.");

                var notes = _noteService.GetAllNotes(userId)?
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

                _logger.LogInformation("Notes retrieved successfully.");
                return Ok(new { Message = "Notes retrieved successfully.", Success = true, Data = notes });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in GetAllNotes method.");
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetAllNotes method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetNote(int id)
        {
            _logger.LogInformation($"GetNote method called for note ID: {id}.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in GetNote method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Retrieving note ID: {id} for user ID: {userId}.");

                var note = _noteService.GetNoteById(id);

                if (note == null || note.CreatedBy != userId)
                {
                    _logger.LogWarning($"Note ID: {id} not found or access denied for user ID: {userId}.");
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

                _logger.LogInformation($"Note ID: {id} retrieved successfully.");
                return Ok(new { Message = "Note retrieved successfully.", Success = true, Data = noteResponse });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in GetNote method.");
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetNote method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpPut("{id}")]
        public IActionResult Update([FromRoute] int id, [FromBody] NoteModel noteDto)
        {
            _logger.LogInformation($"Update method called for note ID: {id}.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in Update method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            if (noteDto == null || string.IsNullOrEmpty(noteDto.Title) || string.IsNullOrEmpty(noteDto.Description))
            {
                _logger.LogWarning("Invalid input in Update method.");
                return BadRequest(new { Message = "Title and description are required.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Updating note ID: {id} for user ID: {userId}.");

                var note = _noteService.GetNoteById(id);

                if (note == null || (note.CreatedBy != userId && !note.Collaborators.Any(c => c.Id == userId)))
                {
                    _logger.LogWarning($"Note ID: {id} not found or access denied for user ID: {userId}.");
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                note.Title = noteDto.Title;
                note.Description = noteDto.Description;
                _noteService.EditNote(note, userId);

                _logger.LogInformation($"Note ID: {id} updated successfully.");
                return Ok(new { Message = "Note updated successfully.", Success = true, Data = noteDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in Update method.");
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Update method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpPut("{id}/archive")]
        public IActionResult ArchiveNote([FromRoute] int id)
        {
            _logger.LogInformation($"ArchiveNote method called for note ID: {id}.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in ArchiveNote method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Archiving note ID: {id} for user ID: {userId}.");

                _noteService.ArchiveNote(id, userId);

                _logger.LogInformation($"Note ID: {id} archived successfully.");
                return Ok(new { Message = "Note archived successfully.", Success = true });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in ArchiveNote method.");
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in ArchiveNote method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpPut("{id}/unarchive")]
        public IActionResult UnarchiveNote([FromRoute] int id)
        {
            _logger.LogInformation($"UnarchiveNote method called for note ID: {id}.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in UnarchiveNote method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Unarchiving note ID: {id} for user ID: {userId}.");

                var note = _noteService.GetNoteById(id);

                if (note == null || (note.CreatedBy != userId && !note.Collaborators.Any(c => c.Id == userId)))
                {
                    _logger.LogWarning($"Note ID: {id} not found or access denied for user ID: {userId}.");
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                if (note.IsTrashed)
                {
                    _logger.LogWarning($"Cannot unarchive a trashed note ID: {id}.");
                    return BadRequest(new { Message = "Cannot unarchive a trashed note. Please restore it first.", Success = false });
                }

                _noteService.UnarchiveNote(id, userId);

                _logger.LogInformation($"Note ID: {id} unarchived successfully.");
                return Ok(new { Message = "Note unarchived successfully.", Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in UnarchiveNote method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpGet("archived")]
        public IActionResult GetArchivedNotes()
        {
            _logger.LogInformation("GetArchivedNotes method called.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in GetArchivedNotes method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Retrieving archived notes for user ID: {userId}.");

                var archivedNotes = _noteService.GetArchivedNotes(userId);

                _logger.LogInformation("Archived notes retrieved successfully.");
                return Ok(new { Message = "Archived notes retrieved successfully.", Data = archivedNotes, Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetArchivedNotes method.");
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message, Success = false });
            }
        }

        [HttpPut("{id}/trash")]
        public IActionResult TrashNote([FromRoute] int id)
        {
            _logger.LogInformation($"TrashNote method called for note ID: {id}.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in TrashNote method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Moving note ID: {id} to trash for user ID: {userId}.");

                var note = _noteService.GetNoteById(id);

                if (note == null || (note.CreatedBy != userId && !note.Collaborators.Any(c => c.Id == userId)))
                {
                    _logger.LogWarning($"Note ID: {id} not found or access denied for user ID: {userId}.");
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                if (note.IsArchived)
                {
                    _logger.LogWarning($"Cannot trash an archived note ID: {id}.");
                    return BadRequest(new { Message = "Cannot trash an archived note. Please unarchive it first.", Success = false });
                }

                if (note.IsTrashed)
                {
                    _logger.LogInformation($"Note ID: {id} is already trashed. Deleting permanently.");
                    _noteService.DeleteNotePermanently(id, userId);
                    return Ok(new { Message = "Note permanently deleted.", Success = true });
                }

                _noteService.TrashNote(id, userId);

                _logger.LogInformation($"Note ID: {id} moved to trash successfully.");
                return Ok(new { Message = "Note moved to trash.", Success = true });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in TrashNote method.");
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in TrashNote method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpPut("{id}/restore")]
        public IActionResult RestoreNote([FromRoute] int id)
        {
            _logger.LogInformation($"RestoreNote method called for note ID: {id}.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in RestoreNote method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Restoring note ID: {id} for user ID: {userId}.");

                var note = _noteService.GetNoteById(id);

                if (note == null || (note.CreatedBy != userId && !note.Collaborators.Any(c => c.Id == userId)))
                {
                    _logger.LogWarning($"Note ID: {id} not found or access denied for user ID: {userId}.");
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                if (!note.IsTrashed)
                {
                    _logger.LogWarning($"Note ID: {id} is not in the trash.");
                    return BadRequest(new { Message = "Note is not in the trash.", Success = false });
                }

                _noteService.RestoreNote(id, userId);

                _logger.LogInformation($"Note ID: {id} restored successfully.");
                return Ok(new { Message = "Note restored successfully.", Success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in RestoreNote method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNotePermanently([FromRoute] int id)
        {
            _logger.LogInformation($"DeleteNotePermanently method called for note ID: {id}.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in DeleteNotePermanently method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Deleting note ID: {id} permanently for user ID: {userId}.");

                var note = _noteService.GetNoteById(id);

                if (note == null || note.CreatedBy != userId)
                {
                    _logger.LogWarning($"Note ID: {id} not found or access denied for user ID: {userId}.");
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                if (!note.IsTrashed)
                {
                    _logger.LogWarning($"Cannot delete note ID: {id} that is not in the trash.");
                    return BadRequest(new { Message = "Cannot delete a note that is not in the trash.", Success = false });
                }

                _noteService.DeleteNotePermanently(id, userId);

                _logger.LogInformation($"Note ID: {id} deleted permanently.");
                return Ok(new { Message = "Note deleted permanently.", Success = true });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in DeleteNotePermanently method.");
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in DeleteNotePermanently method.");
                return StatusCode(500, new { Message = "An error occurred.", Success = false });
            }
        }

        [HttpGet("trashed")]
        public IActionResult GetTrashedNotes()
        {
            _logger.LogInformation("GetTrashedNotes method called.");

            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt in GetTrashedNotes method.");
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                _logger.LogInformation($"Retrieving trashed notes for user ID: {userId}.");

                var notes = _noteService.GetTrashedNotes(userId)
                    .Select(n => new NoteResponse
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Description = n.Description,
                        Color = n.Color,
                        IsArchived = n.IsArchived,
                        Labels = n.Labels?.Select(l => l.Name).ToList() ?? new List<string>()
                    })
                    .ToList();

                _logger.LogInformation("Trashed notes retrieved successfully.");
                return Ok(new { Message = "Trashed notes retrieved successfully.", Success = true, Data = notes });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access in GetTrashedNotes method.");
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetTrashedNotes method.");
                return StatusCode(500, new { Message = "An error occurred while retrieving trashed notes.", Success = false, Error = ex.Message });
            }
        }






        // Label API

        [HttpPost("label")]
        [Authorize]
        public IActionResult AddLabel([FromBody] LabelModel labelDto)
        {
            if (labelDto == null || string.IsNullOrEmpty(labelDto.LabelName) || labelDto.NoteId <= 0)
            {
                return BadRequest(new { Message = "Label name and NoteId are required.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();

                // Check if the note exists and belongs to the user
                var note = _noteService.GetNoteById(labelDto.NoteId);
                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                // Create and add label
                var label = new Label
                {
                    Name = labelDto.LabelName,
                    CreatedBy = userId
                };

                _noteService.AddLabel(label);

                // Associate label with note
                _noteService.AddLabelToNote(labelDto.NoteId, label.Id);

                return Ok(new { Message = "Label added to note successfully.", Success = true, Data = labelDto });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }


        [HttpGet("labels")]
        public IActionResult GetAllLabels()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

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
        public IActionResult AddLabelToNote([FromRoute] int noteId, [FromRoute] int labelId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

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
        public IActionResult RemoveLabelFromNote([FromRoute] int noteId, [FromRoute] int labelId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

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


        [HttpDelete("label/{labelId}")]
        public IActionResult DeleteLabel([FromRoute] int labelId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();

                // delete the label by its ID
                bool isDeleted = _noteService.DeleteLabel(labelId, userId);

                if (!isDeleted)
                {
                    return NotFound(new { Message = "Label not found or access denied.", Success = false });
                }

                return Ok(new { Message = "Label deleted successfully.", Success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the label.", Success = false, Error = ex.Message });
            }
        }


        [HttpPost("{noteId}/collaborators/invite")]
        public IActionResult InviteCollaborator([FromRoute] int noteId, [FromBody] CollaboratorModel request)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Message = "Email is required.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                var note = _noteService.GetNoteById(noteId);

                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                _noteService.InviteCollaborator(noteId, request.Email);
                return Ok(new { Message = "Collaborator invited successfully.", Success = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message, Success = false });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }

        [HttpDelete("{noteId}/collaborators/remove")]
        public IActionResult RemoveCollaborator([FromRoute] int noteId, [FromBody] CollaboratorModel request)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { Message = "Email is required.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                var note = _noteService.GetNoteById(noteId);

                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                _noteService.RemoveCollaborator(noteId, request.Email);
                return Ok(new { Message = "Collaborator removed successfully.", Success = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message, Success = false });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }

        [HttpGet("{noteId}/collaborators")]
        public IActionResult GetCollaborators([FromRoute] int noteId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "User is unauthorized.", Success = false });
            }

            try
            {
                int userId = GetUserIdFromToken();
                var note = _noteService.GetNoteById(noteId);

                if (note == null || note.CreatedBy != userId)
                {
                    return NotFound(new { Message = "Note not found or access denied.", Success = false });
                }

                var collaborators = _noteService.GetCollaboratorsByNoteId(noteId)
                    .Select(u => new
                    {
                        UserId = u.Id,
                        Email = u.Email
                    })
                    .ToList();

                return Ok(new { Message = "Collaborators retrieved successfully.", Success = true, Data = collaborators });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message, Success = false });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message, Success = false });
            }
        }

        private int GetUserIdFromToken()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                _logger.LogWarning("Authorization header is missing.");
                return 0;
            }

            var token = authHeader.Replace("Bearer ", "");
            return _userService.GetUserIdFromToken(token);
        }
    }
}