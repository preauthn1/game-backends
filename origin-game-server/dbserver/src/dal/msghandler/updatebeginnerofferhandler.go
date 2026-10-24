package msghandler

import (
	"netproto"
	"module"
	"network"
	"msg"
	"rpcmsg"
	"example.invalid/golang/protobuf/proto"
	"db"
)

//更新玩家新手優惠
type UpdateBeginnerOfferHandler struct{}

func (rh *UpdateBeginnerOfferHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_UpdateBeginnerOfferID)
}

func (rh *UpdateBeginnerOfferHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.FishBeginnerOfferDetail{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.FishBeginnerOfferDetail)

	var err error
	for _, v := range mm.NewPlayerList {
		ps := db.NewSqlParameters()
		ps.AddIntInput("intUserID", v.GetUserID())
		ps.AddIntInput("intBullet", v.GetBullet())
		ps.AddIntInput("intScore", v.GetScore())
		ps.AddIntInput("tnyStatus", v.GetStatus())
		proc := db.NewProcedure("PrGs_Fish_AddBeginnerOffer", ps)
		_, err = DbUtil.ExecProc(proc)
		if err != nil {
			ctx.Error("执行存储过程PrGs_Fish_AddBeginnerOffer错误, %v, %s", err, proc.GetSqlString())
		}
	}

	return []*msg.Message{GetDBRetMsg(0)}
}