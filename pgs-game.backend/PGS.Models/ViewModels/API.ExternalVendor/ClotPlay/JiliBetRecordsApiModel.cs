using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class ClotPlayFetchWagersApiResponseModel
    {
        public List<ClotPlayFetchWagersDetailItemModel> Data { get; set; }
    }

    public class ClotPlayFetchWagersDetailItemModel
    {
        public string Id { get; set; }
        public decimal bet_amount { get; set; }
        public decimal winning { get; set; }
        public string game_id { get; set; }
        public string game_slug { get; set; }
        public string player_unique_id { get; set; }
        public string agent_code { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string game_round_id { get; set; }

    }
}
