package tfnet

import (
	"example.invalid/gorilla/websocket"
	"example.invalid/ServerTeam/Servers/internal/tfserver/router"
	"example.invalid/ServerTeam/Servers/internal/tfserver/tfconfig"
	"example.invalid/ServerTeam/Servers/pkg/conf"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/util"
	"log"
	"net"
	"time"

	"net/http"
	"regexp"
	"strconv"
)

type WebSocketServer struct {
	portProvider TFListenPortProvider
}

func NewWebSocketServer(portProvider TFListenPortProvider) *WebSocketServer {
	wss := new(WebSocketServer)
	wss.portProvider = portProvider

	return wss
}

func (server *WebSocketServer) Start() {
	cfg := tfconfig.ConfigCtx
	upgrader := &websocket.Upgrader{
		//如果有 cross domain 的需求，可加入這個，不檢查 cross domain
		CheckOrigin: func(r *http.Request) bool { return true },
	}
	//http://0.0.0.0/ws/10000
	//http://0.0.0.0/ws/10101
	http.HandleFunc("/ws/", func(w http.ResponseWriter, r *http.Request) {
		c, err := upgrader.Upgrade(w, r, nil)
		if err != nil {
			log.Println("upgrade:", err)
			return
		}
		re, _ := regexp.Compile("/ws/([0-9]+)")
		match := re.FindStringSubmatch(r.URL.Path)
		if len(match) < 1 {
			mlog.Error("url match %s error %v", re.String(), err)
			return
		}

		sourcePort, _ := strconv.Atoi(match[1])
		ip, _ := util.ParseIPAndPort(r.RemoteAddr)
		rd := router.NewRouteData(ip, sourcePort)
		//获取路由目标
		tg := router.RouteCtx.GetRouteTarget(rd)
		if tg == nil {
			mlog.Error("未找到匹配的路由目标 %s, %d.关闭通道.", r.RemoteAddr, sourcePort)
			return
		}

		go server.handleConn(c, tg)
		//




		// read

	})
	mlog.System("server start at :" + strconv.Itoa(cfg.WebSocketServerPort))
	mlog.Fatal(http.ListenAndServe(":" + strconv.Itoa(cfg.WebSocketServerPort), nil).Error())
}

func (server *WebSocketServer) handleConn(clientConn *websocket.Conn, tg *router.RouteTarget) {
	var closeClientConn = func() {
		if clientConn == nil {
			return
		}
		//关闭客户端连接
		if err := clientConn.Close(); err == nil {
			mlog.Warn("client %s disconnect !!", clientConn.RemoteAddr())
		}
	}
	d := net.Dialer{Timeout: 3 * time.Second}
	backendConn, err := d.Dial("tcp", tg.String())
	if err != nil {
		mlog.Error("建立後端連線失敗 %s error %s", clientConn.RemoteAddr(), err.Error())
		closeClientConn()
		return
	}

	backendTcpConn := network.NewTcpConn(backendConn, conf.READBUFLEN)
	var closeBackendConn = func() {
		if backendConn == nil {
			return
		}
		//关闭服務器端连接
		if err := backendConn.Close(); err == nil {
			mlog.Warn("backend %s disconnect !!", backendConn.RemoteAddr())
		}
	}
	//處理服務器端轉發
	go func() {
		defer closeClientConn()
		defer closeBackendConn()
		defer util.PanicErrStack()

		for {
			msg, err := backendTcpConn.ReadMsg()

			if err != nil {
				mlog.Error("backend %s read error: %s and disconnect !!", backendConn.RemoteAddr(), err)
				break
			}

			// 转发消息
			err = clientConn.WriteMessage(websocket.BinaryMessage, msg)
			mlog.Debug("s2c ---%v", msg[:12])
			if err != nil {
				mlog.Error("client %s write error: %s and disconnect !!", clientConn.RemoteAddr(), err)
				break
			}

		}
	}()

	//處理客戶端消息轉發
	go func() {
		defer closeClientConn()
		defer closeBackendConn()
		defer util.PanicErrStack()
		for {
			mtype, msg, err := clientConn.ReadMessage()

			if mtype != websocket.BinaryMessage && mtype != -1 {
				mlog.Error("client %s message type error mtype [%d] and disconnect !!", clientConn.RemoteAddr(), mtype)
				break
			} else if mtype == -1 {
				break
			}
			if err != nil {
				mlog.Error("client %s read error: %s and disconnect !!", clientConn.RemoteAddr(), err)
				break
			}
			msglen := util.BytesToInt(msg[:4], true)
			if msglen <= 0 {
				clientConn.Close()
				return
			}
			_, err = backendTcpConn.WriteMsg(msg)
			mlog.Debug("c2s ---%v", msg[:12])
			if err != nil {
				mlog.Error("backend %s write error: %s and disconnect !!", backendConn.RemoteAddr(), err)
				break
			}

		}
	}()
	//if err != nil {
	//	mlog.Error("建立后端路由通道错误,%s,%v.关闭通道", tg, err)
	//}

}