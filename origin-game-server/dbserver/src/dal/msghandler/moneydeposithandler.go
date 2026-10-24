package msghandler

import (
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"util"
)

import (
	"db"
	"math"
	"module"
	"msg"
	"netproto"
	"network"
	"rpcmsg"
)

//银行存取款
type MoneyDepositHandler struct {
}

func (rh *MoneyDepositHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_DepositMoneyID)
}

func (rh *MoneyDepositHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.MoneyDeposit{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.MoneyDeposit)

	userid := rpcinfo.GetUserID()
	ip := rpcinfo.GetIPAddress()
	amount := int64(math.Abs(float64(mm.GetAmount())))
	op := mm.GetOP()
	moneypwd := mm.GetMoneyPassword()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	ps.AddIntInput("tintOpType", op)
	if moneypwd != "" {
		ps.AddVarcharInput("chvBankPwd", moneypwd, 32)
	}
	ps.AddBigIntInput("lngAmount", amount)
	ps.AddVarcharInput("chvIPAddress", ip, 15)
	ps.AddBigIntOutput("lngCurrentCashAmount", 0)
	ps.AddBigIntOutput("lngCurrentBankAmount", 0)
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrPs_Money_Deposit", ps)

	var ret db.RecordSet
	var err error
	ret, err = DbUtil.ExecProc(proc)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().SetData(ret).Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()

	var code int32 = 0
	retmessage := ""
	var currentcash int64 = 0
	var currentbank int64 = 0
	if err != nil {
		ctx.Error("执行存储过程PrPs_Money_Deposit错误, %v", err)
		code = 0
		retmessage = "服务器错误"
	} else {
		if ret.GetReturnValue() == 1 {
			code = 1
		}
		retmessage = ret.GetOutputParamValue("chvErrMsg").(string)
		currentcash = int64(ret.GetOutputParamValue("lngCurrentCashAmount").(int64))
		currentbank = int64(ret.GetOutputParamValue("lngCurrentBankAmount").(int64))
	}

	depositret := new(netproto.MoneyDepositRet)
	depositret.Amount = proto.Int64(amount)
	depositret.OP = proto.Int32(op)
	depositret.Code = proto.Int32(code)
	depositret.Message = proto.String(retmessage)
	depositret.CurrentMoney = proto.Int64(currentcash)
	depositret.CurrentBank = proto.Int64(currentbank)

	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Hall)
	retmsg.SClassID = int32(netproto.HallMsgClassID_DepositMoneyRetID)
	retmsg.MsgData = depositret

	return []*msg.Message{retmsg}
}
