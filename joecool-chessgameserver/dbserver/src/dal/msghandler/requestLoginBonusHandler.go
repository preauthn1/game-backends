package msghandler

import proto "example.invalid/golang/protobuf/proto"

import (
	"db"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
	"util"
)

//領取登入獎勵
type RequestLoginBonusHandler struct {
}

func (rh *RequestLoginBonusHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && (sclsid == int32(netproto.HallMsgClassID_RequestLoginBonusID) || sclsid == int32(netproto.HallMsgClassID_ApplyLoginBonusID))
}

func (rh *RequestLoginBonusHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo := rpcmsg.GetRPCInfo(m)
	var retData *netproto.LoginBonusData
	userid := rpcinfo.GetUserID()
	ip := rpcinfo.GetIPAddress()
	if m.SClassID == int32(netproto.HallMsgClassID_RequestLoginBonusID) { //查詢
		ps := db.NewSqlParameters()
		ps.AddIntInput("intUserID", userid)
		ps.AddIntOutput("LoginTimes", 0)
		ps.AddIntOutput("GiftCash", 0)
		ps.AddVarcharOutput("chvRtnMsg", "")
		proc := db.NewProcedure("PrPs_Event_LoginBonusCheck", ps)
		ret, err := DbUtil.ExecProc(proc)
		defer func() {
			if err := recover(); err != nil {
				errorStr := util.GetStackInfo()
				mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
				panic(err)
			}
		}()
		var code int32 = 0
		var loginTimes int64 = 0
		var giftCash int64 = 0
		var retmessage = ""
		if err != nil {
			ctx.Error("执行存储过程PrPs_Event_LoginBonusCheck错误, %v", err)
			code = -99
			retmessage = "服务器错误"
		} else {
			code = int32(ret.GetReturnValue())
			if code == 1 { //查詢的 code 若等於1, 則回給Client 2
				code = 2
			}
			retmessage = ret.GetOutputParamValue("chvRtnMsg").(string)
			loginTimes = ret.GetOutputParamValue("LoginTimes").(int64)
			giftCash = ret.GetOutputParamValue("GiftCash").(int64)
		}
		retData = new(netproto.LoginBonusData)
		retData.RetCode = proto.Int32(code)
		retData.RetMsg = proto.String(retmessage)
		retData.LoginTimes = proto.Int32(int32(loginTimes))
		retData.GiftCash = proto.Int64(giftCash)

	} else if m.SClassID == int32(netproto.HallMsgClassID_ApplyLoginBonusID) { //領取
		ps := db.NewSqlParameters()
		ps.AddIntInput("intUserID", userid)
		ps.AddVarcharInput("chvIPAddress", ip, 15)
		ps.AddIntOutput("LoginTimes", 0)
		ps.AddIntOutput("lngCurrentAmount", 0)
		ps.AddIntOutput("GiftCash", 0)
		ps.AddVarcharOutput("chvRtnMsg", "")
		proc := db.NewProcedure("PrPs_Event_LoginBonus", ps)
		ret, err := DbUtil.ExecProc(proc)
		defer func() {
			if err := recover(); err != nil {
				errorStr := util.GetStackInfo()
				mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
				panic(err)
			}
		}()
		var code int32 = 0
		var loginTimes int64 = 0
		var currentAmount int64 = 0
		var giftCash int64 = 0
		var retmessage = ""
		if err != nil {
			ctx.Error("执行存储过程PrPs_Event_LoginBonus错误, %v", err)
			code = -99
			retmessage = "服务器错误"
		} else {
			code = int32(ret.GetReturnValue())
			retmessage = ret.GetOutputParamValue("chvRtnMsg").(string)
			loginTimes = ret.GetOutputParamValue("LoginTimes").(int64)
			currentAmount = ret.GetOutputParamValue("lngCurrentAmount").(int64)
			giftCash = ret.GetOutputParamValue("GiftCash").(int64)
		}
		retData = new(netproto.LoginBonusData)
		retData.RetCode = proto.Int32(code)
		retData.RetMsg = proto.String(retmessage)
		retData.LoginTimes = proto.Int32(int32(loginTimes))
		retData.CurrentAmount = proto.Int64(currentAmount)
		retData.GiftCash = proto.Int64(giftCash)
	}
	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Hall)
	retmsg.SClassID = int32(netproto.HallMsgClassID_RequestLoginBonusRetID)
	retmsg.MsgData = retData

	return []*msg.Message{retmsg}
}
