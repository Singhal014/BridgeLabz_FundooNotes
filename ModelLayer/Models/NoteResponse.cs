namespace ModelLayer.Models
{
    public class NoteResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public bool IsArchived { get; set; }
        public List<string> Labels { get; set; } = new List<string>();
    }
}