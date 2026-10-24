using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.Game
{

    public class CertificationRawEntity
    {
        [Key] 
        public int UserID { get; set; }

        public string CerCode { get; set; }


    }
}
