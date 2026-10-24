using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using Repository.Entity.agentclub;
using Repository.Models;

namespace Agent.Helper
{
    public class verification
    {
        public static bool ValidateToken(string token, ref Repository.Entity.agent_total userInfo)
        {
            var simplePrinciple = JwtManager.GetPrincipal(token);

            if (simplePrinciple == null) return false;

            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            var userData = identity.FindFirst(ClaimTypes.UserData);

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            userInfo = json_serializer.Deserialize<Repository.Entity.agent_total>(userData.Value);

            if (userInfo == null)
                return false;

            return true;
        }

        public static bool ValidateToken(string token, ref AgentUsers userInfo)
        {
            var simplePrinciple = JwtManager.GetPrincipal(token);

            if (simplePrinciple == null) return false;

            var identity = simplePrinciple.Identity as ClaimsIdentity;

            if (identity == null)
                return false;

            if (!identity.IsAuthenticated)
                return false;

            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            var userData = identity.FindFirst(ClaimTypes.UserData);

            JavaScriptSerializer json_serializer = new JavaScriptSerializer();
            userInfo = json_serializer.Deserialize<AgentUsers>(userData.Value);

            if (userInfo == null)
                return false;

            return true;
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            
            StringBuilder sBuilder = new StringBuilder();
           
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}