using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class BOAnnouncementEntity
    {
        [Key]
        public int BOAnnouncementID { get; set; }

        public int Type { get; set; }

        public int Status { get; set; }

        public int HintType { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public int OwnerOrganizationID { get; set; }

        /// <summary>
        /// 為了讓功能不要太複雜。先設計成只有發文者自己看的到自己的文章
        /// </summary>
        public int OwnerUserID { get; set; }

        public Guid AssignTemplateID { get; set; }

        public int AssignType { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateBy { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateBy { get; set; }



    }
}
