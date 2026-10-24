package module

import (
	"fmt"
	"os"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//模块运行上下文
type MouduleCtx struct {
	EvtDispatcher *event.EventDispatcher //事件分派器
	Logger        mlog.Logger            //日志记录器
	Config        *config.ConfigGroup    //配置信息
	Server        *msgserver.MsgServer   //服务器
}

//向控制台输出
func (ctx *MouduleCtx) Printf(format string, args ...interface{}) {
	fmt.Printf(format+"\r\n", args...)
}

func (ctx *MouduleCtx) Println(v ...interface{}) {
	fmt.Println(v...)
}

//写日志
func (ctx *MouduleCtx) WriteLog(lvl int, format string, a ...interface{}) {
	if lvl < conf.CLogLevel {
		return
	}

	ctx.Logger.WriteLog(lvl, format, a...)

	if lvl >= int(mlog.FatalLevel) {
		os.Exit(1)
	}
}

//调试信息
func (ctx *MouduleCtx) Trace(format string, a ...interface{}) {
	ctx.WriteLog(int(mlog.TraceLevel), format, a...)
}

//调试信息
func (ctx *MouduleCtx) Debug(format string, a ...interface{}) {
	ctx.WriteLog(int(mlog.DebugLevel), format, a...)
}

//日志信息
func (ctx *MouduleCtx) Info(format string, a ...interface{}) {
	ctx.WriteLog(int(mlog.InfoLevel), format, a...)
}

//错误信息
func (ctx *MouduleCtx) Error(format string, a ...interface{}) {
	ctx.WriteLog(int(mlog.ErrorLevel), format, a...)
}

//崩溃信息
func (ctx *MouduleCtx) Fatal(format string, a ...interface{}) {
	ctx.WriteLog(int(mlog.FatalLevel), format, a...)
}

//获取配置信息
func (ctx *MouduleCtx) GetConfig(mname string) *config.Config {
	cfg := ctx.Config.GetConfig(mname)

	return cfg
}

//获取配置节
func (ctx *MouduleCtx) GetConfigSection(mname, secname string) config.ConfigSection {
	cfg := ctx.GetConfig(mname)
	if cfg == nil {
		return nil
	}

	return cfg.Sections[secname]
}

//获取配置数据
func (ctx *MouduleCtx) GetConfigData(mname, secname, key string) string {
	cfgsex := ctx.GetConfigSection(mname, secname)

	if cfgsex == nil {
		return ""
	}

	return cfgsex[key]
}

//触发事件
func (ctx *MouduleCtx) TriggerEvent(evt *event.Event) *event.EventRet {
	return ctx.EvtDispatcher.OnEvent(evt)
}

//触发异步事件
func (ctx *MouduleCtx) TriggerAsyncEventCallback(evt *event.Event, callback event.CallbackFun) {
	ctx.EvtDispatcher.OnAsyncEvent(evt, callback)
}

//触发异步事件
func (ctx *MouduleCtx) TriggerAsyncEvent(evt *event.Event) {
	ctx.EvtDispatcher.OnAsyncEvent(evt, nil)
}

//注册消息解析器
func (ctx *MouduleCtx) RegisterMsgParser(bclsID int32, sclsid int32, bcp msg.MsgParser) {
	ctx.Server.GetMsgParserGroup().RegisterParser(bclsID, sclsid, bcp)
}

//注册单消息解析器函数
func (ctx *MouduleCtx) RegisterMsgParserFunc(bclsID int32, sclsid int32, f func() proto.Message) {
	umf := func(data []byte) (*msg.Message, error) {
		m1 := new(msg.Message)
		m1.BClassID = bclsID
		m1.SClassID = sclsid

		mm := f()

		err := proto.Unmarshal(data[8:], mm)
		if err != nil {
			return nil, err
		}
		m1.MsgData = mm

		return m1, nil
	}

	mf := func(msg *msg.Message) ([]byte, error) {
		b1 := util.IntToBytes(int32(bclsID), true)
		b2 := util.IntToBytes(int32(sclsid), true)

		d := msg.MsgData.(proto.Message)
		b3, err := proto.Marshal(d)
		if err != nil {
			return nil, err
		}

		return append(append(b1, b2...), b3...), nil
	}

	ctx.Server.GetMsgParserGroup().RegisterParser(bclsID, sclsid, msg.NewCommonMsgParser(mf, umf))
}

//注册事件处理器
func (ctx *MouduleCtx) RegisterEventHandler(name string, f func(evt *event.Event, ret *event.EventRet) bool, evts ...string) {
	h := event.NewCommonHandler(name, f, evts...)

	ctx.EvtDispatcher.PushHandler(h)
}

//客户端连接
func (ctx *MouduleCtx) OnConnect(f func(clt *network.ConnInfo) bool) {
	h := event.NewCommonHandler("conn", func(evt *event.Event, evtret *event.EventRet) bool {
		clt := msgserver.GetClientInfo(evt.Args)
		return f(clt)
	}, msgserver.EVT_CLIENTCONNECT)

	ctx.EvtDispatcher.PushHandler(h)
}

//客户端断开连接
func (ctx *MouduleCtx) OnDisConnect(f func(clt *network.ConnInfo) bool) {
	h := event.NewCommonHandler("disconn", func(evt *event.Event, evtret *event.EventRet) bool {
		clt := msgserver.GetClientInfo(evt.Args)

		return f(clt)
	}, msgserver.EVT_CLIENTDISCONNECT)

	ctx.EvtDispatcher.PushHandler(h)
}

//客户端消息
func (ctx *MouduleCtx) OnMessage(f func(clt *network.ConnInfo, msg *msg.Message) bool) {
	h := event.NewCommonHandler("msg", func(evt *event.Event, evtret *event.EventRet) bool {
		clt := msgserver.GetClientInfo(evt.Args)
		m := msgserver.GetClientMsg(evt.Args)

		return f(clt, m)
	}, msgserver.EVT_CLIENTMSG)

	ctx.EvtDispatcher.PushHandler(h)
}
