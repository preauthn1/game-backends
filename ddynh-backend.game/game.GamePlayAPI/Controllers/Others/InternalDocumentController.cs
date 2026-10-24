using FW.Core.Helper.Enum;
using game.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace game.GamePlayAPI.Controllers
{
    public class InternalDocumentController : BaseController
    {
        
        /// <summary>
        /// 列出後端定義的所有enum
        /// </summary>
        [HttpGet]
        [Route("enums")]
        public object GetAllEnumsList()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            var allEnums = GetAllEnums();
            
            string hostUrl = System.Web.HttpContext.Current.Request.Url.Authority;

            foreach (var item in allEnums)
            {
                dic.Add(item.Name, string.Format("http://{0}/enums/{1}", hostUrl, item.Name.ToString()));
            }

            return dic
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, z => z.Value);
        }

     
        [HttpGet]
        [Route("enums/{enumName}")]
        public dynamic GetAllEnumsList(string enumName)
        {
            Dictionary<string, int> enumValues = new Dictionary<string, int>();
            Dictionary<string, string> descriptionValues = new Dictionary<string, string>();


            var allEnums = GetAllEnums();

            foreach (Type type in allEnums)
            {
                if (type.Name == enumName)
                {
                    foreach (var enumValue in System.Enum.GetValues(type))
                    {
                        int intValue = enumValue.GetHashCode();
                        AddNoneDuplicateKeys(enumValues, enumValue.ToString(), intValue);

                        var desAttr = ExtensionOfEnum.GetAttributeFromEnum<EnumDescriptionAttribute>((Enum)enumValue);

                        if (desAttr != null)
                        {
                            AddNoneDuplicateKeys(descriptionValues, enumValue.ToString(), desAttr.DisplayName);
                        }
                    }
                    break;
                }
            }

            return new
            {
                allValues = enumValues,
                allText = descriptionValues
            };
        }


        private void AddNoneDuplicateKeys<T>(Dictionary<string, T> dic, string key, T value)
        {
            if (dic.ContainsKey(key))
                return;

            dic.Add(key, value);
        }

        

        private List<Type> GetAllEnums()
        {
            List<string> allNamespace = new List<string>
            {
                "game.Entity",
                "game.Entity.Event",
                "game.Entity.Money",
                "game.Entity.Users",
                "game.Entity.Area",
                "game.Entity.Game",
                "game.Entity.Time",
                "game.Entity.Pay",
                "game.Entity.Warning",
                "game.Entity.Customer",
                "game.Entity.AdminSys",
                "game.Entity.Message",
                "game.Entity.Notify",
                "game.Entity.ForbidUser",
                "game.Entity.Enums.ThirdPartyPay"
            };

            allNamespace = allNamespace.Distinct().ToList();

            List<Type> all = new List<Type>();
            foreach (string nameSpace in allNamespace)
            {
                var listOfEnums1 = GetTypesInNamespace(Assembly.Load("game.Entity"), nameSpace);

                all.AddRange(listOfEnums1);
            }

            List<string> allNamespace_infra = new List<string>
            {
                "game.Entity",               
            };
            
            foreach (string nameSpace in allNamespace_infra)
            {
                var listOfEnums1 = GetTypesInNamespace(Assembly.Load("game.Infrastructure"), nameSpace);
                all.AddRange(listOfEnums1);
            }


            return all;
        }



        private List<Type> GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly
                .GetTypes()
                .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal) &&  !t.IsClass && t.IsEnum)
                .ToList();
        }



    }
}
