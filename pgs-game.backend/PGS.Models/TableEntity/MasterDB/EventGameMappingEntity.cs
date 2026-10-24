using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventGameMappingEntity
    {
        [Key]
        public int Id { get; set; }
        public int GameID { get; set; }
        public int EventID { get; set; }

        public decimal FlowRate { get; set; }

    }
}
