using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using example.invalid;

namespace TG_Hook_API.Models
{
  
    public class SkypeChannelData
    {
        public string text { get; set; }
    }

    public class SkypeConversation
    {
        public bool isGroup { get; set; }
        public string id { get; set; }
    }

    public class SkypeEntity
    {
        public SkypeMentioned mentioned { get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public string locale { get; set; }
        public string country { get; set; }
        public string platform { get; set; }
        public string timezone { get; set; }
    }

    public class SkypeFrom
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class SkypeMentioned
    {
        public string id { get; set; }
    }

    public class SkypeRecipient
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class SkypeWebHookRequest
    {
        public string text { get; set; }
        public string type { get; set; }
        public DateTime timestamp { get; set; }
        public string id { get; set; }
        public string channelId { get; set; }
        public string serviceUrl { get; set; }
        public SkypeFrom from { get; set; }
        public SkypeConversation conversation { get; set; }
        public SkypeRecipient recipient { get; set; }
        public List<SkypeEntity> entities { get; set; }
        public SkypeChannelData channelData { get; set; }
        public string locale { get; set; }
    }



}
