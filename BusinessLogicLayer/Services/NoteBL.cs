using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Collections.Generic;
using BusinessLogicLayer.Interfaces;
using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;

public class NoteBl : INoteService
{
    private readonly INoteDl _noteRepository;
    private readonly IUserBl _userService;

    public NoteBl(INoteDl noteRepository, IUserBl userService)
    {
        _noteRepository = noteRepository;
        _userService = userService;
    }

    public void AddNote(Note note, int userId)
    {
        note.CreatedBy = userId;
        _noteRepository.AddNote(note);
    }

    public Note GetNoteById(int noteId)
    {
        return _noteRepository.GetNoteById(noteId);
    }

    public IEnumerable<Note> GetAllNotes()
    {
        return _noteRepository.GetAllNotes();
    }

    public void EditNote(Note note)
    {
        _noteRepository.UpdateNote(note);
    }

    public void TrashNote(int noteId)
    {
        _noteRepository.DeleteNote(noteId);
    }

    public int GetUserIdFromToken(string token)
    {
        return _userService.GetUserIdFromToken(token);
    }
}
