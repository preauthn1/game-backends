package gamelogic

//游戏信息
type Game struct {
	GameID    int32  //游戏ID
	GameName  string //游戏名称
	MinPlayer int32  //最小玩家数
	MaxPlayer int32  //最大玩家数
}

//创建游戏对象
func NewGame() *Game {
	g := new(Game)

	return g
}
