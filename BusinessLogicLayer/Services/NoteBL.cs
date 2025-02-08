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

    public IEnumerable<Note> GetAllNotes()
    {
        return _noteRepository.GetAllNotes();
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
}
