namespace DataAccessLayer.Models
{
    public class UserResponseModel
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
    }
}