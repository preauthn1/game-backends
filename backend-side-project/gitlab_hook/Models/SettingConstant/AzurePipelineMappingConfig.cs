using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitlab_hook.Models.MergeRequest
{

    public static class AzurePipelineMappingConfig
    {

        private static string _jenkinsHost = "http://0.0.0.0:8080";
        private static string _jenkinsToken = [REDACTED]";


        private static List<MappingConfigItemModel> GetItems()
        {
            return new List<MappingConfigItemModel>
            {
                new MappingConfigItemModel(GenerateRemoteTriggerJenkinsUrl("DD.Website.UAT") ,"backend.game" , "UAT" ),
                new MappingConfigItemModel(GenerateRemoteTriggerJenkinsUrl("DD.Website.Production") ,"backend.game" , "Production" ),
            };
        }


        private static string GenerateRemoteTriggerJenkinsUrl(string jenkins_job_name)
        {
            return $"{_jenkinsHost}/job/{jenkins_job_name}/build?token=[REDACTED]";
        }


        public static string GetJenkinsTriggerUrl(string repName, string targetBranch)
        {
            repName = repName.ToLower();
            targetBranch = targetBranch.ToLower();

            var allItems = GetItems();

            var match = allItems.FirstOrDefault(x => x.RepositoryName == repName && x.TargetBranchName == targetBranch);

            if (match == null)
                return "";

            return match.JenkinsTriggerUrl;
        }

    }


    public class MappingConfigItemModel
    {
        public MappingConfigItemModel() { }


        public MappingConfigItemModel(string jenkinsUrl, string repName, string change_branchName)
        {
            this.RepositoryName = repName;
            this.TargetBranchName = change_branchName;
            this.JenkinsTriggerUrl = jenkinsUrl;

        }

        public string RepositoryName { get; set; }
        public string TargetBranchName { get; set; }
        public string JenkinsTriggerUrl { get; set; }

    }


}