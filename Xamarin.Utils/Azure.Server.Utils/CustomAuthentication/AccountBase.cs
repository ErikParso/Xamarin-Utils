using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Azure.Server.Utils.CustomAuthentication
{
    /// <summary>
    /// Account base for custom Authentication.
    /// </summary>
    /// <seealso cref="Microsoft.Azure.Mobile.Server.EntityData" />
    public abstract class AccountBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Server id. Account identifier.
        /// </summary>
        public string Sid { get; set; }

        /// <summary>
        /// Provider name. Specified in <see cref="CustomRegistrationController{C, A}"/> controller.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// The salt.
        /// </summary>
        public byte[] Salt { get; set; }

        /// <summary>
        /// Pwd hash.
        /// </summary>
        public byte[] Hash { get; set; }

        /// <summary>
        /// Determines, if chils account information was set.
        /// </summary>
        public bool AccountInformationSet { get; set; }

        /// <summary>
        /// Value indicating whether this <see cref="AccountBase"/> is verified.
        /// </summary>
        public bool Verified { get; set; }

        /// <summary>
        /// Gets or sets the confirmation hash.
        /// User will receive confirmation email with this key.
        /// </summary>
        public byte[] ConfirmationHash { get; set; }

        /// <summary>
        /// The refresh token. Will be set at login and refresh token methods call.
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
