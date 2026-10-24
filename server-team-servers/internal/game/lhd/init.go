package lhd

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/app"
	"example.invalid/ServerTeam/Servers/pkg/gamelogic"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

// LHDGameFunc 取得 LHD Game 方式
type LHDGameFunc func(gamelogic.GameLogic) *LongHudouLogic

func init() {
	app.Context.Provide(func() LHDGameFunc {
		return func(g gamelogic.GameLogic) *LongHudouLogic {
			game := g.(*LongHudouLogic)
			return game
		}
	})

	msg.InstanceProvider.Register(int32(netproto.MessageBClassID_Game), int32(netproto.LHD_GameMessageClassID_LHDBetID), func() proto.Message { return new(netproto.LHD_BetArr) })
}
