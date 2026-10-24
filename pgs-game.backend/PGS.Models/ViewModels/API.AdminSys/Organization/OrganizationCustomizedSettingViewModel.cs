namespace target.Models.ViewModels
{
    public class OrganizationCustomizedSettingViewModel
    {
        public int OrganizationID { get; set; }

        /// <summary>
        /// 對應 OrganizationCustomizedSettingKey
        /// </summary>
        public int SettingKey { get; set; }

        public string SettingValue { get; set; }
    }
}
