using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid.Http;
using example.invalid.Http.Formatting;
using example.invalid.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.WebAPI
{
    public class JsonpMediaTypeFormatter : JsonMediaTypeFormatter
    {
        public string Callback { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="callback"></param>
        public JsonpMediaTypeFormatter(string callback = null)
        {
            this.Callback = callback;
        }

        public override Task WriteToStreamAsync(Type type, object value, example.invalid.Stream writeStream, example.invalid.Http.HttpContent content, example.invalid.TransportContext transportContext)
        {
            if (String.IsNullOrWhiteSpace(this.Callback))
            {
                return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
            }

            try
            {
                this.WriteToStream(type, value, writeStream, content);
                return Task.FromResult<AsyncVoid>(new AsyncVoid());
            }
            catch (Exception exception)
            {
                TaskCompletionSource<AsyncVoid> source = new TaskCompletionSource<AsyncVoid>();
                source.SetException(exception);
                return source.Task;
            }
        }

        private void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            JsonSerializer serializer = JsonSerializer.Create(this.SerializerSettings);
            using (StreamWriter streamWriter = new StreamWriter(writeStream, this.SupportedEncodings.First()))
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter) { CloseOutput = false })
            {
                jsonTextWriter.WriteRaw(this.Callback + "(");
                serializer.Serialize(jsonTextWriter, value);
                jsonTextWriter.WriteRaw(")");
            }
        }
        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            if (request.Method != HttpMethod.Get)
            {
                return this;
            }
            string callback;
            if (request.GetQueryNameValuePairs().ToDictionary(pair => pair.Key,
                 pair => pair.Value).TryGetValue("callback", out callback))
            {
                var jsonpFormatter = new JsonpMediaTypeFormatter(callback);
                jsonpFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); 
                return jsonpFormatter;
            }
            return this;
        }


        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct AsyncVoid
        { }



    }
}
