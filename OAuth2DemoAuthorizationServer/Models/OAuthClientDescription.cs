using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using OAuth2DemoDbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAuth2DemoAuthorizationServer.Models
{
    public class OAuthClientDescription : OAuth_Client, IClientDescription
    {
        public OAuthClientDescription(OAuth_Client client)
        {
            this.ClientId = client.ClientId;
            this.Callback = client.Callback;
            this.ClientIdentifier = client.ClientIdentifier;
            this.ClientSecret = client.ClientSecret;
            this.ClientType = client.ClientType;
            this.Name = client.Name;
        }

        #region IConsumerDescription Members

        /// <summary>
        /// Gets the callback to use when an individual authorization request
        /// does not include an explicit callback URI.
        /// </summary>
        /// <value>
        /// An absolute URL; or <c>null</c> if none is registered.
        /// </value>
        Uri IClientDescription.DefaultCallback
        {
            get { return string.IsNullOrEmpty(this.Callback) ? null : new Uri(this.Callback); }
        }

        /// <summary>
        /// Gets the type of the client.
        /// </summary>
        ClientType IClientDescription.ClientType
        {
            get { return (ClientType)this.ClientType; }
        }

        /// <summary>
        /// Gets a value indicating whether a non-empty secret is registered for this client.
        /// </summary>
        bool IClientDescription.HasNonEmptySecret
        {
            get { return !string.IsNullOrEmpty(this.ClientSecret); }
        }

        /// <summary>
        /// Determines whether a callback URI included in a client's authorization request
        /// is among those allowed callbacks for the registered client.
        /// </summary>
        /// <param name="callback">The absolute URI the client has requested the authorization result be received at.</param>
        /// <returns>
        ///   <c>true</c> if the callback URL is allowable for this client; otherwise, <c>false</c>.
        /// </returns>
        bool IClientDescription.IsCallbackAllowed(Uri callback)
        {
            if (string.IsNullOrEmpty(this.Callback))
            {
                // No callback rules have been set up for this client.
                return true;
            }

            // In this sample, it's enough of a callback URL match if the scheme and host match.
            // In a production app, it is advisable to require a match on the path as well.
            Uri acceptableCallbackPattern = new Uri(this.Callback);
            if (string.Equals(acceptableCallbackPattern.GetLeftPart(UriPartial.Authority), callback.GetLeftPart(UriPartial.Authority), StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the specified client secret is correct.
        /// </summary>
        /// <param name="secret">The secret obtained from the client.</param>
        /// <returns><c>true</c> if the secret matches the one in the authorization server's record for the client; <c>false</c> otherwise.</returns>
        /// <remarks>
        /// All string equality checks, whether checking secrets or their hashes,
        /// should be done using <see cref="MessagingUtilities.EqualsConstantTime"/> to mitigate timing attacks.
        /// </remarks>
        bool IClientDescription.IsValidClientSecret(string secret)
        {
            return MessagingUtilities.EqualsConstantTime(secret, this.ClientSecret);
        }

        #endregion
    }
}