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



//Slot 紀錄寫入
type SlotAddGameLogHandler struct {}

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
			return []*msg.Message{dbutil.GetDBRetMsg(0)}
		}

		//須把mm組為可寫入DB的模式
		mm := rmm.(*netproto.Slot_HistoryLogData)
		ps := db.NewSqlParameters()
		ps.AddIntInput("intUserID", mm.GetUserID())
		ps.AddIntInput("intGameID", mm.GetGameID())
		ps.AddIntInput("intServerID", mm.GetServerID())
		ps.AddBigIntInput("lngCurrentAmount", mm.GetCurrentAmount())
		ps.AddBigIntInput("lngBetMoney", mm.GetBetMoney())
		ps.AddBigIntInput("lngWinMoney", mm.GetWinMoney())
		ps.AddBigIntInput("lngFinalScore", mm.GetFinalScore())
		ps.AddIntInput("intGameType", mm.GetGameType())
		ps.AddIntInput("intLogType", mm.GetLogType())
		ps.AddVarcharInput("chvGameData", mm.GetGameData(),100)

		ps.AddVarcharOutput("chvMsg","" )
		proc := db.NewProcedure("PrGs_Slot_AddGameLog", ps)
		ret, err := dbutil.DbUtil.ExecProc(proc)
		defer func() {
			if err := recover(); err != nil {
				errorStr := util.GetStackInfo()
				mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
				panic(err)
			}
		}()
		retcode := int32(ret.GetReturnValue())
		if retcode == -1 {
			ctx.Error("操作失敗 : 设定数据错误")
		}
		if err != nil {
			ctx.Error("执行存储过程[PrGs_Slot_AddGameLog]错误, %v, %s", err, proc.GetSqlString())
			retcode = int32(0)
		}

	return []*msg.Message{dbutil.GetDBRetMsg(1)}
}

