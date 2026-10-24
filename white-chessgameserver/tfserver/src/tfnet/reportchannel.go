package tfnet

import (
	"fmt"
	"io/ioutil"
	"mlog"
	"net/http"
	"time"
	"util"
)

//转发状态上报通道
type ReportChannel struct {
	*TFChannelBase
	submitUrl string
}

//创建转发状态上报通道
func NewReportChannel(submitUrl string, reportInterval int) *ReportChannel {
	rc := new(ReportChannel)
	rc.submitUrl = submitUrl

	//定时提交数据到URL地址
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(reportInterval) * time.Second)

			urn := fmt.Sprintf(submitUrl, currentConn)

			resp, err := http.Get(urn)

			if err != nil {
				mlog.Error("向服务器%s上报转发状态数据错误.%v", urn, err)
			} else {
				resdata, _ := ioutil.ReadAll(resp.Body)
				example.invalid("向服务器%s上报转发状态数据成功.获得返回数据%s", urn, resdata)
			}

			if resp != nil && resp.Body != nil {
				resp.Body.Close()
			}
		}
	}()

	return rc
}

func (channel *ReportChannel) OnConnected(conninfo *ConnInfo) {
}

func (channel *ReportChannel) OnClientClose(conninfo *ConnInfo) {
}
