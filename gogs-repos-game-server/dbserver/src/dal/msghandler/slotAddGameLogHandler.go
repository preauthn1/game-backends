package msghandler

import (
	"db"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
	"util"

	"example.invalid/golang/protobuf/proto"
)

//Slot 紀錄寫入
type SlotAddGameLogHandler struct{}

func (fsp *SlotAddGameLogHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && (sclsid == int32(netproto.GameMessageClassID_SlotAddGameLogID))
}

func (fsp *SlotAddGameLogHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Game)
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_HistoryLogData{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	//須把mm組為可寫入DB的模式
	mm := rmm.(*netproto.Slot_HistoryLogData)
	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddIntInput("intGameID", mm.GetGameID())
	ps.AddIntInput("intServerID", mm.GetServerID())
	ps.AddBigIntInput("lngCurrentAmount", mm.GetCurrentAmount())
	ps.AddBigIntInput("lngCurrentCoupon", mm.GetCurrentCoupon())
	ps.AddBigIntInput("lngBetMoney", mm.GetBetMoney())
	ps.AddBigIntInput("lngBetCoupon", mm.GetBetCoupon())
	ps.AddBigIntInput("lngWinMoney", mm.GetWinMoney())
	ps.AddBigIntInput("lngFinalScore", mm.GetFinalScore())
	ps.AddBigIntInput("lngFinalCoupon", mm.GetFinalCoupon())
	ps.AddIntInput("intGameType", mm.GetGameType())
	ps.AddIntInput("intLogType", mm.GetLogType())
	ps.AddVarcharInput("chvGameData", mm.GetGameData(), 100)

	ps.AddVarcharOutput("chvMsg", "")
	proc := db.NewProcedure("PrGs_Slot_AddGameLog", ps)
	ret, err := DbUtil.ExecProc(proc)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	retcode := int32(ret.GetReturnValue())
	if retcode == -1 {
		mlog.Error("操作失敗 : [PrGs_Slot_AddGameLog] 设定数据错误, %s", proc.GetSqlString())
	}
	if err != nil {
		ctx.Error("执行存储过程[PrGs_Slot_AddGameLog]错误, %v, %s", err, proc.GetSqlString())
		retcode = int32(0)
	}

	return []*msg.Message{GetDBRetMsg(1)}
}
