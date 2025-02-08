using ModelLayer.Models;
using System.Collections.Generic;
using RepoLayer.Entity;

namespace BusinessLayer.Interfaces
{
    public interface INoteBL
    {
        void AddNote(Note note, int userId);
        Note GetNoteById(int noteId);
        IEnumerable<Note> GetAllNotes();
        void EditNote(Note note);
        void TrashNote(int noteId);

        void AddLabel(Label label);
        void AddLabelToNote(int noteId, int labelId);
        void RemoveLabelFromNote(int noteId, int labelId);
        IEnumerable<Label> GetLabelsByNoteId(int noteId);
        IEnumerable<Label> GetAllLabels();
    }
}
