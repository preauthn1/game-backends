using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class TelegramAnnouncementSubmitViewModel
    {

        public string BannerImageUrl { get; set; }

        public string BasicGameImageUrl { get; set; }

        public string Content { get; set; }

        public string ContentForSkype { get; set; }


        public int SendTargetsByTagID { get; set; }

        /// <summary>
        /// TelegramAnnouncementTemplateEnum
        /// </summary>
        public int TemplateID { get; set; }


        public List<int> ExcludeChannelIDs { get; set; }



        public List<TelegramAnnouncementButtonItemModel> Buttons { get; set; }

    }

    public class GameInfoQueryTempModel
    {
        public string GameNameCN { get; set; }
        public string GameNameEN { get; set; }
        public string Setting_RTP { get; set; }
        public string Setting_lines { get; set; }
        public string Setting_GridFormat { get; set; }
        public string Setting_maxWin { get; set; }
    }



    public class TelegramAnnouncementButtonItemModel
    {
        public bool IsDisabled { get; set; }

        public string Text { get; set; }

        public string LinkUrl { get; set; }
    }


    public class IMChannelAnnouncementHistoryViewModel
    {
        public DateTime CreateTime { get; set; }

        public int TemplateID { get; set; }

        public string TemplateTypeText { get; set; }

        public string DebugResponse { get; set; }

        public string Memo3 { get; set; }

        public string UUID { get; set; }

        public string ResultMessageID { get; set; }

    }




}
