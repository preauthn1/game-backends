package task

import (
	"db"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"netproto"
	"network"
	"util"
)

type UserLoginFishHandler struct {

}

//捕鱼登入
func NewUserLoginFishHandler() *UserLoginFishHandler {
	handler := new(UserLoginFishHandler)
	return handler
}

func (h *UserLoginFishHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NewByDBServer) && sclsid == int32(netproto.NewBYDBServerClassID_UserLoginFishID)
}

func (h *UserLoginFishHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	defer util.PanicErrStack()
	mmbyte := m.MsgData.([]byte)

	mm := &netproto.UserLoginFishMsg{}
	err := proto.Unmarshal(mmbyte, mm)

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_UserLoginFishRetID), nil)
	retdata := new(netproto.UserLoginFishRetMsg)
	retdata.UserID = proto.Int32(mm.GetUserID())
	retdata.UserType = proto.Int32(0)
	retdata.NickName = proto.String("")
	retdata.FaceID = proto.Int32(0)
	retdata.Sex = proto.Int32(0)
	retdata.CashAmount = proto.Int64(0)
	retdata.CouponAmount = proto.Int64(0)
	retdata.TotalCharge = proto.Int64(0)
	retdata.WinCount = proto.Int32(0)
	retdata.LostCount = proto.Int32(0)
	retdata.DrawCount = proto.Int32(0)
	retdata.IsOffLine = proto.Int32(0)
	retdata.ServerAddr = proto.String("")
	retdata.GameBuff = proto.String("")
	retdata.TotalCharge = proto.Int64(0)
	retdata.TradeID = proto.Int64(0)

	if err != nil {
		ctx.Error("解析UserLoginFish错误,%v", err)
		return []*msg.Message{retmsg}
	}

	d := GetDatabase()
	ps := db.NewSqlParameters()
	ps.AddIntInput("sintServerID", mm.GetServerID())
	ps.AddIntInput("intUserID", mm.GetUserID())
	ps.AddVarcharInput("chvIPAddress", mm.GetIPAddress(), 15)
	ps.AddVarcharInput("chvCer", mm.GetCer(), 32)
	ps.AddIntInput("tnyHDType", mm.GetHDType())
	ps.AddVarcharInput("chvHDCode", mm.GetHDCode(), 64)
	ps.AddVarcharOutput("chvErrMsg", "")

	proc := db.NewProcedure("PrGs_UserLoginFish", ps)
	mlog.System("%v", proc.GetSqlString())
	dbret, err := d.ExecProc(proc)
	defer func() {
		if err := recover(); err != nil {
			errorStr := util.GetStackInfo()
			mlog.NewDefaultLogger().Error("发生错误%s, 執行存儲過程%s", errorStr, proc.GetSqlString())
			panic(err)
		}
	}()

	if err != nil {
		mlog.Error("执行存储过程PrGs_UserLoginFish错误, %v , 執行存儲過程%s", err, proc.GetSqlString())
	}
	if dbret.GetReturnValue() != 1 {
		mlog.Error("执行存储过程PrGs_UserLoginFish错误, %v , 執行存儲過程%s", dbret.GetOutputParamValue("chvErrMsg").(string), proc.GetSqlString())
	}


	if dbret.GetReturnValue() == 1 {
		//userdata.UserID = proto.Int32(ret[0].GetSingleValueInt32("UserID"))
		//TODO
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}

