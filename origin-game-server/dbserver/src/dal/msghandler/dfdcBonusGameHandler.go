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

//DFDC BonusGame紀錄寫入及讀取
type DfdcBonusGameHandler struct{}

func (fsp *DfdcBonusGameHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Game) && (sclsid == int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID) || sclsid == int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID))
}

func (fsp *DfdcBonusGameHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Game)
	if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameID) { //多福多財BonusGame紀錄
		_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
			mm := &netproto.DFDC_JackpotBonusGameRecord{}
			return mm
		})

		if rmm == nil {
			return []*msg.Message{GetDBRetMsg(0)}
		}

		//須把mm組為可寫入DB的模式(8組)
		mm := rmm.(*netproto.DFDC_JackpotBonusGameRecord)
		ps := db.NewSqlParameters()
		ps.AddIntInput("intUserID", mm.GetUserID())
		ps.AddVarcharInput("chvRecordStr", mm.GetChvRecordStr(), 100)
		ps.AddBigIntInput("lngWinMoney", mm.GetWinmoney())
		ps.AddVarcharOutput("chvMsg", "")
		proc := db.NewProcedure("PrGs_DFDC_JackpotBonusGameRecord", ps)
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
			mlog.Error("操作失敗 : [PrGs_DFDC_JackpotBonusGameRecord] 设定数据错误, %s", proc.GetSqlString())
		}
		if err != nil {
			ctx.Error("执行存储过程[PrGs_DFDC_JackpotBonusGameRecord]错误, %v, %s", err, proc.GetSqlString())
			retcode = int32(0)
		}
		return []*msg.Message{GetDBRetMsg(1)}
	} else if m.SClassID == int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusGameRetID) { //多福多財讀取BonusGame紀錄
		_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
			mm := &netproto.DFDC_JackpotBonusGameRet{}
			return mm
		})

		if rmm == nil {
			return []*msg.Message{GetDBRetMsg(0)}
		}

		//須把mm組為可寫入DB的模式(8組)
		mm := rmm.(*netproto.DFDC_JackpotBonusGameRet)
		ps := db.NewSqlParameters()
		ps.AddIntInput("intUserID", mm.GetUserID())
		ps.AddVarcharOutput("chvMsg", "")
		proc := db.NewProcedure("PrGs_DFDC_JackpotBonusGameRead", ps)
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
			mlog.Error("操作失敗 : [PrGs_DFDC_JackpotBonusGameRead] 设定数据错误, %s", proc.GetSqlString())
		}
		if err != nil {
			ctx.Error("执行存储过程[PrGs_DFDC_JackpotBonusGameRead]错误, %v, %s", err, proc.GetSqlString())
			retcode = int32(0)
		}

		retmsg = msg.NewMessage(int32(netproto.MessageBClassID_Game), int32(netproto.DFDC_GameMessageClassID_DFDCJackpotBonusDetailID), nil)
		msg := new(netproto.DFDC_JackpotBonusGame)
		msg.Money = proto.Int64(0)
		jackpotBonusGameRecord := make([]*netproto.DFDC_JackpotBonusGameDetail, 0)
		if retcode == 1 {
			if ret.GetRetTableCount() > 0 {
				tbData := ret[0]
				for i := 0; i < len(tbData.Rows); i++ {
					detail := new(netproto.DFDC_JackpotBonusGameDetail)
					detail.UserID = proto.Int32(int32(tbData.GetValueByColName(i, "UserID").(int64)))
					detail.TurnID = proto.Int32(int32(tbData.GetValueByColName(i, "TurnID").(int64)))
					detail.Place = proto.Int32(int32(tbData.GetValueByColName(i, "Place").(int64)))
					detail.Icon = proto.Int32(int32(tbData.GetValueByColName(i, "Icon").(int64)))
					msg.Money = proto.Int64(tbData.GetValueByColName(i, "WinMoney").(int64))
					jackpotBonusGameRecord = append(jackpotBonusGameRecord, detail)
				}
			}
		} else {
			//沒有資料，丟空值
			detail := new(netproto.DFDC_JackpotBonusGameDetail)
			detail.UserID = proto.Int32(0)
			detail.Icon = proto.Int32(0)
			detail.Place = proto.Int32(0)
			detail.TurnID = proto.Int32(0)
			jackpotBonusGameRecord = append(jackpotBonusGameRecord, detail)
		}
		msg.BonusDetail = jackpotBonusGameRecord
		retmsg.MsgData = msg
	}
	return []*msg.Message{retmsg}
}
