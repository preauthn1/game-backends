namespace target.Models.Models
{
    public class GeneralTagMappingInfoQueryModel
    {     
        public int TargetID { get; set; }

        public int TagID { get; set; }

        public string Name { get; set; }

    }

    public class GeneralTagMappingCountModel
    {
        public int TagID { get; set; }

        public int TotalCount { get; set; }
    }

}
