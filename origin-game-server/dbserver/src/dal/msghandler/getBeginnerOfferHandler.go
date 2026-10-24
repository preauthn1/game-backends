package msghandler

import (
	"db"
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

//取得玩家新手優惠
type GetBeginnerOfferHandler struct {
}

func (rh *GetBeginnerOfferHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.GameMessageClassID_GetBeginnerOfferID)
}

func (rh *GetBeginnerOfferHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.FindFishBeginnerOfferByUserID{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.FindFishBeginnerOfferByUserID)
	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	proc := db.NewProcedure("PrGs_Fish_GetBeginnerOffer", ps)
	dbret, err := DbUtil.ExecProc(proc)
	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_GetBeginnerOfferRetID), nil)
	retdata := new(netproto.FishBeginnerOfferDetail)
	retdata.NewPlayerAdjustSyspool = proto.Int32(mm.GetNewPlayerAdjustSyspool())

	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrGs_Fish_GetBeginnerOffer错误, %v", err)
		ctx.Error(strMsg)
	} else {
		if dbret.GetRetTableCount() > 0 {
			tb := dbret[0]
			for i := 0; i < len(tb.Rows); i++ {
				list := new(netproto.FishBeginnerOffer)
				list.UserID = proto.Int32(int32(tb.GetValueByColName(i, "UserID").(int64)))
				list.Bullet = proto.Int32(int32(tb.GetValueByColName(i, "Bullet").(int64)))
				list.Score = proto.Int32(int32(tb.GetValueByColName(i, "Score").(int64)))
				list.Status = proto.Int32(int32(tb.GetValueByColName(i, "Status").(int64)))
				retdata.NewPlayerList = append(retdata.NewPlayerList, list)
			}
			if len(tb.Rows) == 0 {
				list := new(netproto.FishBeginnerOffer)
				list.UserID = proto.Int32(mm.GetUserID())
				list.Bullet = proto.Int32(0)
				list.Score = proto.Int32(0)
				list.Status = proto.Int32(0)
				retdata.NewPlayerList = append(retdata.NewPlayerList, list)
			}
		} else {
			list := new(netproto.FishBeginnerOffer)
			list.UserID = proto.Int32(mm.GetUserID())
			list.Bullet = proto.Int32(0)
			list.Score = proto.Int32(0)
			list.Status = proto.Int32(0)
			retdata.NewPlayerList = append(retdata.NewPlayerList, list)
		}
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}
