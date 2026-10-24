namespace target.Models.ViewModels
{
    public class AGGCommonHashRequestModel<T> where T : class
    {
        public T data { get; set; }

        public string hash { get; set; }
    }

    public class AGGApiCommonResponse<T> where T : class
    {
        public int code { get; set; }
        public string message { get; set; }
        public T result { get; set; }
    }

}
