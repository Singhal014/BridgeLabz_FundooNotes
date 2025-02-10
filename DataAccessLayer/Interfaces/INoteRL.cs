using RepoLayer.Entity;
using System.Collections.Generic;

namespace RepoLayer.Interfaces
{
    public interface INoteRL
    {
        void AddNote(Note note);
        Note GetNoteById(int noteId);
        IEnumerable<Note> GetAllNotes(int userId);
        void UpdateNote(Note note, int userId);
        IEnumerable<Note> GetTrashedNotes(int userId);
        IEnumerable<Note> GetArchivedNotes(int userId);
        void DeleteNote(int noteId, int userId); 

        // Label Methods
        void AddLabel(Label label);
        void AddLabelToNote(int noteId, int labelId);
        bool DeleteLabel(int labelId, int userId);
        void RemoveLabelFromNote(int noteId, int labelId);
        IEnumerable<Label> GetLabelsByNoteId(int noteId);
        IEnumerable<Label> GetAllLabels();

        // Collaborator Methods
        void AddCollaborator(int noteId, int userId);
        void RemoveCollaborator(int noteId, int userId);
        IEnumerable<User> GetCollaboratorsByNoteId(int noteId);
    }
}
