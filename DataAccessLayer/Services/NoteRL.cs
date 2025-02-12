using ModelLayer.Models;
using DataAccessLayer.Context;
using RepoLayer.Interfaces;
using RepoLayer.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace RepoLayer.Services
{
    public class NoteRL : INoteRL
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public NoteRL(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Add a Note
        public void AddNote(Note note)
        {
            _context.Notes.Add(note);
            _context.SaveChanges();
            InvalidateCache($"notes_{note.CreatedBy}");
        }

        // Get a Note by ID
        public Note GetNoteById(int noteId)
        {
            var cacheKey = $"note_{noteId}";
            var cachedNote = _cache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cachedNote))
            {
                return JsonConvert.DeserializeObject<Note>(cachedNote);
            }

            var note = _context.Notes
                .Include(n => n.Labels)
                .Include(n => n.Collaborators)
                .FirstOrDefault(n => n.Id == noteId);

            if (note != null)
            {
                _cache.SetString(cacheKey, JsonConvert.SerializeObject(note));
            }

            return note;
        }

        // Get All Notes
        public IEnumerable<Note> GetAllNotes(int userId)
        {
            var cacheKey = $"notes_{userId}";
            var cachedNotes = _cache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cachedNotes))
            {
                return JsonConvert.DeserializeObject<IEnumerable<Note>>(cachedNotes);
            }

            var notes = _context.Notes
                .Where(n => n.CreatedBy == userId && !n.IsTrashed && !n.IsArchived)
                .Include(n => n.Labels)
                .Include(n => n.Collaborators)
                .AsSplitQuery()
                .ToList();

            _cache.SetString(cacheKey, JsonConvert.SerializeObject(notes));

            return notes;
        }

        // Update a Note
        public void UpdateNote(Note note, int userId)
        {
            var existingNote = _context.Notes
                .Include(n => n.Collaborators)
                .FirstOrDefault(n => n.Id == note.Id);

            if (existingNote != null &&
                (existingNote.CreatedBy == userId || existingNote.Collaborators.Any(c => c.Id == userId)))
            {
                existingNote.Title = note.Title;
                existingNote.Description = note.Description;
                existingNote.IsArchived = note.IsArchived;
                existingNote.IsTrashed = note.IsTrashed;
                _context.SaveChanges();
                InvalidateCache($"notes_{userId}");
                InvalidateCache($"note_{note.Id}");
            }
        }

        // Get Trashed Notes
        public IEnumerable<Note> GetTrashedNotes(int userId)
        {
            return _context.Notes
                .Where(n => n.CreatedBy == userId && n.IsTrashed)
                .Include(n => n.Labels)
                .Include(n => n.Collaborators)
                .AsSplitQuery()
                .ToList();
        }

        // Delete a Note
        public void DeleteNote(int noteId, int userId)
        {
            var note = _context.Notes
                .Include(n => n.Collaborators)
                .FirstOrDefault(n => n.Id == noteId);

            if (note != null &&
                (note.CreatedBy == userId || note.Collaborators.Any(c => c.Id == userId)))
            {
                _context.Notes.Remove(note);
                _context.SaveChanges();
                InvalidateCache($"notes_{userId}");
                InvalidateCache($"note_{noteId}");
            }
        }

        // Get Archived Notes
        public IEnumerable<Note> GetArchivedNotes(int userId)
        {
            return _context.Notes
                .Where(n => (n.CreatedBy == userId || n.Collaborators.Any(c => c.Id == userId)) && n.IsArchived)
                .Include(n => n.Labels)
                .Include(n => n.Collaborators)
                .AsSplitQuery()
                .ToList();
        }

        // Label Methods
        public void AddLabel(Label label)
        {
            _context.Labels.Add(label);
            _context.SaveChanges();
            InvalidateCache($"labels_{label.CreatedBy}");
        }

        public void AddLabelToNote(int noteId, int labelId)
        {
            var note = _context.Notes.Include(n => n.Labels).FirstOrDefault(n => n.Id == noteId);
            var label = _context.Labels.FirstOrDefault(l => l.Id == labelId);

            if (note != null && label != null)
            {
                note.Labels.Add(label);
                _context.SaveChanges();
                InvalidateCache($"note_{noteId}");
                InvalidateCache($"notes_{note.CreatedBy}");
            }
        }

        public bool DeleteLabel(int labelId, int userId)
        {
            var label = _context.Labels.FirstOrDefault(l => l.Id == labelId && l.CreatedBy == userId);
            if (label == null) return false;

            _context.Labels.Remove(label);
            _context.SaveChanges();
            InvalidateCache($"labels_{userId}");
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
                InvalidateCache($"note_{noteId}");
                InvalidateCache($"notes_{note.CreatedBy}");
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

        // Collaborator Methods
        public void AddCollaborator(int noteId, int userId)
        {
            var note = _context.Notes.Include(n => n.Collaborators).FirstOrDefault(n => n.Id == noteId);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (note != null && user != null)
            {
                note.Collaborators.Add(user);
                _context.SaveChanges();
                InvalidateCache($"note_{noteId}");
                InvalidateCache($"notes_{note.CreatedBy}");
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
                InvalidateCache($"note_{noteId}");
                InvalidateCache($"notes_{note.CreatedBy}");
            }
        }

        public IEnumerable<User> GetCollaboratorsByNoteId(int noteId)
        {
            return _context.Notes
                .Where(n => n.Id == noteId)
                .SelectMany(n => n.Collaborators)
                .ToList();
        }

        // Invalidate Cache Helper Method
        private void InvalidateCache(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }
    }
}
