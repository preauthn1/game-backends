using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class GameLanguageTextTempModel
    {
        public GameLanguageTextTempModel()
        {

        }

        public GameLanguageTextTempModel(int gameID , string text)
        {
            this.GameID = gameID;
            this.LanguageText = text;
        }

        public int GameID { get; set; }

        public string LanguageText { get; set; }

    }
}
