using System;

namespace game.Entity.SQL
{
    public class QuerySystemJobResultModel
    {

        public Guid Job_ID { get; set; }

        public string Name { get; set; }

        public int Enable { get; set; }

        public bool IsEnable 
        { 
            get
            {
                return Enable == 1;
            }
        }

    }
    
}
