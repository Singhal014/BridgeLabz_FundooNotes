using BusinessLogicLayer.Interfaces;
using System.Net;
using System.Net.Mail;

namespace BusinessLogicLayer.Services
{
    public class EmailService : IEmailService
    {
        public void SendEmail(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("singhalps014@gmail.com", "boiy wbwm ufkm lwnk"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("singhalps014@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);
            smtpClient.Send(mailMessage);
        }
    }
}
