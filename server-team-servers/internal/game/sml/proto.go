package sml

import (
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

func TransforGameStep(step GameStep) netproto.SML_Step {
	switch step {
	default:
		return netproto.SML_Step_SML_StepPrepare
	case Prepare:
		return netproto.SML_Step_SML_StepPrepare
	case Start:
		return netproto.SML_Step_SML_StepStart
	case WaitBet:
		return netproto.SML_Step_SML_StepWaitBet
	case ShowAward:
		return netproto.SML_Step_SML_StepShowAward
	case Finish:
		return netproto.SML_Step_SML_StepFinish
	}
}

func TransforBetType(t BetType) netproto.SML_BetType {
	switch t {
	default:
		return netproto.SML_BetType_SML_BetTypeNone
	case Goal:
		return netproto.SML_BetType_SML_BetTypeGoal
	case GoalLeft:
		return netproto.SML_BetType_SML_BetTypeGoalLeft
	case GoalMiddle:
		return netproto.SML_BetType_SML_BetTypeGoalMiddle
	case GoalRight:
		return netproto.SML_BetType_SML_BetTypeGoalRight
	case Save:
		return netproto.SML_BetType_SML_BetTypeSave
	case SaveLeft:
		return netproto.SML_BetType_SML_BetTypeSaveLeft
	case SaveMiddle:
		return netproto.SML_BetType_SML_BetTypeSaveMiddle
	case SaveRight:
		return netproto.SML_BetType_SML_BetTypeSaveRight
	}
}

func ReverseBetType(t netproto.SML_BetType) BetType {
	switch t {
	default:
		return 0
	case netproto.SML_BetType_SML_BetTypeGoal:
		return Goal
	case netproto.SML_BetType_SML_BetTypeGoalLeft:
		return GoalLeft
	case netproto.SML_BetType_SML_BetTypeGoalMiddle:
		return GoalMiddle
	case netproto.SML_BetType_SML_BetTypeGoalRight:
		return GoalRight
	case netproto.SML_BetType_SML_BetTypeSave:
		return Save
	case netproto.SML_BetType_SML_BetTypeSaveLeft:
		return SaveLeft
	case netproto.SML_BetType_SML_BetTypeSaveMiddle:
		return SaveMiddle
	case netproto.SML_BetType_SML_BetTypeSaveRight:
		return SaveRight
	}
}

func TransforGameUser(u *gamelogic.GameUser) *netproto.SML_UserInfo {
	info := new(netproto.SML_UserInfo)
	info.UserID = proto.Int32(u.UserID)
	info.UserName = proto.String(strconv.Itoa(int(u.UserID)))
	info.FaceID = proto.Int32(u.FaceID)
	info.Money = proto.Int64(u.Score)
	return info
}
