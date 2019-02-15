using Azure.Server.Utils.Communication.Authentication;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    /// <summary>
    /// Registration controller for custom autentication.
    /// </summary>
    /// <typeparam name="C">Db context.</typeparam>
    /// <typeparam name="A">Account.</typeparam>
    /// <seealso cref="ApiController" />
    public abstract class CustomRegistrationController<C,A> : ApiController
        where C : DbContext
        where A : AccountBase, new()
    {
        private readonly C _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRegistrationController{C, A}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="providerName">
        /// Name of the provider.
        /// Will be set in <see cref="A.Provider"/> on account creation.
        /// </param>
        public CustomRegistrationController(C context)
        {
            _context = context;
        }

        /// <summary>
        /// Posts the specified registration request.
        /// </summary>
        /// <param name="registrationRequest">The registration request.</param>
        /// <returns></returns>
        public RegistrationResult Post(RegistrationRequest registrationRequest)
        {
            DbSet<A> accounts = GetAccountsDbSet(_context);
            A account = accounts
                .Where(a => a.Provider == "Federation")
                .Where(a => a.Sid == registrationRequest.UserId)
                .SingleOrDefault();

            if (account != null)
            {
                return RegistrationResult.AlreadyRegistered;
            }
            else
            {
                byte[] salt = CustomLoginProviderUtils.GenerateSalt();
                A newAccount = new A
                {
                    Sid = registrationRequest.UserId,
                    Provider = "Federation",
                    Salt = salt,
                    Hash = CustomLoginProviderUtils.Hash(registrationRequest.Password, salt)
                };
                accounts.Add(newAccount);
                _context.SaveChanges();
                return RegistrationResult.Registered;
            }
        }

        protected abstract DbSet<A> GetAccountsDbSet(C fromContext);
    }
}
