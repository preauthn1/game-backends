using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Utility.Extensions;
using target.Utility.Helper.Dependency;
using target.Models.Table;
using target.Utility;
using target.Models.Models;
using target.Models;
using target.Utility.Helper;
using System.Reflection;
using target.Web.Modules.Filter;
using target.Utility.Utility.ThirdParty;

namespace target.Utility.Extensions.Web
{
    public class ApiConfigDefineHelper
    {
        private static Dictionary<string, ApiConfigAnalyzeModel> _allApiActions = new Dictionary<string, ApiConfigAnalyzeModel>();

        private static List<string> _skipLogResponseApis = new List<string>();

        private static List<string> _skipLogRequestBodyApis = new List<string>();


        /// <summary>
        /// 專案啟動後，先解析所有API是否有標記apiConfig或某些特殊attribute
        /// </summary>
        public static void AnalyzeAllApiConfigDefines(Assembly targetProject , bool isGenerateTypeScriptText = false)
        {
            var allController = targetProject.GetTypes()
                     .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
                     .ToList();

            foreach(var controller in allController)
            {
                var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                    .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                    .ToList();

                foreach(MethodInfo eachAction in methods)
                {
                    string controllerName = eachAction.DeclaringType.Name.Replace("Controller", "");
                    string actionUniqueName = $"{controllerName}_{eachAction.Name}";

                    var loggingAttributes = (LoggingPlayerID)eachAction.GetCustomAttributes(typeof(LoggingPlayerID), false).FirstOrDefault();

                    var configAttributes = (ApiConfig)eachAction.GetCustomAttributes(typeof(ApiConfig), false).FirstOrDefault();

                    ApiConfigAnalyzeModel apiConfig = InstanceDefaultApiConfigValue();

                    if (configAttributes != null)
                    {
                        //若有自訂配置 以自訂的attribute設定為優先
                        apiConfig = configAttributes.ReturnInjecteObject<ApiConfigAnalyzeModel>();
                    }

                    if (loggingAttributes != null)
                        apiConfig.IsLoggingPlayerID = loggingAttributes.IsEnable;


                    //測試環境才會產生的內容 (產生typeScript程式碼)
                    if(isGenerateTypeScriptText && SettingHelper.IsDev)
                    {
                        string allClassDefineText = GetTypeScriptClassDefine(eachAction, actionUniqueName);
                        apiConfig.Develop_TypeScriptClassDefine = allClassDefineText;
                    }

                    try
                    {
                        _allApiActions.Add(actionUniqueName, apiConfig);
                    }
                    catch(Exception ex)
                    {
                        //好多次了，發個通知吧
                        MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                        {
                            TelegramChannelEnum = TelegramChannelEnum.pgs_common_error,
                            Message = "AnalyzeAllApiConfigDefines_" + ex.Message
                        });

                        //如果你看到這邊拋錯誤 代表有API的method名稱一樣 請將method改成不一樣(不跟其他重複)的名稱
                        //如果你看到這邊拋錯誤 代表有API的method名稱一樣 請將method改成不一樣(不跟其他重複)的名稱
                        //如果你看到這邊拋錯誤 代表有API的method名稱一樣 請將method改成不一樣(不跟其他重複)的名稱
                        //如果你看到這邊拋錯誤 代表有API的method名稱一樣 請將method改成不一樣(不跟其他重複)的名稱
                        throw;
                    }


                }
            }

            return;
        }



        private static string GetApiUrl(MethodInfo eachAction)
        {
            try
            {
                return ((RouteAttribute)eachAction.GetCustomAttribute(typeof(RouteAttribute)))?.Template;
            }
            catch
            {

            }

            return string.Empty;
        }

        private static bool IsHttpGet(MethodInfo eachAction)
        {
            try
            {
                return ((HttpGetAttribute)eachAction.GetCustomAttribute(typeof(HttpGetAttribute))) != null;
            }
            catch
            {

            }

            return false;
        }



        private static string GetTypeScriptClassDefine(MethodInfo eachAction, string actionUniqueName)
        {
            string requestClassName = string.Empty;
            string returnClassName = string.Empty;

            var parameters = eachAction.GetParameters().Where(z => z.ParameterType.IsClass).ToList();

            string allClassDefineText = string.Empty;

            //debug用
            if (actionUniqueName == "Dropdown_GetAvailableOrgs")
            {
                string debugggggggg = "";
            }

            if (parameters.IsNotEmptyNull())
            {
                var parameterType = parameters.FirstOrDefault();

                var requestClass = TypeScriptConvert.ConvertClassToTypeScriptModel(parameterType.ParameterType, actionUniqueName);

                requestClassName = requestClass.LootClassName;
                allClassDefineText += requestClass.FinalClassText;
            }

            var returnType = eachAction.ReturnType;

            if (returnType != null)
            {
                var returnClass = TypeScriptConvert.ConvertClassToTypeScriptModel(returnType, actionUniqueName);

                returnClassName = returnClass.LootClassName;
                allClassDefineText += returnClass.FinalClassText;
            }

            allClassDefineText += Environment.NewLine + GenerateApiRequestTypeScriptCode(eachAction , requestClassName, returnClassName);
            return allClassDefineText;
        }


        private static string GenerateApiRequestTypeScriptCode(MethodInfo method , string requestClass , string returnClass)
        {
            string apiUrl = GetApiUrl(method);

            bool isWithoutParameter = string.IsNullOrWhiteSpace(requestClass);

            bool isHttpGet = IsHttpGet(method);

            string methodName = method.Name.ToCamelCase();

            string modelPrefix = "ResponseModel.";

            string parameterName = isHttpGet ? "filter:": "payload:";
            parameterName = isWithoutParameter ? "" : parameterName;

            string postParameter = isHttpGet ? "" : Environment.NewLine + " data: payload ";


            if (!isWithoutParameter)
                requestClass = modelPrefix + requestClass;

            if (!string.IsNullOrWhiteSpace(returnClass))
                returnClass =  modelPrefix + returnClass;

            string methodType = isHttpGet ? "get" : "post";

            if(isHttpGet)
            {
                apiUrl += "?${Qs.stringify(filter)}";
            }

            string text = string.Format(
                            @"async {0}({1}):Promise<ResponseModel.IResponseData<{2}>> {{
		                        const config = {{
			                        url: `{3}`,
			                        method: '{4}', {5}
		                        }};
		                        const result = await Handler.request(config);
		                        return (<ResponseModel.IResponseData<{6}>>result.data);
	                        }},", methodName,(parameterName + requestClass) , returnClass, apiUrl, methodType, postParameter, returnClass);

            return text;
        }


        public static ApiConfigAnalyzeModel GetApiConfigSettingByCurrentRequest()
        {
            string uniqueRouteName = HeaderValueHelper.HeaderTemp_ApiUniqueRouteName();

            return GetApiConfigSettingByUniqueRouteName(uniqueRouteName);
        }

        public static ApiConfigAnalyzeModel GetApiConfigSettingByUniqueRouteName(string uniqueRouteName)
        {
            if (string.IsNullOrWhiteSpace(uniqueRouteName))
                return InstanceDefaultApiConfigValue();

            if (!_allApiActions.ContainsKey(uniqueRouteName))
                return InstanceDefaultApiConfigValue();

            return _allApiActions[uniqueRouteName];
        }


        private static ApiConfigAnalyzeModel InstanceDefaultApiConfigValue()
        {
            //這邊有定義 apiConfig各種預設值
            var defaultSetting = new ApiConfig();

            var convert = defaultSetting.ReturnInjecteObject<ApiConfigAnalyzeModel>();
            convert.IsLoggingPlayerID = false;

            return convert;
        }




    }
}
