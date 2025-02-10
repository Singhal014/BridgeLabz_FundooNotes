using ModelLayer.Models;
using DataAccessLayer.Context;
using RepoLayer.Interfaces;
using RepoLayer.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace RepoLayer.Services
{
    public class NoteRL : INoteRL
    {
        private readonly ApplicationDbContext _context;

        public NoteRL(ApplicationDbContext context)
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
            return _context.Notes
                .Include(n => n.Labels)
                .Include(n => n.Collaborators) 
                .FirstOrDefault(n => n.Id == noteId);
        }

        public IEnumerable<Note> GetAllNotes(int userId)
        {
            return _context.Notes
                .Include(n => n.Labels)
                .Include(n => n.Collaborators) 
                .Where(n => n.CreatedBy == userId || n.Collaborators.Any(c => c.Id == userId))
                .ToList();
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

        // Label Methods Implementation
        public void AddLabel(Label label)
        {
            _context.Labels.Add(label);
            _context.SaveChanges();
        }

        public void AddLabelToNote(int noteId, int labelId)
        {
            var note = _context.Notes.Include(n => n.Labels).FirstOrDefault(n => n.Id == noteId);
            var label = _context.Labels.FirstOrDefault(l => l.Id == labelId);

            if (note != null && label != null)
            {
                note.Labels.Add(label);
                _context.SaveChanges();
            }
        }

        public bool DeleteLabel(int labelId, int userId)
        {
            
            var label = _context.Labels.FirstOrDefault(l => l.Id == labelId && l.CreatedBy == userId);

            if (label == null)
            {
                return false; 
            }

            _context.Labels.Remove(label);
            _context.SaveChanges();
            return true;
        }


        public void RemoveLabelFromNote(int noteId, int labelId)
        {
            var note = _context.Notes.Include(n => n.Labels).FirstOrDefault(n => n.Id == noteId);
            var label = note?.Labels.FirstOrDefault(l => l.Id == labelId);

            if (note != null && label != null)
            {
                note.Labels.Remove(label);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Label> GetLabelsByNoteId(int noteId)
        {
            return _context.Notes
                .Where(n => n.Id == noteId)
                .SelectMany(n => n.Labels)
                .ToList();
        }
        public IEnumerable<Label> GetAllLabels()
        {
            return _context.Labels.ToList();
        }

        public void AddCollaborator(int noteId, int userId)
        {
            var note = _context.Notes.Include(n => n.Collaborators).FirstOrDefault(n => n.Id == noteId);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (note != null && user != null)
            {
                note.Collaborators.Add(user);
                _context.SaveChanges();
            }
        }

        public void RemoveCollaborator(int noteId, int userId)
        {
            var note = _context.Notes.Include(n => n.Collaborators).FirstOrDefault(n => n.Id == noteId);
            var user = note?.Collaborators.FirstOrDefault(u => u.Id == userId);

            if (note != null && user != null)
            {
                note.Collaborators.Remove(user);
                _context.SaveChanges();
            }
        }

        public IEnumerable<User> GetCollaboratorsByNoteId(int noteId)
        {
            return _context.Notes
                .Where(n => n.Id == noteId)
                .SelectMany(n => n.Collaborators)
                .ToList();
        }
    }
}
