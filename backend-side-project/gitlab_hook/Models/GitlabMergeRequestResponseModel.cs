using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gitlab_hook.Models
{



    public class TimeStats
    {
        public int time_estimate { get; set; }
        public int total_time_spent { get; set; }
        public object human_time_estimate { get; set; }
        public object human_total_time_spent { get; set; }
    }

    public class TaskCompletionStatus
    {
        public int count { get; set; }
        public int completed_count { get; set; }
    }

    public class DiffRefs
    {
        public string base_sha { get; set; }
        public string head_sha { get; set; }
        public string start_sha { get; set; }
    }

    public class User
    {
        public bool can_merge { get; set; }
    }

    public class GitlabMergeRequestResponseModel
    {
        public int id { get; set; }
        public int iid { get; set; }
        public int project_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string state { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object merged_by { get; set; }
        public object merged_at { get; set; }
        public object closed_by { get; set; }
        public object closed_at { get; set; }
        public string target_branch { get; set; }
        public string source_branch { get; set; }
        public int user_notes_count { get; set; }
        public int upvotes { get; set; }
        public int downvotes { get; set; }
        public object assignee { get; set; }
        public Author author { get; set; }
        public List<object> assignees { get; set; }
        public int source_project_id { get; set; }
        public int target_project_id { get; set; }
        public List<object> labels { get; set; }
        public bool work_in_progress { get; set; }
        public object milestone { get; set; }
        public bool merge_when_pipeline_succeeds { get; set; }
        public string merge_status { get; set; }
        public string sha { get; set; }
        public object merge_commit_sha { get; set; }
        public object discussion_locked { get; set; }
        public object should_remove_source_branch { get; set; }
        public object force_remove_source_branch { get; set; }
        public string reference { get; set; }
        public string web_url { get; set; }
        public TimeStats time_stats { get; set; }
        public bool squash { get; set; }
        public TaskCompletionStatus task_completion_status { get; set; }
        public bool subscribed { get; set; }
        public string changes_count { get; set; }
        public object latest_build_started_at { get; set; }
        public object latest_build_finished_at { get; set; }
        public object first_deployed_to_production_at { get; set; }
        public object pipeline { get; set; }
        public object head_pipeline { get; set; }
        public DiffRefs diff_refs { get; set; }
        public object merge_error { get; set; }
        public User user { get; set; }






        public bool Debug_IsFail { get; set; }
        public string Debug_Response { get; set; }





    }




}