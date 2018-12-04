using Azure.Server.Utils.Communication.Authentication;
using System;
using System.Linq;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    public abstract class CustomRegistrationControllerBase<T> : ApiController
        where T : AccountBase, new()
    {
        private readonly CustomAuthenticationContext<T> _context;

        public CustomRegistrationControllerBase(CustomAuthenticationContext<T> context)
        {
            _context = context;
        }

        public RegistrationResult Post(RegistrationRequest registrationRequest)
        {
            T account = _context.Accounts
            .Where(a => a.Provider == Provider.Custom)
            .Where(a => a.Sid == registrationRequest.Email)
            .SingleOrDefault();

            if (account != null)
            {
                return RegistrationResult.AlreadyRegistered;
            }
            else
            {
                byte[] salt = CustomLoginProviderUtils.GenerateSalt();
                T newAccount = new T
                {
                    Id = Guid.NewGuid().ToString(),
                    Provider = Provider.Custom,
                    Sid = registrationRequest.Email,
                    Salt = salt,
                    Hash = CustomLoginProviderUtils.Hash(registrationRequest.Password, salt)
                };
                CreateUserProfile(newAccount, registrationRequest);
                _context.Accounts.Add(newAccount);
                _context.SaveChanges();
                return RegistrationResult.Registered;
            }
        }

        protected abstract void CreateUserProfile(T account, RegistrationRequest registrationRequest);
    }
}
