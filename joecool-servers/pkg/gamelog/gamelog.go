package gamelog

import (
	"bytes"
	"fmt"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//游戏动作时间类型
const (
	GameStepTimeType_SimpleTime int32 = 0 //精简时间
	GameStepTimeType_FullTime   int32 = 1 //完整时间
)

//游戏动作
type GameStep struct {
	StepTime string //动作发生时间
	StepDesc string //动作描述
}

//玩家資訊
type UserData struct {
	Score   int64 //当前积分
	TradeID int64 //每次進入遊戲房間會獲取一個唯一的值
}

//创建游戏动作对象
func NewGameStep(stepdesc string, timetype int32) *GameStep {
	step := new(GameStep)
	step.StepDesc = stepdesc
	if timetype == GameStepTimeType_SimpleTime {
		step.StepTime = time.Now().Format("15:04:05")
	} else {
		step.StepTime = util.FormatTimeStr(time.Now())
	}

	return step
}

//牌局记录
type GameLog interface {
	//开始游戏记录
	BeginGameLog(urs map[int32]*UserData)

	//添加游戏步骤
	AddGameStep(step *GameStep)

	//游戏步骤结束
	EndGameLog(uscores map[int32]int64)

	//更新GameUsers
	UpdateGameUsers(urs map[int32]*UserData)

	//从牌局记录清除gameuser
	RemoveGameUsers(userID int32)

	//配置指定開獎
	SetNextJiangRecord(record *netproto.NextJiangRecord)
}

type gameLogBase struct {
	gameSteps       []*GameStep
	gameUsers       map[int32]*UserData
	gameBeginTime   time.Time
	gameEndTime     time.Time
	nextJiangRecord *netproto.NextJiangRecord
}

//创建牌局记录基类对象
func NewGameLogBase() *gameLogBase {
	glb := new(gameLogBase)
	glb.gameSteps = make([]*GameStep, 0)
	glb.gameUsers = make(map[int32]*UserData, 0)

	return glb
}

//添加游戏步骤
func (l *gameLogBase) AddGameStep(step *GameStep) {
	l.gameSteps = append(l.gameSteps, step)
}

//清除牌局记录步骤
func (l *gameLogBase) clearGameStep() {
	l.gameSteps = make([]*GameStep, 0)
	l.gameUsers = make(map[int32]*UserData, 0)
}

//开始游戏记录
func (l *gameLogBase) BeginGameLog(urs map[int32]*UserData) {
	l.gameBeginTime = time.Now()

	l.clearGameStep()
	l.gameUsers = urs
	l.nextJiangRecord = nil
}

//获取牌局记录描述
func (l *gameLogBase) getGameLogDesc() string {
	var buf bytes.Buffer

	buf.WriteString("\r\n")
	for _, st := range l.gameSteps {
		buf.WriteString(fmt.Sprintf("%s %s\r\n", st.StepTime, st.StepDesc))
	}

	return buf.String()
}

//在牌局結束時更新一次gameUser
func (l *gameLogBase) UpdateGameUsers(urs map[int32]*UserData) {
	for userid, ur := range urs {
		if l.gameUsers[userid] == nil {
			l.gameUsers[userid] = ur
		}
	}
}

//从牌局记录清除gameuser
func (l *gameLogBase) RemoveGameUsers(userID int32) {
	delete(l.gameUsers, userID)
}

func (l *gameLogBase) SetNextJiangRecord(record *netproto.NextJiangRecord) {
	l.nextJiangRecord = record
}
