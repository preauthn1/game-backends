using FW.Core.Helper;
using System;
using System.Collections.Generic;

namespace game.Entity
{
    public class QueryEventSettingRequestViewModel : PaginationParameter
    {
        public string PlatformIDsText { get; set; }

        public List<int> PlatformIDs { get; set; }

        public DateTime? StartTimeBegin { get; set; }

        public DateTime? StartTimeEnd { get; set; }

        public DateTime? EndTimeBegin { get; set; }

        public DateTime? EndTimeEnd { get; set; }

        public int? EventType { get; set; }

        public List<int> EventTypes { get; set; }

        public int? Status { get; set; }

        public bool IsIncludeDeleteData { get; set; }

        public int? UserID { get; set; }

        public int? EventSettingID { get; set; }

    }




}
