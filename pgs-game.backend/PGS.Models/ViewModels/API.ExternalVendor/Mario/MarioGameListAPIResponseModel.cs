using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class MarioGameListAPIResponseModel : MarioAPICommonResponse
    {
        public List<MarioGameListItemModel> GameList { get; set; }

    }




    public class MarioGameListItemModel
    {
        public string gameCode { get; set; }
        public string gameName { get; set; }
        public string gameType { get; set; }
        public string special { get; set; }
        public List<MarioGameLanguageModel> language { get; set; }
    }


    public class MarioGameLanguageModel
    {
        public string code { get; set; }

        public string name { get; set; }
    }




}
