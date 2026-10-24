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

type UpdateUserMoneyHandler struct {
}

//捕鱼更新金币
func NewUpdateUserMoneyHandler() *UpdateUserMoneyHandler {
	handler := new(UpdateUserMoneyHandler)
	return handler
}

func (h *UpdateUserMoneyHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NewByDBServer) && sclsid == int32(netproto.NewBYDBServerClassID_UpdateUserMoneyID)
}

func (h *UpdateUserMoneyHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	defer util.PanicErrStack()
	mmbyte := m.MsgData.([]byte)

	mm := &netproto.UpdateUserMoney{}
	err := proto.Unmarshal(mmbyte, mm)

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_UpdateUserMoneyRetID), nil)
	retdata := new(netproto.NewByDBRegMsg)

	if err != nil {
		ctx.Error("解析UpdateUserMoney错误,%v", err)

		retdata.Code = proto.Int32(-1)
		retdata.ChvMsg = proto.String("解析UpdateUserMoney错误")
		retmsg.MsgData = retdata
		return []*msg.Message{retmsg}
	}

	audiLogUpdateMoney(mm)

	proc, err := h.getData(mm)

	if err != nil {
		mlog.Error("执行存储过程PrGs_UpdateUserMoney错误, %v , 執行存儲過程%s", err, proc.GetSqlString())
		retdata.Code = proto.Int32(int32(-1))
		retdata.ChvMsg = proto.String(err.Error())
	} else {
		retdata.Code = proto.Int32(0)
		retdata.ChvMsg = proto.String("")
	}

	retmsg.MsgData = retdata
	return nil
}

func (h *UpdateUserMoneyHandler) getData(mm *netproto.UpdateUserMoney) (*db.Procedure, error) {
	var err error
	endChannel := make(chan bool)
	var proc *db.Procedure
	var dbret db.RecordSet

	go func(){
		for i:=0; i<1000000; i++ {
			fmt.Println("update money 重新连线尝试 ", i)
			d := GetDatabase()
			ps := db.NewSqlParameters()
			ps.AddIntInput("intMsgID", mm.GetMsgID())
			ps.AddIntInput("intUserID", mm.GetUserID())
			ps.AddIntInput("lngUpdateAmount", mm.GetUpdateAmount())
			ps.AddIntInput("lngTaxAmount", mm.GetTaxAmount())
			ps.AddIntInput("intServerID", mm.GetServerID())
			ps.AddIntInput("lngCouponAmount", mm.GetCouponAmount())
			proc := db.NewProcedure("PrGs_UpdateUserMoney", ps)
			dbret, err = d.ExecProc(proc)

			if err != nil {
				mlog.Error("执行存储过程PrGs_UpdateUserMoney错误, %v , 執行存儲過程%s", err, proc.GetSqlString())
				time.Sleep(time.Second * 1)
			} else {
				if dbret.GetReturnValue() == 1 {
					endChannel <- true
					break
				} else {
					if i == 4 {
						endChannel <- true
						break
					}
					mlog.Error("写分失败,执行存储过程PrGs_UpdateUserMoney错误, %v , 執行存儲過程%s", err, proc.GetSqlString())
					time.Sleep(time.Second * 15)
				}
			}
		}
	}()

	select {
	case _ = <-endChannel:
		mlog.NewDefaultLogger().Debug("chan update end")
	}

	return proc, err
}

func audiLogUpdateMoney(mm *netproto.UpdateUserMoney) {
	if conf.TestMode {
		return
	}

	type LogData struct {
		GroupName    string //系統水池
		MsgID        int32
		UserID       int32
		UpdateMoney  int32
		TaxAmount    int32
		ServerID     int32
		CouponAmount int32
		CreateTime   string
	}

	msg := LogData{
		GroupName:    "UpdateUserMoney",
		MsgID:        mm.GetMsgID(),
		UserID:       mm.GetUserID(),
		UpdateMoney:  mm.GetUpdateAmount(),
		TaxAmount:    mm.GetTaxAmount(),
		ServerID:     mm.GetServerID(),
		CouponAmount: mm.GetCouponAmount(),
		CreateTime:   time.Now().Format("2006-01-02 15:04:05.000Z"),
	}

	mylog.MongoLog(mm.GetUserID(), 0, 0, msg)
}
