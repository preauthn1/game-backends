package task

import (
	"db"
	"fmt"
	"io/ioutil"
	"mlog"
	"msg"
	"net/http"
	"netproto"
	"time"
)

import proto "example.invalid/golang/protobuf/proto"

//数据库连接字符串
var ConnString string
var PoolSize int = 100
var database *db.Database
var DbUtil db.DatabaseEngine
var MonitorName string

func InitDatabase() {
	dao := new(db.Database)
	dao.Connstring = ConnString
	dao.Connpool = db.NewConnPool(PoolSize, ConnString)
	database = db.NewDatabase(ConnString, PoolSize)
	DbUtil = db.NewDatabaseEngine(database)
}

func GetDatabase() *db.Database {
	return database
}

//获取提示消息
func GetErrMsgValue(set db.RecordSet) string {
	rlen := len(set)

	return set[rlen-2].GetValueByColName(0, "chvErrMsg").(string)
}

//获取数据库执行结果消息
func GetDBRetMsg(retCode int32) *msg.Message {
	ret := new(netproto.DBServerRet)
	ret.Code = proto.Int32(retCode)

	msg := new(msg.Message)
	msg.BClassID = int32(netproto.MessageBClassID_DBServer)
	msg.SClassID = int32(netproto.DBServerClassID_DBRetID)
	msg.MsgData = ret

	return msg
}

func getRetMsg(code int32, msgContent string) *netproto.RetMessage {
	retmsg := new(netproto.RetMessage)
	retmsg.Code = proto.Int32(code)
	retmsg.Message = proto.String(msgContent)

	return retmsg
}

type TelegramBase struct {
	AlterBotAPI string
}
func GetTelegramBase() *TelegramBase {
	t := new(TelegramBase)
	t.AlterBotAPI = "https://example.invalid/bot847197978:AAGatYITJ7S92p6RfQvks61-rXTn0dGzXWU/sendMessage?chat_id=-367664652&text=%s"
	return t
}

func SendMsgToTelegram(context string) {
	t := GetTelegramBase()
	context = fmt.Sprintf("[%v] %v", MonitorName, context)
	urlmsg := fmt.Sprintf(t.AlterBotAPI, context)
	timeout := time.Duration(10 * time.Second)
	client := http.Client{
		Timeout: timeout,
	}
	resp, err := client.Get(urlmsg)
	//resp, err := http.Post(url, "application/x-www-form-urlencoded", strings.NewReader(codeId))

	if err != nil {
		mlog.Error("調用 %s 錯誤, %v", urlmsg, err)
	}

	defer resp.Body.Close()

	body, err := ioutil.ReadAll(resp.Body)

	if err != nil {
		// handle error
		mlog.Error("調用 %s 錯誤, %v", urlmsg, err)
	}
	mlog.Debug("body[%v]", string(body))
}
