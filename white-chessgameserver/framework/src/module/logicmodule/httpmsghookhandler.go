package logicmodule

import (
	"crypto/md5"
	"encoding/base64"
	"fmt"
	"io"
	"mlog"
	"net/http"
	"sort"
	"strings"
	"util"
)

import proto "example.invalid/golang/protobuf/proto"

//http消息处理器接口定义
type HttpMsgHookHandler interface {
	OnHttpMsg(semaphore util.Semaphore, w http.ResponseWriter, req *http.Request) bool
}

//http消息处理器基类定义
type HttpMsgHookHandlerBase struct {
}

//创建Http消息处理器基类对象
func NewHttpMsgHookHandlerBase() *HttpMsgHookHandlerBase {
	hb := new(HttpMsgHookHandlerBase)
	return hb
}

func (hb *HttpMsgHookHandlerBase) OnHttpMsg(semaphore util.Semaphore, w http.ResponseWriter, req *http.Request) bool {
	ret := hb.HttpHandler(w, req)
	if ret != nil {
		data, _ := proto.Marshal(ret)

		if data != nil {
			encodeStr := base64.StdEncoding.EncodeToString(data)
			io.WriteString(w, encodeStr)
		}
	}

	semaphore.Release()

	return false
}

//获取参数值
func (hb *HttpMsgHookHandlerBase) GetParamValue(req *http.Request, pName string) string {
	return req.PostFormValue(pName)
}

func (hb *HttpMsgHookHandlerBase) HttpHandler(w http.ResponseWriter, req *http.Request) proto.Message {
	return nil
}

//http消息处理器组
type HttpMsgHookHandlerGroup struct {
	handlers []HttpMsgHookHandler
}

//创建Http消息处理器组对象
func NewHttpMsgHookHandlerGroup() *HttpMsgHookHandlerGroup {
	hlg := new(HttpMsgHookHandlerGroup)
	hlg.handlers = make([]HttpMsgHookHandler, 0)

	return hlg
}

//添加处理器
func (hlg *HttpMsgHookHandlerGroup) AddHandler(h HttpMsgHookHandler) {
	hlg.handlers = append(hlg.handlers, h)
}

func (hlg *HttpMsgHookHandlerGroup) OnHttpMsg(semaphore util.Semaphore, w http.ResponseWriter, req *http.Request) bool {
	defer func() {
		semaphore.Release()
	}()

	for _, h := range hlg.handlers {
		ret := h.OnHttpMsg(semaphore, w, req)

		if !ret {
			return true
		}
	}

	return true
}

//http消息校验处理器
type ValidateHttpMsgHookHandler struct {
	code string
}

//创建http消息校验处理器对象
func NewValidateHttpMsgHookHandler(code string) *ValidateHttpMsgHookHandler {
	vh := new(ValidateHttpMsgHookHandler)
	vh.code = code

	return vh
}

func (vh *ValidateHttpMsgHookHandler) OnHttpMsg(semaphore util.Semaphore, w http.ResponseWriter, req *http.Request) bool {

	//获取校验码
	pvcode := req.PostFormValue("vcode")
	if pvcode == "" {
		io.WriteString(w, "vcode error.")
		return false
	}

	//对参数排序
	psarr := make([]string, 0)
	for p, _ := range req.PostForm {
		if strings.ToLower(p) == "vcode" {
			continue
		}

		psarr = append(psarr, p)
	}
	sort.Sort(sort.StringSlice(psarr))

	//构造参数加密字符串
	strData := ""
	for _, ps := range psarr {
		strData += ps
		strData += req.PostFormValue(ps)
	}
	strData += vh.code

	//生成校验码
	cryptW := md5.New()
	io.WriteString(cryptW, strData)
	vcode := fmt.Sprintf("%x", cryptW.Sum(nil))

	//检查校验码
	if strings.ToUpper(vcode) == strings.ToUpper(pvcode) {
		return true
	}

	mlog.Debug("http vlidate,orign:%s,md5:%s ", strData, vcode)

	io.WriteString(w, "vcode error.")

	return false
}
