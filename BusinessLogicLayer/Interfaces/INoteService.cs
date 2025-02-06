using DataAccessLayer.Models;

namespace BusinessLogicLayer.Interfaces
{
    public interface INoteService
    {
        void AddNote(Note note, int userId);  // Updated method to include userId
        Note GetNoteById(int noteId);
        IEnumerable<Note> GetAllNotes();
        void EditNote(Note note);
        void TrashNote(int noteId);
    }
}
