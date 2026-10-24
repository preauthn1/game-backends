using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
  
    public class DocumentMemo : System.Attribute
    {
        public string Text { get; set; }

        public DocumentMemo(string text)
        {
            this.Text = text;
        }
        public DocumentMemo(string text, string text2)
        {
            this.Text = text;
        }

        public DocumentMemo(string text , string text2 , string text3)
        {
            this.Text = text;
        }

    }

}
