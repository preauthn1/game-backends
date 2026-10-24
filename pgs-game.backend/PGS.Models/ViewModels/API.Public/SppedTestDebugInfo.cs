using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class SppedTestDebugInfo
    {
        public string ServerTime_UTC8 { get; set; }

        public List<SppedTestApiUrlModel> Urls { get; set; }

        public List<SppedTestApiUrlModel> Images { get; set; }
    }

    public class SppedTestApiUrlModel
    {
        public string DisplayName { get; set; }
        public string Url { get; set; }
    }


}
