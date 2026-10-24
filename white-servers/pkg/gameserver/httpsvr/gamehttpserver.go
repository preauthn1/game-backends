package httpsvr

import (
	"net/http"

	"example.invalid/ServerTeam/Servers/pkg/event"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//http服务
type GameHttpServer struct {
	addr        string
	eventChanel *event.EventDispatcher
}

//创建游戏Http服务器
func NewGameHttpServer(addr string, evtChannel *event.EventDispatcher) *GameHttpServer {
	svr := new(GameHttpServer)
	svr.addr = addr
	svr.eventChanel = evtChannel

	return svr
}

//启动服务器
func (svr *GameHttpServer) Start() {
	http.HandleFunc("/game", svr.gameHandler)

	go func() {
		defer util.PanicErrStack()

		err := http.ListenAndServe(svr.addr, nil)
		if err != nil {
			mlog.Error("%v", err)
		}
	}()

	example.invalid("HTTP服务器启动成功,侦听在地址%s...", svr.addr)
}

//http事件响应接口
func (svr *GameHttpServer) gameHandler(w http.ResponseWriter, req *http.Request) {
	mlog.Debug("接收到Http请求.%v", req)

	//构造http消息事件
	httpEvtData := make(map[string]interface{})

	callBackChan := util.NewSemaphore(0)
	httpEvtData[msgserver.HTTPMSGEVTDATA_SEMAPHORE] = callBackChan
	httpEvtData[msgserver.HTTPMSGEVTDATA_HTTPRESPONSEWRITER] = w
	httpEvtData[msgserver.HTTPMSGEVTDATA_HTTPREQUEST] = req

	httpEvt := msgserver.NewHttpMsgEvent(httpEvtData)
	svr.eventChanel.OnAsyncEvent(httpEvt, nil)

	callBackChan.Acquire()
	mlog.Debug("Http请求处理完成.%v", req)
}
