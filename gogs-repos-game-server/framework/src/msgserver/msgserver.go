package msgserver

import (
	"command"
	"conf"
	"errors"
	"event"
	"fmt"
	"mlog"
	"msg"
	"netproto"
	"network"
	"time"
	"util"
)

import (
	proto "example.invalid/golang/protobuf/proto"
)

//服务器运行模式
const (
	ServerMode_Direct = 0 //直连
	ServerMode_Route  = 1 //转发
)

//断线提示类型
const (
	TipMessageTypeID_Hall int32 = 2 //退出大厅
)

//服务器
type MsgServer struct {
	MsgParser    msg.MsgParser               //网络消息解析器
	NetServer    network.NetServer           //网络服务器
	EventChannel *event.EventDispatcher      //事件分派器
	MsgPackerFun func() network.MsgPacker    //消息分包处理器
	NetEventChan *network.ServerNetEventChan //网络服务器事件通道
	LogicHandler MsgServerLogicHandler       //服务器逻辑处理组件
	ServerMode   int                         //服务器模式
}

//创建后端消息服务器
func NewBgMsgServer(throttle int) *MsgServer {
	return NewMsgServer(throttle, ServerMode_Direct, 1024*500)
}

//创建客户端消息服务器
func NewFacadeMsgServer(throttle int, servermode int) *MsgServer {
	return NewMsgServer(throttle, servermode, conf.MSGPACKERBUF)
}

//创建客户端消息服务器
func NewMsgServer(throttle int, servermode int, msgMaxLen int32) *MsgServer {
	svr := new(MsgServer)

	//消息解析器
	msgparser := msg.NewMsgParserGroup()
	svr.MsgParser = msgparser
	svr.ServerMode = servermode

	//事件分派器
	evtdispatcher := event.NewAsyncEventDispatcher(throttle)
	svr.EventChannel = evtdispatcher

	//网络服务器事件通道
	svr.NetEventChan = network.NewNetEventChan(conf.EVTCHANSIZE)

	//消息分包处理器
	svr.MsgPackerFun = func() network.MsgPacker {
		return network.NewHeadMsgPacker(msgMaxLen, true)
	}

	//服务器组
	svrg := network.NewNetServerGroup("svrgroup")
	svr.NetServer = svrg

	//逻辑处理器
	logic := NewMsgServerLogicGroup()
	svr.LogicHandler = logic
	logic.closCltFunc = func(clt *network.ConnInfo) {

		svr.sendTipMessage(clt, "系统检测到您有异常的操作,拒绝了您当前的请求.请稍后再试!")

		go func() {
			time.Sleep(time.Duration(2) * time.Second)

			svr.NetServer.CloseConn(clt)
		}()
	}

	//添加命令
	cmd := NewMsgServerCmd(svr)
	command.AddCommand(cmd)

	return svr
}

//初始化服务器
func (s *MsgServer) InitNetServer() {

}

//添加TCP服务器
func (s *MsgServer) AddTCPServer(cfg *network.ServerCfg) network.NetServer {
	//tcp服务器
	tcpsvr, _ := network.NewTcpServer(s.MsgPackerFun, cfg)
	s.GetServerGroup().AddServer(tcpsvr)

	return tcpsvr
}

//启动服务器
func (s *MsgServer) Start() error {

	go func() {
		for {
			select {
			case clt := <-s.NetEventChan.ConnectSig:
				//直连模式,直接通知服务器逻辑处理.转发模式需要等到IP上报
				if s.ServerMode == ServerMode_Direct {
					s.LogicHandler.OnClientConnect(clt)
				}

				connevt := NewClientConnectEvt(clt)
				s.EventChannel.OnAsyncEvent(connevt, nil)

			case data := <-s.NetEventChan.DataSig:
				s.LogicHandler.OnClientData(data)

				dataevt := NewClientDataEvent(data.Conn, data.Data)
				s.EventChannel.OnAsyncEvent(dataevt, nil)

			case data := <-s.NetEventChan.MsgSig:
				msg, err := s.MsgParser.UnMarshal(data.Data)
				if err == nil && msg != nil {
					//處理 Token 協議
					//2019.3.8從tfapi改用遊戲盾因此不再需要檢查token.
					//s.checkTokenMsg(data.Conn, msg)
					//客户端连接在转发模式下工作,处理IP上报协议
					if s.ServerMode == ServerMode_Route && !data.Conn.IsServerConn(conf.TCPSERVERNAME_HALL_GAMESERVER) {
						s.checkIPReportMsg(data.Conn, msg)
						//转发模式,IP没上报,不处理任何消息
						if !data.Conn.IPRouted {
							mlog.Debug("客户端%v未上报IP", data.Conn)
							//s.sendTipMessage(data.Conn, "非法操作.")

							continue
						}
					}

					s.LogicHandler.OnClientMessage(data.Conn, msg)

					msgevt := NewClientMessageEvent(data.Conn, msg)
					s.EventChannel.OnAsyncEvent(msgevt, nil)
				} else {
					mlog.Error("解析消息错误%v,关闭客户端%s连接", err, data.Conn)
					util.AsyncSafeCall(func() {
						s.CloseConn(data.Conn)
					})

				}

			case clt := <-s.NetEventChan.DisConnectSig:
				s.LogicHandler.OnClientDisconnect(clt)

				disevt := NewClientDisconnectEvt(clt)
				s.EventChannel.OnAsyncEvent(disevt, nil)
			}
		}
	}()

	err := s.NetServer.Start(s.NetEventChan)
	if err != nil {
		return err
	}

	return nil
}

//检查IP上报协议
func (s *MsgServer) checkIPReportMsg(clt *network.ConnInfo, msg *msg.Message) {
	defer util.PanicErrStack()
	if msg.BClassID == int32(netproto.MessageBClassID_Common) && msg.SClassID == int32(netproto.PlatformCommonClassID_IPReportID) {
		dd := msg.MsgData.([]byte)
		mm := &netproto.IPReport{}

		err := proto.Unmarshal(dd, mm)

		if err != nil {
			mlog.Error("客户端%v发送IP上报数据包解析错误,%v", clt, err)

			s.NetServer.CloseConn(clt)
			return
		}

		//如果IP已经上报
		if clt.IPRouted {
			return
		}

		//设置客户端真实IP
		clt.IPAddress = mm.GetIPAddress()
		//设置IP上报标记
		clt.IPRouted = true

		mlog.Debug("客户端%v设置IP%s", clt, clt.IPAddress)

		//通知客户端连接
		s.LogicHandler.OnClientConnect(clt)

	}
}

//處理token上报协议
//從tfapi改用遊戲盾因此不再需要檢查token.
//func (s *MsgServer) checkTokenMsg(clt *network.ConnInfo, msg *msg.Message) {
//	defer util.PanicErrStack()
//	if msg.BClassID == int32(netproto.MessageBClassID_Common) && msg.SClassID == int32(netproto.PlatformCommonClassID_ConnTokenID) {
//		dd := msg.MsgData.([]byte)
//		mm := &netproto.ConnTokenMessage{}
//
//		err := proto.Unmarshal(dd, mm)
//
//		if err != nil {
//			mlog.Error("客户端%v发送Token数据包解析错误,%v", clt, err)
//			s.NetServer.CloseConn(clt)
//			return
//		}
//		//解密連線口令
//		token, err := ConnTokenDecryptor(mm.GetToken())
//		if err == nil {
//			tValid := time.Unix(int64(token.Time), +int64(1*time.Minute))
//			if time.Now().Before(tValid) {
//				clt.ConnToken = [REDACTED]
//				mlog.Debug("客户端%v设置ConnToken%s", clt, clt.ConnToken)
//			} else {
//				mlog.Warn("驗證連線 token 超時 tValid %s", tValid)
//			}
//		} else {
//			mlog.Error("驗證連線發生問題, %s, tokenstr %s", err, mm.GetToken())
//			//s.NetServer.CloseConn(clt) //先不掛掉連線
//			return
//		}
//		ret, err := NodeDefDecryptor(mm.GetToken())
//		if err == nil {
//			//设置客户端真实IP
//			clt.IPAddress = ret.Data.UserIP
//			//设置IP上报标记
//			clt.IPRouted = true
//			mlog.Debug("客户端%v设置IP%s", clt, clt.IPAddress)
//			//通知客户端连接
//			s.LogicHandler.OnClientConnect(clt)
//		}
//	}
//}

//向客户端发送断线提示消息
func (s *MsgServer) sendTipMessage(clt *network.ConnInfo, str string) {
	m := new(msg.Message)
	m.BClassID = int32(netproto.MessageBClassID_Common)
	m.SClassID = int32(netproto.PlatformCommonClassID_TipMessageID)

	tm := new(netproto.TipMessage)
	tm.Message = proto.String(str)
	tm.MsgType = proto.Int32(TipMessageTypeID_Hall)
	data, err := proto.Marshal(tm)

	if err != nil {
		mlog.Error("服务器防护,序列化断线提示消息错误%v", err)
		return
	}

	m.MsgData = data

	s.SendMessage(clt, m)
}

//触发RPC消息事件
func (s *MsgServer) OnRPCMessageEvent(msg *msg.Message) {
	evt := NewRPCMessageEvent(msg)

	s.EventChannel.OnAsyncEvent(evt, nil)
}

//停止服务器
func (s *MsgServer) Stop() {

	s.NetServer.Stop()
}

//发送消息
func (s *MsgServer) SendMessage(clt *network.ConnInfo, msg *msg.Message) (int, error) {
	data, err := s.MsgParser.Marshal(msg)

	if err != nil {
		return 0, err
	}

	if data == nil {
		return 0, errors.New("序列化消息错误,消息数据为空")
	}

	return s.NetServer.SendMessage(clt, data)
}

//发送数据
func (s *MsgServer) SendData(clt *network.ConnInfo, data []byte) (int, error) {
	return s.NetServer.SendMessage(clt, data)
}

//根据连接ID发送消息
func (s *MsgServer) SendMessageByConnID(connid string, msg *msg.Message) (int, error) {
	data, err := s.MsgParser.Marshal(msg)

	if err != nil {
		return 0, err
	}

	if data == nil {
		return 0, errors.New("序列化消息错误,消息数据为空")
	}

	clt := s.NetServer.GetConnByID(connid)

	if clt == nil {
		return 0, errors.New(fmt.Sprintf("连接%s不存在", connid))
	}

	return s.NetServer.SendMessage(clt, data)
}

//根据连接ID发送数据
func (s *MsgServer) SendDataByConnID(connid string, data []byte) (int, error) {
	clt := s.NetServer.GetConnByID(connid)

	if clt == nil {
		return 0, errors.New(fmt.Sprintf("连接%s不存在", connid))
	}

	return s.NetServer.SendMessage(clt, data)
}

//广播消息
func (s *MsgServer) SendAllMessage(msg *msg.Message) error {
	data, err := s.MsgParser.Marshal(msg)

	if err != nil {
		return err
	}

	if data == nil {
		return errors.New("序列化消息错误,消息数据为空")
	}

	s.NetServer.SendAllMessage(data)

	return nil
}

//广播数据
func (s *MsgServer) SendAllData(data []byte) error {
	s.NetServer.SendAllMessage(data)

	return nil
}

//获取客户端
func (s *MsgServer) GetClientByID(cid string) *network.ConnInfo {
	return s.NetServer.GetConnByID(cid)
}

//遍历客户端
func (s *MsgServer) RangeConn(f func(clt *network.ConnInfo)) {
	s.NetServer.RangeConn(f)
}

//关闭客户端连接
func (s *MsgServer) CloseConn(clt *network.ConnInfo) {
	s.NetServer.CloseConn(clt)
}

//关闭客户端连接
func (s *MsgServer) CloseConnByConnID(connid string) {
	clt := s.GetClientByID(connid)

	if clt != nil {
		s.CloseConn(clt)
	}
}

//获取服务器组
func (s *MsgServer) GetServerGroup() *network.NetServerGroup {
	return s.NetServer.(*network.NetServerGroup)
}

//获取消息解析器集合
func (s *MsgServer) GetMsgParserGroup() *msg.MsgParserGroup {
	return s.MsgParser.(*msg.MsgParserGroup)
}

//获取逻辑处理组件集合
func (s *MsgServer) GetLogicHandlerGroup() *MsgServerLogicGroup {
	return s.LogicHandler.(*MsgServerLogicGroup)
}
