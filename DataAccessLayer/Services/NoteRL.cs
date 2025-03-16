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

        public void AddNote(Note note)
        {
            _context.Notes.Add(note);
            _context.SaveChanges();

            var cacheKey = $"notes_{note.CreatedBy}";
            var cachedNotes = _cache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cachedNotes))
            {
                var notesList = JsonConvert.DeserializeObject<List<Note>>(cachedNotes);
                notesList.Add(note); 
                _cache.SetString(cacheKey, JsonConvert.SerializeObject(notesList));
            }
        }

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

        public IEnumerable<Note> GetAllNotes(int userId)
        {
            var cacheKey = $"notes_{userId}";
            var cachedNotes = _cache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(cachedNotes))
            {
                var notesList = JsonConvert.DeserializeObject<List<Note>>(cachedNotes);

                return notesList.Where(n => !n.IsTrashed && !n.IsArchived).ToList();
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

                _cache.SetString($"note_{note.Id}", JsonConvert.SerializeObject(existingNote));

                var cachedNotes = _cache.GetString($"notes_{userId}");
                if (!string.IsNullOrEmpty(cachedNotes))
                {
                    var notesList = JsonConvert.DeserializeObject<List<Note>>(cachedNotes);
                    var noteIndex = notesList.FindIndex(n => n.Id == note.Id);
                    if (noteIndex != -1)
                    {
                        notesList[noteIndex] = existingNote; 
                        _cache.SetString($"notes_{userId}", JsonConvert.SerializeObject(notesList));
                    }
                }
            }
        }

        public IEnumerable<Note> GetTrashedNotes(int userId)
        {
            return _context.Notes
                .Where(n => n.CreatedBy == userId && n.IsTrashed)
                .Include(n => n.Labels)
                .Include(n => n.Collaborators)
                .AsSplitQuery()
                .ToList();
        }

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

                InvalidateCache($"note_{noteId}");

                var notes = _context.Notes
                    .Where(n => n.CreatedBy == userId && !n.IsTrashed && !n.IsArchived)
                    .Include(n => n.Labels)
                    .Include(n => n.Collaborators)
                    .AsSplitQuery()
                    .ToList();

                _cache.SetString($"notes_{userId}", JsonConvert.SerializeObject(notes));
            }
        }


        public IEnumerable<Note> GetArchivedNotes(int userId)
        {
            return _context.Notes
                .Where(n => (n.CreatedBy == userId || n.Collaborators.Any(c => c.Id == userId)) && n.IsArchived)
                .Include(n => n.Labels)
                .Include(n => n.Collaborators)
                .AsSplitQuery()
                .ToList();
        }

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

                _cache.SetString($"note_{noteId}", JsonConvert.SerializeObject(note));

                var cachedNotes = _cache.GetString($"notes_{note.CreatedBy}");
                if (!string.IsNullOrEmpty(cachedNotes))
                {
                    var notesList = JsonConvert.DeserializeObject<List<Note>>(cachedNotes);
                    var noteIndex = notesList.FindIndex(n => n.Id == noteId);
                    if (noteIndex != -1)
                    {
                        notesList[noteIndex] = note; 
                        _cache.SetString($"notes_{note.CreatedBy}", JsonConvert.SerializeObject(notesList));
                    }
                }
            }
        }


        public bool DeleteLabel(int labelId, int userId)
        {
            var label = _context.Labels.FirstOrDefault(l => l.Id == labelId && l.CreatedBy == userId);
            if (label == null) return false;

            _context.Labels.Remove(label);
            _context.SaveChanges();

            var cachedLabels = _cache.GetString($"labels_{userId}");
            if (!string.IsNullOrEmpty(cachedLabels))
            {
                var labelsList = JsonConvert.DeserializeObject<List<Label>>(cachedLabels);
                labelsList.RemoveAll(l => l.Id == labelId);
                _cache.SetString($"labels_{userId}", JsonConvert.SerializeObject(labelsList));
            }

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

                _cache.SetString($"note_{noteId}", JsonConvert.SerializeObject(note));

                var cachedNotes = _cache.GetString($"notes_{note.CreatedBy}");
                if (!string.IsNullOrEmpty(cachedNotes))
                {
                    var notesList = JsonConvert.DeserializeObject<List<Note>>(cachedNotes);
                    var noteIndex = notesList.FindIndex(n => n.Id == noteId);
                    if (noteIndex != -1)
                    {
                        notesList[noteIndex] = note;
                        _cache.SetString($"notes_{note.CreatedBy}", JsonConvert.SerializeObject(notesList));
                    }
                }
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

                _cache.SetString($"note_{noteId}", JsonConvert.SerializeObject(note));

                var cachedNotes = _cache.GetString($"notes_{note.CreatedBy}");
                if (!string.IsNullOrEmpty(cachedNotes))
                {
                    var notesList = JsonConvert.DeserializeObject<List<Note>>(cachedNotes);
                    var noteIndex = notesList.FindIndex(n => n.Id == noteId);
                    if (noteIndex != -1)
                    {
                        notesList[noteIndex] = note;
                        _cache.SetString($"notes_{note.CreatedBy}", JsonConvert.SerializeObject(notesList));
                    }
                }
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

                _cache.SetString($"note_{noteId}", JsonConvert.SerializeObject(note));

                var cachedNotes = _cache.GetString($"notes_{note.CreatedBy}");
                if (!string.IsNullOrEmpty(cachedNotes))
                {
                    var notesList = JsonConvert.DeserializeObject<List<Note>>(cachedNotes);
                    var noteIndex = notesList.FindIndex(n => n.Id == noteId);
                    if (noteIndex != -1)
                    {
                        notesList[noteIndex] = note;
                        _cache.SetString($"notes_{note.CreatedBy}", JsonConvert.SerializeObject(notesList));
                    }
                }
            }
        }


        public IEnumerable<User> GetCollaboratorsByNoteId(int noteId)
        {
            return _context.Notes
                .Where(n => n.Id == noteId)
                .SelectMany(n => n.Collaborators)
                .ToList();
        }

        private void InvalidateCache(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }
    }
}
