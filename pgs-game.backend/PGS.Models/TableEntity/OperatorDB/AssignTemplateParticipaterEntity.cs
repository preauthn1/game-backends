using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AssignTemplateParticipaterEntity
    {
        [Key]
        public Guid AssignTemplateID { get; set; }

        [Key]
        public int AssignType { get; set; }

        [Key]
        public int ParticipaterID { get; set; }


    }
}
