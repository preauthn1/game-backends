using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models
{
    public class JQueryDataTablePagingModel
    {

        public JQueryDataTablePagingModel()
        {
            this.Start = 0;
            this.Length = 20;

        }        

        public int Start { get; set; }


        public int Length { get; set; }


        public int? Draw { get; set; }
        
    }
}