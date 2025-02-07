using DataAccessLayer.Models;

namespace DataAccessLayer.Interfaces
{
    public interface INoteDl
    {
        void AddNote(Note note);
        Note GetNoteById(int noteId);
        IEnumerable<Note> GetAllNotes();
        void UpdateNote(Note note);
        void DeleteNote(int noteId);
    }
}