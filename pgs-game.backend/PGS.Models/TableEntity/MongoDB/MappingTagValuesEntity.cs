using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{


    public class MongoDBMappingTagValuesEntity
    {

        public string UniqueID { get; set; }

        public DateTime CreateTime { get; set; }

        public MongoDBRequestDetailInfo Detail { get; set; }

    }

    public class MongoDBRequestDetailInfo
    {

        public string ReqeustBody { get; set; }

        public string ReqeustHeaders { get; set; }

        public string ResponseContent { get; set; }

        public string WebsiteName { get; set; }

    }


}