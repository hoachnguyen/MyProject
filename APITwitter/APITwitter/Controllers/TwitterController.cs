using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using APITwitter.Models;
using APITwitter.App_Code;
using Newtonsoft.Json;

namespace APITwitter.Controllers
{
    public class TwitterController : Controller
    {

        private TwitterAPI oauthTwitter = new TwitterAPI();

        // GET: Twitter
        public ActionResult Index()
        {
            List<Timeline> allPost = new List<Timeline>();

            string json = oauthTwitter.GetTimeline();
            dynamic data = JsonConvert.DeserializeObject(json);
            foreach (var posts in data)
            {
                Timeline post = new Timeline();
                post.Name = posts.user.name;
                post.ScreenName = posts.user.screen_name;
                post.CreatedAt = posts.created_at;
                post.TextPost = posts.text;
                allPost.Add(post);

            }
            return View(allPost);
        }

        public ActionResult Post(string txtTweet)
        {
            oauthTwitter.PostTweet(txtTweet);
            return RedirectToAction("Index");
        }

        public ActionResult Refresh()
        {
            return RedirectToAction("Index");
        }
    }
}

       