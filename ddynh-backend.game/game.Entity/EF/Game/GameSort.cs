
namespace game.Entity.Game
{
    public class GameSort
    {
        public int Id { get; set; }
        public string GameName { get; set; }
        public int PlatformID { get; set; }
        public int AreaID { get; set; }
        public int GameID { get; set; }
        public int Sort { get; set; }
        public string AliasName { get; set; }
        public byte Status { get; set; }
        public byte? Popular { get; set; }
        public int IconID { get; set; }
    }
}
