package gamelogic

import (
	"bytes"
	"fmt"
	"strconv"
	"util"
)

type GameCmd struct {
	gamelogics *GameLogicManager
}

//创建游戏命令对象
func NewGameCmd(logics *GameLogicManager) *GameCmd {
	gcmd := new(GameCmd)
	gcmd.gamelogics = logics

	return gcmd
}

func (cmd *GameCmd) Name() string {
	return "game"
}

func (cmd *GameCmd) Help() string {
	return "游戏管理，eg:game stat | game list (0/1) | game find tableno"
}

func (cmd *GameCmd) Exec(args []string) string {
	if len(args) == 0 {
		return "error.seeref usage"
	}
	cmdname := args[0]

	switch cmdname {
	case "stat":
		gamecount := 0
		freecount := 0
		for _, g := range cmd.gamelogics.logics {
			if g.IsGaming() {
				gamecount++
			} else {
				freecount++
			}
		}

		return fmt.Sprintf("game TableManager count:%d,game count:%d,free count:%d", gamecount+freecount, gamecount, freecount)
	case "list":
		if len(args) != 2 {
			return "error,seeref usage"
		}

		stype := args[1]
		var buf bytes.Buffer

		for _, g := range cmd.gamelogics.logics {
			if g.IsGaming() {
				if stype == "1" {
					buf.WriteString(fmt.Sprintf("第%d桌,", g.TableNo()))
				}
			} else {
				if stype == "0" {
					buf.WriteString(fmt.Sprintf("第%d桌,", g.TableNo()))
				}
			}
		}

		buf.WriteString("\r\n")

		return buf.String()
	case "find":
		if len(args) != 2 {
			return "error,seeref usage"
		}

		sno := args[1]

		if !util.IsNumber(sno) {
			return "error,桌号必须为数字"
		}

		tno, _ := strconv.Atoi(sno)

		glc := cmd.gamelogics.GetGameLogic(int32(tno))

		if glc == nil {
			return "error,桌号不存在"
		}

		return fmt.Sprintf("第%d桌,游戏状态:%s", glc.TableNo(), glc.IsGaming())

	default:
		return ""
	}

}
