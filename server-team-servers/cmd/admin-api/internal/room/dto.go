package room

type PlayerInfo struct {
	ID      int32
	IsRobot bool
}

type TableInfo struct {
	ID      int32
	Gaming  bool
	Summary string
	Players []PlayerInfo
	Robots  []PlayerInfo
}

type RoomInfo struct {
	ID        int32
	Name      string
	GameID    int32
	GameName  string
	Flag      int32
	GameCode  string
	Tables    []TableInfo
	PlayerCnt int32
	RobotCnt  int32
	Actived   bool
}
