using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace game.Transform.Controllers
{
    public class ServerListController : Controller
    {
        string logid = Guid.NewGuid().ToString().Substring(0, 5);

        public ActionResult MyIP()
        {
            var r = HttpContext.Request;
            var REMOTE_ADDR = r.ServerVariables["REMOTE_ADDR"];
            var HTTP_X_FORWARDED_FOR = r.ServerVariables["HTTP_X_FORWARDED_FOR"];
            var HTTP_ALI_CDN_REAL_IP = r.ServerVariables["HTTP_ALI_CDN_REAL_IP"];
            string ipinfo = string.Format("REMOTE_ADDR={0}<BR>HTTP_X_FORWARDED_FOR={1}<BR>HTTP_ALI_CDN_REAL_IP={2}", REMOTE_ADDR, HTTP_X_FORWARDED_FOR, HTTP_ALI_CDN_REAL_IP);


            string header = "";
            foreach (string var in Request.ServerVariables)
            {
                header += var + "=" + Request[var] + "<p>";
            }

            return Content(string.Format("[IP Data]<br>{0}<p>[Header]<br>{1}", ipinfo, header));

        }

        /// <summary>
        /// 節點防護轉發(舊)
        /// </summary>
        /// <param name="gui">機器碼</param>
        /// <param name="channel">管道號</param>
        /// <param name="simulation">0為正常，1為模擬器用戶</param>
        /// <param name="userid"></param>
        /// <param name="levelid"></param>
        /// <param name="key">未使用</param>
        /// <returns></returns>
        public async Task<string> Index(int userid, int levelid, string key,string gui = "", string channel = "", int simulation = 0)
        {
            //return await new TransformServices(logid).GetAddrAsync(gui, channel, simulation, userid, levelid, key);
            var tt = new TransferServices(logid);
            tt.pushLog("ServerList");
            NodeResult result = await tt.GetAddrAsync(gui, channel, simulation, userid, levelid);
            if (result.result != 1)
                return result.message;
            string node = $"{result.data.Addr}:{result.data.TransferPort}|{result.data.GiveUserIP}|{result.data.From}";
            return node;
        }

        /// <summary>
        /// 節點防護轉發
        /// </summary>
        /// <param name="gui">機器碼</param>
        /// <param name="channel">管道號</param>
        /// <param name="simulation">0為正常，1為模擬器用戶</param>
        /// <param name="userid"></param>
        /// <param name="levelid"></param>
        /// <param name="key">未使用</param>
        /// <returns></returns>
        public async Task<string> Transfer(int userid, int levelid, string key, string gui = "", string channel = "", int simulation = 0)
        {
            //return await new TransformServices(logid).TransferAsync(gui, channel, simulation, userid, levelid, key);
            var tt = new TransferServices(logid);
            tt.pushLog("ServerList");
            NodeResult result = await tt.TransferAsync(gui, channel, simulation, userid, levelid, 1);
            if (result.result != 1)
                return result.message;
            string node = $"{result.data.Addr}:{result.data.TransferPort}|{result.data.GiveUserIP}|{result.data.From}|{result.data.UserIP}";
            return node;

        }

        /// <summary>
        /// 提供給蘋果提審使用
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="areaid"></param>
        /// <param name="levelid"></param>
        /// <param name="HDcode"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> AppleReview(int? userid = null, int? areaid = null, int? levelid = null, string HDcode = "", string key = "")
        {
            //return await new TransformServices(logid).GetAddrInAppleReviewAsync(userid, areaid, levelid, HDcode, key);
            var tt = new TransferServices(logid);
            tt.pushLog("ServerList");
            NodeResult result = await tt.AppReviewAsync(userid??0, areaid??0, levelid??0, HDcode, "0");
            if (result.result != 1)
                return result.message;
            string node = $"{result.data.Addr}:{result.data.TransferPort}|1|{result.data.From}|{result.data.UserIP}";
            return node;
        }

    }

    public class ServerNodeController : Controller
    {
        string logid = Guid.NewGuid().ToString().Substring(0, 5);

        public async Task<string> Transfer(string data)
        {   
            //decry to json string
            string j = Encoding.UTF8.GetString(AESEncryption.AESDecrypt(AESEncryption.HexToByte(data), TransferServices.SecKey));
            //json to object
            TransferData obj = JsonConvert.DeserializeObject<TransferData>(j);

            var tt = new TransferServices(logid);
            tt.pushLog("ServerNode");
            NodeResult result = await tt.TransferAsync(obj.GUI, obj.Channel, obj.Simulation, obj.UserID, obj.VIP, obj.Action);
            //object to json
            string s = JsonConvert.SerializeObject(result);
            //encry json
            string h = AESEncryption.ByteToHex(AESEncryption.AESEncrypt(s, TransferServices.SecKey)).Replace("-", "");

            return h;
        }
        public async Task<string> AppleReview(string data)
        {
            
            //decry to json string
            string j = Encoding.UTF8.GetString(AESEncryption.AESDecrypt(AESEncryption.HexToByte(data), TransferServices.SecKey));
            //json to object
            AppReviewData obj = JsonConvert.DeserializeObject<AppReviewData>(j);
            var tt = new TransferServices(logid);
            tt.pushLog("ServerNode");
            NodeResult result = await tt.AppReviewAsync(obj.UserID, obj.AreaID, obj.VIP, obj.GUI, obj.Ver);
            //object to json
            string s = JsonConvert.SerializeObject(result);
            //encry json
            string h = AESEncryption.ByteToHex(AESEncryption.AESEncrypt(s, TransferServices.SecKey)).Replace("-", "");

            return h;
        }
        /// <summary>
        /// 移除快取
        /// </summary>
        /// <returns></returns>
        public long ResetCache() { TransferServices.ResetCache(); return DateTime.Now.Ticks; }

    }

}