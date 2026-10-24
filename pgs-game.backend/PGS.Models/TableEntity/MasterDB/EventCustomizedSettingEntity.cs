using target.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventCustomizedSettingEntity
    {
        public EventCustomizedSettingEntity()
        {

        }

        public EventCustomizedSettingEntity(int eventID , EventCustomizedSettingKeyEnum settingKey)
        {
            this.EventID = eventID;
            this.SettingKey = (int)settingKey;
            this.SettingValue = "";
        }

        public EventCustomizedSettingEntity(int eventID, EventCustomizedSettingKeyEnum settingKey , string settingValue)
        {
            this.EventID = eventID;
            this.SettingKey = (int)settingKey;
            this.SettingValue = settingValue;
        }


        [Key]
        public int Id { get; set; }
        public int EventID { get; set; }

        /// <summary>
        /// 請參考 EventCustomizedSettingKeyEnum
        /// </summary>
        public int SettingKey { get; set; }
        public string SettingValue { get; set; }


    }
}
