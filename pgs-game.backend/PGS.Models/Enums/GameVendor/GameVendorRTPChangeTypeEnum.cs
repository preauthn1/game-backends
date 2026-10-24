using System.ComponentModel;

namespace target.Models.Enums
{
    public enum GameVendorRTPChangeTypeEnum
    {
        UpdateGameRTP = 11,

        ResetGameRTP = 12,

        UpdatePlayerRTP = 21,

        ResetPlayerRTP = 22,

        CallApiFail = 999,
    }

}
