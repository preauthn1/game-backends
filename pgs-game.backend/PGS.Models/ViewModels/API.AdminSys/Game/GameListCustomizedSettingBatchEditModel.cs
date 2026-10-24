using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class GameListCustomizedSettingBatchEditModel
    {
        public int GameID { get; set; }

        public string GameName { get; set; }


        public List<GameListCustomizedSettingItemModel> Items { get; set; }

    }


    public class GameListCustomizedSettingItemModel
    {
        public int GameID { get; set; }

        public int SettingKey { get; set; }

        public string SettingKeyText { get; set; }

        public string Value { get; set; }
    }



}
