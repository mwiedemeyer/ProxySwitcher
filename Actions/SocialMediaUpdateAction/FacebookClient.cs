using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace SocialMediaUpdateAction
{
    internal class FacebookClient
    {
        private static readonly string appId = "151862704834966";
        private static readonly string apiSecret = "f6240da17595cff1de6171236dc4a8f3";
        public static readonly string SUCCESS_URL = "http://proxyswitcher.net/facebook.dummy";
        public static readonly string LOGIN_URL = String.Format("https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&display=popup&scope=publish_stream,offline_access", appId, SUCCESS_URL);
        public static string ACCESS_TOKEN_URL = "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";

        internal static void UpdateStatus(string accessToken, string newStatusMessage)
        {
            string messageUrl = String.Format("https://graph.facebook.com/me/feed?access_token={0}&message={1}", accessToken, newStatusMessage);

            Post(messageUrl);
        }

        internal static string GetTokenFromResponseUri(Uri uri)
        {
            var badSessionInfoException = new ArgumentException("The session response (" + uri.AbsoluteUri + ") does not contain connection information.", "uri");

            string query = uri.Query;

            if (!query.Contains("code="))
                throw badSessionInfoException;

            string[] splittedQuery = query.Split('=');
            if (!splittedQuery[0].Contains("code"))
                throw badSessionInfoException;

            var code = splittedQuery[1];

            string accessTokenUrl = String.Format(ACCESS_TOKEN_URL, appId, SUCCESS_URL, apiSecret, code);
            var token = Post(accessTokenUrl);

            if (!token.Contains("access_token="))
                throw badSessionInfoException;

            var accessToken = token.Split('=')[1];

            return accessToken;
        }

        private static string Post(string url)
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";

            byte[] bytes = Encoding.UTF8.GetBytes(string.Empty);

            request.ContentLength = bytes.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
