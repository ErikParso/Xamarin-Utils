using Azure.Server.Utils.Extensions;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace Azure.Server.Utils.CustomAuthentication
{
    /// <summary>
    /// Provides account information for current user.
    /// </summary>
    /// <typeparam name="C">Db context.</typeparam>
    /// <typeparam name="A">Account entity.</typeparam>
    /// <seealso cref="ApiController" />
    public abstract class AccountsController<C, A> : ApiController
        where C : DbContext
        where A : AccountBase, new()
    {
        private readonly C _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountsController{C, A}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public AccountsController(C context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets account for current user. Creates one if not exists.
        /// </summary>
        [HttpPost]
        [Authorize]
        public A GetCurrentAccount()
        {
            DbSet<A> accounts = GetAccountsDbSet(_context);
            var sid = this.GetCurrentUserClaim(ClaimTypes.NameIdentifier);
            // try get account base instance for current user
            A acc = GetAccountsDbSet(_context)
                .Where(a => a.Provider == User.Identity.AuthenticationType)
                .Where(a => a.Sid == sid)
                .SingleOrDefault();
            // if not found, create account base info
            if (acc == null)
            {
                acc = CreateAccountBase();
                accounts.Add(acc);
            }
            // set account information if not specified
            if (!acc.AccountInformationSet)
            {
                SetAccountInformation(acc);
                acc.AccountInformationSet = true;
            }
            _context.SaveChanges();
            return acc;
        }

        private A CreateAccountBase()
        {
            return new A
            {
                Id = Guid.NewGuid().ToString(),
                Sid = this.GetCurrentUserClaim(ClaimTypes.NameIdentifier),
                Verified = true,
                Provider = User.Identity.AuthenticationType
            };
        }

        protected abstract DbSet<A> GetAccountsDbSet(C fromContext);

        protected abstract void SetAccountInformation(A newAccount);
    }
}
