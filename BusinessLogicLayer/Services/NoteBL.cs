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


    public void EditNote(Note note)
    {
        _noteRepository.UpdateNote(note);
    }

    public void TrashNote(int noteId)
    {
        _noteRepository.DeleteNote(noteId);
    }

    public int GetUserIdFromToken(string token)
    {
        return _userService.GetUserIdFromToken(token);
    }

    // Label Methods
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
        // Check if the email is registered
        var user = _userService.GetUserByEmail(email);
        if (user == null)
        {
            throw new InvalidOperationException("User with this email is not registered.");
        }

        // Check if the note exists
        var note = _noteRepository.GetNoteById(noteId);
        if (note == null)
        {
            throw new InvalidOperationException("Note not found.");
        }

        // Add collaborator to the note
        _noteRepository.AddCollaborator(noteId, user.Id);
    }

    public void RemoveCollaborator(int noteId, string email)
    {
        // Check if the email is registered
        var user = _userService.GetUserByEmail(email);
        if (user == null)
        {
            throw new InvalidOperationException("User with this email is not registered.");
        }

        // Check if the note exists
        var note = _noteRepository.GetNoteById(noteId);
        if (note == null)
        {
            throw new InvalidOperationException("Note not found.");
        }

        // Remove collaborator from the note
        _noteRepository.RemoveCollaborator(noteId, user.Id);
    }

    public IEnumerable<User> GetCollaboratorsByNoteId(int noteId)
    {
        // Check if the note exists
        var note = _noteRepository.GetNoteById(noteId);
        if (note == null)
        {
            throw new InvalidOperationException("Note not found.");
        }

        // Get collaborators for the note
        return _noteRepository.GetCollaboratorsByNoteId(noteId);
    }
}
