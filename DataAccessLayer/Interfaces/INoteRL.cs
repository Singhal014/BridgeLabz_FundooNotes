using RepoLayer.Entity;

namespace RepoLayer.Interfaces
{
    public interface INoteRL
    {
        void AddNote(Note note);
        Note GetNoteById(int noteId);
        IEnumerable<Note> GetAllNotes(int userId); // Updated to accept userId
        void UpdateNote(Note note);
        void DeleteNote(int noteId);

        // Label Methods
        void AddLabel(Label label);
        void AddLabelToNote(int noteId, int labelId);
        bool DeleteLabel(int labelId, int userId);
        void RemoveLabelFromNote(int noteId, int labelId);
        IEnumerable<Label> GetLabelsByNoteId(int noteId);
        IEnumerable<Label> GetAllLabels();

        void AddCollaborator(int noteId, int userId);
        void RemoveCollaborator(int noteId, int userId);
        IEnumerable<User> GetCollaboratorsByNoteId(int noteId);
    }

}