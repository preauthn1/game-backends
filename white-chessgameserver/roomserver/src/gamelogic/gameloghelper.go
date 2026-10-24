package gamelogic

import (
	"fmt"
	"gamelog"
	"example.invalid/golang/protobuf/proto"
	"netproto"
)

//牌局记录辅助器
type GameLogHelper struct {
	innerGameLog gamelog.GameLog
	glc          *GameLogicBase
}

//创建牌局记录辅助器
func NewGameLogHelper(gl gamelog.GameLog, glc *GameLogicBase) *GameLogHelper {
	glh := new(GameLogHelper)
	glh.innerGameLog = gl
	glh.glc = glc

	return glh
}

//游戏开始
func (glh *GameLogHelper) GameBegin() {
	urs := glh.glc.AllGameUser()

	stepdesc := "游戏开始."
	stepdesc += fmt.Sprintf("桌子号:%d.", glh.glc.TableNo())
	uds := make(map[int32]*gamelog.UserData, 0)
	for _, ur := range urs {
		ud := new(gamelog.UserData)
		ud.Score = ur.Score
		ud.TradeID = ur.TradeID
		uds[ur.UserID] = ud
		stepdesc += fmt.Sprintf("userid:%d,seat:%d,score:%d,", ur.UserID, ur.SeatNo, ur.Score)
	}
	glh.innerGameLog.BeginGameLog(uds)

	step := gamelog.NewGameStep(stepdesc, gamelog.GameStepTimeType_FullTime)

	glh.innerGameLog.AddGameStep(step)
}

//游戏结束
func (glh *GameLogHelper) GameEnd(uscores map[int32]int64) {
	urs := glh.glc.AllGameUser()
	uds := make(map[int32]*gamelog.UserData, 0)
	for _, ur := range urs {
		ud := new(gamelog.UserData)
		ud.Score = ur.Score
		ud.TradeID = ur.TradeID
		uds[ur.UserID] = ud
	}
	glh.innerGameLog.UpdateGameUsers(uds)
	stepdesc := "游戏结束."
	for uid, score := range uscores {
		stepdesc += fmt.Sprintf("userid:%d,winamount:%d,", uid, score)
	}
	step := gamelog.NewGameStep(stepdesc, gamelog.GameStepTimeType_FullTime)
	glh.innerGameLog.AddGameStep(step)

	glh.innerGameLog.EndGameLog(uscores)
}

//添加游戏动作
func (glh *GameLogHelper) AddGameStep(format string, a ...interface{}) {
	stepdesc := fmt.Sprintf(format, a...)

	step := gamelog.NewGameStep(stepdesc, gamelog.GameStepTimeType_SimpleTime)
	glh.innerGameLog.AddGameStep(step)
}

//更新用戶
func (glh *GameLogHelper) UpdateGameUsers(urs map[int32]*gamelog.UserData) {
	glh.innerGameLog.UpdateGameUsers(urs)
}

//发牌
func (glh *GameLogHelper) Fapai(uid int32, carddesc string) {
	stepdesc := fmt.Sprintf("发牌,userid:%d,牌型:%s", uid, carddesc)
	step := gamelog.NewGameStep(stepdesc, gamelog.GameStepTimeType_SimpleTime)
	glh.innerGameLog.AddGameStep(step)
}

//配置指定開獎信息
func (glh *GameLogHelper) SetNextJiangRecord(adminuser string, jiangtype string) {
	record := new(netproto.NextJiangRecord)
	record.AdminUser = proto.String(adminuser)
	record.JiangType = proto.String(jiangtype)

	glh.innerGameLog.SetNextJiangRecord(record)
}
