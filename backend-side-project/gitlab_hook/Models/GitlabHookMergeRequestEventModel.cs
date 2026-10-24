using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitlab_hook.Models.MergeRequest
{

    public class User
    {
        public string name { get; set; }
        public string username { get; set; }
        public string avatar_url { get; set; }
    }

    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string web_url { get; set; }
        public object avatar_url { get; set; }
        public string git_ssh_url { get; set; }
        public string git_http_url { get; set; }
        public string @namespace { get; set; }
        public int visibility_level { get; set; }
        public string path_with_namespace { get; set; }
        public string default_branch { get; set; }
        public object ci_config_path { get; set; }
        public string homepage { get; set; }
        public string url { get; set; }
        public string ssh_url { get; set; }
        public string http_url { get; set; }
    }

    public class MergeParams
    {
        public string force_remove_source_branch { get; set; }
    }

    public class Source
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string web_url { get; set; }
        public object avatar_url { get; set; }
        public string git_ssh_url { get; set; }
        public string git_http_url { get; set; }
        public string @namespace { get; set; }
        public int visibility_level { get; set; }
        public string path_with_namespace { get; set; }
        public string default_branch { get; set; }
        public object ci_config_path { get; set; }
        public string homepage { get; set; }
        public string url { get; set; }
        public string ssh_url { get; set; }
        public string http_url { get; set; }
    }

    public class Target
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string web_url { get; set; }
        public object avatar_url { get; set; }
        public string git_ssh_url { get; set; }
        public string git_http_url { get; set; }
        public string @namespace { get; set; }
        public int visibility_level { get; set; }
        public string path_with_namespace { get; set; }
        public string default_branch { get; set; }
        public object ci_config_path { get; set; }
        public string homepage { get; set; }
        public string url { get; set; }
        public string ssh_url { get; set; }
        public string http_url { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public string email { get; set; }
    }

    public class LastCommit
    {
        public string id { get; set; }
        public string message { get; set; }
        public DateTime timestamp { get; set; }
        public string url { get; set; }
        public Author author { get; set; }
    }

    public class ObjectAttributes
    {
        public object assignee_id { get; set; }
        public int author_id { get; set; }
        public string created_at { get; set; }
        public string description { get; set; }
        public object head_pipeline_id { get; set; }
        public int id { get; set; }
        public int iid { get; set; }
        public object last_edited_at { get; set; }
        public object last_edited_by_id { get; set; }
        public string merge_commit_sha { get; set; }
        public object merge_error { get; set; }
        public MergeParams merge_params { get; set; }
        public string merge_status { get; set; }
        public object merge_user_id { get; set; }
        public bool merge_when_pipeline_succeeds { get; set; }
        public object milestone_id { get; set; }
        public string source_branch { get; set; }
        public int source_project_id { get; set; }
        public string state { get; set; }
        public string target_branch { get; set; }
        public int target_project_id { get; set; }
        public int time_estimate { get; set; }
        public string title { get; set; }
        public string updated_at { get; set; }
        public object updated_by_id { get; set; }
        public string url { get; set; }
        public Source source { get; set; }
        public Target target { get; set; }
        public LastCommit last_commit { get; set; }
        public bool work_in_progress { get; set; }
        public int total_time_spent { get; set; }
        public object human_total_time_spent { get; set; }
        public object human_time_estimate { get; set; }
        public List<object> assignee_ids { get; set; }
        public string action { get; set; }
    }

    public class State
    {
        public string previous { get; set; }
        public string current { get; set; }
    }

    public class UpdatedAt
    {
        public string previous { get; set; }
        public string current { get; set; }
    }

    public class TotalTimeSpent
    {
        public object previous { get; set; }
        public int current { get; set; }
    }

    public class Changes
    {
        public State state { get; set; }
        public UpdatedAt updated_at { get; set; }
        public TotalTimeSpent total_time_spent { get; set; }
    }

    public class Repository
    {
        public string name { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string homepage { get; set; }
    }

    public class GitlabHookMergeRequestEventModel
    {
        public string object_kind { get; set; }
        public string event_type { get; set; }
        public User user { get; set; }
        public Project project { get; set; }
        public ObjectAttributes object_attributes { get; set; }
        public List<object> labels { get; set; }
        public Changes changes { get; set; }
        public Repository repository { get; set; }
    }

}