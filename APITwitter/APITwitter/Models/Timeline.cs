using System;
using System.Net; 
using System.Text; 
using System.Diagnostics; 
using System.Collections.Generic; 
using System.Security.Cryptography; 


namespace APITwitter.Models
{
   public class Timeline
   {
       public string Name { get; set; }
       public string ScreenName { get; set; }
       public string CreatedAt { get; set; }
       public string TextPost { get; set; }
   }
} 

