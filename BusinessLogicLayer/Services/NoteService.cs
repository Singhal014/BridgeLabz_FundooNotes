using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;

    public NoteService(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public void AddNote(Note note, int userId)
    {
        note.CreatedBy = userId; // Set the CreatedBy field from the token
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
}
