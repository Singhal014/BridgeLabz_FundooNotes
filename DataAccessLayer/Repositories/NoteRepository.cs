using DataAccessLayer.Models;
using DataAccessLayer.Context;
using DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly ApplicationDbContext _context;

        public NoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddNote(Note note)
        {
            _context.Notes.Add(note);
            _context.SaveChanges();
        }

        public Note GetNoteById(int noteId)
        {
            return _context.Notes.FirstOrDefault(n => n.Id == noteId);
        }

        public IEnumerable<Note> GetAllNotes()
        {
            return _context.Notes.ToList();
        }

        public void UpdateNote(Note note)
        {
            _context.Notes.Update(note);
            _context.SaveChanges();
        }

        public void DeleteNote(int noteId)
        {
            var note = _context.Notes.FirstOrDefault(n => n.Id == noteId);
            if (note != null)
            {
                _context.Notes.Remove(note);
                _context.SaveChanges();
            }
        }
    }
}
