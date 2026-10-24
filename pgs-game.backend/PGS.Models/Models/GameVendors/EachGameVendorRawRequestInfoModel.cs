using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{



    /// <summary>
    /// 根據遊戲商不同 可能有不同的物件、原始欄位可操作
    /// 建議在 APIController將這些物件傳進class，以利後續interface操作
    /// </summary>
    public class EachGameVendorRawRequestInfoModel
    {

        public FunkyApiRawRequestInfoVendor Funky { get; set; }

        public AGGApiRawRequestInfoVendor AGG { get; set; }

        public AGGLiveApiRawRequestInfoVendor AGGLive { get; set; }

        public AGGLiveApiRawRequestInfoVendor YesGame { get; set; }

        public AGGFishApiRawRequestInfoVendor AGGFish { get; set; }

        public FishingApiRawRequestInfoVendor Fishing { get; set; }

        public JiliApiRawRequestInfoVendor Jili { get; set; }

        public MarioApiRawRequestInfoVendor Mario { get; set; }

        public GPIApiRawRequestInfoVendor GPI { get; set; }

        public ATApiRawRequestInfoVendor AT { get; set; }

        public ATGanesApiRawRequestInfoVendor ATGames { get; set; }

        public GalaxyApiRawRequestInfoVendor Galaxy { get; set; }

        public AGinApiRawRequestInfoVendor AGin { get; set; }

        public ClotPlayApiRawRequestInfoVendor ClotPlay { get; set; }

        public RGApiRawRequestInfoVendor RG { get; set; }

        public SlotManiaApiRawRequestInfoVendor SlotMania { get; set; }

        public FumaApiRawRequestInfoVendor Fuma { get; set; }

        public EFGlayApiRawRequestInfoVendor EFG { get; set; }

    }


    public class FunkyApiRawRequestInfoVendor
    {
        public FunkySingleWalletAPIPlaceBetRequestModel PlaceBetInfo { get; set; }

        public FunkySingleWalletAPISettleBetRequestModel SettleBetInfo { get; set; }

        public FunkySingleWalletCancelBetRequestModel CancelBet { get; set; }

    }


    public class AGGApiRawRequestInfoVendor
    {
        public AGGSingleWalletAPITransactionRequestModel PlaceBetInfo { get; set; }

        public AGGSingleWalletAPITransactionRequestModel SettleBetInfo { get; set; }

        public AGGSingleWalletAPITransactionRequestModel CancelBet { get; set; }

    }


    public class AGGLiveApiRawRequestInfoVendor
    {
        public AGGLivePlaceBetDetailRequestModel PlaceBetInfo { get; set; }

        public AGGLiveSettleDetailRequestModel SettleBetInfo { get; set; }

        public AGGLiveCancelDetailRequestModel CancelBet { get; set; }

        public AGGLiveRefundDetailRequestModel Refund { get; set; }

    }

    public class AGGFishApiRawRequestInfoVendor
    {
        public AGGLivePlaceBetDetailRequestModel PlaceBetInfo { get; set; }

        public AGGFishSettleDetailRequestModel SettleBetInfo { get; set; }

        public AGGLiveCancelDetailRequestModel CancelBet { get; set; }

        public AGGLiveRefundDetailRequestModel Refund { get; set; }

    }


    public class FishingApiRawRequestInfoVendor
    {
        public FishingSingleWalletCheckAccountTokenRequestModel TokenAuth { get; set; }

        public FishingSingleWalletPlaceBetRequestModel PlaceBetInfo { get; set; }

        public FishingSingleWalletSettleBetRequestModel SettleBetInfo { get; set; }

        public AGGSingleWalletAPITransactionRequestModel CancelBet { get; set; }

    }



    public class JiliApiRawRequestInfoVendor
    {
        public JiliAuthAPIRequestModel TokenAuth { get; set; }

        public JiliBetAPIRequestModel PlaceBetInfo { get; set; }

        public JiliBetAPIRequestModel SettleBetInfo { get; set; }

        public JiliCancelAPIRequestModel CancelBet { get; set; }

    }


    public class ATApiRawRequestInfoVendor
    {
        public ATAuthAPIRequestModel TokenAuth { get; set; }

        public ATBetAPIRequestModel PlaceBetInfo { get; set; }

        public ATBetAPIRequestModel SettleBetInfo { get; set; }

        public ATBetAPIRequestModel CancelBet { get; set; }

    }

    public class ATGanesApiRawRequestInfoVendor
    {
        public ATGamesBetCallbackModel TokenAuth { get; set; }

        public ATGamesBetCallbackModel PlaceBetInfo { get; set; }

        public ATGamesBetCallbackModel SettleBetInfo { get; set; }

        public ATGamesBetCallbackModel CancelBet { get; set; }

    }

    public class RGApiRawRequestInfoVendor
    {
        public RGBalanceCallbackRequestModel TokenAuth { get; set; }

        public RGBetCallbackRequestModel PlaceBetInfo { get; set; }

        public RGSettleCallbackRequestModel SettleBetInfo { get; set; }

        public RGBalanceCallbackRequestModel CancelBet { get; set; }

    }


    public class SlotManiaApiRawRequestInfoVendor
    {
        public SlotManiaBalanceAPIRequestModel TokenAuth { get; set; }

        public SlotManiaBetAPIRequestModel PlaceBetInfo { get; set; }

        public SlotManiaBetAPIRequestModel SettleBetInfo { get; set; }

        public SlotManiaBetAPIRequestModel CancelBet { get; set; }

    }

    public class ClotPlayApiRawRequestInfoVendor
    {
        public ClotPlayBetCallbackRequestModel TokenAuth { get; set; }

        public ClotPlayBetCallbackRequestModel PlaceBetInfo { get; set; }

        public ClotPlayBetCallbackRequestModel SettleBetInfo { get; set; }

        public ClotPlayBetCallbackRequestModel CancelBet { get; set; }

    }


    public class GalaxyApiRawRequestInfoVendor
    {
        public GalaxyBalanceAPIRequestModel TokenAuth { get; set; }

        public GalaxyBetAPIRequestModel PlaceBetInfo { get; set; }

        public GalaxyBetAPIRequestModel SettleBetInfo { get; set; }

        public GalaxyBetAPIRequestModel CancelBet { get; set; }

    }

    public class AGinApiRawRequestInfoVendor
    {

        public AGinBetAPIRequestModel PlaceBetInfo { get; set; }

        public AGinBetAPIRequestModel SettleBetInfo { get; set; }

        public AGinBetAPIRequestModel CancelBet { get; set; }

    }


    public class MarioApiRawRequestInfoVendor
    {
        public long Timestamp { get; set; }

        public string API_ID { get; set; }

        public JiliAuthAPIRequestModel TokenAuth { get; set; }

        public MarioGameResultAPIRequestModel PlaceBetInfo { get; set; }

        public MarioGameResultAPIRequestModel SettleBetInfo { get; set; }

        public MarioDepositAPIRequestModel CancelBet { get; set; }

    }

    public class GPIApiRawRequestInfoVendor
    {
        public GPIAuthAPIRequestModel TokenAuth { get; set; }

        public GPIPlaceBetAPIRequestModel PlaceBetInfo { get; set; }

        public GPISettleAPIRequestModel SettleBetInfo { get; set; }

        public GPICancelAPIRequestModel CancelBet { get; set; }

    }



    public class FumaApiRawRequestInfoVendor
    {
        public FumaAPIAllParameterRequestModel TokenAuth { get; set; }

        public FumaAPIAllParameterRequestModel PlaceBetInfo { get; set; }

        public FumaAPIAllParameterRequestModel SettleBetInfo { get; set; }

        public FumaAPIAllParameterRequestModel CancelBet { get; set; }
    }



    public class EFGlayApiRawRequestInfoVendor
    {
        public EFGPlaceBetAPIRequestModel TokenAuth { get; set; }

        public EFGPlaceBetAPIRequestModel PlaceBetInfo { get; set; }

        public EFGSettleAPIRequestModel SettleBetInfo { get; set; }

        public EFGPlaceBetAPIRequestModel CancelBet { get; set; }

    }




}
