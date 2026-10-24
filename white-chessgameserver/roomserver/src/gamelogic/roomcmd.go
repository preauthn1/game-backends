package gamelogic

import (
	"bytes"
	"fmt"
	"strconv"
	"util"
)

//房间命令
type RoomCmd struct {
	room *Room
}

//创建房间命令对象
func NewRoomCmd(room *Room) *RoomCmd {
	rcmd := new(RoomCmd)
	rcmd.room = room

	return rcmd
}

func (cmd *RoomCmd) Name() string {
	return "room"
}

func (cmd *RoomCmd) Help() string {
	return "房间管理，eg:room stat | room tablelist | room tablefind index | room finduser userid"
}

func (cmd *RoomCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error.seeref usage"
	}

	cmdname := args[0]

	switch cmdname {
	case "stat":
		formatstr := "gameid:%d,gamename:%s,serverid:%d,servername:%s,flag:%s,addr:%s:%d,tablecount:%d,seatcount:%d"
		return fmt.Sprintf(formatstr, cmd.room.GameInfo.GameID, cmd.room.GameInfo.GameName, cmd.room.ServerID, cmd.room.ServerName, cmd.room.Flag, cmd.room.IPAddress, cmd.room.Port, cmd.room.TableCount, cmd.room.SeatCount)
	case "tablelist":
		var buf bytes.Buffer

		for _, t := range cmd.room.Tables {
			buf.WriteString(fmt.Sprintf("tableno:%d,", t.TableNo))

			for _, st := range t.Seats {
				buf.WriteString(fmt.Sprintf("seatno:%d,user:%v", st.SeatNo, st.SeatUser))
			}

			buf.WriteString("\r\n")
		}

		return buf.String()
	case "tablefind":
		if len(args) != 2 {
			return "error,room tablefind index"
		}
		sno := args[1]

		if !util.IsNumber(sno) {
			return "error,TableManager index 必须为数字"
		}

		tno, _ := strconv.Atoi(sno)

		if int32(tno) >= cmd.room.TableCount {
			return fmt.Sprintf("error,桌号不能超出%d", cmd.room.TableCount-1)
		}

		table := cmd.room.Tables[tno]

		var buf bytes.Buffer

		buf.WriteString(fmt.Sprintf("tableno:%d,", table.TableNo))

		for _, st := range table.Seats {
			buf.WriteString(fmt.Sprintf("seatno:%d,user:%v", st.SeatNo, st.SeatUser))
		}

		buf.WriteString("\r\n")

		return buf.String()
	case "finduser":
		if len(args) != 2 {
			return "error,room finduser userid"
		}
		suserid := args[1]
		if !util.IsNumber(suserid) {
			return "error,用户id必须为数字"
		}

		userid, _ := strconv.Atoi(suserid)

		user := cmd.room.RoomUserManager.GetGameUserByUserID(int32(userid))

		if user == nil {
			return "用户不在房间中"
		}

		tno := user.TableNo

		if tno == FreeTableNo {
			return fmt.Sprintf("%v", user)
		}

		table := cmd.room.Tables[tno]

		var buf bytes.Buffer

		buf.WriteString(fmt.Sprintf("tableno:%d,", table.TableNo))

		for _, st := range table.Seats {
			buf.WriteString(fmt.Sprintf("seatno:%d,user:%v", st.SeatNo, st.SeatUser))
		}

		buf.WriteString("\r\n")

		return buf.String()

	default:
		return ""
	}

}
