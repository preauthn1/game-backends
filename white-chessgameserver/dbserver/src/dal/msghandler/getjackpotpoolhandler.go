package msghandler

import (
	"netproto"
	"module"
	"network"
	"msg"
	"rpcmsg"
	"example.invalid/golang/protobuf/proto"
	"db"
	"fmt"
)

type GetJackpotPoolHandler struct {
}

func (rh *GetJackpotPoolHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && sclsid == int32(netproto.LHJ_GameMessageClassID_LHJGetJackpotPoolID)
}

func (rh *GetJackpotPoolHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.LHJ_GetJackpotMsg{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.LHJ_GetJackpotMsg)
	ps := db.NewSqlParameters()
	ps.AddIntInput("sintServerID", mm.GetServerID())
	ps.AddIntInput("intFlagID", mm.GetFlagID())
	ps.AddIntInput("intTableNo", mm.GetTableNo())
	ps.AddIntInput("intJackPotID", mm.GetJackpotID())

	proc := db.NewProcedure("PrGs_WildCat_JackpotPool_S", ps)
	dbret, err := WildCatDbUtil.ExecProc(proc)

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.LHJ_GameMessageClassID_LHJGetJackpotPoolRetID), nil)
	retdata := new(netproto.LHJ_GetJackpotRet)

	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrGs_WildCat_JackpotPool_S错误, %v", err)
		ctx.Error(strMsg)
	} else {
		if dbret.GetRetTableCount() > 0 {
			//TODO 有資料
			tb := dbret[0]
			for i := 0; i < len(tb.Rows); i++ {
				retdata.ServerID = proto.Int32(int32(tb.GetValueByColName(i, "ServerID").(int64)))
				retdata.FlagID = proto.Int32(int32(tb.GetValueByColName(i, "FlagID").(int64)))
				retdata.TableNo = proto.Int32(int32(tb.GetValueByColName(i, "TableNo").(int64)))
				retdata.JackpotID = proto.Int32(int32(tb.GetValueByColName(i, "JackPotID").(int64)))
				retdata.SpinStack = proto.Int64(tb.GetValueByColName(i, "SpinStack").(int64))
				retdata.PoolAmount = proto.Int64(tb.GetValueByColName(i, "PoolAmount").(int64))
				retdata.JPAmountOut = proto.Int64(tb.GetValueByColName(i, "JPAmountOut").(int64))
			}
		} else {
			retdata.ServerID = proto.Int32(0)
			retdata.FlagID = proto.Int32(0)
			retdata.TableNo = proto.Int32(0)
			retdata.JackpotID = proto.Int32(0)
			retdata.SpinStack = proto.Int64(0)
			retdata.PoolAmount = proto.Int64(0)
			retdata.JPAmountOut = proto.Int64(0)
		}
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}
