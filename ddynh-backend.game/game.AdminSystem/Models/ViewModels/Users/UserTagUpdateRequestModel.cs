namespace game.AdminSystem.Controllers.Users
{
    public class UserTagUpdateRequestModel
    {
        public int UserID { get; set; }

        public int TagID { get; set; }

    }

    public class UserMemoDeleteRequestModel
    {
        public int Id { get; set; }

    }



    public class UserMemoUpdateRequestModel
    {
        public int UserID { get; set; }

        public string Memo { get; set; }

    }



    public class UserTagViewModel
    {
        public int Id { get; set; }

        public int TagID { get; set; }

        public string TagName { get; set; }


        /// <summary>
        /// 強調顯示
        /// </summary>
        public bool IsHighlightDisplay { get; set; }

        public string CreateBy { get; set; }

        public string CreateDate { get; set; }

        public string CreateTime { get; set; }
    }


    public class UserMemoViewModel
    {

        public int Id { get; set; }

        public string Memo { get; set; }

        public string CreateBy { get; set; }

        public string CreateTime { get; set; }

        public string CreateDate { get; set; }
    }



}