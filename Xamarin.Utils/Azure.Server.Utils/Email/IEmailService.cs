using Azure.Server.Utils.CustomAuthentication;

namespace Azure.Server.Utils.Email
{
    public interface IEmailService<A>
        where A: AccountBase
    {
        void SendEmail(string subject, string message, A account);
    }
}
