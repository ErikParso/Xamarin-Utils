namespace Azure.Server.Utils.Email
{
    public interface IEmailService
    {
        void SendEmail(string subject, string message, string to);
    }
}
