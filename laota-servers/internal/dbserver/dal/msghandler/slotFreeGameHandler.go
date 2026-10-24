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

//Slot FreeSpin紀錄寫入及讀取
type SlotFreeGameHandler struct{}

func (fsp *SlotFreeGameHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && (sclsid == int32(netproto.GameMessageClassID_SlotFreeSpinID) || sclsid == int32(netproto.GameMessageClassID_SlotFreeSpinRetID))
}

func (fsp *SlotFreeGameHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Game)
	if m.SClassID == int32(netproto.GameMessageClassID_SlotFreeSpinID) { //Slot FreeSpin 紀錄
		_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
			mm := &netproto.Slot_FreeSpinDetail{}
			return mm
		})

		if rmm == nil {
			return []*msg.Message{dbutil.GetDBRetMsg(0)}
		}

		//須把mm組為可寫入DB的模式(8組)
		mm := rmm.(*netproto.Slot_FreeSpinDetail)
		ps := db.NewSqlParameters()
		ps.AddIntInput("intUserID", mm.GetUserID())
		ps.AddIntInput("intGameID", mm.GetGameID())
		ps.AddIntInput("intFlagID", mm.GetFlagID())
		ps.AddBigIntInput("intUserBet", mm.GetUserBet())
		ps.AddIntInput("intFreeSpinGroup", mm.GetFreeSpinGroup())
		ps.AddIntInput("intTimes", mm.GetTimes())
		ps.AddIntInput("intChooseRate", mm.GetChooseRate())
		ps.AddBigIntInput("lngWinMoney", mm.GetWinMoney())
		ps.AddBigIntInput("intOriginID", mm.GetOriginID())
		ps.AddIntInput("intWinTimes", mm.GetWinTimes())

		//tnyIsCoupon := int32(0)
		//// if mm.GetIsCoupon() {
		//// 	tnyIsCoupon = 1
		//// }
		//
		//ps.AddTinyIntInput("tnyIsCoupon", byte(tnyIsCoupon))
		ps.AddVarcharOutput("chvMsg", "")
		proc := db.NewProcedure("PrGs_Slot_FreeSpinRecord", ps)
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
			mlog.Error("操作失敗 : [PrGs_Slot_FreeSpinRecord] 设定数据错误, %s", proc.GetSqlString())
		}
		if err != nil {
			ctx.Error("执行存储过程[PrGs_Slot_FreeSpinRecord]错误, %v, %s", err, proc.GetSqlString())
			retcode = int32(0)
		}
		return []*msg.Message{dbutil.GetDBRetMsg(1)}
	} else if m.SClassID == int32(netproto.GameMessageClassID_SlotFreeSpinRetID) { //Slot FreeSpin紀錄
		_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
			mm := &netproto.Slot_FreeSpinRead{}
			return mm
		})

		if rmm == nil {
			return []*msg.Message{dbutil.GetDBRetMsg(0)}
		}

		//須把mm組為可寫入DB的模式(8組)
		mm := rmm.(*netproto.Slot_FreeSpinRead)
		ps := db.NewSqlParameters()
		ps.AddIntInput("intUserID", mm.GetUserID())
		ps.AddIntInput("intGameID", mm.GetGameID())
		ps.AddIntInput("intFlagID", mm.GetFlagID())
		ps.AddVarcharOutput("chvMsg", "")
		proc := db.NewProcedure("PrGs_Slot_FreeSpinRead", ps)
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
			mlog.Error("操作失敗 : [PrGs_Slot_FreeSpinRead] 设定数据错误, %s", proc.GetSqlString())
		}
		if err != nil {
			ctx.Error("执行存储过程[PrGs_Slot_FreeSpinRead]错误, %v, %s", err, proc.GetSqlString())
			retcode = int32(0)
		}

		retmsg = msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.GameMessageClassID_SlotFreeSpinDetailID), nil)
		//初始化預設資料
		detail := new(netproto.Slot_FreeSpinDetail)
		detail.UserID = proto.Int32(0)
		detail.GameID = proto.Int32(0)
		detail.FlagID = proto.Int32(0)
		detail.UserBet = proto.Int64(0)
		detail.FreeSpinGroup = proto.Int32(0)
		detail.Times = proto.Int32(0)
		detail.ChooseRate = proto.Int32(0)
		detail.WinMoney = proto.Int64(0)
		// detail.IsCoupon = proto.Bool(false)
		if retcode == 1 {
			if ret.GetRetTableCount() > 0 {
				tbData := ret[0]
				for i := 0; i < len(tbData.Rows); i++ {
					detail.UserID = proto.Int32(int32(tbData.GetValueByColNameInt64(i, "UserID")))
					detail.GameID = proto.Int32(int32(tbData.GetValueByColNameInt64(i, "GameID")))
					detail.FlagID = proto.Int32(int32(tbData.GetValueByColNameInt64(i, "FlagID")))
					detail.UserBet = proto.Int64(tbData.GetValueByColNameInt64(i, "UserBet"))
					detail.FreeSpinGroup = proto.Int32(int32(tbData.GetValueByColNameInt64(i, "FreeSpinGroup")))
					detail.Times = proto.Int32(int32(tbData.GetValueByColNameInt64(i, "Times")))
					detail.ChooseRate = proto.Int32(int32(tbData.GetValueByColNameInt64(i, "ChooseRate")))
					detail.WinMoney = proto.Int64(tbData.GetValueByColNameInt64(i, "WinMoney"))
					// if tbData.GetValueByColNameInt64(i, "IsCoupon") == 1 {
					// 	detail.IsCoupon = proto.Bool(true)
					// } else {
					// 	detail.IsCoupon = proto.Bool(false)
					// }
					break
				}
			}
		}

		retmsg.MsgData = detail
	}
	return []*msg.Message{retmsg}
}
