using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using APITwitter.Models;
using APITwitter.App_Code;

namespace APITwitter.Controllers
{
    public class TwitterController : Controller
    {

        private TwitterAPI oauthTwitter = new TwitterAPI();

        // GET: Twitter
        public ActionResult Index()
        {
            var json = oauthTwitter.GetPost("hoachnguyen0313");
            List<Timeline> allPost = oauthTwitter.GetTimeline(json);
            return View(allPost);
        }

        public ActionResult Post(string txtTweet)
        {
            oauthTwitter.PostTwitter(txtTweet);
            return RedirectToAction("Index");
        }

        public ActionResult Refresh()
        {
            return RedirectToAction("Index");
        }
    }
}

       