using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class GeneralTagViewModel
    {

        public int TagID { get; set; }

        /// <summary>
        /// TagTypeEnum
        /// </summary>
        public int Type { get; set; }
        public string TypeText { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public int AssignTargetCount { get; set; }
        public List<int> AssignTargetIDs { get; set; }


    }


    public class GeneralTagCreateRequestModel
    {
        public int TagID { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
    }



    public class GeneralTagUpdateBasicInfoRequestModel
    {
        public int TagID { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
    }



    public class GeneralTagUpdateRequestModel
    {
        public int TagID { get; set; }

        public List<int> AssignTargetIDs { get; set; }

    }

    public class GeneralTagUpdateByAssignerRequestModel
    {
        /// <summary>
        /// TagTypeEnum
        /// </summary>
        public int Type { get; set; }

        public int AssignTargetID { get; set; }

        public List<int> TagIDs { get; set; }

    }


}
