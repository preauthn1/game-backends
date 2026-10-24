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

// 無限級預提按鈕
type UnlimitConvertOrderHandler struct {
}

func (rh *UnlimitConvertOrderHandler) IsHook(bclsid int32, sclsid int32) bool {
	// mlog.System("HANDLER名稱:UnlimitConvertOrderHandler,接收HOOK") // DEBUG專用
	return bclsid == int32(netproto.MessageBClassID_Hall) &&
		sclsid == int32(netproto.HallMsgClassID_UnlimitConvertOrderID)
}

func (rh *UnlimitConvertOrderHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.UnlimitConvertOrderMsg{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.UnlimitConvertOrderMsg)

	if mm.GetLngAmount() > 0 {
		returnMessage := rh.OnMessageInner(ctx, clt, m, mm)
		return returnMessage
	}

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UnlimitConvertOrderRetID), nil)
	retdata := new(netproto.UnlimitConvertOrderRet) // 回傳封包

	if mm.GetLngAmount() <= 0 {
		retdata.ChvMsg = proto.String("可提现余额不正确，金额需大于0")
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}

func (rh *UnlimitConvertOrderHandler) OnMessageInner(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message, mm *netproto.UnlimitConvertOrderMsg) []*msg.Message {
	// mlog.System("HANDLER名稱:UnlimitConvertOrderHandler,進入OnMessage") // DEBUG專用

	ps := db.NewSqlParameters()

	//在這裡設定輸入參數，須和資料庫輸出欄位相同
	ps.AddIntInput("intUserID", mm.GetUserID()) //使用者ID
	ps.AddIntInput("intType", mm.GetType())
	ps.AddVarcharInput("chvIPaddress", mm.GetChvIPaddress(), 32)
	ps.AddIntInput("lngAmount", mm.GetLngAmount()) // 需要防呆
	ps.AddVarcharOutput("chvMsg", "")              //chvMsg的輸出

	//開始準備SP字串
	proc := db.NewProcedure("PrPs_Unlimit_ConvertOrder", ps)
	// mlog.System("HANDLER名稱:UnlimitConvertOrderHandler,SP字串:%v", proc.GetSqlString()) // DEBUG專用
	dbret, err := DbUtil.ExecProc(proc)

	//資料庫回傳值
	retcode := int32(dbret.GetReturnValue()) //retcode必須為1
	// mlog.System("HANDLER名稱:UnlimitConvertOrderHandler,retcode:%v", retcode) // DEBUG專用

	// 準備回傳給CLIENT的封包
	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_UnlimitConvertOrderRetID), nil)
	retdata := new(netproto.UnlimitConvertOrderRet) // 回傳封包

	if err != nil || retcode != 1 {
		strMsg := fmt.Sprintf("执行存储过程 PrPs_Unlimit_ConvertOrder 错误 %v", err)
		ctx.Error(strMsg)
	} else {
		if retcode == 1 { // 準備回傳資料
			// 第一張表
			tb := dbret[0]
			if len(tb.Rows) > 0 {
				retdata.ChvMsg = proto.String(string(tb.GetValueByColName(0, "chvMsg").(string)))
			} else {
				retdata.ChvMsg = proto.String("")
			}
		}
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}
