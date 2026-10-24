package msghandler

import (
	"example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/db/dbutil"
)

import (
	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/rpcmsg"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

import (
	"time"
)



// SlotGetGameLogHandler Slot Slot讀取紀錄
type SlotGetGameLogHandler struct {}

// IsHook 是否這隻Handler該處理
func (fsp *SlotGetGameLogHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && (sclsid == int32(netproto.GameMessageClassID_SlotGetGameLogID))
}

// OnMessage 處理訊息
func (fsp *SlotGetGameLogHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_LoadHistoryData{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	//須把mm組為可寫入DB的模式
	mm := rmm.(*netproto.Slot_LoadHistoryData)

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddIntInput("intGameID", mm.GetGameID())
	ps.AddVarcharInput("dtStartTime", mm.GetStartTime(), 100)
	ps.AddVarcharInput("dtEndTime", mm.GetEndTime(), 100)
	ps.AddIntInput("intPageIndex", mm.GetPageIndex())
	ps.AddIntInput("intPageSize", mm.GetPageSize())
	ps.AddIntOutput("intPageCount", 0)
	ps.AddVarcharOutput("chvMsg", "")

	proc := db.NewProcedure("PrGs_Slot_GetGameLog", ps)

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SlotSendGameLogID), nil)
	var retcode int32

	ret, err := dbutil.DbUtil.ExecProc(proc)
	if err != nil {
		ctx.Error("执行存储过程[PrGs_Slot_GetGameLog]错误, %v, %s", err, proc.GetSqlString())
		retcode = 0
		gamelog := new(netproto.Slot_SendHistoryData)
		gamelog.GameID = proto.Int32(mm.GetGameID())
		gamelog.UserID = proto.Int32(mm.GetUserID())
		gamelog.SendHistoryLog = make([]*netproto.Slot_SendHistoryLog, 0)
		gamelog.PageCount = proto.Int32(0)
		retmsg.MsgData = gamelog
		return []*msg.Message{retmsg}
	}

	// ???????
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	// ???????

	retcode = int32(ret.GetReturnValue())

	gamelog := new(netproto.Slot_SendHistoryData)
	gamelog.GameID = proto.Int32(mm.GetGameID())
	gamelog.UserID = proto.Int32(mm.GetUserID())
	gamelog.SendHistoryLog = make([]*netproto.Slot_SendHistoryLog, 0)
	gamelog.PageCount = proto.Int32(0)

	if retcode == 1 {
		if ret.GetRetTableCount() > 0 {
			for i := 0; i < len(ret[0].Rows); i++ {
				detail := new(netproto.Slot_SendHistoryLog)
				detail.RowNum = proto.Int32(ret[0].GetValueByColNameInt32(i, "rowNum"))
				detail.CreateTime = proto.String(ret[0].GetValueByColName(i, "CreateTime").(time.Time).Format("2006-01-02 15:04:05"))
				detail.OrderID = proto.Int64(ret[0].GetValueByColNameInt64(i, "OrderID"))
				detail.BetMoney = proto.Int64(ret[0].GetValueByColNameInt64(i, "BetMoney"))
				detail.Profit = proto.Int64(ret[0].GetValueByColNameInt64(i, "FinalScore"))

				gamelog.SendHistoryLog = append(gamelog.SendHistoryLog, detail)
			}
			gamelog.PageCount = proto.Int32(ret[1].GetValueByColNameInt32(0, "intPageCount"))
		}
	} else {
		mlog.Error("操作失敗 : [PrGs_Slot_GetGameLog] 设定数据错误, %s", proc.GetSqlString())
	}

	retmsg.MsgData = gamelog
	return []*msg.Message{retmsg}
}
