using game.Entity.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class THubPlatformGameLaunchRequestParameter
    {
        public string ProviderCode { get; set; }

        public int GameCode { get; set; }

        public string AuthToken { get; set; }

        public int Platform { get; set; }

        public bool Demo { get; set; }

        public string Language { get; set; }

        public string BackUrl { get; set; }

        public int Orientation { get; set; }
    }


    public class THubPlatformGameLaunchResponseModel : BaseThubRequestResponseModel
    {
        [JsonProperty("Value")]
        public string Value { get; set; }       
    }



    public class THubPlatformGetGameListRequestParameter
    {
        public string ProviderCode { get; set; }

        public int PageNo { get; set; }

        public int PageSize { get; set; }

    }


    public class THubPlatformGetGameListResponseModel : BaseThubRequestResponseModel
    {

        [JsonProperty("Value")]
        public THubPlatformGameListValueModel Value { get; set; }

    }


    public class THubPlatformGameListValueModel
    {
        [JsonProperty("Total")]
        public int Total { get; set; }

        [JsonProperty("Games")]
        public List<THubPlatformGameListDetailItemModel> Games { get; set; }

    }


    public class THubPlatformGameListDetailItemModel
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Type")]
        public int Type { get; set; }

        [JsonProperty("Category")]
        public int Category { get; set; }

        [JsonProperty("HasDemo")]
        public bool HasDemo { get; set; }

        [JsonProperty("HasJackpot")]
        public bool HasJackpot { get; set; }

        [JsonProperty("HasFreespin")]
        public bool HasFreespin { get; set; }

        [JsonProperty("HasBuyFeature")]
        public bool HasBuyFeature { get; set; }

        [JsonProperty("AddedTimestamp")]
        public string AddedTimestamp { get; set; }

        [JsonProperty("NameLocalizations")]
        public List<THubGameListNameLocalizations> NameLocalizations  { get; set; }

        [JsonProperty("Thumbnails")]
        public List<THubGameListNameThumbnails> Thumbnails { get; set; }

    }



    public class THubGameListNameLocalizations
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Language")]
        public string Language { get; set; }
    }


    public class THubGameListNameThumbnails
    {
        [JsonProperty("Language")]
        public string Language  { get; set; }

        [JsonProperty("ImagePath")]
        public string ImagePath { get; set; }

        [JsonProperty("VideoPath")]
        public string VideoPath { get; set; }
    }


}
