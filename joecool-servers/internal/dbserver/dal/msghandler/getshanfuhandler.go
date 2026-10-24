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
)

import (
	"bytes"
	"crypto/md5"
	"encoding/hex"
	"fmt"
	"io/ioutil"
	"net/http"
	"strconv"
	"strings"
	"time"
)

const (
	SHANFU_Col_AccountID		int32 = iota
	SHANFU_Col_Sign
	SHANFU_Col_SumAmount
	SHANFU_Col_PayAgentUrl
	SHANFU_Col_Count
)

//閃付
type GetShanFuHandler struct {
}

func (rh *GetShanFuHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) &&
		sclsid == int32(netproto.HallMsgClassID_GetShanFuID)
}

func (rh *GetShanFuHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	rpcInfo, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.GetShanFuMsg{}
		return mm
	})

	if rmm == nil || rpcInfo == nil {
		return []*msg.Message{dbutil.GetDBRetMsg(0)}
	}

	// 欄位名稱
	ary_strColName := [SHANFU_Col_Count]string {
		"AccountID",
		"Sign",
		"SumAmount",
		"PayAgentUrl",
	}
	// 各欄位回傳值，依所需資料型態做區分
	map_strColValue := make(map[string]string)
	map_i64ColValue := make(map[string]int64)

	mm := rmm.(*netproto.GetShanFuMsg)
	ps := db.NewSqlParameters()
	ps.AddIntInput("intUserID", mm.GetUserID())
	proc := db.NewProcedure("PrPs_GetShanFuKey", ps)
	dbret, err := dbutil.DbUtil.ExecProc(proc)
	retcode := int32(dbret.GetReturnValue())

	retmsg := msg.NewMessage(int32(netproto.MessageBClassID_Hall), int32(netproto.HallMsgClassID_GetShanFuRetID), nil)
	retdata := new(netproto.GetShanFuRet)

	if err != nil {
		strMsg := fmt.Sprintf("执行存储过程PrPs_GetShanFuKey错误,%v", err)
		ctx.Error(strMsg)
	} else {
		if retcode == 1 {
			tb := dbret[0]
			if len(tb.Rows) > 0 {
				for _, strName := range ary_strColName {
					defer func() {
						if Error := recover(); Error != nil {
							mlog.NewDefaultLogger().SetData(tb).Error("存取資料錯誤：欄位[%s]", strName)
						}
					}()

					switch strName {
					case ary_strColName[SHANFU_Col_SumAmount]:
						map_i64ColValue[strName] = tb.GetValueByColName(0, strName).(int64)
					default:
						map_strColValue[strName] = tb.GetValueByColName(0, strName).(string)
					}
				}

				accountid := map_strColValue[ary_strColName[SHANFU_Col_AccountID]]
				publickey := map_strColValue[ary_strColName[SHANFU_Col_Sign]]
				sumamount := map_i64ColValue[ary_strColName[SHANFU_Col_SumAmount]]
				url := map_strColValue[ary_strColName[SHANFU_Col_PayAgentUrl]]
				url = fmt.Sprintf("%v/agent/agents", url)

				stramount := "000"
				if sumamount > 0 {
					stramount = strconv.Itoa(int(sumamount))
				}
				retdata.Data = proto.String(SendShanFu(accountid, stramount, publickey, url))
				mlog.System("閃付回應:  %v", retdata.GetData())
			}
		}
	}

	retmsg.MsgData = retdata
	return []*msg.Message{retmsg}
}

func SendShanFu(account string, sumamount string, publickey, url string) string {
	sec := time.Now().Unix()

	var buf bytes.Buffer
	buf.WriteString("accountId=")
	buf.WriteString(account)
	buf.WriteString("&amount=")
	buf.WriteString(sumamount)
	buf.WriteString("&timestamp=")
	buf.WriteString(strconv.Itoa(int(sec)))
	buf.WriteString("&key=")
	buf.WriteString(publickey)

	signVal := buf.String()

	md5Ctx := md5.New()
	md5Ctx.Write([]byte(signVal))
	cipherStr := md5Ctx.Sum(nil)
	sign := hex.EncodeToString(cipherStr)
	sign = strings.ToUpper(sign)
	resBody := proxyRequest(url, account, sumamount, sec, sign)
	return resBody
}

func proxyRequest(url string, accountid string, amount string, sec int64, sign string) string {
	timeout := time.Duration(30 * time.Second)
	client := http.Client{
		Timeout: timeout,
	}

	sendBody := fmt.Sprintf("accountId=%v&amount=%v&timestamp=%v&sign=%v",
		accountid, amount, sec, sign)
	mlog.System("sendBody : %v", sendBody)
	reqest, err := http.NewRequest("POST", url, strings.NewReader(sendBody))

	if err != nil {
		mlog.Error("調用 %s 錯誤, %v", url, err)
		return ""
	}

	reqest.Header.Set("Content-Type", "application/x-www-form-urlencoded;param=value") //必须设定该参数,POST参数才能正常提交

	resp, err := client.Do(reqest) //发送请求
	if err != nil {
		mlog.Error("調用 %s 錯誤, %v", url, err)
		return ""
	}
	defer resp.Body.Close() //一定要关闭resp.Body

	content, err := ioutil.ReadAll(resp.Body)

	if err != nil {
		mlog.Error("調用 %s 錯誤, %v", url, err)
	}
	return string(content)
}
