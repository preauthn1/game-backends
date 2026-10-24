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



//Slot 非連接型Jackpot紀錄寫入及讀取
type SlotJackpotNoLinkHandler struct {}

func (fsp *SlotJackpotNoLinkHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && (sclsid == int32(netproto.GameMessageClassID_SlotJackpotNoLinkID))
}

func (fsp *SlotJackpotNoLinkHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	//傳送syspool data
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.Slot_JackpotData{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.Slot_JackpotData)
	ps := db.NewSqlParameters()
	ps.AddIntInput("intGameID", mm.GetGameID())
	ps.AddIntInput("intFlagID", mm.GetFlagID())
	ps.AddIntInput("intTableNo", mm.GetTableNo())
	ps.AddBigIntInput("lngContribution", mm.GetContribution())
	ps.AddBigIntInput("lngTotalContribution", mm.GetTotalContribution())
	ps.AddBigIntInput("lngAwardTotal", mm.GetAwardTotal())
	ps.AddIntInput("intLevelID", mm.GetLevel())
	ps.AddVarcharOutput("chvMsg", "")
	proc := db.NewProcedure("PrGs_Slot_JackpotPool_NoLink", ps)
	//mlog.System("-------:",proc.GetSqlString())
	ret, err := dbutil.DbUtil.ExecProc(proc)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()
	retcode := int32(ret.GetReturnValue())

	if err != nil {
		ctx.Error("执行存储过程[PrGs_Slot_JackpotPool_NoLink]错误, %v, %s", err, proc.GetSqlString())
		retcode = int32(0)
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SlotJackpotNoLinkRetID), nil)
	bmsg := new(netproto.Slot_JackpotAllData)
	pdata := make([]*netproto.Slot_JackpotData, 0)
	//get syspooldata
	if retcode == 1 {
		if ret.GetRetTableCount() > 0 {
			tbData := ret[0]
			for i := 0; i < len(tbData.Rows); i++ {
				detail := new(netproto.Slot_JackpotData)
				detail.GameID = proto.Int32(tbData.GetValueByColNameInt32(i, "GameID"))
				detail.FlagID = proto.Int32(tbData.GetValueByColNameInt32(i, "FlagID"))
				detail.TableNo = proto.Int32(tbData.GetValueByColNameInt32(i, "TableNo"))
				detail.Contribution = proto.Int64(tbData.GetValueByColNameInt64(i, "Contribution"))
				detail.TotalContribution = proto.Int64(tbData.GetValueByColNameInt64(i, "TotalContribution"))
				detail.AwardTotal = proto.Int64(tbData.GetValueByColNameInt64(i, "AwardTotal"))
				detail.Level = proto.Int32(tbData.GetValueByColNameInt32(i, "LevelID"))
				pdata = append(pdata, detail)
			}
		}
	}
	bmsg.AllData = pdata
	retmsg.MsgData = bmsg

	return []*msg.Message{retmsg}
}