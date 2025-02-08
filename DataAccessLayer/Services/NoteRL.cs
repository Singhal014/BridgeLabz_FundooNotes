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
            return _context.Notes.Include(n => n.Labels).FirstOrDefault(n => n.Id == noteId);
        }

        public IEnumerable<Note> GetAllNotes()
        {
            return _context.Notes.Include(n => n.Labels).ToList();
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
    }
}
