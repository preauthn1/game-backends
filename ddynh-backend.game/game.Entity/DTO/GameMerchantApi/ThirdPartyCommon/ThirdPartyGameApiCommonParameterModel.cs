using game.Entity.Gamegame.Entity.Game;

namespace game.Entity.ViewModels
{
    public class ThirdPartyGameApiCommonParameterModel
    {
        public ThirdPartyGameApiCommonParameterModel() { }


        public ThirdPartyGameApiCommonParameterModel(int userID , int platformID)
        {
            this.UserID = userID;
            this.PlatformID = platformID;
        }

        public int UserID { get; set; }

        public int PlatformID { get; set; }


    }




}
