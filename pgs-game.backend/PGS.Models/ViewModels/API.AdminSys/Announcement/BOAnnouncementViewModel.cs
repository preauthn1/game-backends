using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{



    public class BOAnnouncementViewModel
    {
        public int BOAnnouncementID { get; set; }

        public int Type { get; set; }

        public int HintType { get; set; }

        public string TypeText { get; set; }

        public string HintTypeText { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateBy { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateBy { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public Guid AssignTemplateID { get; set; }

        public int AssignType { get; set; }

        public string AssignTypeText { get; set; }

        public AssignParticipateRequest AssignParticipate { get; set; }

    }

    public class BOAnnouncementDetailViewModel
    {
        public int BOAnnouncementID { get; set; }

        public int Type { get; set; }

        public int HintType { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreateTime { get; set; }

        public Guid AssignTemplateID { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public AssignParticipateRequest AssignParticipate { get; set; }

    }


    public class BOAnnouncementCreateRequestModel: IBOAnnouncementRequestModel
    {
        public int Type { get; set; }

        public int HintType { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        /// <summary>
        /// 修改發文者名稱
        /// </summary>
        public string ReplaceCreateBy { get; set; }

        public AssignParticipateRequest AssignParticipate { get; set; }

    }


    public class BOAnnouncementUpdateRequestModel: IBOAnnouncementRequestModel
    {
        public int BOAnnouncementID { get; set; }

        public int Type { get; set; }

        public int HintType { get; set; }

        public int Status { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public Guid AssignTemplateID { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public AssignParticipateRequest AssignParticipate { get; set; }
    }


    public interface IBOAnnouncementRequestModel
    {
        public int Type { get; set; }

        public int HintType { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }


    public class AssignParticipateRequest
    {
        /// <summary>
        /// AssignType
        /// </summary>
        public int AssignType { get; set; }

        public List<int> ParticipaterIDs { get; set; }
    }


}
