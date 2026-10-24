using game.Infrastructure.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Web.Http;
using game.Infrastructure.Extensions;
using gitlab_hook.Models.MergeRequest;
using game.Infrastructure.Utility;
using gitlab_hook.Models;

namespace gitlab_hook.Controllers
{

    /// <summary>
    /// 透過gitlab自動開啟MR的API
    /// </summary>
    public partial class GitAutoCreateMRController : ApiController
    {

        //----------------------
        //----------------------
        // 使用方式，重要檔案
        // AzurePipelineMappingConfig.cs 這個檔案很重要，要用來設置編號對應
        // SystemConstant.cs 管理一些全域變數
        //----------------------
        //----------------------

        Type _Plesase_watch_this_files_1 = typeof(AzurePipelineMappingConfig);
        Type _Plesase_watch_this_files_2 = typeof(SystemConstant);
        Type _Plesase_watch_this_files_3 = typeof(GitlabRepositoryEnum);


        [HttpGet, HttpPost]
        [Route("git/merge_to/UAT")]
        public string Merge_Branch_To_UAT(string type = "")
        {
            if (string.IsNullOrWhiteSpace(type))
                return "type是必要的";

            type = type.ToLower();

            switch (type)
            {
                case "ad":
                case "admin":
                case "adminsys":
                case "gameapi":
                case "game.api":
                    BranchMerge_AdminSys_Or_API("master", "UAT");
                    break;

                case "ag":
                case "agent":
                case "agent77":
                case "club":
                case "club02":
                    BranchMerge_AgentClub("master", "UAT_Agent77");
                    break;

                case "gp":
                case "gameplay":
                case "gameplay.api":
                    BranchMerge_AdminSys_Or_API("master", "UAT_GamePlayAPI");
                    break;

                default:
                    return "未定義的type";
            }

            return "OK";
        }

        [HttpGet, HttpPost]
        [Route("git/merge_to/Production")]
        public string Merge_Branch_To_Production(string type = "")
        {
            if (string.IsNullOrWhiteSpace(type))
                return "type是必要的";

            type = type.ToLower();

            switch (type)
            {
                case "ad":
                case "admin":
                case "adminsys":
                case "gameapi":
                case "game.api":
                    BranchMerge_AdminSys_Or_API("UAT", "Production");
                    break;


                case "ag":
                case "agent":
                case "agent77":
                case "club":
                case "club02":
                    BranchMerge_AgentClub("UAT_Agent77", "Production_Agent77");
                    break;

                case "pay":
                case "payment":
                case "website":
                    BranchMerge_AdminSys_Or_API("master", "Production_Payment" , "(金流支付API)");
                    break;

                case "gp":
                case "gameplay":
                case "gameplay.api":
                    BranchMerge_AdminSys_Or_API("UAT_GamePlayAPI", "Production_GamePlayAPI");
                    break;

                default:
                    return "未定義的type";                
            }

            return "OK";
        }


        private List<string> BranchMerge_AdminSys_Or_API(string sourceBranch , string targetBranch , string extraText = "")
        {
            string dateText = DateTime.UtcNow.AddHours(8).ToString("yyyyMMdd_HHmm");
            
            string prefix = $"{sourceBranch} -> {targetBranch} 分支合併 ";
            string title = $" {prefix} {dateText}";


            var allResults = new List<GitlabMergeRequestResponseModel>();


            List<string> defaultLabels = AddDefaultLableByTargetBranch(targetBranch);

            string defaultDescription = "create by api " + dateText;


            var result1 = CreateMR(GitlabRepositoryEnum.DDYNH_Backend_Game , sourceBranch, targetBranch, title, defaultDescription, defaultLabels);

            var result2 = CreateMR(GitlabRepositoryEnum.DDYNH_Using, sourceBranch, targetBranch, title, defaultDescription);

            allResults.Add(result1);
            allResults.Add(result2);

            List<string> allMrMessage = GenerateSlackMsgText(allResults);
                       
            //發送MR slack通知
            SendResultSlackMsg(allMrMessage, dateText, extraText, sourceBranch, targetBranch, prefix);
            
            return allMrMessage;
        }



        private List<string> BranchMerge_AgentClub(string sourceBranch, string targetBranch, string extraText = "")
        {
            string dateText = DateTime.UtcNow.AddHours(8).ToString("yyyyMMdd_HHmm");

            string prefix = $"{sourceBranch} -> {targetBranch} 分支合併 ";
            string title = $" {prefix} {dateText}";

            var allResults = new List<GitlabMergeRequestResponseModel>();
            
            List<string> defaultLabels = AddDefaultLableByTargetBranch(targetBranch);

            string defaultDescription = "create by api " + dateText;
            
            var result1 = CreateMR(GitlabRepositoryEnum.DDYNH_Club77, sourceBranch, targetBranch, title, defaultDescription, defaultLabels);
            var result2 = CreateMR(GitlabRepositoryEnum.DDYNH_Using, sourceBranch, targetBranch, title, defaultDescription);
            
            allResults.Add(result1);
            allResults.Add(result2);


            List<string> allMrMessage = GenerateSlackMsgText(allResults);

            //發送MR slack通知
            SendResultSlackMsg(allMrMessage, dateText, extraText, sourceBranch, targetBranch, prefix);

            return allMrMessage;
        }
        

        private void SendResultSlackMsg(List<string> allMrMessage , string dateText , string extraText , string sourceBranch ,string targetBranch , string prefix)
        {
            bool isProd = IsProduction(targetBranch);
            var targetSlackChannel = isProd ? SlackChannelEnum.backend_release_prod : SlackChannelEnum.backend_release;
            string line_Seperator = "-----------------------------------------------------------------------------------------";

            if (allMrMessage != null && allMrMessage.Count() > 0)
            {
                string msg = string.Empty;
                msg += $"{sourceBranch} --> {targetBranch} `{dateText}`  {extraText} {Environment.NewLine}";
                msg += string.Join(Environment.NewLine, allMrMessage);

                //如果是merge回production的Branch 會有額外提示
                if (isProd)
                {
                    msg += Environment.NewLine + line_Seperator;
                    msg += Environment.NewLine + "1. 後台權限？是否有增加？";
                    msg += Environment.NewLine + "2. 資料庫差異？是否已確認完畢？";
                }

                SendSlackMsg(msg , targetSlackChannel);
            }
            else
            {
                string msg = $"{prefix} 無任何差異，因此無開啟任何MR";
                SendSlackMsg(msg, targetSlackChannel);
            }
        }


        private List<string> GenerateSlackMsgText(List<GitlabMergeRequestResponseModel> allResults)
        {
            var allMrMessage = new List<string>();

            foreach (var item in allResults)
            {
                //MR重複開啟：Another open merge request already exists for this source branch
                if (item.Debug_IsFail)
                    continue;

                string projectText = GetProjectName(item.project_id);

                if (item.changes_count == "0" || string.IsNullOrWhiteSpace(item.changes_count))
                {
                    CloseMR(item.project_id , item.iid);
                    continue;
                }

                string msg = $" `{item.sha.Substring(0, 8)}` {item.web_url} （{projectText}）";
                allMrMessage.Add(msg);
            }

            return allMrMessage;
        }


        private bool IsProduction(string targetBranch)
        {
            if (targetBranch.Contains("Production"))
            {
                return true;
            }

            return false;
        }


        private List<string> AddDefaultLableByTargetBranch(string targetBranch)
        {
            if (targetBranch == "UAT")
            {
                return new List<string>
                {
                    "Merge_Branch_To_UAT"
                };
            }
            else if (targetBranch.Contains("Production"))
            {
                return new List<string>
                {
                    "Merge_Branch_To_Production"
                };
            }

            return new List<string>();
        }



        private void SendSlackMsg(string msg , SlackChannelEnum channel = SlackChannelEnum.backend_release)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Message = msg,
                Channel = channel
            });
        }



        private string GetProjectName(int number)
        {
            return ((GitlabRepositoryEnum)number).ToString();
        }


        private GitlabMergeRequestResponseModel CreateMR(
            GitlabRepositoryEnum targetRepository, 
            string sourceBranch, 
            string targetBranch, 
            string title, 
            string description , 
            List<string> labels = null)
        {
            int repId = (int)targetRepository;
            var requestPara = new RequestParameters
            {
                BaseUrl = SystemConstant.Gitlab_Url,
                ApiUrl = $"api/v4/projects/{repId}/merge_requests",
                Method = HttpMethodType.HttpPost,
            };

            if(labels == null)
            {
                labels = new List<string>();
            }

            title = title.Replace("-->", "to").Replace("->", "to");

            var postBody = new
            {
                source_branch = sourceBranch,
                target_branch = targetBranch,
                title = title,
                description = description,
                labels = labels
            };

            requestPara.AddHeaderData("PRIVATE-TOKEN", SystemConstant.Gitlab_Token);

            var res = APIRequestHelper.RequestApiResult<GitlabMergeRequestResponseModel>(requestPara, postBody);

            if (res.Exception != null)
            {
                return new GitlabMergeRequestResponseModel
                {
                    Debug_IsFail = true,
                    Debug_Response = res.ResponseText
                };
            }

            return res.ResultObject;
        }


        private GitlabMergeRequestResponseModel CloseMR(int rep_id , int mr_id)
        {
            var requestPara = new RequestParameters
            {
                BaseUrl = SystemConstant.Gitlab_Url,
                ApiUrl = $"api/v4/projects/{rep_id}/merge_requests/{mr_id}",
                Method = HttpMethodType.HttpPut,
            };

            var postBody = new
            {
                state_event = "close"
            };

            requestPara.AddHeaderData("PRIVATE-TOKEN", SystemConstant.Gitlab_Token);

            var res = APIRequestHelper.RequestApiResult<GitlabMergeRequestResponseModel>(requestPara, postBody);

            if (res.Exception != null)
            {
                return new GitlabMergeRequestResponseModel
                {
                    Debug_IsFail = true,
                    Debug_Response = res.ResponseText
                };
            }

            return res.ResultObject;
        }

    }



}
