namespace BusinessLogicLayer.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string to, string subject, string body);
    }
}
