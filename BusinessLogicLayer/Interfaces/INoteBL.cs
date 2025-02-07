using DataAccessLayer.Models;
using System.Collections.Generic;

namespace BusinessLogicLayer.Interfaces
{
    public interface INoteService
    {
        void AddNote(Note note, int userId);
        Note GetNoteById(int noteId);
        IEnumerable<Note> GetAllNotes();
        void EditNote(Note note);
        void TrashNote(int noteId);

        int GetUserIdFromToken(string token);

    }
}
