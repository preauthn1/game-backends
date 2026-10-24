using FW.Core.Engine;
using System;
using System.Web;
using FW.Core.Helper;


namespace example.invalid
{
    public class ResourceHttpHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string fileid = context.Request.QueryString["fileid"];
            if (String.IsNullOrWhiteSpace(fileid))
            {
                throw new StorageException("文件名不能为空");
            }
            string owner = context.Request.QueryString["owner"];
            if (String.IsNullOrWhiteSpace(owner))
            {
                throw new StorageException("文件表识不能为空");
            }

            IStorageFile storage = EngineContext.Current.Resolve<IStorageFile>();
            string contentType = IOHelper.GetContentType(fileid);
            var data = storage.GetFile(fileid, owner);
            if (data == null || data.Data.Length == 0)
            {
                throw new StorageException("文件不存在");
            }
            context.Response.ContentType = data.ContentType;

            context.Response.OutputStream.Write(data.Data, 0, data.Data.Length);
            context.Response.OutputStream.Flush();
            context.Response.End();

        }
    }
}
