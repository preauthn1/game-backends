using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class AreaEntity
    {
        [Key]
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public string ClientPlatform { get; set; }
        public int? PlatformID { get; set; }
        public string Memo { get; set; }
        public int AdminID { get; set; }
        public int Sort { get; set; }
        public string ShareURL { get; set; }
        public long ShowConvertLimit { get; set; }
        public long ShowAllGameLimit { get; set; }
        public long ShowVIPAgent { get; set; }
        public long ShowCS { get; set; }
        public byte ShowConvertBindPhone { get; set; }
        public byte ShowAllGameBindPhone { get; set; }
        public byte ShowVIPAgentBindPhone { get; set; }
        public byte ShowCSBindPhone { get; set; }
        public int PlaytimeCS { get; set; }
        public int PlaytimeConvert { get; set; }
        public int PlaytimeVIPAgent { get; set; }
        public DateTime CreateTime { get; set; }
        public bool AreaStatus { get; set; }
        public string FAQContent { get; set; }
        public string PayTips { get; set; }
        public string UpGradeMsg { get; set; }
        public string AnnMsg { get; set; }
        public byte? AnnMsgShow { get; set; }
        public byte? SetDefault { get; set; }
        public string AreaConfig { get; set; }
        public long GiftMoney { get; set; }
        public long DefaultMoney { get; set; }
        public byte ShopNameID { get; set; }
        public byte AgentNameID { get; set; }
        public byte ShowConvertPopup { get; set; }
        public string ConvertPopupText { get; set; }
    }







}
