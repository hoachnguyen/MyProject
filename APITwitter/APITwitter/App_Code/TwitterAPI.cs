using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace APITwitter.App_Code
{
    public partial class TwitterAPI
    {
        // oauth implementation details
        private const string oauthVersion = "1.0";
        private const string oauthSignatureMethod = "HMAC-SHA1";

        // oauth application keys
        //the oauth_token parameter represents a user’s permission to share access to their account
        //the oauth_consumer_key identifies which application is making the request
        private const string oauthToken = "3221099095-xFD2Hy5GIEFsfDhpDiIYBOShJ5SUkpRYIFX6wgs";
        private const string oauthTokenSecret = "3Mnp8X7GyMgMqmiupQkvy6FSaEe3PVe6a9hq5kQv3ot0d";
        private const string oauthConsumerKey = "dRk4UoDSBj1YMm4D5S8pkgDgm";
        private const string oauthConsumerSecret = "AIXZBnu6ObQXVfTNSp5DopMZC7srSj8ULnMIm7yJbMUyPa2hZd";

        // screen name
        private const string screenName = "hoachnguyen0313";

        public string GetTimeline()
        {
            // message api details
            var method = "GET";
            var resourceUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json";

            // make the request and get json data
            string responseData = MakeRequest(screenName, method, resourceUrl);

            return responseData;
        }

        public void PostTweet(string status)
        {
            // message api details
            var method = "POST";
            var resourceUrl = "https://api.twitter.com/1.1/statuses/update.json";

            // make the request to twitter server
            string request = MakeRequest(status, method, resourceUrl);
        }

        public string MakeRequest(string status, string method, string resourceUrl)
        {
            var postBody = "";
            var responseData = "";

            //building the header string
            string authHeader = CreateHeader(status, resourceUrl, method);

            if (method == "GET")
            {
                postBody = "screen_name=" + Uri.EscapeDataString(status);
                resourceUrl += "?" + postBody;
            }

            ServicePointManager.Expect100Continue = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resourceUrl);
            request.Headers.Add("Authorization", authHeader);
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";

            if (method == "POST")
            {
                postBody = "status=" + Uri.EscapeDataString(status);
                using (Stream stream = request.GetRequestStream())
                {
                    byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                    stream.Write(content, 0, content.Length);
                }

                WebResponse response = request.GetResponse();
                return "";
            }

            else
            {
                WebResponse response = request.GetResponse();
                return responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
        }

        public string CreateHeader(string status, string resourceUrl, string type)
        {
            //the oauth_nonce parameter is a unique token the application should generate for each unique request
            var OauthNonce = Convert.ToBase64String(
                new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));

            //the oauth_timestamp parameter indicates when the request was created
            var TimeSpan = DateTime.UtcNow
                - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var OauthTimeStamp = Convert.ToInt64(TimeSpan.TotalSeconds).ToString();

            // the oauth_signature parameter contains a value which is generated 
            //by running all of the other request parameters and two secret values through a signing algorithm
            string OauthSignature = CreateOauthSignature(status, resourceUrl, OauthNonce, OauthTimeStamp, type);
            
            // create the request header
            var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                               "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                               "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                               "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(OauthNonce),
                                    Uri.EscapeDataString(oauthSignatureMethod),
                                    Uri.EscapeDataString(OauthTimeStamp),
                                    Uri.EscapeDataString(oauthConsumerKey),
                                    Uri.EscapeDataString(oauthToken),
                                    Uri.EscapeDataString(OauthSignature),
                                    Uri.EscapeDataString(oauthVersion)
                            );

            return authHeader;
        }

        public string CreateOauthSignature(string status, 
                                    string resourceUrl, 
                                    string oauthNonce, 
                                    string oauthTimestamp, 
                                    string method)
        {
            string OauthSignature = "";
            //building base string for the oauth_signature
            var BaseString = CreateBaseString(method, status, resourceUrl, oauthNonce, oauthTimestamp);

            var compositeKey = string.Concat(Uri.EscapeDataString(oauthConsumerSecret),
                                    "&", Uri.EscapeDataString(oauthTokenSecret));

            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                OauthSignature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(BaseString)));
            }

            return OauthSignature;
        }

        public string CreateBaseString(string method, 
                                    string status, 
                                    string ResourceUrl, 
                                    string OauthNonce, 
                                    string OauthTimestamp)
        {

            var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                           "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";

            if (method == "POST")
            {
                baseFormat += "&status={6}";
            }

            else
            {
                baseFormat += "&screen_name={6}";
            }

            var baseString = string.Format(baseFormat,
                                        oauthConsumerKey,
                                        OauthNonce,
                                        oauthSignatureMethod,
                                        OauthTimestamp,
                                        oauthToken,
                                        oauthVersion,
                                        Uri.EscapeDataString(status)
                                        );

            baseString = string.Concat(method + "&", Uri.EscapeDataString(ResourceUrl), "&", Uri.EscapeDataString(baseString));

            return baseString;
        }        
    }
}
