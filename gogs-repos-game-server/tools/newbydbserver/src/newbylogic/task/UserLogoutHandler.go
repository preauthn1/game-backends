package task

import (
	"conf"
	"db"
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"mylog"
	"netproto"
	"network"
	"time"
	"util"
)

type UserLogoutFishHandler struct {
}

//捕鱼登出
func NewUserLogoutFishHandler() *UserLogoutFishHandler {
	handler := new(UserLogoutFishHandler)
	return handler
}

func (h *UserLogoutFishHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NewByDBServer) && sclsid == int32(netproto.NewBYDBServerClassID_UserLogoutFishID)
}

func (h *UserLogoutFishHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	defer util.PanicErrStack()
	mmbyte := m.MsgData.([]byte)

	mm := &netproto.UserLogoutFishMsg{}
	err := proto.Unmarshal(mmbyte, mm)

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_UserLogoutFishRetID), nil)
	retdata := new(netproto.NewByDBRegMsg)

	if err != nil {
		ctx.Error("解析UserLogoutFish错误,%v", err)

		retdata.Code = proto.Int32(-1)
		retdata.ChvMsg = proto.String("解析UserLogoutFish错误")
		retmsg.MsgData = retdata
		return []*msg.Message{retmsg}
	}
	audiLogLogout(mm)

	proc, dbret, err := h.getData(mm, retdata)
	if err != nil {
		mlog.Error("重新1百万次连线尝试失败.")
		retdata.Code = proto.Int32(int32(dbret.GetReturnValue()))
		retdata.ChvMsg = proto.String(err.Error())
	}

	if dbret.GetReturnValue() != 1 {
		mlog.Error("执行存储过程PrGs_UserLogout错误, %v , 執行存儲過程%s", dbret.GetOutputParamValue("chvErrMsg").(string), proc.GetSqlString())
		retdata.Code = proto.Int32(int32(dbret.GetReturnValue()))
		retdata.ChvMsg = proto.String(dbret.GetOutputParamValue("chvErrMsg").(string))
	}

	if dbret.GetReturnValue() == 1 {
		retdata.Code = proto.Int32(int32(dbret.GetReturnValue()))
		retdata.ChvMsg = proto.String("")
	}

	retmsg.MsgData = retdata
	return nil
}

func (h *UserLogoutFishHandler) getData(mm *netproto.UserLogoutFishMsg, retdata *netproto.NewByDBRegMsg) (*db.Procedure, db.RecordSet, error) {
	var err error
	endChannel := make(chan bool)
	var proc *db.Procedure
	var dbret db.RecordSet

	go func() {
		for i := 0; i < 1000000; i++ {
			fmt.Println("logout 重新连线尝试 ", i)
			d := GetDatabase()
			ps := db.NewSqlParameters()
			ps.AddIntInput("intMsgID", mm.GetMsgID())
			ps.AddIntInput("intUserID", mm.GetUserID())
			ps.AddIntInput("lngUpdateAmount", mm.GetUpdateAmount())
			ps.AddIntInput("lngTaxAmount", mm.GetTaxAmount())
			ps.AddIntInput("lngFlowAmount", mm.GetFlowAmount())
			ps.AddIntInput("intWinCount", mm.GetWinCount())
			ps.AddIntInput("intLoseCount", mm.GetLoseCount())
			ps.AddIntInput("intDrawCount", mm.GetDrawCount())
			ps.AddVarcharInput("dtmLoginTime", mm.GetLoginTime(), 32)
			ps.AddVarcharInput("dtmLogoutTime", mm.GetLogoutTime(), 32)
			ps.AddVarcharInput("chvIPAddress", mm.GetIPAddress(), 15)
			ps.AddIntInput("tnyHDType", mm.GetHDType())
			ps.AddVarcharInput("chvHDCode", mm.GetHDCode(), 64)
			ps.AddIntInput("intGameTime", mm.GetGameTime())
			ps.AddIntInput("sintGameID", mm.GetGameID())
			ps.AddIntInput("sintServerID", mm.GetServerID())
			ps.AddBigIntInput("lngLoginMoney", mm.GetLoginMoney())
			ps.AddIntInput("tnyScoreUpdateMode", mm.GetScoreUpdateMode())
			ps.AddBigIntInput("lngTradeID", mm.GetTradeID())
			ps.AddIntInput("intLastWinLose", mm.GetLastWinLose())
			ps.AddIntInput("lngCouponAmount", mm.GetCouponAmount())
			ps.AddVarcharOutput("chvErrMsg", "")
			proc = db.NewProcedure("PrGs_UserLogout", ps)
			dbret, err = d.ExecProc(proc)

			if err != nil {
				mlog.Error("执行存储过程PrGs_UserLogout错误, %v , 執行存儲過程%s", err, proc.GetSqlString())
				mlog.Error("重新连线尝试 %v次", i)

				retdata.Code = proto.Int32(int32(dbret.GetReturnValue()))
				retdata.ChvMsg = proto.String(err.Error())
				time.Sleep(time.Second * 1)
			} else {
				endChannel <- true
				break
			}
		}
	}()

	select {
	case _ = <-endChannel:
		mlog.NewDefaultLogger().Debug("chan logout end")
	}
	return proc, dbret, err
}

func audiLogLogout(mm *netproto.UserLogoutFishMsg) {
	if conf.TestMode {
		return
	}

	type LogData struct {
		GroupName       string //系統水池
		MsgID           int32
		UserID          int32
		UpdateAmount    int32
		TaxAmount       int32
		FlowAmount      int32
		WinCount        int32
		LoseCount       int32
		DrawCount       int32
		LoginTime       string
		LogoutTime      string
		IPAddress       string
		HDType          int32
		HDCode          string
		GameTime        int32
		GameID          int32
		ServerID        int32
		LoginMoney      int64
		ScoreUpdateMode int32
		TradeID         int64
		LastWinLose     int32
		CouponAmount    int32
		CreateTime      string
	}

	msg := LogData{
		GroupName:       "UserLogout",
		MsgID:           mm.GetMsgID(),
		UserID:          mm.GetUserID(),
		UpdateAmount:    mm.GetUpdateAmount(),
		TaxAmount:       mm.GetTaxAmount(),
		FlowAmount:      mm.GetFlowAmount(),
		WinCount:        mm.GetWinCount(),
		LoseCount:       mm.GetLoseCount(),
		DrawCount:       mm.GetDrawCount(),
		LoginTime:       mm.GetLoginTime(),
		LogoutTime:      mm.GetLogoutTime(),
		IPAddress:       mm.GetIPAddress(),
		HDType:          mm.GetHDType(),
		HDCode:          mm.GetHDCode(),
		GameTime:        mm.GetGameTime(),
		ServerID:        mm.GetServerID(),
		LoginMoney:      mm.GetLoginMoney(),
		ScoreUpdateMode: mm.GetScoreUpdateMode(),
		TradeID:         mm.GetTradeID(),
		LastWinLose:     mm.GetLastWinLose(),
		CouponAmount:    mm.GetCouponAmount(),
		CreateTime:      time.Now().Format("2006-01-02 15:04:05.000Z"),
	}

	mylog.MongoLog(mm.GetUserID(), 0, 0, msg)
}
