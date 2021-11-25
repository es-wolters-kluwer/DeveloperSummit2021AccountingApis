using IdentityModel.Client;
using System.Security.Claims;

namespace WkeInnuvaDeveloperSummit.Wke
{
    public class WkeProcessAuthorizationCodeResult
    {
        /// <summary>
        /// Gets or sets the claims.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public IList<Claim> Claims { get; set; }

        /// <summary>
        /// Gets or sets the access token response.
        /// </summary>
        /// <value>
        /// The access token response.
        /// </value>
        public TokenResponse AccessTokenResponse { get; set; }

    }
}
