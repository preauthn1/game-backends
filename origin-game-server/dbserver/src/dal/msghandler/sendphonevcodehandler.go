package msghandler

import proto "example.invalid/golang/protobuf/proto"

import (
	"db"
	"fmt"
	"io/ioutil"
	"mlog"
	"module"
	"msg"
	"net/http"
	"netproto"
	"network"
	"rpcmsg"
	"strings"
)

//发送验证码
type SendPhoneVCodeHandler struct {
}

func (rh *SendPhoneVCodeHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_SendPhoneVCodeID)
}

func (rh *SendPhoneVCodeHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcinfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.SendPhoneVCode{}
		return mm
	})

	if rmm == nil {
		return []*msg.Message{GetDBRetMsg(0)}
	}

	mm := rmm.(*netproto.SendPhoneVCode)
	tel := mm.GetTel()
	codetype := mm.GetCodeType()

	userid := rpcinfo.GetUserID()

	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", userid)
	if tel != "" {
		ps.AddVarcharInput("chvTel", tel, 32)
	}
	ps.AddIntInput("tnyCodeType", codetype)
	ps.AddIntOutput("tnyCountdown", 0)
	ps.AddVarcharOutput("chvErrMsg", "")
	ps.AddIntOutput("intCodeID", 0)

	proc := db.NewProcedure("PrPs_User_SendSMSCode", ps)
	//ret, err := DbUtil.ExecProc(proc)
	var ret db.RecordSet
	var err error
	ret, err = DbUtil.ExecProc(proc)

	code := 0
	retmessage := ""
	var codeId int32 = 0
	var countdown int32 = 0
	if err != nil {
		ctx.Error("执行存储过程PrPs_User_SendSMSCode错误, %v, %v", err, proc.GetSqlString())
		code = 0
		retmessage = "服务器错误"
	} else {
		if ret.GetReturnValue() == 1 {
			code = 1
		}
		codeId = int32(ret.GetOutputParamValue("intCodeID").(int64))
		countdown = int32(ret.GetOutputParamValue("tnyCountdown").(int64))
		retmessage = ret.GetOutputParamValue("chvErrMsg").(string)
	}

	coderet := &netproto.SendPhoneVCodeRet{}
	coderet.Code = proto.Int32(int32(code))
	coderet.Message = proto.String(retmessage)
	coderet.CountDown = proto.Int32(countdown)
	coderet.CodeID = proto.Int32(codeId)

	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Hall)
	retmsg.SClassID = int32(netproto.HallMsgClassID_SendPhoneVCodeRetID)
	retmsg.MsgData = coderet

	SendSMSCode(codeId)

	return []*msg.Message{retmsg}
}

//用Web API發送第三方簡訊服務
func SendSMSCode(pCode int32) {
	codeId := fmt.Sprintf("codeID=%v", pCode)
	UrlAPI := fmt.Sprintf("http://%s", SMSWebAPI)
	//mlog.Debug(err.Error())
	resp, err := http.Post(UrlAPI, "application/x-www-form-urlencoded", strings.NewReader(codeId))

	if err != nil {
		mlog.Error("調用 %s 錯誤, %v", UrlAPI, err)
	}

	defer resp.Body.Close()

	body, err := ioutil.ReadAll(resp.Body)

	if err != nil {
		// handle error
		mlog.Error("調用 %s 錯誤, %v", UrlAPI, err)
	}
	mlog.Debug("調用 API 成功, 返回 %s", body)
}
