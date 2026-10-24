package roulette

import (
	"strconv"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

func TransforGameStep(step GameStep) netproto.Roulette_Step {
	switch step {
	default:
		return netproto.Roulette_Step_Roulette_StepPrepare
	case Prepare:
		return netproto.Roulette_Step_Roulette_StepPrepare
	case Start:
		return netproto.Roulette_Step_Roulette_StepStart
	case WaitBet:
		return netproto.Roulette_Step_Roulette_StepWaitBet
	case ShowAward:
		return netproto.Roulette_Step_Roulette_StepShowAward
	case Finish:
		return netproto.Roulette_Step_Roulette_StepFinish
	}
}

func TransforBetType(t BetType) netproto.Roulette_BetType {
	switch t {
	default:
		return netproto.Roulette_BetType_Roulette_BetTypeNone
	case Odd:
		return netproto.Roulette_BetType_Roulette_BetTypeOdd
	case Even:
		return netproto.Roulette_BetType_Roulette_BetTypeEven
	case Red:
		return netproto.Roulette_BetType_Roulette_BetTypeRed
	case Black:
		return netproto.Roulette_BetType_Roulette_BetTypeBlack
	case Small:
		return netproto.Roulette_BetType_Roulette_BetTypeSmall
	case Big:
		return netproto.Roulette_BetType_Roulette_BetTypeBig
	case First12:
		return netproto.Roulette_BetType_Roulette_BetTypeFirst12
	case Second12:
		return netproto.Roulette_BetType_Roulette_BetTypeSecond12
	case Third12:
		return netproto.Roulette_BetType_Roulette_BetTypeThird12
	case FirstRow:
		return netproto.Roulette_BetType_Roulette_BetTypeFirstRow
	case SecondRow:
		return netproto.Roulette_BetType_Roulette_BetTypeSecondRow
	case ThirdRow:
		return netproto.Roulette_BetType_Roulette_BetTypeThirdRow
	case OneNumber:
		return netproto.Roulette_BetType_Roulette_BetTypeOneNumber
	case TwoNumber:
		return netproto.Roulette_BetType_Roulette_BetTypeTwoNumber
	case ThreeNumber:
		return netproto.Roulette_BetType_Roulette_BetTypeThreeNumber
	case FourNumber:
		return netproto.Roulette_BetType_Roulette_BetTypeFourNumber
	case SixNumber:
		return netproto.Roulette_BetType_Roulette_BetTypeSixNumber
	case SuperZero:
		return netproto.Roulette_BetType_Roulette_BetTypeSuperZero
	}
}

func ReverseBetType(t netproto.Roulette_BetType) BetType {
	switch t {
	default:
		return 0
	case netproto.Roulette_BetType_Roulette_BetTypeOdd:
		return Odd
	case netproto.Roulette_BetType_Roulette_BetTypeEven:
		return Even
	case netproto.Roulette_BetType_Roulette_BetTypeRed:
		return Red
	case netproto.Roulette_BetType_Roulette_BetTypeBlack:
		return Black
	case netproto.Roulette_BetType_Roulette_BetTypeSmall:
		return Small
	case netproto.Roulette_BetType_Roulette_BetTypeBig:
		return Big
	case netproto.Roulette_BetType_Roulette_BetTypeFirst12:
		return First12
	case netproto.Roulette_BetType_Roulette_BetTypeSecond12:
		return Second12
	case netproto.Roulette_BetType_Roulette_BetTypeThird12:
		return Third12
	case netproto.Roulette_BetType_Roulette_BetTypeFirstRow:
		return FirstRow
	case netproto.Roulette_BetType_Roulette_BetTypeSecondRow:
		return SecondRow
	case netproto.Roulette_BetType_Roulette_BetTypeThirdRow:
		return ThirdRow
	case netproto.Roulette_BetType_Roulette_BetTypeOneNumber:
		return OneNumber
	case netproto.Roulette_BetType_Roulette_BetTypeTwoNumber:
		return TwoNumber
	case netproto.Roulette_BetType_Roulette_BetTypeThreeNumber:
		return ThreeNumber
	case netproto.Roulette_BetType_Roulette_BetTypeFourNumber:
		return FourNumber
	case netproto.Roulette_BetType_Roulette_BetTypeSixNumber:
		return SixNumber
	case netproto.Roulette_BetType_Roulette_BetTypeSuperZero:
		return SuperZero
	}
}

func TransforGameUser(u *gamelogic.GameUser) *netproto.Roulette_UserInfo {
	info := new(netproto.Roulette_UserInfo)
	info.UserID = proto.Int32(u.UserID)
	info.UserName = proto.String(strconv.Itoa(int(u.UserID)))
	info.FaceID = proto.Int32(u.FaceID)
	info.Money = proto.Int64(u.Score)
	return info
}
