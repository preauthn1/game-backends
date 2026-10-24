using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitlab_hook.Models.MergeRequest
{

    public static class SystemConstant
    {

        public static string Gitlab_Token = [REDACTED]";

        public const string Gitlab_Url = "http://example.invalid:1688";


    }


    /// <summary>
    /// 請設定為Gitlab的 projectID
    /// </summary>
    public enum GitlabRepositoryEnum
    {
        DDYNH_Using = 197,

        DDYNH_Backend_Game = 196,

        DDYNH_Club77 = 198
    }



}