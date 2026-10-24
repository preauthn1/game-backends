package msghandler

import (
	"player"
	"util"
	"netproto"
	"module"
	"network"
	"msg"
	"rpcmsg"
	"example.invalid/golang/protobuf/proto"
	"time"
	"net/http"
	"mlog"
	"io/ioutil"
	"bytes"
	"net/url"
	"strings"
)

type ProxyPOSTRequestHandler struct {
	PlayerManager *player.PlayerManager
	evtChans      chan func()
}

func NewProxyPOSTRequestHandler(PlayerManager *player.PlayerManager) *ProxyPOSTRequestHandler {
	handler := new(ProxyPOSTRequestHandler)
	handler.PlayerManager = PlayerManager
	handler.evtChans = make(chan func(), 10000)
	for i:=0;i<10;i++ {
		go func() {
			for {
				callfunc := <- handler.evtChans
				if callfunc != nil {
					util.SafeCall(callfunc)
				}
			}
		}()
	}
	return handler
}

func (h *ProxyPOSTRequestHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_ProxyPOSTRequestID)
}

func (h *ProxyPOSTRequestHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	_, rmm := rpcmsg.ParseRPCMessage(m, func() proto.Message {
		mm := &netproto.ProxyPOSTData{}
		return mm
	})

	msgData := rmm.(*netproto.ProxyPOSTData)

	if pr := h.PlayerManager.GetPlayerByConnID(clt.ConnID());pr != nil || !pr.IsGuest(){
		evtfunc := func() {
			url2 := msgData.GetUrl()
			body := h.proxyPostRequest(url2, msgData.GetHtmlData())
			retmsg := new(msg.Message)
			retmsg.BClassID = int32(netproto.MessageBClassID_Hall)
			retmsg.SClassID = int32(netproto.HallMsgClassID_ProxyPOSTRequestRetID)
			data := new(netproto.ProxyData)
			data.Data = proto.String(body)

			data.Type = proto.String(msgData.GetType())

			retmsg.MsgData = data
			ctx.Server.SendMessage(clt, retmsg)
		}
		h.evtChans <- evtfunc
	} else {
		ctx.Error("找不到連線用戶資料 %v", pr)
	}
	return nil
}

func (h *ProxyPOSTRequestHandler) proxyPostRequest(url2 string, htmldata string) string {
	timeout := time.Duration(10 * time.Second)
	client := http.Client{
		Timeout: timeout,
	}

	v := url.Values{}

	sectionStr := strings.Split(htmldata, "&")
	for _, sectionVal := range sectionStr {
		if len(sectionVal) == 0 {
			break
		}
		param := strings.Split(sectionVal, "=")
		v.Add(param[0], param[1])
	}

	mlog.Debug("post URL : %v", url2)
	mlog.Debug("Post內容 : %v", v.Encode())
	resp, err := client.Post(url2, "application/x-www-form-urlencoded", bytes.NewBufferString(v.Encode()))

	if err != nil {
		mlog.Error("調用 %s 錯誤, %v", url2, err)
		return ""
	}

	defer resp.Body.Close()

	body, err := ioutil.ReadAll(resp.Body)

	if err != nil {
		// handle error
		mlog.Error("調用 %s 錯誤, %v", url2, err)
	}
	mlog.Debug("POST Proxy Result : %v", string(body))
	return string(body)
}

