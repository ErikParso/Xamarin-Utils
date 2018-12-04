using System.Data.Entity;

namespace Azure.Server.Utils.CustomAuthentication
{
    public abstract class CustomAuthenticationContext<T> : DbContext
        where T : AccountBase
    {
        public CustomAuthenticationContext(string connection) : base(connection)
        {

        }

        public DbSet<T> Accounts { get; set; }
    }
}
