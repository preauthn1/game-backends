package jzyq

import (
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

func TransforGameStep(step GameStep) netproto.JZYQ_Step {
	switch step {
	default:
		return netproto.JZYQ_Step_JZYQ_StepPrepare
	case Prepare:
		return netproto.JZYQ_Step_JZYQ_StepPrepare
	case Start:
		return netproto.JZYQ_Step_JZYQ_StepStart
	case WaitBet:
		return netproto.JZYQ_Step_JZYQ_StepWaitBet
	case ShowAward:
		return netproto.JZYQ_Step_JZYQ_StepShowAward
	case Finish:
		return netproto.JZYQ_Step_JZYQ_StepFinish
	}
}

func TransforBetType(t BetType) netproto.JZYQ_BetType {
	switch t {
	default:
		return netproto.JZYQ_BetType_JZYQ_BetTypeNone
	case Goal:
		return netproto.JZYQ_BetType_JZYQ_BetTypeGoal
	case GoalInside:
		return netproto.JZYQ_BetType_JZYQ_BetTypeGoalInside
	case GoalOutside:
		return netproto.JZYQ_BetType_JZYQ_BetTypeGoalOutside
	case Lost:
		return netproto.JZYQ_BetType_JZYQ_BetTypeLost
	case LostInside:
		return netproto.JZYQ_BetType_JZYQ_BetTypeLostInside
	case LostOutside:
		return netproto.JZYQ_BetType_JZYQ_BetTypeLostOutside
	}
}

func ReverseBetType(t netproto.JZYQ_BetType) BetType {
	switch t {
	default:
		return 0
	case netproto.JZYQ_BetType_JZYQ_BetTypeGoal:
		return Goal
	case netproto.JZYQ_BetType_JZYQ_BetTypeGoalInside:
		return GoalInside
	case netproto.JZYQ_BetType_JZYQ_BetTypeGoalOutside:
		return GoalOutside
	case netproto.JZYQ_BetType_JZYQ_BetTypeLost:
		return Lost
	case netproto.JZYQ_BetType_JZYQ_BetTypeLostInside:
		return LostInside
	case netproto.JZYQ_BetType_JZYQ_BetTypeLostOutside:
		return LostOutside
	}
}

func TransforGameUser(u *gamelogic.GameUser) *netproto.JZYQ_UserInfo {
	info := new(netproto.JZYQ_UserInfo)
	info.UserID = proto.Int32(u.UserID)
	info.UserName = proto.String(strconv.Itoa(int(u.UserID)))
	info.FaceID = proto.Int32(u.FaceID)
	info.Money = proto.Int64(u.Score)
	return info
}
