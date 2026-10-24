using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class RolePermissionCompareViewModel
    {
        public List<RoleBriefViewModel> Roles { get; set; }

        public List<PermissionCompareBriefViewModel> Permissions { get; set; }
    }


    public class PermissionCompareBriefViewModel
    {
        public int PermissionID { get; set; }

        public string PermissionToken { get; set; }

        public string ParentPermissionToken { get; set; }


        public bool HasParent { get; set; }

        public string RootPermissionToken { get; set; }

        public List<string> PermissionNevigatePath { get; set; }

        public List<string> RoleEnableFlag { get; set; }

        /// <summary>
        /// 若為true。則代表此權限 完全沒有分配給查詢的角色 (介面上可隱藏顯示此)
        /// </summary>
        public bool IsUnAssignToRoles { get; set; }

        /// <summary>
        /// 是否有任一個角色權限有差異
        /// </summary>
        public bool IsAllValueEqual { get; set; }
    }



    public class RoleBriefViewModel
    {
        public int RoleID { get; set; }

        public string RoleName { get; set; }
    }



}
