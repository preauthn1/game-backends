using Newtonsoft.Json;
using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class MessageViewModel
    {
        public decimal TimeZone { get; set; }

        public List<MessageViewModelItemViewModel> Items { get; set; }
    }


    public class MessageViewModelItemViewModel
    {
        public long MessageID { get; set; }
        public int AssignType { get; set; }
        public int ContentTemplateID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string LanguageCode { get; set; }

        [JsonIgnore]
        public string ParameterJson { get; set; }

        public string Memo { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateTimeText { get; set; }
    }

    public class MessageReadRequestModel
    {
        public long MessageID { get; set; }
    }


}
