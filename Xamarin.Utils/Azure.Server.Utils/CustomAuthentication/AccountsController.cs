using Azure.Server.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    public abstract class AccountsController<T> : ApiController
        where T : AccountBase, new()
    {
        private readonly CustomAuthenticationContext<T> _context;
        private readonly Dictionary<string, Provider> _providers;

        public AccountsController(CustomAuthenticationContext<T> context)
        {
            _context = context;
            _providers = new Dictionary<string, Provider>()
            {
                { "google", Provider.Google}, { "facebook", Provider.Facebook }
            };
        }

        [HttpPost]
        [Authorize]
        public T GetCurrent()
        {
            var sid = this.GetCurrentUserClaim(ClaimTypes.NameIdentifier);
            var acc = _context.Accounts.FirstOrDefault(a => a.Sid == sid);
            if (acc == null)
            {
                acc = CreateAccount();
                _context.Accounts.Add(acc);
                _context.SaveChanges();
            }
            return acc;
        }

        private T CreateAccount()
        {
            T acc = new T()
            {
                Id = Guid.NewGuid().ToString(),
                Sid = this.GetCurrentUserClaim(ClaimTypes.NameIdentifier),
                Verified = true,
                Provider = _providers[User.Identity.AuthenticationType]
            };

            CreateNewAccount(acc);
            return acc;
        }

        protected abstract void CreateNewAccount(T newAccount);
    }
}
