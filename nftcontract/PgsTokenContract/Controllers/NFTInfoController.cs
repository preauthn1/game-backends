using Microsoft.AspNetCore.Mvc;
using target.Utility;
using PgsTokenContract.Models;
using PgsTokenContract.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace target.API.AdminSys.Controllers
{
   
    public class NFTInfoController : BaseController
    {
        public NFTService NFTService = new NFTService();


        [Route("NFT/{tokenID}/info")]
        [HttpGet]
        public ApiResult<NFTInfoViewModel> GetNFTInfo(int tokenID)
        {
            var result = NFTService.GetNFTInfo(tokenID);
            return ConvertToStandardFormat(result);
        }

        [Route("NFT/dragonTiger/{tokenID}/info")]
        [HttpGet]
        public ApiResult<NFTInfoViewModel> GetDragonTigerNFTInfo(int tokenID)
        {
            var result = NFTService.GetNFTInfoByContractConfig(tokenID , ContractConfig.NFTContractDragonTiger);
            return ConvertToStandardFormat(result);
        }


        [Route("NFT/mint")]
        [HttpPost]
        public ApiResult<NFTMintResultViewModel> MintNFT(NFTMintRequestModel model)
        {
            var result = NFTService.MintNFT(model);
            return ConvertToStandardFormat(result);
        }


        [Route("NFT/TokenURI/{tokenID}")]
        [Route("NFT/TokenURI/v2/{tokenID}")]
        [Route("NFT/TokenURI/v3/{tokenID}")]
        [Route("NFT/TokenURI/v4/{tokenID}")]
        [HttpGet]
        public object NFTURIInfo(int tokenID)
        {
            var result = TryGetNFTInfo(tokenID, ContractConfig.NFTContract);

            string defaultName = $"target NFT id {tokenID}.";

            if(result != null)
            {
                defaultName = result.Name;
            }

            string imageUrl = "https://example.invalid/public/365949f1-1495-42a2-b194-0bf94229cc90.png";
            
            return new 
            {
                description = "This is target NFT.",
                name = defaultName,
                external_url = "https://example.invalid/",
                image = imageUrl,
                attributes = new List<object>(),
            };
        }



        [Route("NFT/DragonTiger/TokenURI/{tokenID}")]
        [Route("NFT/DragonTiger/TokenURI/v2/{tokenID}")]
        [Route("NFT/DragonTiger/TokenURI/v3/{tokenID}")]
        [Route("NFT/DragonTiger/TokenURI/v4/{tokenID}")]
        [HttpGet]
        public object DragonTigerNFTURIInfo(int tokenID)
        {
            var result = TryGetNFTInfo(tokenID , ContractConfig.NFTContractDragonTiger);

            string defaultName = $"DragonTiger NFT id {tokenID}.";

            if (result != null)
            {
                defaultName = result.Name;
            }

            string imageUrl = "https://example.invalid/game-icon/lhd.png";

            return new
            {
                description = "This is target NFT.",
                name = defaultName,
                external_url = "https://example.invalid/",
                image = imageUrl,
                attributes = new List<object>(),
            };
        }


        private NFTInfoViewModel TryGetNFTInfo(int tokenID , ContractConfigModel config)
        {
            try
            {
                return NFTService.GetNFTInfoByContractConfig(tokenID , config);
            }
            catch
            {

            }

            return null;
        }



    }
}
