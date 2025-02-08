using RepoLayer.Entity;

namespace RepoLayer.Interfaces
{
    public interface INoteRL
    {void AddNote(Note note);
        Note GetNoteById(int noteId);
        IEnumerable<Note> GetAllNotes();
        void UpdateNote(Note note);
        void DeleteNote(int noteId);

        // Label Methods
        void AddLabel(Label label);
        void AddLabelToNote(int noteId, int labelId);
        void RemoveLabelFromNote(int noteId, int labelId);
        IEnumerable<Label> GetLabelsByNoteId(int noteId);
        IEnumerable<Label> GetAllLabels();
    }
}