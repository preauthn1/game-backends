package msghandler

import (
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"util"
)

import (
	"db"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

type UpdateUserScoreHandler struct {
}

func (rh *UpdateUserScoreHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_DBServer) && sclsid == int32(netproto.DBServerClassID_UpdateUserScoreID)
}

func (rh *UpdateUserScoreHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpc, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.UpdateUserScore{}
		return mm
	})

	if rpc == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.UpdateUserScore)
	example.invalid("update score %v", mm)

	msgid := rpc.GetQueueID()
	userid := mm.GetUserID()
	score := mm.GetScore()
	serverid := mm.GetServerID()
	taxamount := mm.GetTaxAmount()
	couponAmount := mm.GetCouponAmount()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intMsgID", msgid)
	ps.AddIntInput("intUserID", userid)
	ps.AddBigIntInput("lngUpdateAmount", score)
	ps.AddIntInput("intServerid", serverid)
	ps.AddBigIntInput("lngTaxAmount", taxamount)
	ps.AddBigIntInput("lngCouponAmount", couponAmount)

	proc := db.NewProcedure("PrGs_UpdateUserMoney", ps)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	var retcode int32 = 1
	_, err := DbUtil.ExecProc(proc)
	for i := 0; err != nil && i < 100; i++ {
		mlog.Error("执行存储过程PrGs_UpdateUserMoney错误, %v", err)
		_, err = DbUtil.ExecProc(proc) //重试一次
		if err == nil {
			mlog.Warn("第%v次重试执行存储过程PrGs_UpdateUserMoney成功,msgid: %d, userid: %d", i+1, msgid, userid)
			break
		}
	}

	if err != nil {
		ctx.Error("PrGs_UpdateUserMoney失败,msgid: %d, userid: %d, score: %d, serverid: %d, taxamount: %d", msgid, userid, score, serverid, taxamount)
		ctx.Error("/*【写分需补单】*/ USE CenterDB; %s \nGO\n/*【end需补单】*/", proc.GetSqlString())
		retcode = 0
	}

	return []*msg.Message{GetDBRetMsg(retcode)}
}
