package fixtures

import (
	"fmt"
	"net"
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/player"
	"example.invalid/ServerTeam/Servers/pkg/robot/robotcomm"
)

func GetGameUser() (users []*gamelogic.GameUser) {
	indexUserIdx := int32(0)
	genUser := func(nickname string, score int64, userType int32) (nur *gamelogic.GameUser) {
		indexUserIdx++
		userid := 9000000 + indexUserIdx
		nur = gamelogic.NewGameUser()
		nur.ConnInfo = nil
		nur.WinCount = 0
		nur.DrawCount = 0
		nur.LoseCount = 0

		nur.HDCode = "000000"
		nur.HDType = 1

		nur.UserID = userid
		nur.NickName = nickname
		nur.Score = score
		nur.LoginScore = score
		nur.UserSex = 0
		//设定用户类型为机器人
		nur.UserType = userType // urdata.GetUserType()
		nur.FaceID = 3
		return
	}
	users = append(users, genUser("台北市", 100000, gamelogic.UserType_Rebot))
	users = append(users, genUser("武汉市", 100000, gamelogic.UserType_Normal))
	users = append(users, genUser("武汉市", 100000, gamelogic.UserType_Normal))
	users = append(users, genUser("武汉市", 100000, gamelogic.UserType_Normal))
	users = append(users, genUser("武汉市", 100000, gamelogic.UserType_Normal))
	users = append(users, genUser("武汉市", 100000, gamelogic.UserType_Normal))
	return
}

func GetGameUsers() (users []*gamelogic.GameUser, robots []*gamelogic.GameUser) {
	indexUserIdx := int32(0)
	genUser := func(nickname string, score int64, userType int32) (nur *gamelogic.GameUser) {
		indexUserIdx++
		baseuid, _ := strconv.Atoi(fmt.Sprintf("9%d00000", userType))
		userid := int32(baseuid) + indexUserIdx
		nur = gamelogic.NewGameUser()
		nur.ConnInfo = nil
		nur.WinCount = 0
		nur.DrawCount = 0
		nur.LoseCount = 0

		nur.HDCode = "000000"
		nur.HDType = 1

		nur.UserID = userid
		nur.NickName = nickname
		nur.Score = score
		nur.LoginScore = score
		nur.UserSex = 0
		//设定用户类型为机器人
		nur.UserType = userType // urdata.GetUserType()
		nur.FaceID = 3
		return
	}
	for i := 0; i < 10; i++ {
		users = append(users, genUser("台北市", 1000000, gamelogic.UserType_Normal))
	}
	for i := 0; i < 10; i++ {
		robots = append(robots, genUser("台中市", 1000000, gamelogic.UserType_Rebot))
	}

	return
}

//龍虎鬥
func GetGameUserLhd() []*gamelogic.GameUser {
	var users []*gamelogic.GameUser
	nur := gamelogic.NewGameUser()
	nur.TableNo = 0
	nur.ConnInfo = nil
	nur.WinCount = 0
	nur.DrawCount = 0
	nur.LoseCount = 0

	nur.HDCode = "000000"
	nur.HDType = 1

	nur.UserID = 0
	nur.SeatNo = 0
	nur.NickName = "武汉市"
	nur.Score = 100000
	nur.LoginScore = 100000
	nur.UserSex = 0
	//设定用户类型为机器人
	nur.UserType = int32(gamelogic.UserType_Normal) // urdata.GetUserType()
	nur.FaceID = 3

	users = append(users, nur)
	nur1 := gamelogic.NewGameUser()
	nur1.TableNo = 1
	nur1.SeatNo = 1
	nur1.ConnInfo = nil
	nur1.WinCount = 0
	nur1.DrawCount = 0
	nur1.LoseCount = 0

	nur1.HDCode = "000000"
	nur1.HDType = 1

	nur1.UserID = 1
	nur1.NickName = "台北市"
	nur1.Score = 200000
	nur1.LoginScore = 200000
	nur1.UserSex = 0
	//设定用户类型为机器人
	nur1.UserType = int32(gamelogic.UserType_Normal) // urdata.GetUserType()
	nur1.FaceID = 3
	users = append(users, nur1)

	nur2 := gamelogic.NewGameUser()
	nur2.TableNo = 1
	nur2.SeatNo = 2
	nur2.ConnInfo = nil
	nur2.WinCount = 0
	nur2.DrawCount = 0
	nur2.LoseCount = 0
	nur2.HDCode = "000000"
	nur2.HDType = 1
	nur2.UserID = 2
	nur2.NickName = "台北市"
	nur2.Score = 200000
	nur2.LoginScore = 200000
	nur2.UserSex = 0
	//设定用户类型为机器人
	nur2.UserType = int32(gamelogic.UserType_Normal) // urdata.GetUserType()
	nur2.FaceID = 3
	users = append(users, nur2)

	return users
}

func GetUserBet(t int32, c int32) []byte {
	bet := new(netproto.BJL_Bet)
	bet.BetType = proto.Int32(t)
	bet.Count = proto.Int32(c)

	data := new(netproto.BJL_BetArr)
	data.BetArr = append(data.BetArr, bet)
	b, _ := proto.Marshal(data)
	return b
}

func GetPlayers() (players []*player.Player) {
	l, _ := net.Listen("tcp", ":1111")
	defer func() {
		l.Close()
	}()
	indexUserIdx := int32(0)
	players = make([]*player.Player, 0)
	genPlayer := func(nickname string, score int64) (p *player.Player) {
		conn, _ := net.Dial("tcp", "localhost:1111")
		tcpconn := network.NewTcpConn(conn, 2048)
		conninfo := network.NewConnInfo(tcpconn, "123", 1)
		p = player.NewGuest(conninfo)
		fmt.Println(conn)
		indexUserIdx++
		userid := 9000000 + indexUserIdx
		nur := gamelogic.NewGameUser()
		nur.ConnInfo = nil
		nur.WinCount = 0
		nur.DrawCount = 0
		nur.LoseCount = 0

		nur.HDCode = "000000"
		nur.HDType = 1

		nur.UserID = userid
		nur.NickName = nickname
		nur.Score = score
		nur.LoginScore = score
		nur.UserSex = 0
		//设定用户类型为机器人
		nur.UserType = gamelogic.UserType_Normal // urdata.GetUserType()
		nur.FaceID = 3

		//设置player数据
		p.Cer = "11111111"
		p.HDCode = "000000"
		p.HDType = 1
		p.UserID = userid
		p.UserData = nur

		return
	}
	for i := 0; i < 10; i++ {
		players = append(players, genPlayer("台北市", 100000))
	}

	return
}

func GetRobots() (robots []*robotcomm.Robot) {
	indexUserIdx := int32(0)
	genUser := func(nickname string, score int64) (nur *gamelogic.GameUser) {
		indexUserIdx++
		baseuid, _ := strconv.Atoi(fmt.Sprintf("9%d00000", gamelogic.UserType_Rebot))
		userid := int32(baseuid) + indexUserIdx
		nur = gamelogic.NewGameUser()
		nur.ConnInfo = nil
		nur.WinCount = 0
		nur.DrawCount = 0
		nur.LoseCount = 0

		nur.HDCode = "000000"
		nur.HDType = 1

		nur.UserID = userid
		nur.NickName = nickname
		nur.Score = score
		nur.LoginScore = score
		nur.UserSex = 0
		//设定用户类型为机器人
		nur.UserType = gamelogic.UserType_Rebot // urdata.GetUserType()
		nur.FaceID = 3
		return
	}
	for i := 0; i < 10; i++ {

		//foo := new robotcomm.Robot{genUser("台中市", 1000000), ""}
		foo := &robotcomm.Robot{genUser("台中市", 1000000), 5, 0, false}
		robots = append(robots, foo)
	}
	return
}
