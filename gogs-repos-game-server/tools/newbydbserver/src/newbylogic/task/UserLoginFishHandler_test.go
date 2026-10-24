package task

import (
	"config"
	"example.invalid/golang/protobuf/proto"
	"module"
	"msg"
	"netproto"
	"network"
	"path"
	"runtime"
	"strconv"
	"strings"
	"testing"
)

func TestNewUserLoginFishHandler(t *testing.T) {
	initDBConnection()

	msgdata := new(netproto.UserLoginFishMsg)
	msgdata.ServerID = proto.Int32(300)
	msgdata.UserID = proto.Int32(1021234)
	msgdata.IPAddress = proto.String("0.0.0.0")
	msgdata.Cer = proto.String("0D75753B9A924D67853B71B89E1B1A92")
	msgdata.HDType = proto.Int32(1)
	msgdata.HDCode = proto.String("ae9a9b34-2a69-4bf9-a48d-ddbcffb6d4c4")

	msgData, _ := proto.Marshal(msgdata)

	m := msg.NewMessage(int32(netproto.MessageBClassID_NewByDBServer), int32(netproto.NewBYDBServerClassID_UserLoginFishID), msgData)
	handler := new(UserLoginFishHandler)
	if !handler.IsHook(m.BClassID, m.SClassID) {
		t.Error("hook 錯誤")
	}

	ctx := new(module.MouduleCtx)
	clt := new(network.ConnInfo)
	handler.OnMessage(ctx, clt, m)
	ret := handler.OnMessage(ctx, clt, m)
	if ret[0].BClassID != int32(netproto.MessageBClassID_NewByDBServer) || ret[0].SClassID != int32(netproto.NewBYDBServerClassID_UserLoginFishRetID) {
		t.Error("OnMessage 錯誤")
	}

	t.Log("查看回傳訊息", ret[0].MsgData.(*netproto.UserLoginFishRetMsg).GetUserID())
}

func initDBConnection() {
	_, filename, _, _ := runtime.Caller(0)

	//加载配置信息
	configDir := path.Dir(filename) + "/../../../config"

	//將單元測試編譯成執行程式, 將下行configDir註解打開, 在console下指令go test -c , 即可找到msghandler.test檔案
	//在C建立dbserverTestTool資料夾, 新增log資料夾, 新增config資料夾與設定. 將檔案與資料放一起即可.
	//configDir = "C:/dbserverTestTool/config"

	cfgs := new(config.ConfigGroup)
	cfgs.LoadConfig(configDir, 0)

	ctx := new(module.MouduleCtx)
	//初始化配置
	ctx.Config = cfgs
	//设置数据库连接
	ConnString = ctx.GetConfigData("ns", "conn", "connstring")

	ConnString = strings.Replace(ConnString, "\"", "", -1)
	//设置连接池大小
	psize := ctx.GetConfigData("db", "conn", "pool")
	ipsize, _ := strconv.Atoi(psize)
	PoolSize = ipsize

	InitDatabase()
}