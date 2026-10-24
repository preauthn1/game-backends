//using game.Infrastructure.Utility.ThirdParty;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using example.invalid;
//using example.invalid.Http;
//using System.Web.Http;
//using game.Infrastructure.Extensions;
//using gitlab_hook.Models.MergeRequest;
//using game.Infrastructure.Utility;

//namespace gitlab_hook.Controllers
//{

//    /// <summary>
//    /// 接收gitlab的hook事件，觸發某些行為
//    /// </summary>
//    public class HookApiController : ApiController
//    {

//        //----------------------
//        //----------------------
//        // 使用方式，重要檔案
//        // AzurePipelineMappingConfig.cs 這個檔案很重要，要用來設置編號對應
//        // SystemConstant.cs 管理一些全域變數
//        //----------------------
//        //----------------------

//        Type _plesase_watch_this_files_1 = typeof(AzurePipelineMappingConfig);
//        Type _plesase_watch_this_files_2 = typeof(SystemConstant);
//        Type _plesase_watch_this_files_3 = typeof(GitlabRepositoryEnum);
        

//        public static bool _slackMsg_enable_flag = false;


//        [HttpGet, HttpPost]
//        [Route("setting/flag")]
//        public string OpenFlag(bool boolValue = false)
//        {
//            _slackMsg_enable_flag = boolValue;
//            return _slackMsg_enable_flag.ToString();
//        }



//        [HttpGet, HttpPost]
//        [Route("azure/pipe")]
//        public string TriggerAzurePipeline(int definitionId = 1)
//        {
//            TriggerAzurePipelineByApi(definitionId, "UAT");
//            return "";
//        }



//        [HttpGet , HttpPost]
//        [Route("hook/git")]
//        public string DoHookLogic()
//        {
//            string body = System.Web.HttpContext.Current.Request.GetRequestBodyString();

//            //如果有開啟，總是發slack訊息
//            if(_slackMsg_enable_flag)
//            {
//                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
//                {
//                    Message = body,
//                    Channel = SlackChannelEnum.n_gitlab_hook
//                });
//            }


//            var convert = Newtonsoft.Json.JsonConvert.DeserializeObject<GitlabHookMergeRequestEventModel>(body);

//            if (convert == null)
//            {
//                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
//                {
//                    Message = "無法解析request",
//                    Channel = SlackChannelEnum.n_gitlab_hook
//                });
//                return "";
//            }

//            string repName = convert.project.name;

//            string mrName = convert.object_attributes.title;

//            string userName = convert.user.name;

//            var isMerged = convert.changes.state.current == "merged";

//            var isClosed = convert.changes.state.current == "closed";


//            if(isMerged)
//            {
//                string mMsg = $"`{repName}` , MR: {mrName} 已被 `@{userName}` 合併。 ";

//                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
//                {
//                    Message = mMsg,
//                    Channel = SlackChannelEnum.n_gitlab_hook
//                });

//                string targetBranch = convert.object_attributes.target_branch;

//                var jenkinsTriggerUrl = GetJenkinsTriggerUrlByRepName(repName , targetBranch);

//                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
//                {
//                    Message = $"def編號：{jenkinsTriggerUrl}，並且targetBranch：{targetBranch}",
//                    Channel = SlackChannelEnum.n_gitlab_hook
//                });

//                if (string.IsNullOrWhiteSpace(jenkinsTriggerUrl))
//                {
//                    TriggerAzurePipelineByApi(definitionId , targetBranch);
//                }
//            }
//            else if(isClosed)
//            {
//                string mMsg = $"`{repName}` , MR: {mrName} 已被 `@{userName}` 關閉。 ";

//                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
//                {
//                    Message = mMsg,
//                    Channel = SlackChannelEnum.n_gitlab_hook
//                });
//            }
//            else
//            {
               
//            }

//            return "OK";
//        }



//        private string GetJenkinsTriggerUrlByRepName(string repName, string targetBranch)
//        {
//            string repNameToLower = repName.ToLower();

//            //記得去這邊設置編號
//            return AzurePipelineMappingConfig.GetJenkinsTriggerUrl(repName, targetBranch);
//        }


//        private void TriggerAzurePipelineByApi(string jenkinsUrl , string defaultBranach)
//        {
//            //definition對照表，
//            //編號5：AdminSys專案 + Game.Api


//            string org = SystemConstant.AzurePipeline_OrgName;
//            string project = SystemConstant.AzurePipeline_ProjectName;

//            var requestPara = new RequestParameters
//            {
//                BaseUrl = jenkinsUrl,
//                ApiUrl = APIRequestHelper.No_Url_Keyword,
//                Method = HttpMethodType.HttpPost
//            };

//            Dictionary<string, string> otherPara = new Dictionary<string, string>();
//            otherPara.Add("BuildConfiguration", "release");
//            otherPara.Add("BuildPlatform", "any cpu");
//            otherPara.Add("system.debug", "false");

//            var postBody = new
//            {
//                agentSpecification = new
//                {
//                    identifier = "vs2017-win2016"
//                },
//                definition = new 
//                {
//                    id = definitionId
//                },
//                sourceBranch = $"refs/heads/{defaultBranach}",
//                sourceVersion = "",
//                reason = "1",
//                demands = new List<string>(),
//                parameters = Newtonsoft.Json.JsonConvert.SerializeObject(otherPara),
//            };

//            //add_new_sln

//            requestPara.AddHeaderData("Authorization", SystemConstant.Azure_Devops_Token);

//            var res = APIRequestHelper.RequestApiResult<string>(requestPara, postBody);

//            string text = res.ResponseText;

//            if(res.Exception != null)
//            {
//                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
//                {
//                    Message = "triggerAzure失敗",
//                    Channel = SlackChannelEnum.n_gitlab_hook
//                }); 
//            }
//            else
//            {
//                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
//                {
//                    Message = "觸發成功～",
//                    Channel = SlackChannelEnum.n_gitlab_hook
//                });
//            }

//            return;



//        }







//    }
//}
