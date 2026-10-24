package task

import (
	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/db"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
)

//数据库连接字符串
var ConnString string
var PoolSize int = 100
var database *db.Database
var DbUtil db.DatabaseEngine

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
