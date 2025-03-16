using BusinessLayer.Interfaces;
using BusinessLogicLayer.Interfaces;
using RepoLayer.Interfaces;
using System.Collections.Generic;
using RepoLayer.Entity;

public class NoteBL : INoteBL
{
    private readonly INoteRL _noteRepository;
    private readonly IUserBL _userService;

    public NoteBL(INoteRL noteRepository, IUserBL userService)
    {
        _noteRepository = noteRepository;
        _userService = userService;
    }

    public void AddNote(Note note, int userId)
    {
        note.CreatedBy = userId;
        _noteRepository.AddNote(note);
    }

    public Note GetNoteById(int noteId)
    {
        return _noteRepository.GetNoteById(noteId);
    }

    public IEnumerable<Note> GetAllNotes(int userId)
    {
        return _noteRepository.GetAllNotes(userId);
    }


    public void EditNote(Note note, int userId)
    {
        _noteRepository.UpdateNote(note, userId);
    }


    public void ArchiveNote(int noteId, int userId)
    {
        var note = _noteRepository.GetNoteById(noteId);
        if (note != null && note.CreatedBy == userId)
        {
            note.IsArchived = true;
            _noteRepository.UpdateNote(note, userId);
        }
    }


    public void UnarchiveNote(int noteId, int userId)
    {
        var note = _noteRepository.GetNoteById(noteId);
        if (note != null && note.CreatedBy == userId)
        {
            if (note.IsTrashed)
            {
                throw new InvalidOperationException("Cannot unarchive a trashed note.");
            }

            note.IsArchived = false;
            _noteRepository.UpdateNote(note, userId);
        }
    }


    public IEnumerable<Note> GetArchivedNotes(int userId)
    {
        return _noteRepository.GetArchivedNotes(userId);
    }



    public void TrashNote(int noteId, int userId)
    {
        var note = _noteRepository.GetNoteById(noteId);
        if (note != null && note.CreatedBy == userId)
        {
            if (note.IsArchived)
            {
                note.IsArchived = false; 
            }

            if (!note.IsTrashed)
            {
                note.IsTrashed = true;
                _noteRepository.UpdateNote(note, userId);
            }
            else
            {
                _noteRepository.DeleteNote(noteId, userId);
            }
        }
    }

    public IEnumerable<Note> GetTrashedNotes(int userId)
    {
        return _noteRepository.GetTrashedNotes(userId);
    }

    public void RestoreNote(int noteId, int userId)
    {
        var note = _noteRepository.GetNoteById(noteId);
        if (note != null && note.CreatedBy == userId)
        {
            note.IsTrashed = false;
            _noteRepository.UpdateNote(note, userId);
        }
    }

    public void DeleteNotePermanently(int noteId, int userId)
    {
        try
        {
            var note = _noteRepository.GetNoteById(noteId);
            if (note != null && note.CreatedBy == userId)
            {
                if (!note.IsTrashed)
                {
                    Console.WriteLine("Cannot delete a note that is not in the trash.");
                    return; 
                }

                _noteRepository.DeleteNote(noteId, userId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting note: {ex.Message}");
        }
    }

    public int GetUserIdFromToken(string token)
    {
        return _userService.GetUserIdFromToken(token);
    }

    public void AddLabel(Label label)
    {
        _noteRepository.AddLabel(label);
    }

    public void AddLabelToNote(int noteId, int labelId)
    {
        _noteRepository.AddLabelToNote(noteId, labelId);
    }

    public bool DeleteLabel(int labelId, int userId)
    {
        return _noteRepository.DeleteLabel(labelId, userId);
    }

    public void RemoveLabelFromNote(int noteId, int labelId)
    {
        _noteRepository.RemoveLabelFromNote(noteId, labelId);
    }

    public IEnumerable<Label> GetLabelsByNoteId(int noteId)
    {
        return _noteRepository.GetLabelsByNoteId(noteId);
    }
    public IEnumerable<Label> GetAllLabels()
    {
        return _noteRepository.GetAllLabels();
    }

    public void InviteCollaborator(int noteId, string email)
    {
        var user = _userService.GetUserByEmail(email);
        if (user == null)
        {
            throw new InvalidOperationException("User with this email is not registered.");
        }

        var note = _noteRepository.GetNoteById(noteId);
        if (note == null)
        {
            throw new InvalidOperationException("Note not found.");
        }

        _noteRepository.AddCollaborator(noteId, user.Id);
    }

    public void RemoveCollaborator(int noteId, string email)
    {
        var user = _userService.GetUserByEmail(email);
        if (user == null)
        {
            throw new InvalidOperationException("User with this email is not registered.");
        }

        var note = _noteRepository.GetNoteById(noteId);
        if (note == null)
        {
            throw new InvalidOperationException("Note not found.");
        }

        _noteRepository.RemoveCollaborator(noteId, user.Id);
    }

    public IEnumerable<User> GetCollaboratorsByNoteId(int noteId)
    {
        var note = _noteRepository.GetNoteById(noteId);
        if (note == null)
        {
            throw new InvalidOperationException("Note not found.");
        }

        return _noteRepository.GetCollaboratorsByNoteId(noteId);
    }
}