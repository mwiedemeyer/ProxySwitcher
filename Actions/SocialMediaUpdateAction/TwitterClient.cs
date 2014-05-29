using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Twitterizer;

namespace SocialMediaUpdateAction
{
    internal static class TwitterClient
    {
        private static readonly string consumerKey = "mi8zSWHPDhRu4romRyuxtw";
        private static readonly string consumerSecret = "jIfzlwahAXUsX3MUM7aE4HxapzE9yJ3nZnS0TeowhwI";

        internal static void UpdateStatus(string newStatusMessage, string accessToken, string accessTokenSecret)
        {
            OAuthTokens tokens = new OAuthTokens();
            tokens.AccessToken = accessToken;
            tokens.AccessTokenSecret = accessTokenSecret;
            tokens.ConsumerKey = consumerKey;
            tokens.ConsumerSecret = consumerSecret;

            TwitterStatus.Update(tokens, newStatusMessage);
        }

        internal static string GetLoginUrl(out string requestToken)
        {
            var token = OAuthUtility.GetRequestToken(consumerKey, consumerSecret, "oob");

            var url = OAuthUtility.BuildAuthorizationUri(token.Token);

            requestToken = token.Token;

            return url.AbsoluteUri;
        }

        internal static void CreateAccessToken(string requestToken, string pin, out string accessToken, out string accessTokenSecret)
        {
            var access = OAuthUtility.GetAccessToken(consumerKey, consumerSecret, requestToken, pin);

            accessToken = access.Token;
            accessTokenSecret = access.TokenSecret;
        }
    }
}
