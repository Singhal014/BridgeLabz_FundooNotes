
using RepoLayer.Entity;

namespace BusinessLayer.Interfaces
{
    public interface INoteBL
    {
        void AddNote(Note note, int userId);
        Note GetNoteById(int noteId);
        IEnumerable<Note> GetAllNotes(int userId);
        void EditNote(Note note);
        void TrashNote(int noteId);

        void AddLabel(Label label);
        void AddLabelToNote(int noteId, int labelId);
        bool DeleteLabel(int labelId, int userId);
        void RemoveLabelFromNote(int noteId, int labelId);
        IEnumerable<Label> GetLabelsByNoteId(int noteId);
        IEnumerable<Label> GetAllLabels();

        void InviteCollaborator(int noteId, string email);
        void RemoveCollaborator(int noteId, string email);
        IEnumerable<User> GetCollaboratorsByNoteId(int noteId);
    }
}
