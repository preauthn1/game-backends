using Newtonsoft.Json;

namespace target.Models.ViewModels
{
    public class AGGLiveErrorResponseModel : IAGGLiveResponse
    {
        public int code { get; set; }

        public string message { get; set; }

        public object result { get; set; }
    }


    public interface IAGGLiveResponse
    {
         int code { get; set; }

         string message { get; set; }

    }


}
