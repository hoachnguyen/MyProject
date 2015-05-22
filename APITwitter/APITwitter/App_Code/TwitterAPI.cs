using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json;
using APITwitter.Models;

namespace APITwitter.App_Code
{
    public partial class TwitterAPI
    {
        // oauth implementation details
        public const string OauthVersion = "1.0";
        public const string OauthSignatureMethod = "HMAC-SHA1";

        // oauth application keys
        public const string OauthToken = "3221099095-xFD2Hy5GIEFsfDhpDiIYBOShJ5SUkpRYIFX6wgs";
        public const string OauthTokenSecret = "3Mnp8X7GyMgMqmiupQkvy6FSaEe3PVe6a9hq5kQv3ot0d";
        public const string OauthConsumerKey = "dRk4UoDSBj1YMm4D5S8pkgDgm";
        public const string OauthConsumerSecret = "AIXZBnu6ObQXVfTNSp5DopMZC7srSj8ULnMIm7yJbMUyPa2hZd";

        // screen name
        public const string ScreenName = "hoachnguyen0313";


        public List<Timeline> GetTimeline(string json)
        {
            List<Timeline> timeline = new List<Timeline>();

            dynamic data = JsonConvert.DeserializeObject(json);

            foreach (var posts in data)
            {
                Timeline post = new Timeline();
                post.Name = posts.user.name;
                post.ScreenName = posts.user.screen_name;
                post.CreatedAt = posts.created_at;
                post.TextPost = posts.text;
                timeline.Add(post);

            }
            return timeline;
        }

        public string CreateOauthSignature(string status, string resource_url, string oauth_nonce, string oauth_timestamp, string baseString)
        {
            string OauthSignature = "";

            var compositeKey = string.Concat(Uri.EscapeDataString(OauthConsumerSecret),
                                    "&", Uri.EscapeDataString(OauthTokenSecret));

            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                OauthSignature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            return OauthSignature;
        }

        public string CreateBaseString(string type, string status, string ResourceUrl, string OauthNonce, string OauthTimestamp)
        {

            var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                           "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}";

            if (type == "POST")
            {
                baseFormat += "&status={6}";
            }
            else
            {
                baseFormat += "&screen_name={6}";
            }

            var baseString = string.Format(baseFormat,
                                        OauthConsumerKey,
                                        OauthNonce,
                                        OauthSignatureMethod,
                                        OauthTimestamp,
                                        OauthToken,
                                        OauthVersion,
                                        Uri.EscapeDataString(status)
                                        );

            baseString = string.Concat(type + "&", Uri.EscapeDataString(ResourceUrl), "&", Uri.EscapeDataString(baseString));

            return baseString;
        }

        public string CreateHeader(string status, string resource_url, string type)
        {
            // unique request details
            var OauthNonce = Convert.ToBase64String(
                new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var TimeSpan = DateTime.UtcNow
                - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var OauthTimeStamp = Convert.ToInt64(TimeSpan.TotalSeconds).ToString();

            var BaseString = CreateBaseString(type, status, resource_url, OauthNonce, OauthTimeStamp);

            // create oauth signature
            string oauth_signature = CreateOauthSignature(status, resource_url, OauthNonce, OauthTimeStamp, BaseString);



            // create the request header
            var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                               "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                               "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                               "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(OauthNonce),
                                    Uri.EscapeDataString(OauthSignatureMethod),
                                    Uri.EscapeDataString(OauthTimeStamp),
                                    Uri.EscapeDataString(OauthConsumerKey),
                                    Uri.EscapeDataString(OauthToken),
                                    Uri.EscapeDataString(oauth_signature),
                                    Uri.EscapeDataString(OauthVersion)
                            );

            return authHeader;

        }

        public string MakeRequest(string status, string type, string resource_url, string authHeader)
        {
            var postBody = "";
            var responseData = "";

            // make the request
            if (type == "POST")
            {
                postBody = "status=" + Uri.EscapeDataString(status);
            }
            else
            {
                postBody = "screen_name=" + Uri.EscapeDataString(status);
                resource_url += "?" + postBody;
            }

            ServicePointManager.Expect100Continue = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = type;
            request.ContentType = "application/x-www-form-urlencoded";

            if (type == "POST")
            {
                using (Stream stream = request.GetRequestStream())
                {
                    byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                    stream.Write(content, 0, content.Length);
                }

                WebResponse response = request.GetResponse();
                return null;
            }
            else
            {
                WebResponse response = request.GetResponse();
                return responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
        }

        public void PostTwitter(string status)
        {
            // message api details
            var type = "POST";
            //var status = "post new twitter for me";// text_post.Text;
            var resource_url = "https://api.twitter.com/1.1/statuses/update.json";

            string authHeader = CreateHeader(status, resource_url, type);

            string request = MakeRequest(status, type, resource_url, authHeader);
        }


        public string GetPost(string screenName)
        {
            var type = "GET";

            // message api details
            var resource_url = "https://api.twitter.com/1.1/statuses/user_timeline.json";

            var authHeader = CreateHeader(screenName, resource_url, type);

            // make the request

            string responseData = MakeRequest(screenName, type, resource_url, authHeader);

            return responseData;

        }

    }
}
